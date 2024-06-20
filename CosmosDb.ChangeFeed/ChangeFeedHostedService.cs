using CosmosDb.ChangeFeed.ChangeModels;
using CosmosDb.ChangeFeed.Configuration;
using CosmosDb.ChangeFeed.Handlers;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmosDb.ChangeFeed
{
    public class ChangeFeedHostedService : IHostedService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IChangeFeedHandler<RequestAcknowledgedEntity> _changeFeedHandler;
        private readonly CosmosChangeFeedOptions _cosmosChangeFeedOptions;
        private readonly ILogger<ChangeFeedHostedService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private ChangeFeedProcessor _changeFeedProcessor;
        private ChangeFeedProcessor _changeFeedEstimator;

        public ChangeFeedHostedService(
            CosmosClient cosmosClient,
            IChangeFeedHandler<RequestAcknowledgedEntity> changeFeedHandler,
            IOptions<CosmosChangeFeedOptions> changeFeedOptions,
            ILogger<ChangeFeedHostedService> logger,
            TelemetryClient telemetryClient)
        {
            _cosmosClient = cosmosClient;
            _changeFeedHandler = changeFeedHandler;
            _cosmosChangeFeedOptions = changeFeedOptions.Value;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{GetType().Name} starting changefeed");
            await StartChangeFeedProcessorAndEstimatorAsync(cancellationToken);
            _logger.LogInformation($"{GetType().Name} started changefeed");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{GetType().Name} stopping changefeed");
            await StopChangeFeedProcessorAndEstimatorAsync(cancellationToken);
            _logger.LogInformation($"{GetType().Name} stopped changefeed");
        }

        private async Task StartChangeFeedProcessorAndEstimatorAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestsContainer = _cosmosClient.GetContainer(_cosmosChangeFeedOptions.RequestsDatabase, _cosmosChangeFeedOptions.RequestsContainer);
            var requestsLeaseContainer = _cosmosClient.GetContainer(_cosmosChangeFeedOptions.RequestsDatabase, _cosmosChangeFeedOptions.RequestsLeaseContainer);

            var processorName = _cosmosChangeFeedOptions.ProcessorName;

            _changeFeedProcessor = requestsContainer
                .GetChangeFeedProcessorBuilder<RequestAcknowledgedEntity>($"{processorName}_", HandleChangesAsync)
                .WithInstanceName($"{processorName}_{_cosmosChangeFeedOptions.InstanceId}")
                .WithMaxItems(10)
                .WithPollInterval(TimeSpan.FromMilliseconds(200))
                .WithLeaseContainer(requestsLeaseContainer)
                .WithStartTime(DateTime.UtcNow.AddDays(-1))
                .Build();

            await _changeFeedProcessor.StartAsync();

            _changeFeedEstimator = requestsContainer
                .GetChangeFeedEstimatorBuilder($"{processorName}_", HandleEstimationAsync, TimeSpan.FromMilliseconds(30000))
                .WithLeaseContainer(requestsLeaseContainer)
                .Build();

            await _changeFeedEstimator.StartAsync();
        }

        private async Task StopChangeFeedProcessorAndEstimatorAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _changeFeedProcessor.StopAsync();
            await _changeFeedEstimator.StopAsync();
        }

        private async Task HandleChangesAsync(IReadOnlyCollection<RequestAcknowledgedEntity> changes, CancellationToken cancellationToken)
        {
            // Use changes.Count to track all changes that the changefeed picks in telemetry
            _telemetryClient.GetMetric("RequestAcknowledgedChanges").TrackValue(changes.Count);

            await _changeFeedHandler.HandleAsync(changes, cancellationToken);
        }

        private async Task HandleEstimationAsync(long estimation, CancellationToken cancellationToken)
        {
            // Use estimation to track remaing changes 
            _telemetryClient.GetMetric("RequestAcknowledgedRemaining").TrackValue(estimation);

            await Task.Delay(0, cancellationToken);
        }
    }
}
