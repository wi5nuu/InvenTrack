using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace InvenTrack.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private IUserRepository? _users;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ISupplierRepository? _suppliers;
    private IStockMovementRepository? _stockMovements;
    private IImportJobRepository? _importJobs;
    private IAlertLogRepository? _alertLogs;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);
    public IStockMovementRepository StockMovements => _stockMovements ??= new StockMovementRepository(_context);
    public IImportJobRepository ImportJobs => _importJobs ??= new ImportJobRepository(_context);
    public IAlertLogRepository AlertLogs => _alertLogs ??= new AlertLogRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
