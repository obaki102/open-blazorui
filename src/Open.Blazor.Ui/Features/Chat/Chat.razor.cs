using Microsoft.AspNetCore.Components;


namespace Open.Blazor.Ui.Features.Chat;

public partial class Chat : ComponentBase
{

    //todo support history
    private Discourse _discourse = new();
    private string _userMessage = string.Empty;
    private bool _isChatOngoing = false;

    [Inject]
    ChatService ChatService { get; set; } = default!;

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_userMessage)) return;
        _isChatOngoing = true;

        var kernel = ChatService.CreateKernel("llama3:latest");

        _discourse.ChatMessages.Add(ChatMessage.New(ChatRole.User, _userMessage));
        _discourse.ChatMessages.Add(ChatMessage.New(ChatRole.Assistant, string.Empty));
        _userMessage = string.Empty;

        StateHasChanged();
        _discourse = await ChatService.ChatCompletionAsStreamAsync(kernel, _discourse, OnStreamCompletion);
        _isChatOngoing = false;
    }


    //See if this can be opimize further
    private async Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        await ScrollToBottom();
        await InvokeAsync(StateHasChanged);

    }

}
