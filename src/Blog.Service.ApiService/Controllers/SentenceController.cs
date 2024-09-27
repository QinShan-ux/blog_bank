using Blog.Service.Common.Model;
using Blog.Service.Core.Sentence;
using Blog.Service.Model.Dto.Sentence;
using Blog.Service.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Service.ApiService.Controllers;
/// <summary>
/// 每日一句
/// </summary>
[ApiController]
[Route("v1/api/[controller]")]
public class SentenceController : ControllerBase
{
    public readonly ISentenceService _SentenceService;

    public SentenceController(ISentenceService sentenceService)
    {
        _SentenceService = sentenceService;
    }

    /// <summary>
    /// 获取句子
    /// </summary>
    /// <returns></returns>
    [HttpGet("sentence/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(Result<SentenceDto>))]
    public async Task<IActionResult> GetSentence(long id)
    {
        var res = await _SentenceService.GetSentenceAsync(id);
        return Ok(Result<SentenceDto>.Success(res));
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="entity"></param>
    [HttpPost("sentence")]
    [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(Result))]
    public async Task InsertSentence([FromBody] SentenceEntity entity)
    {
        await _SentenceService.InsertAsync(entity);
    }
    /// <summary>
    /// 获取全部的句子
    /// </summary>
    /// <returns></returns>
    [HttpGet("sentences")]
    [ProducesResponseType(StatusCodes.Status200OK,Type = typeof(Result<List<SentenceDto>>))]
    public async Task<IActionResult> GetSentences()
    {
        var res = await _SentenceService.GetSentencesAsync();
        return Ok(Result<List<SentenceDto>>.Success(res));
    }
}