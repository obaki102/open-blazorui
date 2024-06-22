
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Open.Blazor.Core.Features.Shared;


namespace Open.Blazor.Core.Features.Chat;

internal sealed class ChatService
{
    private readonly Config _config;

    public ChatService(Config config) =>
        _config = config;

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

