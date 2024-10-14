using HackerNewsAPI.SDK.Models;
using Microsoft.Extensions.Logging;

namespace HackerNewsAPI.SDK
{
    public class NewsApi : BaseApiClient, INewsApi
    {
        public NewsApi(HttpClient httpClient, ILogger<NewsApi> logger)
            : base(httpClient, logger)
        {
        }

        public async Task<IEnumerable<int>> GetBestStories(CancellationToken cancellationToken = default)
            => await Get<IEnumerable<int>>("beststories.json", cancellationToken);

        public async Task<Story> GetStory(int id, CancellationToken cancellationToken = default)
            => await Get<Story>($"item/{id}.json", cancellationToken);
    }
}
