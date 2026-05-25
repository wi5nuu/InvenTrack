using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class ImportJobRepository : Repository<ImportJob>, IImportJobRepository
{
    public ImportJobRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ImportJob>> GetPendingJobsAsync()
    {
        return await _dbSet
            .Include(j => j.User)
            .Where(j => j.Status == Domain.Enums.ImportStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync();
    }

    public override async Task<IEnumerable<ImportJob>> GetAllAsync()
    {
        return await _dbSet
            .Include(j => j.User)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public override async Task<ImportJob?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == id);
    }
}
