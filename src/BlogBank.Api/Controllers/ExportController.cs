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
        var id = await taskService.AddTask();
        // 雪花 ID 以字符串返回，避免 JS 端 JSON 数字精度丢失
        return Ok(new { id = id.ToString() });
    }

    [HttpGet("task")]
    public async Task<IActionResult> GetTask(string id)
    {
        if (!long.TryParse(id, out var taskId))
            return BadRequest(new { message = "任务 ID 格式无效。" });

        var task = await taskService.GetTask(taskId);
        if (task is null) return NotFound();
        return Ok(ToResponse(task));
    }

    private static object ToResponse(ExportTask t) => new
    {
        id           = t.Id.ToString(),
        status       = (int)t.Status,
        progress     = t.Progress,
        fileUrl      = t.FileUrl,
        errorMessage = t.ErrorMessage,
        operatorId   = t.OperatorId,
        completedAt  = t.CompletedAt
    };
}