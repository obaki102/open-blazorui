
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.AI;
using Open.Blazor.Core.Features.Shared;


namespace Open.Blazor.Core.Features.Chat;

internal sealed class ChatService
{
    private readonly Config _config;

    private readonly IChatClient _client;

    public ChatService(Config config)
    {
        _config = config;
        //test
        _client = new OllamaChatClient(new Uri(_config.OllamaUrl), "mistral:latest");
    }

    public Kernel CreateKernel(string model)
    {
        ArgumentNullException.ThrowIfNull(model);
#pragma warning disable SKEXP0010

        var kernelBuilder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: model,
                endpoint: new Uri(_config.OllamaUrl),
                apiKey: null);

        return kernelBuilder.Build();
    }


    public async Task StreamChatMessageContentAsync(Kernel kernel,
        Discourse discourse,
        Func<string, Task> onStreamCompletion,
        ChatSettings chatSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(discourse);
        ArgumentNullException.ThrowIfNull(onStreamCompletion);
        ArgumentNullException.ThrowIfNull(chatSettings);

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        var executionSettings = chatSettings.ToOpenAIPromptExecutionSettings();
        var history = discourse.ToChatHistory();

        await foreach (var completionResult in chatCompletion.GetStreamingChatMessageContentsAsync(history,
                           executionSettings, cancellationToken: cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await onStreamCompletion.Invoke(completionResult.Content ?? string.Empty);

        }
    }

    public async Task StreamChatMessageContentAsync(Discourse discourse,
       Func<string, Task> onStreamCompletion,
       ChatSettings chatSettings,
       CancellationToken cancellationToken = default)
    {

        ArgumentNullException.ThrowIfNull(discourse);
        ArgumentNullException.ThrowIfNull(onStreamCompletion);
        ArgumentNullException.ThrowIfNull(chatSettings);



        var chatOptions = chatSettings.ToChatOptions();
        var history = discourse.ToChatMessages();
        await foreach (var completionResult in _client.CompleteStreamingAsync(history, chatOptions, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await onStreamCompletion.Invoke(completionResult.Text ?? string.Empty);

        }
    }
}

public static class ChatServiceExensions
{
    public static IServiceCollection AddChatServiceAsScoped(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ChatService>();
        return services;
    }
}

