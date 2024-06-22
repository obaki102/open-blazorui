using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Open.Blazor.Core.Features.Shared.Models;
using System.Net.Http.Json;
using System.Text;
using Toolbelt.Blazor.SpeechRecognition;
using Ndjson.AsyncStreams.Net.Http;
using Open.Blazor.Core.Features.Chat;
using Open.Blazor.Core.Features.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Http;


namespace Open.Blazor.Ui.WebAssembly.Features.Chat;

public partial class ChatWebAssembly : ComponentBase, IDisposable
{

    //todo support history
    private Discourse _discourse = new();
    private string _userMessage = string.Empty;
    private bool _isChatOngoing = false;
    private bool _isOllamaUp = false;
    private Ollama? _activeOllamaModels = default!;
    private OllamaModel _selectedModel = default!;
    private CancellationTokenSource _cancellationTokenSource = default!;

    //Speech recognition
    private SpeechRecognitionResult[] _results = Array.Empty<SpeechRecognitionResult>();
    private bool _isSpeechAvailable = false;
    private bool _isListening = false;


    [Inject]
    HttpClient HttpClient { get; set; } = default!;

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
        _selectedModel = defaultModel;
        _cancellationTokenSource = new();

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isSpeechAvailable = await SpeechRecognition.IsAvailableAsync();
            StateHasChanged();
        }

    }


    private async Task SendMessage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_userMessage)) return;
            _isChatOngoing = true;
            _discourse.AddChatMessage(ChatRole.User, _userMessage, _selectedModel.Name);
            _discourse.AddChatMessage(ChatRole.Assistant, string.Empty, _selectedModel.Name);

            var promptRequest = new PromptRequest(_selectedModel.Name, _userMessage);
            _userMessage = string.Empty;
            await InvokeAsync(StateHasChanged);
            await StopListening();
         
            var content = JsonContent.Create(promptRequest);
            var request = new HttpRequestMessage
            {
                Content = content,
                Method = HttpMethod.Post,
                RequestUri = new Uri("http://localhost:11434/api/generate", UriKind.Absolute),
                Version = new Version(3, 0),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };
            request.Headers.Add("Origin", "http://localhost");
            request.SetBrowserResponseStreamingEnabled(true);
            request.Headers.Add("Accept", "application/x-ndjson");
            using HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);

            if (response.IsSuccessStatusCode)
            {
                await AddResponseAsync(response, _cancellationTokenSource.Token);

            }

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

    private async Task AddResponseAsync(HttpResponseMessage response, CancellationToken token)
    {
        await foreach (var promptResponse in response.Content.ReadFromNdjsonAsync<PromptResponse>(cancellationToken: _cancellationTokenSource.Token))
        {
            if (promptResponse is null) continue;
            _discourse.ChatMessages.Last().Content += promptResponse.Response;
            await ScrollToBottom();
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
            if (result.IsFinal)
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

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        SpeechRecognition.Result -= OnSpeechRecognized!;
    }
}
