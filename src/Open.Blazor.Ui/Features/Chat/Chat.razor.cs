using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Open.Blazor.Ui.Features.Shared;
using Open.Blazor.Ui.Features.Shared.Models;


namespace Open.Blazor.Ui.Features.Chat;

public partial class Chat : ComponentBase
{

    //todo support history
    private Kernel _kernel = default!;
    private Discourse _discourse = new();
    private string _userMessage = string.Empty;
    private bool _isChatOngoing = false;
    private bool _isOllamaUp = false;
    private Ollama? _activeOllamaModels = default!;
    private OllamaModel _selectedModel = default!;

    [Inject]
    ChatService ChatService { get; set; } = default!;

    [Inject]
    OllamaService OllamaService { get; set; } = default!;

    [Inject]
    public required IMessageService MessageService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var result = await OllamaService.GetListOfLocalModelsAsync();
        _isOllamaUp = result.IsSuccess;
        if (_isOllamaUp)
        {
            _activeOllamaModels = result.IsSuccess ? result.Value : default!;

            if (_activeOllamaModels is not null && _activeOllamaModels.Models.Count > 0)
            {
                var defaultModel = _activeOllamaModels.Models.First();
                _kernel = ChatService.CreateKernel(defaultModel.Name);
            }
            else
            {
                await MessageService.ShowMessageBarAsync("No models found",
             MessageIntent.Error, @AppSection.MESSAGES_TOP);
            }

        }

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!_isOllamaUp)
            {
                await MessageService.ShowMessageBarAsync("Ollama service is down.",
              MessageIntent.Error, @AppSection.MESSAGES_TOP);
            }
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_userMessage)) return;

        _isChatOngoing = true;

        _discourse.AddChatMessage(ChatRole.User, _userMessage);
        _discourse.AddChatMessage(ChatRole.Assistant, string.Empty);
        _userMessage = string.Empty;

        await ChatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion);

        _isChatOngoing = false;
    }


    //See if this can be opimize further
    private async Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        await ScrollToBottom();
        StateHasChanged();

    }

    private void HandleSelectedOptionChanged(OllamaModel selectedModelChanged)
    {
        _selectedModel = selectedModelChanged;
        _kernel = ChatService.CreateKernel(_selectedModel.Name);

    }

}
