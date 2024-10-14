namespace Santander.API.Application.Configuration
{
    public class BestStoriesConfiguration : IBestStoriesConfiguration
    {
        public bool CacheEnabled { get; set; }

        public int? CacheTimeInMin { get; set; }

        public string CacheKey { get; set; }

        public int? MaxDegreeOfParallelism { get; set; }
    }
}
