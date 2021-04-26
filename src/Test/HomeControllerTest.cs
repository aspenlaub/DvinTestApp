using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Entities;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Test {
    [TestClass]
    public class HomeControllerTest {
        private static IContainer vContainer;

        private readonly WebApplicationFactory<Startup> vFactory;

        public HomeControllerTest() {
            vFactory = new WebApplicationFactory<Startup>();
        }

        [ClassInitialize]
        public static void Initialize(TestContext context) {
            var builder = new ContainerBuilder().UseDvinAndPegh(new DummyCsArgumentPrompter());
            vContainer = builder.Build();

            var errorsAndInfos = new ErrorsAndInfos();
            var folder = TheFolderThatShouldNotBeNeeded(errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            folder.CreateIfNecessary();
        }

        [ClassCleanup]
        public static void CleanUp() {
            var errorsAndInfos = new ErrorsAndInfos();
            var folder = TheFolderThatShouldNotBeNeeded(errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Directory.Delete(folder.FullName);
        }

        private static IFolder TheFolderThatShouldNotBeNeeded(IErrorsAndInfos errorsAndInfos) {
            var folderResolver = vContainer.Resolve<IFolderResolver>();
            return folderResolver.ResolveAsync(@"$(GitHub)\DvinTestApp\src\Aspenlaub.Net.GitHub.CSharp.DvinTestApp\", errorsAndInfos).Result;
        }

        [TestMethod]
        public async Task CanCreateTestClient() {
            var dvinApp = await GetDvinApp();
            var url = $"http://localhost:{dvinApp.Port}/Home";

            using var client = vFactory.CreateClient();
            Assert.IsNotNull(client);
            var response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Hello World says your dvin app"), content);
        }

        [TestMethod]
        public async Task CanHandleCrashes() {
            var dvinApp = await GetDvinApp();
            var url = $"http://localhost:{dvinApp.Port}/Home/Crash";

            foreach (var file in FilesWithDeliberateExceptionLogged(dvinApp)) {
                File.Delete(file);
            }

            using var client = vFactory.CreateClient();
            Assert.IsNotNull(client);
            var response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("An exception was logged"));

            var files = FilesWithDeliberateExceptionLogged(dvinApp);
            Assert.AreEqual(1, files.Count);
            foreach (var file in files) {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task CanPublishMyself() {
            var dvinApp = await GetDvinApp();
            var url = $"http://localhost:{dvinApp.Port}/Publish";
            var fileSystemService = new FileSystemService();

            using var client = vFactory.CreateClient();
            Assert.IsNotNull(client);
            var timeBeforePublishing = DateTime.Now;
            var response = await client.GetAsync(url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Your dvin app just published itself"), content);
            var lastPublishedAt = dvinApp.LastPublishedAt(fileSystemService);
            Assert.IsTrue(lastPublishedAt > timeBeforePublishing);
        }

        private async Task<DvinApp> GetDvinApp() {
            var repository = vContainer.Resolve<IDvinRepository>();
            var errorsAndInfos = new ErrorsAndInfos();
            var dvinApp = await repository.LoadAsync(Constants.DvinSampleAppId, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
            Assert.IsNotNull(dvinApp);
            return dvinApp;
        }

        private static IList<string> FilesWithDeliberateExceptionLogged(IDvinApp dvinApp) {
            return Directory.GetFiles(dvinApp.ExceptionLogFolder, "Ex*.txt", SearchOption.TopDirectoryOnly)
                .Where(f => File.ReadAllText(f).Contains("This is a deliberate crash")).ToList();
        }
    }
}
