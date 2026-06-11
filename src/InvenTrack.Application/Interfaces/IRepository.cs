using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

/// <summary>Generic repository with CRUD and pagination for all domain entities.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
}
