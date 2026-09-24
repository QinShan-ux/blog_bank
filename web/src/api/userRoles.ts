import { http } from './request'
import type { UserRoleRef } from '@/types/api'

export function getUserRoles(userId: string): Promise<UserRoleRef[]> {
  return http.get<UserRoleRef[]>(`/api/user-roles/user/${userId}`)
}

export function getUserByRole(roleId: number | string): Promise<UserRoleRef[]> {
  return http.get<UserRoleRef[]>(`/api/user-roles/role/${roleId}`)
}

export function assignUserRole(userId: string, roleId: number | string): Promise<void> {
  return http.post('/api/user-roles', { userId, roleId })
}

/** 撤销用户角色（DELETE 带 body） */
export function revokeUserRole(userId: string, roleId: number | string): Promise<void> {
  return http.delete('/api/user-roles', { data: { userId, roleId } })
}
