using System;

namespace Common.Dtos
{
    public class DocumentDbConfig
    {
        public Uri EndpointUrl { get; set; }
        public string PrimaryKey { get; set; }
        public string CollectionId { get; set; }
        public string DatabaseId { get; set; }

        public DocumentDbConfig(string endpointUrl, string primaryKey, string collectionId, string databaseId)
        {
            EndpointUrl = new Uri(endpointUrl);
            PrimaryKey = primaryKey;
            CollectionId = collectionId;
            DatabaseId = databaseId;
        }
    }
}
