using InvenTrack.Domain.Entities;

namespace InvenTrack.Application.Interfaces;

public interface IImportJobRepository : IRepository<ImportJob>
{
    Task<IEnumerable<ImportJob>> GetPendingJobsAsync();
}
