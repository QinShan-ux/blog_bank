import { http } from './request'
import type { AuditLogItem, AuditLogQuery, PagedResult } from '@/types/api'

/** 剔除空字符串/undefined 的查询参数，避免后端把空串当作过滤条件 */
function cleanParams(query: AuditLogQuery): Record<string, string | number> {
  const params: Record<string, string | number> = { page: query.page, pageSize: query.pageSize }
  for (const key of ['userId', 'action', 'tableName', 'startTime', 'endTime'] as const) {
    const value = query[key]
    if (value) params[key] = value
  }
  return params
}

export function getAuditLogs(query: AuditLogQuery): Promise<PagedResult<AuditLogItem>> {
  return http.get<PagedResult<AuditLogItem>>('/api/audit-logs', { params: cleanParams(query) })
}

export function getAuditLog(id: string): Promise<AuditLogItem> {
  return http.get<AuditLogItem>(`/api/audit-logs/${id}`)
}

export function deleteAuditLog(id: string): Promise<void> {
  return http.delete<void>(`/api/audit-logs/${id}`)
}
