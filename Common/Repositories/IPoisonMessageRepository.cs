using System.Threading.Tasks;
using Common.Wrappers;

namespace Common.Repositories
{
    public interface IPoisonMessageRepository
    {
        Task Save(EventDataWrapper eventDataWrapper);
    }
}