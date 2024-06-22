using System.Text.Json.Serialization;

namespace Open.Blazor.Ui.WebAssembly.Features.Chat;

public record PromptResponse(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("created_at")]
    DateTimeOffset CreatedAt,
    [property: JsonPropertyName("response")]
    string Response,
    [property: JsonPropertyName("done")] bool Done
);