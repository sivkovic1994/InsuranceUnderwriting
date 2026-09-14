using InsuranceUnderwriting.Read.Infrastructure.Kafka;
using Marten;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Connection string 'Postgres' not found"));
    // Read side: Marten used purely as a document store for denormalized
    // views, no event streams or projections here - those are populated
    // directly by the Kafka consumer.
});

var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    ?? throw new InvalidOperationException("Kafka:BootstrapServers not configured");
var kafkaConsumerGroup = builder.Configuration["Kafka:ConsumerGroup"]
    ?? throw new InvalidOperationException("Kafka:ConsumerGroup not configured");

builder.Services.AddHostedService(sp => new KafkaConsumerHostedService(
    kafkaBootstrapServers,
    kafkaConsumerGroup,
    sp.GetRequiredService<IServiceScopeFactory>(),
    sp.GetRequiredService<ILogger<KafkaConsumerHostedService>>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
