import { defineStore } from 'pinia'
import { decodeJwtPayload, getAccessToken } from '@/utils/auth'

export const useUserStore = defineStore('user', {
  state: () => ({
    /** 雪花 ID 字符串，来自 JWT 的 UserId claim */
    userId: '',
    account: '',
    nickname: '',
    /** 按钮级权限标识集合（来自 type=3 的菜单 permission 字段） */
    permissions: [] as string[],
  }),
  actions: {
    /** 登录后从 accessToken 解析当前用户信息（后端登录接口不返回用户资料） */
    loadFromToken() {
      const token = getAccessToken()
      const payload = token ? decodeJwtPayload(token) : {}
      this.userId = payload['UserId'] ?? payload['nameid'] ?? ''
      this.account = payload['unique_name'] ?? payload['name'] ?? ''
      this.nickname = payload['Nickname'] ?? payload['nickname'] ?? this.account
    },
    setPermissions(list: string[]) {
      this.permissions = list
    },
    /**
     * 判断是否拥有某按钮权限。
     * 系统未配置任何按钮权限（列表为空）时视为不限制，避免把所有按钮都隐藏。
     */
    has(permission: string): boolean {
      if (!permission) return true
      if (this.permissions.length === 0) return true
      return this.permissions.includes(permission)
    },
  },
})
