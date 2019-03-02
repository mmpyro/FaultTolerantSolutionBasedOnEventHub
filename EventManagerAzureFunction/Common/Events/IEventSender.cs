using System.Threading.Tasks;

namespace Common.Events
{
    public interface IEventSender
    {
        Task SendEvent(string functionName);
    }
}