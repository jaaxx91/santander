using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNewsAPI.SDK.IntegrationTests
{
    internal class NewsApiTests
    {
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";

        private INewsApi _newsApi;
        private Mock<ILogger<NewsApi>> _mockedLogger;

        [SetUp]
        public void Setup()
        {
            _mockedLogger = new Mock<ILogger<NewsApi>>();

            _mockedLogger.Setup(x => x.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

            _newsApi = new NewsApi(new HttpClient { BaseAddress = new Uri(BaseUrl) }, _mockedLogger.Object);
        }

        [Test]
        public async Task Should_ReturnBestStories_Returns()
        {
            // Act
            var bestStories = await _newsApi.GetBestStories();

            // Assert
            Assert.IsNotNull(bestStories);
            Assert.That(bestStories.Count() > 0);

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Exactly(2));
        }

        [Test]
        public async Task Should_ReturnStory_Returns()
        {
            // Arrange
            const int storyId = 8863;

            // Act
            var story = await _newsApi.GetStory(storyId);

            // Assert
            Assert.IsNotNull(story);
            Assert.That(story.Id == storyId);
            Assert.That(!string.IsNullOrWhiteSpace(story.Title));
            Assert.That(!string.IsNullOrWhiteSpace(story.By));
            Assert.That(story.Descendants > 0 && story.Kids.Count() > 0);
            Assert.That(!string.IsNullOrWhiteSpace(story.Url));
            Assert.That(story.Time > 0);
            Assert.That(story.CreatedOn != (DateTime)default);

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Exactly(2));
        }
    }
}