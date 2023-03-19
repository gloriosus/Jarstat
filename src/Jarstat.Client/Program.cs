using Blazored.SessionStorage;
using Jarstat.Application.Abstractions;
using Jarstat.Client;
using Jarstat.Client.Authentication;
using Jarstat.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var clientSettings = builder.Configuration.Get<ClientSettings>();
if (clientSettings is not null )
    builder.Services.AddSingleton(clientSettings);

builder.Services.AddAntDesign();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<NotifyStateService>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
