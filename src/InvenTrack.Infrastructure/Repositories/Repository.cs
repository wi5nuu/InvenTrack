using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using InvenTrack.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => EF.Property<string>(e, "Name") != null && EF.Property<string>(e, "Name").Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}
