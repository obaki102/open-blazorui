
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Open.Blazor.Core.Features.Shared;
using Open.Blazor.Core.Features.Shared.Models;


namespace Open.Blazor.Tests.OllamaServiceTests;
public class MockHttpMessageHandler : DelegatingHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsync(request, cancellationToken);
    }
}
public class OllamaServiceTests
{
    private HttpResponseMessage _response;
    private HttpClient _client;

    public OllamaServiceTests()
    {
        var handler = new MockHttpMessageHandler((request, cancellationToken) => Task.FromResult(_response));
        _client = new HttpClient(handler);
    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldReturnOllama_WhenResponseIsSuccessful()
    {
        // Arrange
        await using var stream = new MemoryStream();

        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };

        await using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.AutoFlush = true;

        var ollama = new Ollama();
        await writer.WriteAsync(JsonSerializer.Serialize(ollama));
        stream.Seek(0, SeekOrigin.Begin);

        var service = new OllamaService(_client);

        // Act
        var result = await service.GetListOfLocalModelsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();

    }

    [Fact]
    public async Task GetListOfLocalModels_ShouldReturnFailure_WhenResponseBodyIsNull()
    {
        // Arrange
        await using var stream = new MemoryStream();

        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };

        await using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.AutoFlush = true;

        var ollama = new Ollama();
        await writer.WriteAsync(JsonSerializer.Serialize(ollama));
        stream.Seek(0, SeekOrigin.Begin);

        var service = new OllamaService(_client);

        // Act
        var result = await service.GetListOfLocalModelsAsync();

        // Assert
        result.Error.ErrorType.Should().Be(Error.NullValue.ErrorType);
    }

    [Fact]
    public async Task GetListOfLocalModels_WithCustomBaseUrl_ShouldReturnOllama_WhenResponseIsSuccessful()
    {
        // Arrange
        await using var stream = new MemoryStream();

        _response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StreamContent(stream)
        };

        await using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.AutoFlush = true;

        var ollama = new Ollama();
        await writer.WriteAsync(JsonSerializer.Serialize(ollama));
        stream.Seek(0, SeekOrigin.Begin);

        var service = new OllamaService(_client);
        var customBaseUrl = "http://localhost:12345";

        // Act
        var result = await service.GetListOfLocalModelsAsync(customBaseUrl);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetListOfLocalModelsc_ShouldThrowArgumentNullException_WhenBaseUrlIsNull()
    {
        //Arrange
        var service = new OllamaService(_client);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>  service.GetListOfLocalModelsAsync(null));
    }

    [Fact]
    public async Task GetListOfLocalModelsc_ShouldThrowArgumentNullException_WhenBaseUrlIsEmptyt()
    {
        //Arrange
        var service = new OllamaService(_client);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetListOfLocalModelsAsync(string.Empty));

    }


}
