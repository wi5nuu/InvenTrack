using InvenTrack.Domain.Entities;

namespace InvenTrack.Application.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByNameAsync(string name);
}
