namespace CosmosDb.ChangeFeed.BootStrapping.Helper
{
    public interface IMapper<TOutput,TInput>
    {
        TOutput MapToEvent(TInput entity);
    }
}
