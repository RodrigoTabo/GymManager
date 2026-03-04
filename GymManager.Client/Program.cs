using GymManager.Client.ApiClients;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace GymManager.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();

        // HttpClient apuntando a tu API
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

        await builder.Build().RunAsync();
    }
}
