using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReportingPortal.Services;

namespace ReportingPortal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string? apiBase = builder.Configuration["ApiBaseAddress"];
            if (string.IsNullOrWhiteSpace(apiBase))
            {
                throw new InvalidOperationException("ApiBaseAddress configuration is missing or empty.");
            }

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(apiBase),
            });

            builder.Services.AddTransient<AuthHeaderHandler>();

            builder.Services.AddHttpClient("AuthorizedClient", client =>
            {
                client.BaseAddress = new Uri(apiBase);
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddScoped<EmailVerificationTokenService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ReportReplyService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<NotificationService>();

            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

            builder.Services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
