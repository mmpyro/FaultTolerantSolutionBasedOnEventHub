using StackExchange.Redis;
using System;

namespace Common.Providers
{
    public class StateProvider : IStateProvider
    {
        private readonly IDatabase _db;

        public StateProvider(string endpint)
        {
            var redis = ConnectionMultiplexer.Connect(endpint);
            _db = redis.GetDatabase();
        }

        public void SetState(string keyName, TimeSpan ttl)
        {
            _db.StringSet(keyName, 0, ttl);
        }

        public long? GetState(string keyName)
        {
            var redisValue = _db.StringGet(keyName);
            if(redisValue.HasValue)
            {
                long value;
                if(redisValue.TryParse(out value))
                {
                    return value;
                }
            }
            return null;
        }

        public void IncrementState(string keyName)
        {
            _db.StringIncrement(keyName);
        }

        public void Reset(string keyName)
        {
            _db.KeyDelete(keyName);
        }
    }
}
