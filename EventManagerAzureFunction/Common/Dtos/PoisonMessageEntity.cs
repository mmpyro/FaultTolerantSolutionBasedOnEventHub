using Common.Helpers;
using Common.Wrappers;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Common.Dtos
{
    public class PoisonMessageEntity : TableEntity
    {
        public PoisonMessageEntity(EventDataWrapper eventDataWrapper)
        {
            PartitionKey = eventDataWrapper.Properties[Constants.VehicleId].ToString();
            RowKey = DateTime.UtcNow.ToEpochTimestamp().ToString();
            Body = eventDataWrapper.Body;
        }

        public PoisonMessageEntity()
        {

        }

        public string Body { get; set; }
    }
}
