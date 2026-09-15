using Elastic.Clients.Elasticsearch;
using InsuranceUnderwriting.Read.Infrastructure;
using InsuranceUnderwriting.Read.Infrastructure.Kafka;
using InsuranceUnderwriting.Read.Infrastructure.Search;
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

var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"]
    ?? throw new InvalidOperationException("Elasticsearch:Uri not configured");
builder.Services.AddSingleton(new ElasticsearchClient(
    new ElasticsearchClientSettings(new Uri(elasticsearchUri)).DefaultIndex(ElasticsearchProjector.IndexName)));

// Every command's integration event flows through both projectors, updating
// the lookup-by-id read model (Marten) and the full-text search index (Elasticsearch).
builder.Services.AddScoped<IReadModelProjector, ApplicationReadModelUpdater>();
builder.Services.AddScoped<IReadModelProjector, ElasticsearchProjector>();

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

await EnsureSearchIndexExists(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureSearchIndexExists(IServiceProvider services)
{
    var client = services.GetRequiredService<ElasticsearchClient>();
    var exists = await client.Indices.ExistsAsync(ElasticsearchProjector.IndexName);
    if (!exists.Exists)
        await client.Indices.CreateAsync(ElasticsearchProjector.IndexName);
}
