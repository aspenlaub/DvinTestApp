using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Entities;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Seoa.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;
using Autofac;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1859

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Test {
    [TestClass]
    public class HomeControllerTest {
        private static IContainer _container;

        private readonly WebApplicationFactory<Program> _Factory = new WebApplicationFactory<Program>();

        [ClassInitialize]
        public static void Initialize(TestContext context) {
            ContainerBuilder builder = new ContainerBuilder().UseDvinAndPegh("DvinTestApp");
            _container = builder.Build();

            var errorsAndInfos = new ErrorsAndInfos();
            IFolder folder = TheFolderThatShouldNotBeNeeded(errorsAndInfos);
            Assert.That.ThereWereNoErrors(errorsAndInfos);
            folder.CreateIfNecessary();
        }

        [ClassCleanup]
        public static void CleanUp() {
            var errorsAndInfos = new ErrorsAndInfos();
            IFolder folder = TheFolderThatShouldNotBeNeeded(errorsAndInfos);
            Assert.That.ThereWereNoErrors(errorsAndInfos);
            Directory.Delete(folder.FullName);
        }

        private static IFolder TheFolderThatShouldNotBeNeeded(IErrorsAndInfos errorsAndInfos) {
            IFolderResolver folderResolver = _container.Resolve<IFolderResolver>();
            return folderResolver.ResolveAsync(@"$(GitHub)\DvinTestApp\src\Aspenlaub.Net.GitHub.CSharp.DvinTestApp\", errorsAndInfos).Result;
        }

        [TestMethod]
        public async Task CanCreateTestClient() {
            DvinApp dvinApp = await GetDvinApp();
            string url = $"http://localhost:{dvinApp.Port}/Home";

            using HttpClient client = _Factory.CreateClient();
            Assert.IsNotNull(client);
            HttpResponseMessage response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Hello World says your dvin app", content, content);
        }

        [TestMethod]
        public async Task CanHandleCrashes() {
            DvinApp dvinApp = await GetDvinApp();
            string url = $"http://localhost:{dvinApp.Port}/Home/Crash";

            foreach (string file in FilesWithDeliberateExceptionLogged(dvinApp)) {
                File.Delete(file);
            }

            using HttpClient client = _Factory.CreateClient();
            Assert.IsNotNull(client);
            HttpResponseMessage response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains("An exception was logged", content);

            IList<string> files = FilesWithDeliberateExceptionLogged(dvinApp);
            Assert.HasCount(1, files);
            foreach (string file in files) {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task CanPublishMyself() {
            DvinApp dvinApp = await GetDvinApp();
            string url = $"http://localhost:{dvinApp.Port}/Publish";
            var fileSystemService = new FileSystemService();

            using HttpClient client = _Factory.CreateClient();
            Assert.IsNotNull(client);
            DateTime timeBeforePublishing = DateTime.Now;
            HttpResponseMessage response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Your dvin app just published itself", content, content);
            DateTime lastPublishedAt = dvinApp.LastPublishedAt(fileSystemService);
            Assert.IsGreaterThan(timeBeforePublishing, lastPublishedAt);
        }

        private static async Task<DvinApp> GetDvinApp() {
            IDvinRepository repository = _container.Resolve<IDvinRepository>();
            var errorsAndInfos = new ErrorsAndInfos();
            DvinApp dvinApp = await repository.LoadAsync(Constants.DvinSampleAppId, errorsAndInfos);
            Assert.That.ThereWereNoErrors(errorsAndInfos);
            Assert.IsNotNull(dvinApp);
            return dvinApp;
        }

        private static IList<string> FilesWithDeliberateExceptionLogged(IDvinApp dvinApp) {
            return Directory.GetFiles(dvinApp.ExceptionLogFolder, "Ex*.txt", SearchOption.TopDirectoryOnly)
                .Where(f => File.ReadAllText(f).Contains("This is a deliberate crash")).ToList();
        }
    }
}
