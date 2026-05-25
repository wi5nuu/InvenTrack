using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;

namespace InvenTrack.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }
}
