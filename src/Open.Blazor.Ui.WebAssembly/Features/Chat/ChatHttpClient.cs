using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Ndjson.AsyncStreams.Net.Http;


namespace Open.Blazor.Ui.WebAssembly.Features.Chat;

public class ChatHttpClient
{
    private readonly HttpClient _httpClient;
    public ChatHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task StreamPostAsync(string url,
    PromptRequest content, Func<string, Task> onStreamCompletion,
    CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Origin", "http://localhost");
        request.SetBrowserResponseStreamingEnabled(true);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-ndjson"));

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await StreamResponseAsync(response, onStreamCompletion, cancellationToken);
    }

    private async Task StreamResponseAsync(HttpResponseMessage response, Func<string, Task> onStreamCompletion, CancellationToken token)
    {
        await foreach (var promptResponse in response.Content.ReadFromNdjsonAsync<PromptResponse>(cancellationToken: token))
        {
            if (promptResponse is null) continue;
                await onStreamCompletion.Invoke(promptResponse.Response ?? string.Empty);
        }
    }
}


public static class ChatHttpClientExtensions
{
    public static IServiceCollection AddChatHttpClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHttpClient<ChatHttpClient>();
        return services;
    }
}

