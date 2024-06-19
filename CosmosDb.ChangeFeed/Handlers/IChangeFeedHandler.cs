namespace CosmosDb.ChangeFeed.Handlers
{
    public interface IChangeFeedHandler<in T>
    {
        Task HandleAsync(IReadOnlyCollection<T> changes, CancellationToken cancellationToken);
    }
}
