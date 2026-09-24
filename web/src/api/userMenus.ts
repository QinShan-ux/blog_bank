import { http } from './request'
import type { UserMenuRef } from '@/types/api'

export function getUserMenus(userId: string): Promise<UserMenuRef[]> {
  return http.get<UserMenuRef[]>(`/api/user-menus/user/${userId}`)
}

export function assignUserMenu(userId: string, menuId: string | number): Promise<void> {
  return http.post('/api/user-menus', { userId, menuId })
}

export function batchAssignUserMenus(userId: string, menuIds: (string | number)[]): Promise<{ added: number }> {
  return http.post('/api/user-menus/batch', { userId, menuIds })
}

/** 重置用户菜单权限：清除全部旧关联再全量写入 */
export function resetUserMenus(userId: string, menuIds: (string | number)[]): Promise<void> {
  return http.put('/api/user-menus/reset', { userId, menuIds })
}

export function revokeUserMenu(userId: string, menuId: string | number): Promise<void> {
  return http.delete('/api/user-menus', { data: { userId, menuId } })
}
