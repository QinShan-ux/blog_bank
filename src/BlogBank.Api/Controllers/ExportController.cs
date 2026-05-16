using BlogBank.Core.Entities;
using BlogBank.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/export")]
public class ExportController(IExportTaskService taskService): ControllerBase
{
    [HttpGet("article")]
    public async Task<IActionResult> Export()
    {
        var res = await taskService.AddTask();
        return Ok(res);
    }

    [HttpGet("task")]
    public async Task<IActionResult> GetTask(long id)
    {
        var res = await taskService.GetTask(id);
        return Ok(res);
    }
}