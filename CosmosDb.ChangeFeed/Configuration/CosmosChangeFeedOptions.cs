namespace CosmosDb.ChangeFeed.Configuration
{
    public class CosmosChangeFeedOptions
    {
        public string RequestsDatabase {  get; set; }

        public string RequestsContainer { get; set; }

        public string RequestsLeaseContainer { get; set; }

        public string ProcessorName { get; set; }

        public string InstanceId { get; set; }
    }
}
