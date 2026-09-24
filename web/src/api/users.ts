import { http } from './request'
import type { UserItem, UserPayload } from '@/types/api'

export function getUsers(): Promise<UserItem[]> {
  return http.get<UserItem[]>('/api/users')
}

export function createUser(data: UserPayload): Promise<UserItem> {
  return http.post<UserItem>('/api/users', data)
}

export function updateUser(id: string, data: UserPayload): Promise<UserItem> {
  return http.put<UserItem>(`/api/users/${id}`, data)
}

export function deleteUser(id: string): Promise<void> {
  return http.delete<void>(`/api/users/${id}`)
}
