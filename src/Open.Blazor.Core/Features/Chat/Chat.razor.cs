﻿using System.Text;
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
    private Ollama? _activeOllamaModels = default!;
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
    private SpeechRecognitionResult[] _results = Array.Empty<SpeechRecognitionResult>();
    private OllamaModel _selectedModel = default!;

    private IList<string> _stopSequences = default!;

    //chat settings
    private double _temperature = 1;
    private double _topP = 1;
    private string _userMessage = string.Empty;


    [Parameter] public bool UseSemanticKernel { get; set; }


    [Inject] private ChatService ChatService { get; set; } = default!;

    [Inject] private OllamaService OllamaService { get; set; } = default!;

    [Inject] public required IToastService ToastService { get; set; }

    [Inject] public required IJSRuntime JsRuntime { get; set; }


    [Inject] public required SpeechRecognition SpeechRecognition { get; set; }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        SpeechRecognition.Result -= OnSpeechRecognized!;
    }

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
        if (UseSemanticKernel) _kernel = ChatService.CreateKernel(defaultModel.Name);

        _selectedModel = defaultModel;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isSpeechAvailable = await SpeechRecognition.IsAvailableAsync();
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

            if (UseSemanticKernel)
                await ChatService.StreamChatMessageContentAsync(_kernel, _discourse, OnStreamCompletion, settings,
                    _cancellationTokenSource.Token);
            else
                await ChatService.StreamChatMessageContentAsync(_discourse, OnStreamCompletion, settings,
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


    //See if this can be optimize further
    private Task OnStreamCompletion(string updatedContent)
    {
        _discourse.ChatMessages.Last().Content += updatedContent;
        return Task.CompletedTask;
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
        ToastService.ShowError(errorMessage);
    }


    private void HandleSelectedOptionChanged(OllamaModel selectedModelChanged)
    {
        _selectedModel = selectedModelChanged;
        if (UseSemanticKernel) _kernel = ChatService.CreateKernel(_selectedModel.Name);
    }

    private async Task StopChat()
    {
        await _cancellationTokenSource.CancelAsync();
    }

    private async Task ScrollToBottom()
    {
        await JsRuntime.InvokeVoidAsync("ScrollToBottom", "chat-window");
        StateHasChanged();
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
            await SpeechRecognition.StartAsync();
            ToastService.ShowSuccess("Listening");
        }
    }

    private async Task StopListening()
    {
        if (!EnsureDeviceIsAvailable())
            return;

        if (_isListening)
        {
            _isListening = false;
            await SpeechRecognition.StopAsync();
            ToastService.ShowWarning("Stopped Listening");
        }
    }
}