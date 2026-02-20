using System;
using System.Net;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Entities;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Dvin.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Attributes;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Aspenlaub.Net.GitHub.CSharp.DvinTestApp.Controllers;

[DvinExceptionFilter]
public class HomeController(IDvinRepository dvinRepository) : Controller {
    protected readonly IDvinRepository DvinRepository = dvinRepository;
    protected bool HasExceptionFilterAttributeBeenSet;

    public async Task<IActionResult> Index() {
        await SetExceptionFilterAttributeIfNecessaryAsync();

        return Ok("Hello World says your dvin app");
    }

    [HttpGet, Route("/Publish")]
    public async Task<IActionResult> Publish() {
        await SetExceptionFilterAttributeIfNecessaryAsync();

        var errorsAndInfos = new ErrorsAndInfos();
        DvinApp dvinApp = await DvinRepository.LoadAsync(Constants.DvinSampleAppId, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return StatusCode((int) HttpStatusCode.InternalServerError, errorsAndInfos.ErrorsToString());
        }

        var fileSystemService = new FileSystemService();
        dvinApp.Publish(fileSystemService, true, errorsAndInfos);
        return errorsAndInfos.AnyErrors()
            ? StatusCode((int)HttpStatusCode.InternalServerError, errorsAndInfos.ErrorsToString())
            : Ok("Your dvin app just published itself");
    }

    public async Task<IActionResult> Crash() {
        await SetExceptionFilterAttributeIfNecessaryAsync();

        throw new NotImplementedException("This is a deliberate crash");
    }

    private async Task SetExceptionFilterAttributeIfNecessaryAsync() {
        if (HasExceptionFilterAttributeBeenSet) { return; }

        var errorsAndInfos = new ErrorsAndInfos();
        DvinApp dvinApp = await DvinRepository.LoadAsync(Constants.DvinSampleAppId, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            throw new Exception("Dvin sample app not registered");
        }
        DvinExceptionFilterAttribute.SetExceptionLogFolder(new Folder(dvinApp.ExceptionLogFolder));
        HasExceptionFilterAttributeBeenSet = true;
    }
}