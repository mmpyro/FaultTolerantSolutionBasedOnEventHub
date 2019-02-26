using Common.Helpers;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace Common.Dtos
{
    public class VehicleSnapshotEntity : TableEntity
    {
        public VehicleSnapshotEntity(VehicleSnapshot vehicleSnapshot, Exception ex)
        {
            PartitionKey = vehicleSnapshot.Id;
            RowKey = DateTime.UtcNow.ToEpochTimestamp().ToString();
            ErrorMessage = ex.Message;
            ErrorType = ex.GetType().FullName;
            VehicleSnapshotJson = JsonConvert.SerializeObject(vehicleSnapshot);
        }

        public VehicleSnapshotEntity()
        {

        }

        public string VehicleSnapshotJson { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
    }
}
