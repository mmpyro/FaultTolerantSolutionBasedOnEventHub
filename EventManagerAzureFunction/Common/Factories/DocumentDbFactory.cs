using Common.Dtos;
using Common.Repositories;
using Microsoft.Extensions.Configuration;

namespace Common.Factories
{
    public class DocumentDbFactory : IRepositoryFactory
    {
        private readonly IConfigurationRoot _configuration;

        public DocumentDbFactory(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public IRepository Create()
        {
            var endpointUrl = _configuration[Constants.DocumentDb.EndpointUrl];
            var primaryKey = _configuration[Constants.DocumentDb.PrimaryKey];
            var collectionId = _configuration[Constants.DocumentDb.CollectionId];
            var dbId = _configuration[Constants.DocumentDb.DatabaseId];
            return new DocumentDbRepository(new DocumentDbConfig(endpointUrl, primaryKey, collectionId, dbId));
        }
    }
}
