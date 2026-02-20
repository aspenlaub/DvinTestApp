using System;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Entities;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Components;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Extensions;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp;

public class Program {
    public static void Main(string[] args) {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) {
        IContainer container = new ContainerBuilder().UseDvinAndPegh("DvinTestApp").Build();
        IDvinRepository repository = container.Resolve<IDvinRepository>();
        var errorsAndInfos = new ErrorsAndInfos();
        DvinApp dvinApp = repository.LoadAsync(Constants.DvinSampleAppId, errorsAndInfos).Result;
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception(errorsAndInfos.ErrorsToString());
        }
        string url = $"http://localhost:{dvinApp.Port}";
        return Host.CreateDefaultBuilder(args)
           .ConfigureWebHostDefaults(builder => {
                   builder.ConfigureServices(Configurator.ConfigureServices);
                   builder.Configure(Configurator.Configure);
                   builder.ConfigureUrl(url);
               }
           );
    }
}