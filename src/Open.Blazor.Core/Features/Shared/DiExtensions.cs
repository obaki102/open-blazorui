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
            services.AddFluentUIComponents();
            services.AddChatServiceAsScoped();
            services.AddOllamaServiceAsScoped();
            services.AddSpeechRecognition();
            return services;
        }
    }
}
