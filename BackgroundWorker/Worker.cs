using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Contracts.Messages;
using Persistence.Models;
using BackgroundWorker.Services;

namespace BackgroundWorker;

/// <summary>
/// Фоновый сервис для обработки PDF из очереди RabbitMQ.
/// </summary>
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
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<PdfProcessingMessage>(json);

                if (message == null)
                {
                    _logger.LogError("Failed to deserialize message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var docService = scope.ServiceProvider.GetRequiredService<IDocumentProcessingService>();
                var extractor = scope.ServiceProvider.GetRequiredService<IPdfExtractor>();

                // 1. Получаем документ
                var doc = await docService.GetAsync(message.DocumentId, stoppingToken);
                if (doc == null)
                {
                    _logger.LogWarning("Document {DocumentId} not found", message.DocumentId);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                // 2. Ставим статус Processing
                await docService.SetStatusAsync(doc, DocumentStatus.Processing, stoppingToken);

                // 3. Проверяем файл
                if (!File.Exists(message.FilePath))
                {
                    _logger.LogError("File not found: {FilePath}", message.FilePath);
                    await docService.SetStatusAsync(doc, DocumentStatus.Failed, stoppingToken);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                // 4. Извлекаем текст через сервис
                string text;
                try
                {
                    text = extractor.ExtractText(message.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting text from PDF");
                    await docService.SetStatusAsync(doc, DocumentStatus.Failed, stoppingToken);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    return;
                }

                // 5. Сохраняем результат
                await docService.SaveExtractedTextAsync(doc, text, stoppingToken);

                // 6. Подтверждаем сообщение
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
}