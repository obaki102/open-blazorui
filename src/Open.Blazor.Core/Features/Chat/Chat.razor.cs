using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using Open.Blazor.Core.Features.Shared;
using Open.Blazor.Core.Features.Shared.Models;
using Toolbelt.Blazor.SpeechRecognition;

namespace Open.Blazor.Core.Features.Chat;

public partial class Chat : ComponentBase, IDisposable
{
    private ChatService _chatService;
    private OllamaService _ollamaService;
    private IToastService _toastService;
    private IJSRuntime _jsRuntime;
    private SpeechRecognition _speechRecognition;

    private Ollama? _activeOllamaModels;
    private CancellationTokenSource _cancellationTokenSource = default!;
    private string? _chatSystemPrompt;
    private Discourse _discourse = new();
    private double _frequencyPenalty;
    private bool _isChatOngoing = false;
    private bool _isListening = false;
    private bool _isOllamaUp = false;
    private bool _isSpeechAvailable = false;

    //todo support history
    private Kernel _kernel = default!;
    private int _maxTokens = 2000;
    private double _presencePenalty;

    //Speech recognition
    private SpeechRecognitionResult[] _results = [];
    private OllamaModel _selectedModel = default!;

    private IList<string> _stopSequences = default!;

    //chat settings
    private double _temperature = 1;
    private double _topP = 1;
    private string _userMessage = string.Empty;

    public Chat(ChatService chatService,
        OllamaService olamaService,
        IToastService toastService,
        IJSRuntime jsRuntime,
        SpeechRecognition speechRecognition)
    {
        _chatService = chatService;
        _ollamaService = olamaService;
        _toastService = toastService;
        _jsRuntime = jsRuntime;
        _speechRecognition = speechRecognition;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _speechRecognition.Result -= OnSpeechRecognized!;
    }

    protected override async Task OnInitializedAsync()
    {
        _speechRecognition.Lang = "en-US";
        _speechRecognition.InterimResults = false;
        _speechRecognition.Continuous = true;
        _speechRecognition.Result += OnSpeechRecognized;

        var result = await _ollamaService.GetListOfLocalModelsAsync();

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
        _kernel = _chatService.CreateKernel(defaultModel.Name);

        _selectedModel = defaultModel;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isSpeechAvailable = await _speechRecognition.IsAvailableAsync();
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
            _discourse.AddChatMessage(MessageRole.User, _userMessage, _selectedModel.Name);
            _discourse.AddChatMessage(MessageRole.Assistant, string.Empty, _selectedModel.Name);

            _userMessage = string.Empty;

            await StopListening();

            var settings = ChatSettings.New(_temperature, _topP, _presencePenalty, _frequencyPenalty, _maxTokens,
                default, _chatSystemPrompt);

            await _chatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion, settings,
                _cancellationTokenSource.Token);

            _discourse.ChatMessages.Last().IsDoneStreaming = true;
        }
        catch (Exception ex)
        {
            //todo implement logger
            ShowError(ex.Message);
        }
        finally
        {
            ResetCancellationTokenSource();
            _isChatOngoing = false;
        }
    }

    private async Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        await ScrollToBottom();
    }

    private void ResetCancellationTokenSource()
    {
        if (!_cancellationTokenSource.TryReset())
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void ShowError(string errorMessage)
    {
        _toastService.ShowError(errorMessage);
    }


    private void HandleSelectedOptionChanged(OllamaModel selectedModelChanged)
    {
        _selectedModel = selectedModelChanged;
        _kernel = _chatService.CreateKernel(_selectedModel.Name);
    }

    private async Task StopChat()
    {
        await _cancellationTokenSource.CancelAsync();
    }

    private async Task ScrollToBottom()
    {
        await _jsRuntime.InvokeVoidAsync("ScrollToBottom", "chat-window");
        await InvokeAsync(StateHasChanged);
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognitionEventArgs args)
    {
        if (args.Results == null || args.Results.Length <= args.ResultIndex) return;

        var transcript = new StringBuilder(_userMessage);
        foreach (var result in args.Results.Skip(args.ResultIndex))
            if (result.IsFinal)
                transcript.Append(result.Items![0].Transcript);

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
            await _speechRecognition.StartAsync();
            _toastService.ShowSuccess("Listening");
        }
    }

    private async Task StopListening()
    {
        if (!EnsureDeviceIsAvailable())
            return;

        if (_isListening)
        {
            _isListening = false;
            await _speechRecognition.StopAsync();
            _toastService.ShowWarning("Stopped Listening");
        }
    }
}