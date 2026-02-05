using Aspenlaub.Net.GitHub.CSharp.Dvin.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp;

public class Program {
    public static void Main(string[] args) {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder
#if DEBUG
                    .UseDvinAndPeghAsync("DvinTestApp", Constants.DvinSampleAppId).Result
#else
                    .UseDvinAndPeghAsync("DvinTestApp", Constants.DvinSampleAppId).Result
#endif
                    .UseStartup<Startup>();
            });
}