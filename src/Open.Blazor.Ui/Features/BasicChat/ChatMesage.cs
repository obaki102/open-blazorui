namespace Open.Blazor.Ui.Features.BasicChat;

public class ChatMessage
{
    public ChatRole Role { get; set; } 
    public string Content { get; set; } = string.Empty;
}
