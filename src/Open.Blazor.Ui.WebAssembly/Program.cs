using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Open.Blazor.Ui.WebAssembly;
using Open.Blazor.Core.Features.Shared;
using Open.Blazor.Ui.WebAssembly.Features.Chat;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddWsCoreDependencies();
builder.Services.AddChatHttpClient();
await builder.Build().RunAsync();
