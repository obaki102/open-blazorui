

using Microsoft.SemanticKernel.ChatCompletion;
namespace Open.Blazor.Ui.Features.Chat;

public class Discourse
{
    public Guid? Id { get; set; }
    public string Model { get; set; } = default!;
    public List<ChatMessage> ChatMessages { get;  set; } = [];
    public void AddChatMessage(ChatRole role, string content) =>
        ChatMessages.Add(ChatMessage.New(role,content));
    
}

public static class DiscourseExtensions
{
    public static ChatHistory ToChatHistory(this Discourse discourse)
    {
        var chatHistory = new ChatHistory();
        foreach (var chat in discourse.ChatMessages.Where(c => !string.IsNullOrEmpty(c.Content.Trim())))
        {
            var role = chat.Role == ChatRole.System
                    ? AuthorRole.System
                    : chat.Role == ChatRole.User
                        ? AuthorRole.User
                        : AuthorRole.Assistant;

            chatHistory.AddMessage(role, chat.Content);
        }

        return chatHistory;
    }

    public static Discourse ToDiscourse(this ChatHistory chatHistory)
    {
        var discourse = new Discourse();
        foreach (var chat in chatHistory)
        {
            var role = chat.Role == AuthorRole.System
                    ? ChatRole.System
                    : chat.Role == AuthorRole.User
                        ? ChatRole.User
                        : ChatRole.Assistant;

            discourse.AddChatMessage(role, chat.Content ?? string.Empty);
        }

        return discourse;
    }
}
