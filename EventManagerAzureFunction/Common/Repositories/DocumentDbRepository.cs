using Common.Dtos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Common.Constants;

namespace Common.Repositories
{
    public class DocumentDbRepository : IRepository
    {
        private DocumentClient _client;

        public DocumentDbRepository(Uri endpointUrl, string primaryKey)
        {
            _client = new DocumentClient(endpointUrl, primaryKey);
        }

        public async Task AddAsync(VehicleSnapshot vehicleSnapshot)
        {
            await _client.CreateDatabaseIfNotExistsAsync(new Database
            {
                Id = DocumentDb.DatabaseId
            });

            await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DocumentDb.DatabaseId), new DocumentCollection
            {
                Id = DocumentDb.CollectionId
            });
            var documentUri = UriFactory.CreateDocumentCollectionUri(DocumentDb.DatabaseId, DocumentDb.CollectionId);
            await _client.UpsertDocumentAsync(documentUri, vehicleSnapshot);
        }

        public VehicleSnapshot Get(string id)
        {
            try
            {
                var documentUri = UriFactory.CreateDocumentCollectionUri(DocumentDb.DatabaseId, DocumentDb.CollectionId);
                return _client.CreateDocumentQuery<VehicleSnapshot>(documentUri).AsEnumerable().FirstOrDefault(d => d.Id == id);
            }
            catch(AggregateException)
            {
                return null;
            }
        }
    }
}
