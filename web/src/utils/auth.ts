/** Token 的 localStorage 存取与 JWT 解码工具。 */

const ACCESS_KEY = 'bb:access-token'
const REFRESH_KEY = 'bb:refresh-token'
const EXPIRES_KEY = 'bb:expires-at'

export function getAccessToken(): string {
  return localStorage.getItem(ACCESS_KEY) ?? ''
}

export function getRefreshToken(): string {
  return localStorage.getItem(REFRESH_KEY) ?? ''
}

export function getExpiresAt(): string {
  return localStorage.getItem(EXPIRES_KEY) ?? ''
}

export function saveTokens(accessToken: string, refreshToken: string, expiresAt: string): void {
  localStorage.setItem(ACCESS_KEY, accessToken)
  localStorage.setItem(REFRESH_KEY, refreshToken)
  localStorage.setItem(EXPIRES_KEY, expiresAt)
}

export function clearTokens(): void {
  localStorage.removeItem(ACCESS_KEY)
  localStorage.removeItem(REFRESH_KEY)
  localStorage.removeItem(EXPIRES_KEY)
}

/**
 * 解码 JWT payload（仅取 claims，不做签名校验）。
 * 后端 TokenService 写入的 claims：UserId / Nickname / unique_name(账号) / nameid(用户ID) 等。
 */
export function decodeJwtPayload(token: string): Record<string, string> {
  try {
    const payload = token.split('.')[1]
    if (!payload) return {}
    return JSON.parse(decodeBase64Url(payload))
  } catch {
    return {}
  }
}

function decodeBase64Url(input: string): string {
  const base64 = input.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4)
  // atob 输出为 latin1，先转 UTF-8 百分号编码再解码，保证中文昵称不乱码
  return decodeURIComponent(
    atob(padded)
      .split('')
      .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
      .join(''),
  )
}
