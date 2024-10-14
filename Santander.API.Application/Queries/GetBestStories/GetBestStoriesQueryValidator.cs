using FluentValidation;

namespace Santander.API.Application.Queries.GetBestStories
{
    public class GetBestStoriesQueryValidator : AbstractValidator<GetBestStoriesQuery>
    {
        public GetBestStoriesQueryValidator()
        {
            RuleFor(q => q.NumberOfStories)
                .GreaterThan(0)
                .WithMessage("Number of stories needs to be greater than 0");
        }
    }
}
