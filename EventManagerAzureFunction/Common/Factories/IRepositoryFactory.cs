using Common.Repositories;

namespace Common.Factories
{
    public interface IRepositoryFactory
    {
        IRepository Create();
    }
}