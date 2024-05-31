
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Open.Blazor.Ui.Features.Shared;

namespace Open.Blazor.Ui.Features.Chat;

internal sealed class ChatService
{
    public Kernel CreateKernel(string model)
    {
        ArgumentNullException.ThrowIfNull(model);
#pragma warning disable SKEXP0010
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                model,
                new Uri(Default.baseUrl),
                apiKey: null)
            .Build();
        return kernel;
    }



    public async Task StreamChatMessageContentAsync(Kernel kernel,
        Discourse discourse,
        Func<string, Task> onStreamCompletion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(discourse);
        ArgumentNullException.ThrowIfNull(onStreamCompletion);

        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        //todo make it configurable
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 2000,
            Temperature = 0.1,
        };

        var history = discourse.ToChatHistory();

        await foreach (var completionResult in chatCompletion.GetStreamingChatMessageContentsAsync(history,
                           executionSettings, cancellationToken: cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return ;
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

