import { http } from './request'
import type { EssayItem, EssayPayload } from '@/types/api'

/** 随笔全量列表（按日期倒序） */
export function getEssays(): Promise<EssayItem[]> {
  return http.get<EssayItem[]>('/api/essays')
}

export function getEssay(id: string): Promise<EssayItem> {
  return http.get<EssayItem>(`/api/essays/${id}`)
}

export function createEssay(data: EssayPayload): Promise<EssayItem> {
  return http.post<EssayItem>('/api/essays', data)
}

export function updateEssay(id: string, data: EssayPayload): Promise<EssayItem> {
  return http.put<EssayItem>(`/api/essays/${id}`, data)
}

export function deleteEssay(id: string): Promise<void> {
  return http.delete<void>(`/api/essays/${id}`)
}
