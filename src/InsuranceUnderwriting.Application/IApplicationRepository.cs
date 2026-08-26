using InsuranceUnderwriting.Domain;

namespace InsuranceUnderwriting.Application;

public interface IApplicationRepository
{
    Task<InsuranceApplication> GetById(Guid id);
    Task SaveNew(Guid id, object @event);
    Task AppendEvent(Guid id, object @event);
}
