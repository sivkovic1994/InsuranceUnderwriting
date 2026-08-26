using InsuranceUnderwriting.Domain;

namespace InsuranceUnderwriting.Application.Handlers;

public class SubmitApplicationHandler
{
    private readonly IApplicationRepository _repo;
    public SubmitApplicationHandler(IApplicationRepository repo) => _repo = repo;

    public async Task<Guid> Handle(SubmitApplicationCommand cmd)
    {
        var (app, @event) = InsuranceApplication.Submit(cmd.ClientName, cmd.InsuranceType);
        await _repo.SaveNew(app.Id, @event);
        return app.Id;
    }
}
