using HackerNewsAPI.SDK.Models;

namespace HackerNewsAPI.SDK
{
    public interface INewsApi
    {
        Task<IEnumerable<int>> GetBestStories(CancellationToken cancellationToken = default);

        Task<Story> GetStory(int id, CancellationToken cancellationToken = default);
    }
}
