using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Open.Blazor.Core.Features.Chat;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Open.Blazor.Core.Features.Shared
{
    public static class DiExtensions
    {
        public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton(serviceProvider =>
            {
                if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out string ollamaBaseUrl))
                {
                    return new Config(ollamaBaseUrl);

                }

                return new Config(Default.baseUrl);
            });

            services.AddFluentUIComponents();
            services.AddChatServiceAsScoped();
            services.AddOllamaServiceAsScoped();
            services.AddSpeechRecognition();
            return services;
        }

        private static bool TryGetEnvironmentVariable(string variableName, out string value)
        {
            value = Environment.GetEnvironmentVariable(variableName) ?? string.Empty;
            return !string.IsNullOrEmpty(value);
        }
    }
}
