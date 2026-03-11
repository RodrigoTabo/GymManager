using GymManager.Client.ApiClients;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using GymManager.Web.Security;
using GymManager.Web.ApiClients;
using GymManager.Client.ApiClients.Common;

namespace GymManager.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();

    
        builder.Services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7093/")
        });

        // ApiClient
        builder.Services.AddScoped<PlanApi>();
        builder.Services.AddScoped<SocioApi>();
        builder.Services.AddScoped<AsistenciaApi>();
        builder.Services.AddScoped<MetodoPagoApi>();
        builder.Services.AddScoped<PagoApi>();
        builder.Services.AddScoped<IntentosAccesoApi>();
        builder.Services.AddScoped<GeneralApi>();
        builder.Services.AddScoped<SucursalApi>();

        //Identity
        builder.Services.AddAuthorizationCore();

        builder.Services.AddScoped<TokenStorageService>();
        builder.Services.AddScoped<JwtAuthStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<JwtAuthStateProvider>());

        builder.Services.AddScoped<ApiHttpClientProvider>();

        builder.Services.AddScoped<AuthApi>();


        await builder.Build().RunAsync();
    }
}
