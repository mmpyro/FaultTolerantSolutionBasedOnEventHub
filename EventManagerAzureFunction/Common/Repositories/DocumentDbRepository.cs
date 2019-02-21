using Common.Dtos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class DocumentDbRepository : IRepository
    {
        private readonly DocumentClient _client;
        private readonly DocumentDbConfig _config;

        public DocumentDbRepository(DocumentDbConfig config)
        {
            var policy = new ConnectionPolicy
            {
                EnableEndpointDiscovery = false
            };
            _client = new DocumentClient(config.EndpointUrl, config.PrimaryKey, policy);
            _config = config;
        }

        public async Task AddAsync(VehicleSnapshot vehicleSnapshot)
        {
            await _client.CreateDatabaseIfNotExistsAsync(new Database
            {
                Id = _config.DatabaseId
            });

            await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_config.DatabaseId), new DocumentCollection
            {
                Id = _config.CollectionId
            });
            var documentUri = UriFactory.CreateDocumentCollectionUri(_config.DatabaseId, _config.CollectionId);
            await _client.UpsertDocumentAsync(documentUri, vehicleSnapshot);
        }

        public VehicleSnapshot Get(string id)
        {
            try
            {
                var documentUri = UriFactory.CreateDocumentCollectionUri(_config.DatabaseId, _config.CollectionId);
                return _client.CreateDocumentQuery<VehicleSnapshot>(documentUri).AsEnumerable().FirstOrDefault(d => d.Id == id);
            }
            catch(AggregateException)
            {
                return null;
            }
        }
    }
}
