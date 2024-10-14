using Ardalis.GuardClauses;
using HackerNewsAPI.SDK;
using HackerNewsAPI.SDK.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Santander.API.Application.Configuration;
using Santander.API.Models.Queries.GetBestStories;
using System.Collections.Concurrent;

namespace Santander.API.Application.Queries.GetBestStories
{
    public class GetBestStoriesQueryHandler : IRequestHandler<GetBestStoriesQuery, GetBestStoriesResponse>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly INewsApi _newsApi;
        private readonly IBestStoriesConfiguration _bestStoriesConfiguration;

        private const int DefaultCacheInMin = 15;
        private const int DefaultMaxDegreeOfParallelism = 20;

        public GetBestStoriesQueryHandler(
            INewsApi newsApi,
            IMemoryCache memoryCache,
            IBestStoriesConfiguration bestStoriesConfiguration)
        {
            Guard.Against.Null(newsApi, nameof(newsApi));
            Guard.Against.Null(memoryCache, nameof(memoryCache));
            Guard.Against.Null(bestStoriesConfiguration, nameof(bestStoriesConfiguration));

            _newsApi = newsApi;
            _memoryCache = memoryCache;
            _bestStoriesConfiguration = bestStoriesConfiguration;
        }

        public async Task<GetBestStoriesResponse> Handle(
            GetBestStoriesQuery request,
            CancellationToken cancellationToken = default)
        {
            var stories = await GetStories(request.NumberOfStories, cancellationToken);

            return new GetBestStoriesResponse
            {
                BestStories = stories.Select(s => new BestStory
                {
                    Title = s.Title,
                    PostedBy = s.By,
                    Score = s.Score,
                    Time = s.CreatedOn,
                    Uri = s.Url,
                    CommentCount = s.Descendants
                })
            };
        }

        private async Task<IEnumerable<Story>> GetStories(
            int numberOfStories,
            CancellationToken cancellationToken)
        {
            IEnumerable<Story> stories = null;

            if (_bestStoriesConfiguration.CacheEnabled)
            {
                int cacheTimeInMin = _bestStoriesConfiguration.CacheTimeInMin is not null
                    ? (int)_bestStoriesConfiguration.CacheTimeInMin
                    : DefaultCacheInMin;

                stories = await _memoryCache.GetOrCreateAsync(
                    _bestStoriesConfiguration.CacheKey,
                    async (entry) =>
                    {
                        entry.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTimeInMin);
                        return await GetStoriesFromAPIOrderByScore(cancellationToken);
                    });
            }
            else
            {
                stories = await GetStoriesFromAPIOrderByScore(cancellationToken);
            }

            return stories.Take(numberOfStories);
        }

        private async Task<IEnumerable<Story>> GetStoriesFromAPIOrderByScore(CancellationToken cancellationToken)
        {
            var bestStoriesIds = await _newsApi.GetBestStories(cancellationToken);

            int maxDegreeOfParallelism = _bestStoriesConfiguration.MaxDegreeOfParallelism is not null
                ? (int)_bestStoriesConfiguration.MaxDegreeOfParallelism
                : DefaultMaxDegreeOfParallelism;

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            var bag = new ConcurrentBag<Story>();

            await Parallel.ForEachAsync(bestStoriesIds, options, async (id, ct) =>
            {
                var story = await _newsApi.GetStory(id, ct);

                bag.Add(story);
            });

            return bag.OrderByDescending(s => s.Score).AsEnumerable();
        }
    }
}
