using Common.Repositories;
using Microsoft.Extensions.Configuration;
using System;

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
            return new DocumentDbRepository(new Uri(endpointUrl), primaryKey);
        }
    }
}
