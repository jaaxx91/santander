using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace HackerNewsAPI.SDK.UnitTests
{
    internal class NewsApiTests
    {
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";

        private INewsApi _newsApi;
        private Mock<ILogger<NewsApi>> _mockedLogger;
        private Mock<HttpMessageHandler> _mockedHttpMessageHandler;

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

            _mockedLogger.Setup(x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            _mockedHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            _newsApi = new NewsApi(
                new HttpClient(_mockedHttpMessageHandler.Object) { BaseAddress = new Uri(BaseUrl) },
                _mockedLogger.Object);
        }

        [Test]
        public async Task Should_ReturnBestStories_WhenRequested_Returns()
        {
            // Arrange
            _mockedHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("[123456, 654321]")
               })
               .Verifiable();

            // Act
            var bestStories = await _newsApi.GetBestStories(default);

            // Assert
            Assert.IsNotNull(bestStories);
            Assert.Greater(bestStories.Count(), 0);

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
        public void Should_Throw_WhenResponseMessageHandlerThrows_Throws()
        {
            // Arrange
            _mockedHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .Throws<HttpRequestException>()
               .Verifiable();

            // Act && Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await _newsApi.GetBestStories(default));

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        [Test]
        public void Should_Throw_WhenStatusCodeNotSuccessful_Throws()
        {
            // Arrange
            _mockedHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.NotFound
               })
               .Verifiable();

            // Act && Assert
            Assert.ThrowsAsync<HttpRequestException>(async () => await _newsApi.GetBestStories(default));

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Exactly(2));

            _mockedLogger.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }
    }
}