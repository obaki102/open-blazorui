using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Open.Blazor.Core.Features.Chat;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OllamaSharp;

namespace Open.Blazor.Core.Features.Shared;

public static class DiExtensions
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(serviceProvider =>
        {
            if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out var ollamaBaseUrl)) return new Config(ollamaBaseUrl);

            return new Config(Default.baseUrl);
        });
        services.AddSingleton<IChatClient>(static serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var ollamaConnectionString = config.GetConnectionString("ollama-phi3-5");
            var connectionBuilder = new DbConnectionStringBuilder {
                ConnectionString = ollamaConnectionString
            };
            
            var endpoint = connectionBuilder["Endpoint"].ToString() ?? string.Empty;
            var model = connectionBuilder["Model"].ToString() ?? string.Empty;
            Console.WriteLine($"model: {model}");
            IChatClient chatClient = new OllamaApiClient(new Uri(endpoint),model); 
            return chatClient;
        });

        services.AddFluentUIComponents();
        services.AddChatServiceAsScoped();
        services.AddOllamaServiceAsScoped();
        services.AddSpeechRecognition();
        return services;
    }

    public static IServiceCollection AddWsCoreDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton(serviceProvider =>
        {
            if (TryGetEnvironmentVariable("OLLAMA_BASE_URL", out var ollamaBaseUrl)) return new Config(ollamaBaseUrl);

            return new Config(Default.baseUrl);
        });

        services.AddFluentUIComponents();
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