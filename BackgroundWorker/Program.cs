using BackgroundWorker;
using BackgroundWorker.Services;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = builder.Configuration["RabbitMQ:User"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest",
        ConsumerDispatchConcurrency = 1  
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IPdfExtractor, PdfExtractor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
