using InsuranceUnderwriting.Read.Domain;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceUnderwriting.Read.Api.Controllers;

// Query side only: serves the denormalized read model kept up to date by the
// Kafka consumer. Commands live in the Write microservice.
[ApiController]
[Route("api/applications")]
public class ApplicationsQueryController : ControllerBase
{
    private readonly IQuerySession _query;

    public ApplicationsQueryController(IQuerySession query) => _query = query;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var app = await _query.LoadAsync<ApplicationReadModel>(id);
        return app is null ? NotFound() : Ok(app);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _query.LoadAsync<ApplicationHistoryView>(id);
        return history is null ? NotFound() : Ok(history);
    }
}
