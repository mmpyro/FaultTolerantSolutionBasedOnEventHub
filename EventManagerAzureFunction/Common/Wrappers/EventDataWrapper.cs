using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Wrappers
{
    public class EventDataWrapper
    {
        public EventDataWrapper(EventData eventData)
        {
            SystemProperties = new Dictionary<string, object>();
            Body = Encoding.UTF8.GetString(eventData.Body);
            Properties = eventData.Properties;
            SystemProperties = eventData.SystemProperties;
            EnqueueTimeUTC = eventData.SystemProperties.EnqueuedTimeUtc;
            Offset = eventData.SystemProperties.Offset;
            PartitionKey = eventData.SystemProperties.PartitionKey;
            SequenceNumber = eventData.SystemProperties.SequenceNumber;
        }

        public EventDataWrapper()
        {

        }

        public virtual string Body { get; protected set; }
        public virtual IDictionary<string, object> SystemProperties { get; protected set; }
        public virtual IDictionary<string, object> Properties { get; protected set; }
        public virtual DateTime EnqueueTimeUTC { get; protected set; }
        public virtual string Offset { get; protected set; }
        public virtual string PartitionKey { get; protected set; }
        public virtual long SequenceNumber { get; protected set; }
    }
}
