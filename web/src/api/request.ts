import axios, { type AxiosError, type AxiosRequestConfig } from 'axios'
import { ElMessage } from 'element-plus'
import { clearTokens, getAccessToken, getRefreshToken, saveTokens } from '@/utils/auth'
import type { LoginResult } from '@/types/api'

const service = axios.create({
  baseURL: '/',
  timeout: 15000,
})

// 请求拦截器：注入 Bearer Token
service.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

/** 认证相关接口自身的 401 不走刷新逻辑，避免循环刷新 */
function isAuthEndpoint(url?: string): boolean {
  return !!url && url.includes('/api/auth/')
}

/** 正在进行的刷新请求；并发 401 共享同一个 Promise，只刷新一次 */
let refreshing: Promise<string> | null = null

async function doRefresh(): Promise<string> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) throw new Error('缺少 refreshToken')
  // 用独立的 axios（不带拦截器）调用，防止刷新请求本身被拦截
  const { data } = await axios.post<LoginResult>('/api/auth/refresh', { refreshToken })
  saveTokens(data.accessToken, data.refreshToken, data.expiresAt)
  return data.accessToken
}

function redirectToLogin(): void {
  clearTokens()
  if (!location.pathname.startsWith('/login')) {
    location.href = `/login?redirect=${encodeURIComponent(location.pathname + location.search)}`
  }
}

// 响应拦截器：直接返回 data；401 时用 refreshToken 无感刷新并重放原请求
service.interceptors.response.use(
  (res) => res.data,
  async (error: AxiosError<{ message?: string }>) => {
    const { response, config } = error
    const message = response?.data?.message

    if (response?.status === 401 && config && !isAuthEndpoint(config.url)) {
      if (!getRefreshToken()) {
        redirectToLogin()
        return Promise.reject(error)
      }
      try {
        refreshing = refreshing ?? doRefresh()
        const accessToken = await refreshing
        refreshing = null
        config.headers.Authorization = `Bearer ${accessToken}`
        return service.request(config)
      } catch {
        refreshing = null
        redirectToLogin()
        return Promise.reject(error)
      }
    }

    if (message) {
      ElMessage.error(message)
    } else if (response) {
      ElMessage.error(`请求失败（${response.status}）`)
    } else {
      ElMessage.error('网络异常，请确认后端服务已启动（http://localhost:5000）')
    }
    return Promise.reject(error)
  },
)

/** 类型安全的请求方法：拦截器已解包响应体，这里直接以业务类型返回 */
export const http = {
  get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return service.get(url, config) as Promise<T>
  },
  post<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
    return service.post(url, data, config) as Promise<T>
  },
  put<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
    return service.put(url, data, config) as Promise<T>
  },
  delete<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return service.delete(url, config) as Promise<T>
  },
}
