import { http } from './request'
import type { ExportTaskItem } from '@/types/api'

/** 发起文章导出任务，返回新任务的雪花 ID（字符串） */
export function startArticleExport(): Promise<{ id: string }> {
  return http.get<{ id: string }>('/api/export/article')
}

export function getExportTask(id: string): Promise<ExportTaskItem> {
  return http.get<ExportTaskItem>('/api/export/task', { params: { id } })
}

/** 导出文件等静态资源的访问源（dev 为后端直连地址，prod 为同源） */
export const API_ORIGIN = import.meta.env.VITE_API_ORIGIN ?? ''

/** 拼接后端静态文件下载地址（fileUrl 形如 export_1.xlsx） */
export function fileUrlToDownloadUrl(fileUrl: string): string {
  if (/^https?:\/\//.test(fileUrl)) return fileUrl
  return `${API_ORIGIN}/${fileUrl.replace(/^\//, '')}`
}
