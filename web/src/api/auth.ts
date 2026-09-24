import { http } from './request'
import type { LoginResult } from '@/types/api'

export function login(data: { account: string; password: string }): Promise<LoginResult> {
  return http.post<LoginResult>('/api/auth/login', data)
}

/** 登出（撤销 refreshToken）；body 携带双 token */
export function logout(accessToken: string, refreshToken: string): Promise<void> {
  return http.post<void>('/api/auth/logout', { accessToken, refreshToken })
}
