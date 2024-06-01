using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Open.Blazor.Ui.Features.Shared;
using Open.Blazor.Ui.Features.Shared.Models;

namespace Open.Blazor.Ui.Features.Chat;

public partial class Chat : ComponentBase, IDisposable
{

    //todo support history
    private Kernel _kernel = default!;
    private Discourse _discourse = new();
    private string _userMessage = string.Empty;
    private bool _isChatOngoing = false;
    private bool _isOllamaUp = false;
    private Ollama? _activeOllamaModels = default!;
    private OllamaModel _selectedModel = default!;
    private CancellationTokenSource _cancellationTokenSource = default!;
    [Inject]
    ChatService ChatService { get; set; } = default!;

    [Inject]
    OllamaService OllamaService { get; set; } = default!;

    [Inject]
    public required IMessageService MessageService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var result = await OllamaService.GetListOfLocalModelsAsync();
        if (result.IsSuccess)
        {
            _activeOllamaModels = result.IsSuccess ? result.Value : default!;

            if (_activeOllamaModels is not null && _activeOllamaModels.Models.Count > 0)
            {
                var defaultModel = _activeOllamaModels.Models.First();
                _kernel = ChatService.CreateKernel(defaultModel.Name);
                _cancellationTokenSource = new();
            }
            else
            {
                await MessageService.ShowMessageBarAsync("No models found",
             MessageIntent.Error, @AppSection.MESSAGES_TOP);
            }

        }
        else
        {
            await MessageService.ShowMessageBarAsync("Ollama service is down.",
            MessageIntent.Error, @AppSection.MESSAGES_TOP);
        }

    }


    private async Task SendMessage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_userMessage)) return;

            _isChatOngoing = true;

            _discourse.AddChatMessage(ChatRole.User, _userMessage);
            _discourse.AddChatMessage(ChatRole.Assistant, string.Empty);

            _userMessage = string.Empty;


            await ScrollToBottom();
            await ChatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion, _cancellationTokenSource.Token);

        }
        finally
        {
            if (!_cancellationTokenSource.TryReset())
            {
                //clean-up the old cts and create a new one if the old one was already cancelled
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new();
            }

            _isChatOngoing = false;

        }
    }


    //See if this can be opimize further
    private async Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        await ScrollToBottom();

    }

    private void HandleSelectedOptionChanged(OllamaModel selectedModelChanged)
    {
        _selectedModel = selectedModelChanged;
        _kernel = ChatService.CreateKernel(_selectedModel.Name);

    }

    private async Task StopChat() =>
        await _cancellationTokenSource.CancelAsync();

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }
}
