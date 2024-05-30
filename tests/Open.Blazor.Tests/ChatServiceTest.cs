
using Open.Blazor.Ui.Features.Chat;
using Microsoft.SemanticKernel;

public class ChatServiceTests
{

    [Fact]
    public void CreateKernel_ShouldCreateKernelWithValidModel()
    {
        // Arrange
        var chatService = new ChatService();
        string model = "test-model";

        // Act
        var kernel = chatService.CreateKernel(model);

        // Assert
        Assert.NotNull(kernel);
    }

    [Fact]
    public void CreateKernel_ShouldThrowArgumentNullException_WhenModelIsNull()
    {
        // Arrange
        var chatService = new ChatService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => chatService.CreateKernel(null));
    }


    [Fact]
    public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenKernelIsNull()
    {
        // Arrange
        var chatService = new ChatService();
        var discourse = new Discourse();
        Func<string, Task> onStreamCompletion = content => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => chatService.ChatCompletionAsStreamAsync(null, discourse, onStreamCompletion));
    }

    [Fact]
    public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenDiscourseIsNull()
    {
        // Arrange
        var chatService = new ChatService();
        var kernel = chatService.CreateKernel("test");
        Func<string, Task> onStreamCompletion = content => Task.CompletedTask;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => chatService.ChatCompletionAsStreamAsync(kernel, null, onStreamCompletion));
    }

    [Fact]
    public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenOnStreamCompletionIsNull()
    {
        // Arrange
        var chatService = new ChatService();
        var kernel = chatService.CreateKernel("test");
        var discourse = new Discourse();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => chatService.ChatCompletionAsStreamAsync(kernel, discourse, null));
    }
}
