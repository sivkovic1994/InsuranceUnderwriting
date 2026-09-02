using InsuranceUnderwriting.Application;
using InsuranceUnderwriting.Infrastructure.Projections;
using Marten;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceUnderwriting.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationRepository _repo;
    private readonly IQuerySession _query;

    public ApplicationsController(IMediator mediator, IApplicationRepository repo, IQuerySession query)
    {
        _mediator = mediator;
        _repo = repo;
        _query = query;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitApplicationCommand cmd)
    {
        var id = await _mediator.Send(cmd);
        return Ok(new { ApplicationId = id });
    }

    [HttpPost("{id}/assess-risk")]
    public async Task<IActionResult> AssessRisk(Guid id)
    {
        await _mediator.Send(new AssessRiskCommand(id));
        return Ok();
    }

    [HttpPost("{id}/calculate-premium")]
    public async Task<IActionResult> CalculatePremium(Guid id)
    {
        await _mediator.Send(new CalculatePremiumCommand(id));
        return Ok();
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _mediator.Send(new ApprovePolicyCommand(id));
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var app = await _repo.GetById(id);
        return Ok(app);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await _query.LoadAsync<ApplicationHistoryView>(id);
        return history is null ? NotFound() : Ok(history);
    }
}
