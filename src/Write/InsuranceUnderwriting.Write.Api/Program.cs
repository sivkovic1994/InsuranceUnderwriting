using InsuranceUnderwriting.Write.Application;
using InsuranceUnderwriting.Write.Domain;
using InsuranceUnderwriting.Write.Domain.Services;
using InsuranceUnderwriting.Write.Infrastructure;
using InsuranceUnderwriting.Write.Infrastructure.Kafka;
using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Connection string 'Postgres' not found"));
    // Write side only: an event store snapshot for aggregate rehydration.
    // No read-model projections here - those live in the Read service.
    options.Projections.Snapshot<InsuranceApplication>(JasperFx.Events.Projections.SnapshotLifecycle.Inline);
});

var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    ?? throw new InvalidOperationException("Kafka:BootstrapServers not configured");

builder.Services.AddSingleton<IIntegrationEventPublisher>(
    _ => new KafkaIntegrationEventPublisher(kafkaBootstrapServers));

builder.Services.AddScoped<IApplicationRepository, MartenApplicationRepository>();
builder.Services.AddScoped<RiskAssessmentService>();
builder.Services.AddScoped<PremiumCalculationService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubmitApplicationCommand).Assembly));

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
