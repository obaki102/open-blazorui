using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Open.Blazor.Ui.WebAssembly;
using Open.Blazor.Core.Features.Chat;
using Open.Blazor.Core.Features.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddFluentUIComponents();
builder.Services.AddChatServiceAsScoped();
builder.Services.AddOllamaServiceAsScoped();




await builder.Build().RunAsync();
