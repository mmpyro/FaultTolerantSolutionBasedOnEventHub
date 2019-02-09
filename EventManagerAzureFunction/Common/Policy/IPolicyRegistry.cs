using Polly;

namespace Common.Policy
{
    public interface IPolicyRegistry
    {
        IAsyncPolicy[] CreateAsyncPolicies();
        Polly.Policy[] CreatePolicies();
    }
}