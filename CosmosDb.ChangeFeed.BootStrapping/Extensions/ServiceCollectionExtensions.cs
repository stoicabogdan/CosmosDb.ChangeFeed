using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmosDb.ChangeFeed.BootStrapping.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosClient(this IServiceCollection services, HostBuilderContext context, IConfiguration configuration)
        {
            return services.AddSingleton(_ => CosmosClientFactory.CreateCosmosClient(context.HostingEnvironment.IsDevelopment(), configuration));
        }
    }
}
