using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
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
    private readonly ElasticsearchClient _searchClient;

    public ApplicationsQueryController(IQuerySession query, ElasticsearchClient searchClient)
    {
        _query = query;
        _searchClient = searchClient;
    }

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

    // Full-text search over client name, insurance type and history entries,
    // backed by the Elasticsearch index - not something the Marten lookup-by-id
    // documents are meant for.
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required");

        var response = await _searchClient.SearchAsync<ApplicationSearchDocument>(s => s
            .Query(query => query
                .MultiMatch(m => m
                    .Fields(new[] { "clientName", "insuranceType", "history" })
                    .Query(q))));

        return Ok(response.Documents);
    }
}
