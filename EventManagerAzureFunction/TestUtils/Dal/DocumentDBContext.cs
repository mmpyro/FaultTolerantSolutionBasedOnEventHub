using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestUtils.Dal
{
    public class DocumentDBContext
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private readonly string _collectionId;

        public DocumentDBContext(Uri endpointUrl, string primaryKey, string databaseId, string collectionId)
        {
            _client = new DocumentClient(endpointUrl, primaryKey);
            _databaseId = databaseId;
            _collectionId = collectionId;
        }

        public T[] Get<T>()
        {
            var documents =_client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
            return documents.ToArray();
        }

        public async Task Reset()
        {
            await _client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId));
        }
    }
}
