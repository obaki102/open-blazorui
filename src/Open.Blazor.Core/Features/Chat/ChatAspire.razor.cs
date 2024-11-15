using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel;
using Open.Blazor.Core.Features.Shared;
using Open.Blazor.Core.Features.Shared.Models;
using Toolbelt.Blazor.SpeechRecognition;

namespace Open.Blazor.Core.Features.Chat;

public partial class ChatAspire : ComponentBase, IDisposable
{
    private readonly ChatService _chatService;
    private readonly IToastService _toastService;
    private readonly IJSRuntime _jsRuntime;
    private readonly SpeechRecognition _speechRecognition;
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _isChatOngoing = false;
    private Discourse _discourse = new();
    private string _model;

    //Speech recognition
    private bool _isListening = false;
    private bool _isSpeechAvailable = false;
    private SpeechRecognitionResult[] _results = [];
    private IList<string> _stopSequences = default!;

    private string _userMessage = string.Empty;

    public ChatAspire(ChatService chatService,
        IToastService toastService,
        IJSRuntime jsRuntime,
        SpeechRecognition speechRecognition)
    {
        _chatService = chatService;
        _toastService = toastService;
        _jsRuntime = jsRuntime;
        _speechRecognition = speechRecognition;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _speechRecognition.Result -= OnSpeechRecognized!;
    }

    protected override void OnInitialized()
    {
        _speechRecognition.Lang = "en-US";
        _speechRecognition.InterimResults = false;
        _speechRecognition.Continuous = true;
        _speechRecognition.Result += OnSpeechRecognized;
        _model = _chatService.GetCurrentModel;
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
            _discourse.AddChatMessage(MessageRole.User, _userMessage, _chatService.GetCurrentModel);
            _discourse.AddChatMessage(MessageRole.Assistant, string.Empty, _model);

            _userMessage = string.Empty;

            await StopListening();

            await _chatService.StreamChatMessageContentAsync(_discourse, OnStreamCompletion,
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