namespace Open.Blazor.Core.Features.Chat;

public class ChatMessage
{
    public Guid Id { get; }
    public ChatMessage(Guid id, ChatRole role, string content, string model)
    {
        Role = role;
        Content = content;
        Id = id;
        Model = model;
    }
    public ChatRole Role { get; }
    public DateTime Date { get; } = DateTime.Now;
    public string Content { get; set; } = string.Empty;
    public bool IsDoneStreaming { get; set; } = false;
    public string Model { get; }

    public static ChatMessage New(ChatRole role, string content, string model) =>
            new(Guid.NewGuid(), role, content, model);
}
