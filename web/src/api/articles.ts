import { http } from './request'
import type { ArticleItem, ArticlePayload } from '@/types/api'

/**
 * 分页获取文章列表。
 * 注意：后端不返回 total，前端以「返回条数 == size」判断可能还有下一页。
 */
export function getArticles(page: number, size: number): Promise<ArticleItem[]> {
  return http.get<ArticleItem[]>('/api/articles', { params: { page, size } })
}

export function getArticleTitles(): Promise<{ id: string; title: string }[]> {
  return http.get('/api/articles/titles')
}

export function getArticle(id: string): Promise<ArticleItem> {
  return http.get<ArticleItem>(`/api/articles/${id}`)
}

export function createArticle(data: ArticlePayload): Promise<ArticleItem> {
  return http.post<ArticleItem>('/api/articles', data)
}

export function updateArticle(id: string, data: ArticlePayload): Promise<ArticleItem> {
  return http.put<ArticleItem>(`/api/articles/${id}`, data)
}

export function deleteArticle(id: string): Promise<void> {
  return http.delete<void>(`/api/articles/${id}`)
}
