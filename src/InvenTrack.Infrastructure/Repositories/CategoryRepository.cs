using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _dbSet.Include(c => c.Products).ToListAsync();
    }

    public override async Task<Category?> GetByIdAsync(int id)
    {
        return await _dbSet.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
    }
}
