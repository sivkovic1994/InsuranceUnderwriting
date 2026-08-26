using InsuranceUnderwriting.Domain;

namespace InsuranceUnderwriting.Application.Handlers;

public class ApprovePolicyHandler
{
    private readonly IApplicationRepository _repo;
    public ApprovePolicyHandler(IApplicationRepository repo) => _repo = repo;

    public async Task Handle(ApprovePolicyCommand cmd)
    {
        await _repo.AppendEvent(cmd.ApplicationId, new PolicyApproved(cmd.ApplicationId, DateTime.UtcNow));
    }
}
