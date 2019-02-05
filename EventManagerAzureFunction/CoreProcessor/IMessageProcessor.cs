using Common.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreProcessor
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(IEnumerable<EventDataWrapper> messages);
    }
}