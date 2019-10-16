using Aspenlaub.Net.GitHub.CSharp.DvinCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp {
    public class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
#if DEBUG
                        .UseDvinAndPegh(Constants.DvinSampleAppId, false, args)
#else
                        .UseDvinAndPegh(Constants.DvinSampleAppId, true, args)
#endif
                        .UseStartup<Startup>();
                });
    }
}
