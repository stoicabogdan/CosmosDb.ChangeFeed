using CosmosDb.ChangeFeed.BootStrapping.Extensions;
using CosmosDb.ChangeFeed.BootStrapping.Helper;
using CosmosDb.ChangeFeed.ChangeModels;
using CosmosDb.ChangeFeed.Handlers;
using CosmosDb.ChangeFeed.ServiceBusEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CosmosDb.ChangeFeed.Configuration
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            var isLocal = context.HostingEnvironment.IsDevelopment();

#if DEBUG
            isLocal = true;
#endif
            ConfigureChangeFeedServices(context, services, isLocal);

            services.AddCosmosClient(context, configuration);

            if(isLocal)
            {
                /* 
                 Create local DB and containers
                 */
            }

            /*
             Configure ServiceBus Topics
            */
        }

        private static void ConfigureChangeFeedServices(HostBuilderContext context, IServiceCollection services, bool isLocal)
        {
            var configuration = context.Configuration;

            services.AddSingleton<IChangeFeedHandler<RequestAcknowledgedEntity>, RequestAcknowledgedHandler>();
            services.AddHostedService<ChangeFeedHostedService>();
            services.Configure<CosmosChangeFeedOptions>(_ => configuration.GetSection("CosmosChangeFeed"));
            services.PostConfigure<CosmosChangeFeedOptions>(changeFeedConfig => changeFeedConfig.InstanceId = Environment.MachineName);
            services.AddSingleton<IMapper<RequestAcknowledgedV1,RequestAcknowledgedEntity>>();
            services.AddSingleton<IServicebusSenderWrapper<RequestAcknowledgedV1>>();
        }
    }
}
