namespace CosmosDb.ChangeFeed.BootStrapping.Helper
{
    public interface IServicebusSenderWrapper<T>
    {
        Task SendMessageAsync(T message);
    }
}
