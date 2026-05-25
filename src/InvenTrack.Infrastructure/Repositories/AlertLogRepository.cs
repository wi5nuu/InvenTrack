using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;

namespace InvenTrack.Infrastructure.Repositories;

public class AlertLogRepository : Repository<AlertLog>, IAlertLogRepository
{
    public AlertLogRepository(AppDbContext context) : base(context)
    {
    }
}
