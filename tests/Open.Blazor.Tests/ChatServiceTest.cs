using Open.Blazor.Ui.Features.Chat;

namespace Open.Blazor.Tests.ChatServiceTests
{
    public class ChatServiceTests
    {
        private const string Model = "llama3:latest";

        [Fact]
        public void Constructor_ShouldInitialize_WhenParametersAreValid()
        {
            var service = new ChatService();
            Assert.NotNull(service);
        }

        [Fact]
        public void CreateKernel_ShouldThrowArgumentNullException_WhenModelIsNull()
        {
            var service = new ChatService();
            Assert.Throws<ArgumentNullException>(() => service.CreateKernel(null));
        }

        [Fact]
        public void CreateKernel_ShouldReturnKernel_WhenModelIsValid()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            Assert.NotNull(kernel);
        }

        [Fact]
        public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenKernelIsNull()
        {
            var service = new ChatService();
            var discourse = new Discourse();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(null, discourse, onStreamCompletion));
        }

        [Fact]
        public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenDiscourseIsNull()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(kernel, null, onStreamCompletion));
        }

        [Fact]
        public async Task ChatCompletionAsStreamAsync_ShouldThrowArgumentNullException_WhenOnStreamCompletionIsNull()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(kernel, discourse, null));
        }

        [Fact]
        public async Task ChatCompletionAsStreamAsync_ShouldReturnDiscourse_WhenCancellationIsRequested()
        {
            // Not an actual unit test, the kernel is called directly so ollama must be up at this point.
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

            using (var cts = new CancellationTokenSource())
            {
                var task = service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion, cts.Token);
                Assert.False(task.IsCompleted);

                cts.Cancel();

                await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            }
        }

        [Fact]
        public async Task ChatCompletionAsStreamAsync_ShouldProcessMessagesSuccessfully()
        {
            // Not an actual unit test, the kernel is called directly so ollama must be up at this point.
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            discourse.AddChatMessage(ChatRole.User, "Hello", Model);
            bool onStreamCompletionCalled = false;

            Func<string, Task> onStreamCompletion = async message =>
            {
                onStreamCompletionCalled = true;
                await Task.CompletedTask;
            };

            await service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion);

            Assert.True(onStreamCompletionCalled);
        }


        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenKernelIsNull()
        {
            var service = new ChatService();
            var discourse = new Discourse();
            var chatSettings = new ChatSettings();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(null, discourse, onStreamCompletion, chatSettings));
        }

        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenDiscourseIsNull()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var chatSettings = new ChatSettings();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(kernel, null, onStreamCompletion, chatSettings));
        }

        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenOnStreamCompletionIsNull()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            var chatSettings = new ChatSettings();
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(kernel, discourse, null, chatSettings));
        }

        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldThrowArgumentNullException_WhenChatSettingsIsNull()
        {
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion, null));
        }

        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldReturnDiscourse_WhenCancellationIsRequested()
        {
            // Not an actual unit test, the kernel is called directly so ollama must be up at this point.
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            var chatSettings = new ChatSettings();
            Func<string, Task> onStreamCompletion = async _ => await Task.CompletedTask;

            using (var cts = new CancellationTokenSource())
            {
                var task = service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion, chatSettings, cts.Token);
                Assert.False(task.IsCompleted);

                cts.Cancel();

                await Assert.ThrowsAsync<TaskCanceledException>(() => task);
            }
        }

        [Fact]
        public async Task StreamChatMessageContentAsync_WithChatSettings_ShouldProcessMessagesSuccessfully()
        {
            // Not an actual unit test, the kernel is called directly so ollama must be up at this point.
            var service = new ChatService();
            var kernel = service.CreateKernel(Model);
            var discourse = new Discourse();
            var chatSettings = new ChatSettings();
            discourse.AddChatMessage(ChatRole.User, "Hello", Model);
            bool onStreamCompletionCalled = false;

            Func<string, Task> onStreamCompletion = async message =>
            {
                onStreamCompletionCalled = true;
                await Task.CompletedTask;
            };

            await service.StreamChatMessageContentAsync(kernel, discourse, onStreamCompletion, chatSettings);

            Assert.True(onStreamCompletionCalled);
        }
    }
}
