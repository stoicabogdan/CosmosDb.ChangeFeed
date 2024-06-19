using CosmosDb.ChangeFeed.BootStrapping.Helper;
using CosmosDb.ChangeFeed.ChangeModels;
using CosmosDb.ChangeFeed.ServiceBusEvents;

namespace CosmosDb.ChangeFeed.Handlers
{
    public class RequestAcknowledgedHandler : IChangeFeedHandler<RequestAcknowledgedEntity>
    {
        private readonly IServicebusSenderWrapper<RequestAcknowledgedV1> _servicebusSender;
        private readonly IMapper<RequestAcknowledgedV1, RequestAcknowledgedEntity> _mapper;

        public RequestAcknowledgedHandler(
            IServicebusSenderWrapper<RequestAcknowledgedV1> servicebusSender, 
            IMapper<RequestAcknowledgedV1, RequestAcknowledgedEntity> mapper)
        {
            _servicebusSender = servicebusSender;
            _mapper = mapper;
        }

        public async Task HandleAsync(IReadOnlyCollection<RequestAcknowledgedEntity> changes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tasks = changes.Select(ProcessAsync).ToList();
            await Task.WhenAll(tasks);
        }

        private async Task ProcessAsync(RequestAcknowledgedEntity change)
        {
            /*
             * Add telemetry code to track changes
             * You can use entity data to restore telemetry traces
             * */
            try
            {
                var requestAcknowledged = _mapper.MapToEvent(change);
                await _servicebusSender.SendMessageAsync(requestAcknowledged);
            }
            catch (Exception e)
            {
                // Log Exceptions
                throw;
            }
        }
    }
}
