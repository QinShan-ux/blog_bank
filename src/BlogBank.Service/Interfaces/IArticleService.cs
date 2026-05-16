using BlogBank.Core.dto;
using BlogBank.Core.Entities;

namespace BlogBank.Service.Interfaces;

public interface IArticleService
{
    Task<IEnumerable<Article>> GetAllAsync();
    Task<List<ArticleSummary>> GetIdTitleListAsync();
    Task<Article?> GetByIdAsync(long id);
    Task<Article> CreateAsync(Article article);
    Task<List<Article>> CreateBatchAsync(IEnumerable<Article> articles);
    Task<Article?> UpdateAsync(long id, Article article);
    Task<bool> DeleteAsync(long id);
}
