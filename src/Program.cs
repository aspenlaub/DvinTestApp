using Aspenlaub.Net.GitHub.CSharp.Dvin.Extensions;
using Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp;

public class Program {
    public static void Main(string[] args) {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) {
        return Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(builder => {
                   builder.UseDvinAndPeghAsync("DvinTestApp", Constants.DvinSampleAppId).Wait();
                   builder.ConfigureServices(Configurator.ConfigureServices);
                   builder.Configure(Configurator.Configure);
               }
           );
    }
}