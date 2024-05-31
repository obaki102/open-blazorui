
using Open.Blazor.Ui.Features.Shared.Models;
using System.Text.Json;


namespace Open.Blazor.Ui.Features.Shared
{
    internal sealed class OllamaService
    {
        private const string API_TAGS = "api/tags";
        private readonly HttpClient _httpClient;

        public OllamaService(HttpClient httpClient) =>
            _httpClient = httpClient;

        public async Task<Result<Ollama>> GetListOfLocalModelsAsync() =>
            await GetLocalModels(Default.baseUrl);

        public async Task<Result<Ollama>> GetListOfLocalModelsAsync(string baseUrl) =>
             await GetLocalModels(baseUrl);


        private async Task<Result<Ollama>> GetLocalModels(string baseUrl)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(baseUrl);
            try
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
                var response = await _httpClient.GetAsync(API_TAGS);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStreamAsync();
                if (responseBody is null)
                {
                    return Result.Failure<Ollama>(Error.NullValue);
                }

                return JsonSerializer.Deserialize<Ollama>(responseBody)!;
            }
            catch (HttpRequestException ex)
            {
                return Result.Failure<Ollama>(Error.Validation(ex.Message));
            }
        }

      

    }

    public static class OllamaServiceExensions
    {
        public static IServiceCollection AddOllamaServiceAsScoped(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddHttpClient<OllamaService>();
            return services;
        }
    }
}
