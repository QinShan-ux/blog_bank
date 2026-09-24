import { defineStore } from 'pinia'
import {
  clearTokens,
  getAccessToken,
  getExpiresAt,
  getRefreshToken,
  saveTokens,
} from '@/utils/auth'

export const useTokenStore = defineStore('token', {
  state: () => ({
    accessToken: getAccessToken(),
    refreshToken: getRefreshToken(),
    expiresAt: getExpiresAt(),
  }),
  getters: {
    isLogged: (state) => !!state.accessToken,
  },
  actions: {
    save(accessToken: string, refreshToken: string, expiresAt: string) {
      this.accessToken = accessToken
      this.refreshToken = refreshToken
      this.expiresAt = expiresAt
      saveTokens(accessToken, refreshToken, expiresAt)
    },
    clear() {
      this.accessToken = ''
      this.refreshToken = ''
      this.expiresAt = ''
      clearTokens()
    },
  },
})
