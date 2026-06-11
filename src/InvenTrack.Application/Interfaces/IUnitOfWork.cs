using InvenTrack.Domain.Entities;

namespace InvenTrack.Application.Interfaces;

/// <summary>Unit of Work pattern — coordinates repository access and database transactions.</summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ISupplierRepository Suppliers { get; }
    IStockMovementRepository StockMovements { get; }
    IImportJobRepository ImportJobs { get; }
    IAlertLogRepository AlertLogs { get; }
    IAuditLogRepository AuditLogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
