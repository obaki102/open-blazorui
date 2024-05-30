namespace Open.Blazor.Ui.Features.Chat;

public class ChatMessage
{
    public ChatMessage(ChatRole role, string content)
    {
        Role = role;
        Content = content;
    }
    public ChatRole Role { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Content { get; set; } = string.Empty;

    public static ChatMessage New(ChatRole role, string content) =>
            new(role, content);
}
