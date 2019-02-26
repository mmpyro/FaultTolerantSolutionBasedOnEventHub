using System;
using System.Threading.Tasks;
using Common.Dtos;
using Common.Wrappers;

namespace Common.Repositories
{
    public interface IPoisonMessageRepository
    {
        Task Save(EventDataWrapper eventDataWrapper);
        Task Save(VehicleSnapshot vehicleSnapshot, Exception err);
    }
}