using Common.Providers;

namespace Common.Factories
{
    public interface IStateProviderFactory
    {
        IStateProvider Create(string endpoint);
    }
}