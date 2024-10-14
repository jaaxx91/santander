namespace Santander.API.Application.Configuration
{
    public interface IBestStoriesConfiguration
    {
        bool CacheEnabled { get; set; }

        int? CacheTimeInMin { get; set; }

        string CacheKey { get; set; }

        int? MaxDegreeOfParallelism { get; set; }
    }
}
