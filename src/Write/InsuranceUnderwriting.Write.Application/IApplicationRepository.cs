using InsuranceUnderwriting.Write.Domain;

namespace InsuranceUnderwriting.Write.Application;

public interface IApplicationRepository
{
    Task<InsuranceApplication> GetById(Guid id);
    Task SaveNew(Guid id, object @event);
    Task AppendEvent(Guid id, object @event);
}
