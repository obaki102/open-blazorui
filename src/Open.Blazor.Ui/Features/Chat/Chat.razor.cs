using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
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
    private ChatSettings _chatSettings = new();
    [Inject]
    ChatService ChatService { get; set; } = default!;

    [Inject]
    OllamaService OllamaService { get; set; } = default!;

    [Inject]
    public required IToastService ToastService { get; set; }

    [Inject]
    public required IJSRuntime JsRuntime { get; set; }
 
    protected override async Task OnInitializedAsync()
    {
        var result = await OllamaService.GetListOfLocalModelsAsync();

        if (!result.IsSuccess)
        {
            ShowError("Ollama service is down.");
            return;
        }

        _activeOllamaModels = result.Value ?? default!;

        if (_activeOllamaModels is null || _activeOllamaModels.Models.Count == 0)
        {
            ShowError("No models found");
            return;
        }

        var defaultModel = _activeOllamaModels.Models.First();
        _kernel = ChatService.CreateKernel(defaultModel.Name);
        _selectedModel = defaultModel;
      _cancellationTokenSource = new();

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_isChatOngoing)
            await ScrollToBottom();
    }
      


    private async Task SendMessage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_userMessage)) return;
            _isChatOngoing = true;
            _discourse.AddChatMessage(ChatRole.User, _userMessage, _selectedModel.Name);
            _discourse.AddChatMessage(ChatRole.Assistant, string.Empty, _selectedModel.Name);
            _userMessage = string.Empty;
            await ChatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion, _chatSettings, _cancellationTokenSource.Token);
            _discourse.ChatMessages.Last().IsDoneStreaming = true;
        }
        catch (Exception ex)
        {
            //todo implement logger
            ShowError(ex.Message);
        }
        finally
        {
            if (!_cancellationTokenSource.TryReset())
            {
                //if cancelled clean-up the old cts and create a new one 
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new();
            }

            _isChatOngoing = false;

        }
    }


    //See if this can be opimize further
    private Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        return Task.CompletedTask;
    }

    private void ShowError(string errorMessage) =>
         ToastService.ShowError(errorMessage);


    private void HandleSelectedOptionChanged(OllamaModel selectedModelChanged)
    {
        _selectedModel = selectedModelChanged;
        _kernel = ChatService.CreateKernel(_selectedModel.Name);
    }

    private async Task StopChat() =>
        await _cancellationTokenSource.CancelAsync();

    private async Task ScrollToBottom()
    {
        await JsRuntime.InvokeVoidAsync("ScrollToBottom", "chat-window");
        StateHasChanged();
    }

    public void Dispose()
    {
        if (_cancellationTokenSource is not null)
            _cancellationTokenSource.Dispose();
    }
}
