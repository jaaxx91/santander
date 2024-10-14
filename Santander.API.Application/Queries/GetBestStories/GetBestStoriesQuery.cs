using MediatR;
using Santander.API.Models.Queries.GetBestStories;

namespace Santander.API.Application.Queries.GetBestStories
{
    public class GetBestStoriesQuery : IRequest<GetBestStoriesResponse>
    {
        public int NumberOfStories { get; set; }
    }
}
