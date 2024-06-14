using Microsoft.Extensions.Logging;
using Open.Blazor.Core.Features.Shared;
using System.Text.Json;
using System.Text;

namespace Open.Blazor.Core.Features.Chat;

public class CustomHttpMessageHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string[] urls = { "api.openai.com", "openai.azure.com" };

        if (request.RequestUri != null && urls.Contains(request.RequestUri.Host))
        {
            request.Headers.Add("Content-Type", "application/json");
            request.RequestUri = new Uri($"{Default.baseUrl}{request.RequestUri.PathAndQuery}");
        }
        //var requestBody = new
        //{
        //    messages = new[]
        //           {
        //                new { content = "Hello", role = "user" }
        //            },
        //    max_tokens = 2000,
        //    temperature = 1,
        //    top_p = 1,
        //    n = 1,
        //    presence_penalty = 0,
        //    frequency_penalty = 0,
        //    stream = true,
        //    model = "mistral:latest"
        //};

        // Log request details
        //  Console.WriteLine("Sending request to {Url}", request.RequestUri);
        //   request.Content = new StringContent("{\"messages\":[{\"content\":\"Hello\",\"role\":\"user\"}],\"max_tokens\":2000,\"temperature\":1,\"top_p\":1,\"n\":1,\"presence_penalty\":0,\"frequency_penalty\":0,\"stream\":true,\"model\":\"mistral:latest\"}", Encoding.UTF8, "application/json");


        string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync() : string.Empty;
        // Send the request and get the response
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        // Read and log the response body
        string responseBody = await response.Content.ReadAsStringAsync();

        // Log response details
        //if (response.IsSuccessStatusCode)
        //{
        //    Console.WriteLine("Received successful response from {Url} with status code {StatusCode}. Response body: {ResponseBody}", request.RequestUri, response.StatusCode, responseBody);
        //}
        //else
        //{
        //    Console.WriteLine("Received error response from {Url} with status code {StatusCode}. Response body: {ResponseBody}", request.RequestUri, response.StatusCode, responseBody);
        //}

        return response;
    }
}

