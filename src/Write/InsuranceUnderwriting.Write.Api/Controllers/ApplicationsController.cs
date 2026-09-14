using InsuranceUnderwriting.Write.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceUnderwriting.Write.Api.Controllers;

// Command side only: accepts writes and appends to the event store.
// Queries live in the Read microservice.
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator) => _mediator = mediator;

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
}
