using Common.Dtos;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace Common.Clients
{
    public class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient _httpClient;

        public HttpClient()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public async Task PostAsync(CircutBreakerEventDto circutBreakerEventDto)
        {
            _httpClient.DefaultRequestHeaders.Add("aeg-sas-key", circutBreakerEventDto.Key);

            var json = JsonConvert.SerializeObject(circutBreakerEventDto.Data);
            var request = new HttpRequestMessage(HttpMethod.Post, circutBreakerEventDto.Topic)
            {
                RequestUri = new Uri(circutBreakerEventDto.Uri),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            await Polly.Policy.Handle<Exception>()
                    .WaitAndRetryAsync(5, (retry) => TimeSpan.FromSeconds(1 * retry))
                    .ExecuteAsync(async () =>
                    {
                        await _httpClient.SendAsync(request);

                    });
        }
    }
}
