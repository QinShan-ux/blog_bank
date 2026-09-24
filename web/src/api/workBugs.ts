import { http } from './request'
import type { WorkBugItem, WorkBugPayload, WorkBugQuery } from '@/types/api'

/** 工作 Bug 列表（按发生日期倒序），支持状态/严重程度/项目/关键字筛选 */
export function getWorkBugs(params?: WorkBugQuery): Promise<WorkBugItem[]> {
  return http.get<WorkBugItem[]>('/api/workbugs', { params })
}

export function getWorkBug(id: string): Promise<WorkBugItem> {
  return http.get<WorkBugItem>(`/api/workbugs/${id}`)
}

export function createWorkBug(data: WorkBugPayload): Promise<WorkBugItem> {
  return http.post<WorkBugItem>('/api/workbugs', data)
}

export function updateWorkBug(id: string, data: WorkBugPayload): Promise<WorkBugItem> {
  return http.put<WorkBugItem>(`/api/workbugs/${id}`, data)
}

export function deleteWorkBug(id: string): Promise<void> {
  return http.delete<void>(`/api/workbugs/${id}`)
}
