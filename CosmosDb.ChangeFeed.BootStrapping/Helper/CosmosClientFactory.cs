using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmosDb.ChangeFeed.BootStrapping.Helper
{
    public static class CosmosClientFactory
    {
        public static CosmosClient CreateCosmosClient(bool isDevelopment, IConfiguration configuration)
        {
            return new CosmosClient(configuration.GetConnectionString("CosmosDb"), CosmosClientOptions(isDevelopment));
        }

        private static CosmosClientOptions CosmosClientOptions(bool isDevelopment)
        {
            var options = new CosmosClientOptions()
            {
                ConnectionMode = ConnectionMode.Gateway, // Need to check this vs Direct
                SerializerOptions = new CosmosSerializationOptions
                {
                    IgnoreNullValues = true,
                    /*
                     * NOTE: It is not possible to set an anum as string serializer option (converter)
                     * AVOID Enums and use strings
                     * https://github.com/Azure/azure-cosmos-dotnet-v3/discussions/2064
                     * https://stackoverflow.com/questions/51677955/serialize-and-deserialize-cosmos-db-document-property-as-string
                     */
                }
            };

            /*
             * Needed so that Docker can connect to the Cosmos DB Emulator running locally 
             */
            if (isDevelopment)
                options.HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return new HttpClient(httpMessageHandler);
                };
            return options;
        }
    }
}
