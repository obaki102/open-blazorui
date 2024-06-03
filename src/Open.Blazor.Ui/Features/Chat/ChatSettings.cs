
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Open.Blazor.Ui.Features.Chat;

public class ChatSettings
{
    public double Temperature { get; set; } = 1;
    public double TopP { get; set; } = 1;
    public double PresencePenalty { get; set; }
    public double FrequencyPenalty { get; set; }
    public int? MaxTokens { get; set; } = 2000;
    public IList<string>? StopSequences { get; set; }
    public string? ChatSystemPrompt { get; set; }

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

