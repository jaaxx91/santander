using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Santander.API.Application.Queries.GetBestStories;

namespace Santander.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NewsController(IMediator mediator)
        {
            Guard.Against.Null(mediator, nameof(mediator));

            _mediator = mediator;
        }

        [HttpGet("best-stories/{numberOfStories}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBestStories(
            [FromRoute] int numberOfStories,
            CancellationToken cancellationToken = default)
        {
            var query = new GetBestStoriesQuery
            {
                NumberOfStories = numberOfStories
            };

            var response = await _mediator.Send(query, cancellationToken);

            if (response is null)
            {
                return NoContent();
            }

            return Ok(response);
        }
    }
}
