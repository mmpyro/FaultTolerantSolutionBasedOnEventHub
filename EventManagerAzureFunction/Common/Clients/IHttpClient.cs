using System.Threading.Tasks;
using Common.Dtos;

namespace Common.Clients
{
    public interface IHttpClient
    {
        Task PostAsync(CircutBreakerEventDto circutBreakerEventDto);
    }
}