using FluentValidation.TestHelper;
using Santander.API.Application.Queries.GetBestStories;

namespace Santander.API.Application.UnitTests.Queries.GetBestStories
{
    internal class GetBestStoriesQueryValidatorTests
    {
        private GetBestStoriesQueryValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new GetBestStoriesQueryValidator();
        }

        [TestCase(-100)]
        [TestCase(0)]
        public void Should_HaveError_WhenNumberOfStoriesProvidedIsLessOrEqualTo0_Error(int numberOfStories)
        {
            // Arrange
            var model = new GetBestStoriesQuery { NumberOfStories = numberOfStories };

            // Act
            var result = _validator.TestValidate(model);

            // Arrange
            result.ShouldHaveValidationErrorFor(m => m.NumberOfStories);
        }

        [TestCase(100)]
        [TestCase(1)]
        public void Should_NotHaveError_WhenNumberOfStoriesProvidedGreaterThan0_Error(int numberOfStories)
        {
            // Arrange
            var model = new GetBestStoriesQuery { NumberOfStories = numberOfStories };

            // Act
            var result = _validator.TestValidate(model);

            // Arrange
            result.ShouldNotHaveValidationErrorFor(m => m.NumberOfStories);
        }
    }
}
