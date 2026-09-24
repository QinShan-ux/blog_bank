import { defineStore } from 'pinia'
import { getMenuTree } from '@/api/menus'
import { getUserMenus } from '@/api/userMenus'
import { useUserStore } from './user'
import { collectPermissions, DEFAULT_MENU_TREE, filterTreeByOwned, menuFullPath, normalizePath } from '@/router/dynamicRoutes'
import type { MenuItem, UserMenuRef } from '@/types/api'

export const useMenuStore = defineStore('menu', {
  state: () => ({
    menuTree: [] as MenuItem[],
    /** 动态路由是否已注册（刷新页面后由路由守卫重建） */
    registered: false,
  }),
  getters: {
    /** 需要 keep-alive 缓存的组件名（与路由 name / defineOptions name 一致） */
    cachedNames(state): string[] {
      const names: string[] = []
      const walk = (nodes: MenuItem[]) => {
        for (const node of nodes) {
          if (node.type === 2 && node.status === 1 && node.isKeepAlive && node.name) {
            names.push(node.name)
          }
          walk(node.children ?? [])
        }
      }
      walk(state.menuTree)
      return names
    },
    /** 侧边栏第一个可见菜单路径，用作 /admin 的跳转目标 */
    firstMenuPath(state): string {
      let result = ''
      const walk = (nodes: MenuItem[], prefix: string) => {
        if (result) return
        for (const node of nodes) {
          if (result) return
          if (node.status === 0 || node.isHide) continue
          const full = menuFullPath(node, prefix)
          if (node.type === 2) {
            result = `/admin/${normalizePath(full)}`
            return
          }
          walk(node.children ?? [], full)
        }
      }
      walk(state.menuTree, '')
      return result || '/admin/dashboard'
    },
  },
  actions: {
    /**
     * 拉取菜单树 + 当前用户菜单权限，计算生效菜单与按钮权限集合。
     * 数据库未配置任何菜单（或用户无菜单记录）时回退到前端内置菜单，保证控制台开箱可用。
     */
    async load() {
      const userStore = useUserStore()
      if (!userStore.userId) userStore.loadFromToken()

      let tree: MenuItem[] = []
      let owned: UserMenuRef[] = []
      try {
        tree = await getMenuTree()
      } catch {
        tree = []
      }
      try {
        owned = userStore.userId ? await getUserMenus(userStore.userId) : []
      } catch {
        owned = []
      }

      const ownedIds = new Set(owned.map((item) => String(item.menuId)))
      const effective = filterTreeByOwned(tree, ownedIds)
      this.menuTree = effective.length > 0 ? effective : DEFAULT_MENU_TREE
      userStore.setPermissions(collectPermissions(this.menuTree))
      this.registered = true
    },
    /** 退出登录时清空，下次进入 /admin 由路由守卫重新加载 */
    reset() {
      this.menuTree = []
      this.registered = false
    },
  },
})
