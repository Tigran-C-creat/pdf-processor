using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Persistence.Data;
using Persistence.Models;
using UglyToad.PdfPig;
using Contracts.Messages;

namespace BackgroundWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly IServiceProvider _serviceProvider;

        public Worker(
            ILogger<Worker> logger,
            IConnection connection,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _connection = connection;
            _serviceProvider = serviceProvider;

            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync(
                queue: "pdf_processing",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            ).GetAwaiter().GetResult();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<PdfProcessingMessage>(json);

                    if (message == null)
                    {
                        _logger.LogError("Failed to deserialize message");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    _logger.LogInformation("Worker received message for document {DocumentId}", message.DocumentId);

                    // ===== 1. СНАЧАЛА МЕНЯЕМ СТАТУС НА PROCESSING =====
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var doc = await db.Documents.FindAsync(message.DocumentId, stoppingToken);
                    if (doc == null)
                    {
                        _logger.LogWarning("Document {DocumentId} not found in database", message.DocumentId);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    // Устанавливаем статус "В обработке"
                    doc.Status = DocumentStatus.Processing;
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Document {DocumentId} status changed to Processing", message.DocumentId);
                    // =============================================

                    // 2. Проверяем файл
                    if (!File.Exists(message.FilePath))
                    {
                        _logger.LogError("File not found: {FilePath}", message.FilePath);
                        doc.Status = DocumentStatus.Failed;
                        await db.SaveChangesAsync(stoppingToken);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    // 3. Извлекаем текст из PDF
                    var extractedText = new StringBuilder();
                    try
                    {
                        using (var pdf = PdfDocument.Open(message.FilePath))
                        {
                            foreach (var page in pdf.GetPages())
                            {
                                extractedText.AppendLine(page.Text);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error extracting text from PDF");
                        doc.Status = DocumentStatus.Failed;
                        await db.SaveChangesAsync(stoppingToken);
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    _logger.LogInformation("PDF extracted: {Length} chars from document {DocumentId}",
                        extractedText.Length, message.DocumentId);

                    // 4. Сохраняем результат
                    doc.TextContent = extractedText.ToString();
                    doc.Status = DocumentStatus.Completed;
                    doc.ProcessedAt = DateTime.UtcNow;

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Document {DocumentId} updated in database with status {Status}",
                        message.DocumentId, DocumentStatus.Completed);

                    // 5. Подтверждаем сообщение
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing PDF");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "pdf_processing",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Worker started listening to queue: pdf_processing");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopping...");

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}