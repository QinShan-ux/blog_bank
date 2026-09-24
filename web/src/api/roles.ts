import { http } from './request'
import type { RoleItem, RolePayload } from '@/types/api'

export function getRoles(): Promise<RoleItem[]> {
  return http.get<RoleItem[]>('/api/roles')
}

export function createRole(data: RolePayload): Promise<RoleItem> {
  return http.post<RoleItem>('/api/roles', data)
}

export function updateRole(id: string, data: RolePayload): Promise<RoleItem> {
  return http.put<RoleItem>(`/api/roles/${id}`, data)
}

export function deleteRole(id: string): Promise<void> {
  return http.delete<void>(`/api/roles/${id}`)
}
