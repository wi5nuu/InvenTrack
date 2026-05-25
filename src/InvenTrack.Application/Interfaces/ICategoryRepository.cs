using InvenTrack.Domain.Entities;

namespace InvenTrack.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}
