using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HackerNewsAPI.SDK
{
    public abstract class BaseApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BaseApiClient(
            HttpClient httpClient,
            ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        protected async Task<TResponse> Get<TResponse>(
            string uri,
            CancellationToken cancellationToken = default) where TResponse : class
        {
            _logger.LogInformation("GET request to {url}", _httpClient.BaseAddress + uri);

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.GetAsync(uri, cancellationToken);

                _logger.LogInformation("GET requuest to {url} response: {response}", _httpClient.BaseAddress + uri, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET request to {uri} failed with error: {error}", _httpClient.BaseAddress + uri, ex.Message);
                throw;
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContentString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<TResponse>(responseContentString);
            }
            else
            {
                _logger.LogError("GET request to {uri} failed with status code: {statusCode}", _httpClient.BaseAddress + uri, response.StatusCode);
                throw new HttpRequestException($"GET request to {_httpClient.BaseAddress + uri} failed.", null, response.StatusCode);
            }
        }
    }
}
