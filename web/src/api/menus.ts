import { http } from './request'
import type { MenuItem } from '@/types/api'

/** 菜单扁平列表 */
export function getMenus(): Promise<MenuItem[]> {
  return http.get<MenuItem[]>('/api/menus')
}

/** 菜单树形结构（children 递归） */
export function getMenuTree(): Promise<MenuItem[]> {
  return http.get<MenuItem[]>('/api/menus/tree')
}

export function createMenu(data: Partial<MenuItem>): Promise<MenuItem> {
  return http.post<MenuItem>('/api/menus', data)
}

export function updateMenu(id: string, data: Partial<MenuItem>): Promise<MenuItem> {
  return http.put<MenuItem>(`/api/menus/${id}`, data)
}

/** 有子菜单时后端返回 400（message 已在拦截器统一提示） */
export function deleteMenu(id: string): Promise<void> {
  return http.delete<void>(`/api/menus/${id}`)
}
