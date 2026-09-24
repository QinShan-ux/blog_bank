import type { App, Directive, DirectiveBinding } from 'vue'
import { useUserStore } from '@/stores/user'

/**
 * 按钮级权限指令：v-permission="'sys:user:add'"
 * 权限集合来自后端菜单树中 type=3（按钮）节点的 permission 字段；
 * 未配置任何按钮权限时视为不限制（见 user store 的 has）。
 */
const permission: Directive<HTMLElement, string> = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string>) {
    const userStore = useUserStore()
    if (!userStore.has(binding.value)) {
      el.parentNode?.removeChild(el)
    }
  },
}

export function setupPermission(app: App): void {
  app.directive('permission', permission)
}
