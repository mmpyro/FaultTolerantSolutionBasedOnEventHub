using System.Threading.Tasks;
using Common.Dtos;

namespace Common.Repositories
{
    public interface IRepository
    {
        Task AddAsync(VehicleSnapshot vehicleSnapshot);
        VehicleSnapshot Get(string id);
    }
}