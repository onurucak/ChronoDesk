using System.Threading.Tasks;

namespace ChronoDesk.Application.Interfaces;

public interface IDataMaintenanceService
{
    Task ClearAllDataAsync();
}
