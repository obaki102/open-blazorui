
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Open.Blazor.Ui.Features.Chat;

public struct ChatSettings
{
    public ChatSettings(
        double temperature,
        double topP,
        double presencePenalty,
        double frequencyPenalty,
        int? maxTokens,
        IList<string>? stopSequences,
        string? chatSystemPrompt)
    {
        Temperature = temperature;
        TopP = topP;
        PresencePenalty = presencePenalty;
        FrequencyPenalty = frequencyPenalty;
        MaxTokens = maxTokens;
        StopSequences = stopSequences;
        ChatSystemPrompt = chatSystemPrompt;
    }

    public double Temperature { get; set; }
    public double TopP { get; set; }
    public double PresencePenalty { get; set; }
    public double FrequencyPenalty { get; set; }
    public int? MaxTokens { get; set; }
    public IList<string>? StopSequences { get; set; }
    public string? ChatSystemPrompt { get; set; }

    public static ChatSettings New(
        double temperature,
        double topP,
        double presencePenalty,
        double frequencyPenalty,
        int? maxTokens,
        IList<string>? stopSequences,
        string? chatSystemPrompt)
    {
        return new ChatSettings(temperature, topP, presencePenalty, frequencyPenalty, maxTokens, stopSequences, chatSystemPrompt);
    }
}

public static class ChatSettingsExtensions
{

    public static OpenAIPromptExecutionSettings ToOpenAIPromptExecutionSettings(this ChatSettings chatSettings)
    {
        ArgumentNullException.ThrowIfNull(chatSettings);

        return new OpenAIPromptExecutionSettings
        {
            Temperature = chatSettings.Temperature,
            TopP = chatSettings.TopP,
            PresencePenalty = chatSettings.PresencePenalty,
            FrequencyPenalty = chatSettings.FrequencyPenalty,
            MaxTokens = chatSettings.MaxTokens,
            StopSequences = chatSettings.StopSequences,
            ChatSystemPrompt = chatSettings.ChatSystemPrompt

        };
    }
}

