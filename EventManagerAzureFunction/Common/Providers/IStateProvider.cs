using System;

namespace Common.Providers
{
    public interface IStateProvider
    {
        long? GetState(string keyName);
        void IncrementState(string keyName);
        void Reset(string keyName);
        void SetState(string keyName, TimeSpan ttl);
    }
}