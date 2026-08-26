using InsuranceUnderwriting.Domain;
using MediatR;

namespace InsuranceUnderwriting.Application.Handlers;

public class SubmitApplicationHandler : IRequestHandler<SubmitApplicationCommand, Guid>
{
    private readonly IApplicationRepository _repo;
    public SubmitApplicationHandler(IApplicationRepository repo) => _repo = repo;

    public async Task<Guid> Handle(SubmitApplicationCommand cmd, CancellationToken cancellationToken)
    {
        var (app, @event) = InsuranceApplication.Submit(cmd.ClientName, cmd.InsuranceType);
        await _repo.SaveNew(app.Id, @event);
        return app.Id;
    }
}
