using Common.Wrappers;
using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Dtos
{
    public class PoisonMessageEntity : TableEntity
    {
        public PoisonMessageEntity(EventDataWrapper eventDataWrapper)
        {
            PartitionKey = eventDataWrapper.Properties[Constants.VehicleId].ToString();
            RowKey = eventDataWrapper.Body;
        }

        public PoisonMessageEntity()
        {

        }
    }
}
