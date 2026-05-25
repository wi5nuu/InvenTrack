using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Name == name);
    }

    public override async Task<IEnumerable<Supplier>> GetAllAsync()
    {
        return await _dbSet.Include(s => s.Products).ToListAsync();
    }

    public override async Task<Supplier?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);
    }
}
