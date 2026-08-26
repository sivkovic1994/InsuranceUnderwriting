using InsuranceUnderwriting.Domain;
using MediatR;

namespace InsuranceUnderwriting.Application.Handlers;

public class ApprovePolicyHandler : IRequestHandler<ApprovePolicyCommand>
{
    private readonly IApplicationRepository _repo;
    public ApprovePolicyHandler(IApplicationRepository repo) => _repo = repo;

    public async Task Handle(ApprovePolicyCommand cmd, CancellationToken cancellationToken)
    {
        await _repo.AppendEvent(cmd.ApplicationId, new PolicyApproved(cmd.ApplicationId, DateTime.UtcNow));
    }
}
