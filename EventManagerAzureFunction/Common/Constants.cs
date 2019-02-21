namespace Common
{
    public static class Constants
    {
        public const string VehicleId = "vehicleId";
        public const short QualityGate = 100;

        public static class Storage
        {
            public const string ConnectionString = "storageConnectionString";
            public const string PoisonTableName = "PoisonMessages";
        }

        public static class DocumentDb
        {
            public const string DatabaseId = "databaseId";
            public const string CollectionId = "collectionId";
            public const string EndpointUrl = "endpointUrl";
            public const string PrimaryKey = "primaryKey";
        }
    }
}
