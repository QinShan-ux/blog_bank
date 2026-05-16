using BlogBank.Core.dto;
using BlogBank.Core.Entities;
using BlogBank.Core.Interfaces;
using BlogBank.Service.Interfaces;

namespace BlogBank.Service.Services;

public class ArticleService(IArticleRepository repo) : IArticleService
{
    public Task<IEnumerable<Article>> GetAllAsync() => repo.GetAllAsync();

    public Task<List<ArticleSummary>> GetIdTitleListAsync() => repo.GetIdTitleListAsync();

    public Task<Article?> GetByIdAsync(long id) => repo.GetByIdAsync(id);

    public Task<Article> CreateAsync(Article article) => repo.CreateAsync(article);

    public Task<List<Article>> CreateBatchAsync(IEnumerable<Article> articles) => repo.CreateBatchAsync(articles);

    public Task<Article?> UpdateAsync(long id, Article article) => repo.UpdateAsync(id, article);

    public Task<bool> DeleteAsync(long id) => repo.DeleteAsync(id);
}
