using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using Open.Blazor.Core.Features.Shared;
using Open.Blazor.Core.Features.Shared.Models;
using System.Text;
using Toolbelt.Blazor.SpeechRecognition;

namespace Open.Blazor.Core.Features.Chat;

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
    //chat settings
    private double _temperature = 1;
    private double _topP = 1;
    private double _presencePenalty;
    private double _frequencyPenalty;
    private int _maxTokens = 2000;
    private IList<string> _stopSequences = default!;
    private string? _chatSystemPrompt;

    //Speech recognition
    private SpeechRecognitionResult[] _results = Array.Empty<SpeechRecognitionResult>();
    private bool _isSpeechAvailable = false;
    private bool _isListening = false;


    [Inject]
    ChatService ChatService { get; set; } = default!;

    [Inject]
    OllamaService OllamaService { get; set; } = default!;

    [Inject]
    public required IToastService ToastService { get; set; }

    [Inject]
    public required IJSRuntime JsRuntime { get; set; }


    [Inject]
    public required SpeechRecognition SpeechRecognition { get; set; }

    protected override async Task OnInitializedAsync()
    {

        SpeechRecognition.Lang = "en-US";
        SpeechRecognition.InterimResults = false;
        SpeechRecognition.Continuous = true;
        SpeechRecognition.Result += OnSpeechRecognized;

        var result = await OllamaService.GetListOfLocalModelsAsync();

        if (!result.IsSuccess)
        {
            ShowError("Ollama service is down. Please ensure Ollama is up and running.");
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
        if (firstRender)
        {
            this._isSpeechAvailable = await this.SpeechRecognition.IsAvailableAsync();
            StateHasChanged();
        }

        if (_isChatOngoing)
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
            await StopListening();

            ChatSettings settings = ChatSettings.New(_temperature, _topP, _presencePenalty, _frequencyPenalty, _maxTokens, default, _chatSystemPrompt);
            await ChatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion, settings, _cancellationTokenSource.Token);
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


    //See if this can be optimize further
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

    private void OnSpeechRecognized(object? sender, SpeechRecognitionEventArgs args)
    {
        if (args.Results == null || args.Results.Length <= args.ResultIndex)
        {
            return; 
        }

        var transcript = new StringBuilder(_userMessage);
        foreach (var result in args.Results.Skip(args.ResultIndex))
        {
            if(result.IsFinal)
                transcript.Append(result.Items![0].Transcript);
        }

        _userMessage = transcript.ToString();

        StateHasChanged();
    }


    private bool EnsureDeviceIsAvailable()
    {
        if (!_isSpeechAvailable)
        {
            ShowError("Device not available");
            return false;
        }
        return true;
    }

    private async Task StartListening()
    {
        if (!EnsureDeviceIsAvailable())
            return;

        if (!_isListening)
        {
            _isListening = true;
            await SpeechRecognition.StartAsync();
        }

        ToastService.ShowSuccess("Listening");
    }

    private async Task StopListening()
    {
        if (!EnsureDeviceIsAvailable())
            return;

        if (_isListening)
        {
            _isListening = false;
            await SpeechRecognition.StopAsync();
        }

        ToastService.ShowWarning("Stopped Listening");
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        SpeechRecognition.Result -= OnSpeechRecognized!;
    }
}
