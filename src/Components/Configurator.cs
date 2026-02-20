using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Components;

public static class Configurator {
    public static void ConfigureServices(IServiceCollection services) {
        services.AddControllersWithViews();
        services.UseDvinAndPegh("DvinTestApp");
    }

    public static void Configure(IApplicationBuilder app) {
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });
    }
}