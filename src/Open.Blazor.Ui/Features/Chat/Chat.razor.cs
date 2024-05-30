using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;


namespace Open.Blazor.Ui.Features.Chat;

public partial class Chat : ComponentBase
{

    //todo support history
    private Kernel _kernel = default!; 
    private Discourse _discourse = new();
    private string _userMessage = string.Empty;
    private bool _isChatOngoing = false;

    [Inject]
    ChatService ChatService { get; set; } = default!;

    protected override void OnInitialized()
    {
        _kernel = ChatService.CreateKernel("llama3:latest");
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_userMessage)) return;
        _isChatOngoing = true;

        _discourse.ChatMessages.Add(ChatMessage.New(ChatRole.User, _userMessage));
        _discourse.ChatMessages.Add(ChatMessage.New(ChatRole.Assistant, string.Empty));
        _userMessage = string.Empty;

        StateHasChanged();
        _discourse = await ChatService.ChatCompletionAsStreamAsync(_kernel, _discourse, OnStreamCompletion);
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
