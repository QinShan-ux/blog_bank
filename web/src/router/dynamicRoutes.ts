import type { RouteComponent, RouteRecordRaw } from 'vue-router'
import type { MenuItem } from '@/types/api'

/** admin 目录下所有页面组件，菜单的 component 字符串在这里解析 */
const modules = import.meta.glob('../views/admin/**/*.vue')

function menu(partial: Partial<MenuItem> & Pick<MenuItem, 'id' | 'type' | 'title'>): MenuItem {
  return {
    pid: '0',
    name: '',
    path: '',
    component: '',
    redirect: '',
    permission: '',
    icon: 'ele-Menu',
    isIframe: false,
    outLink: '',
    isHide: false,
    isKeepAlive: false,
    isAffix: false,
    orderNo: 100,
    status: 1,
    remark: '',
    ...partial,
  }
}

/**
 * 前端内置的后台菜单（含路由 name 与组件 defineOptions name 的对应关系）。
 * 仅当数据库 /api/menus/tree 为空（或用户没有任何菜单权限记录）时作为兜底使用，
 * 一旦后端配置了菜单数据，则完全以后端为准。
 */
export const DEFAULT_MENU_TREE: MenuItem[] = [
  menu({
    id: '901', type: 2, name: 'AdminDashboard', path: 'dashboard', component: 'dashboard/Index',
    title: '仪表盘', icon: 'ele-Odometer', orderNo: 1, isKeepAlive: true, isAffix: true,
  }),
  menu({
    id: '902', type: 1, title: '内容管理', icon: 'ele-Notebook', orderNo: 2,
    children: [
      menu({ id: '921', pid: '902', type: 2, name: 'AdminArticles', path: 'articles', component: 'articles/Index', title: '文章管理', icon: 'ele-Document', orderNo: 1, isKeepAlive: true }),
      menu({ id: '922', pid: '902', type: 2, name: 'AdminEssays', path: 'essays', component: 'essays/Index', title: '随笔管理', icon: 'ele-EditPen', orderNo: 2, isKeepAlive: true }),
    ],
  }),
  menu({
    id: '903', type: 1, title: '权限管理', icon: 'ele-Lock', orderNo: 3,
    children: [
      menu({ id: '931', pid: '903', type: 2, name: 'AdminUsers', path: 'users', component: 'users/Index', title: '用户管理', icon: 'ele-User', orderNo: 1, isKeepAlive: true }),
      menu({ id: '932', pid: '903', type: 2, name: 'AdminRoles', path: 'roles', component: 'roles/Index', title: '角色管理', icon: 'ele-Avatar', orderNo: 2, isKeepAlive: true }),
      menu({ id: '933', pid: '903', type: 2, name: 'AdminMenus', path: 'menus', component: 'menus/Index', title: '菜单管理', icon: 'ele-Menu', orderNo: 3, isKeepAlive: true }),
    ],
  }),
  menu({
    id: '904', type: 1, title: '系统工具', icon: 'ele-SetUp', orderNo: 4,
    children: [
      menu({ id: '941', pid: '904', type: 2, name: 'AdminAuditLogs', path: 'audit-logs', component: 'auditLogs/Index', title: '审计日志', icon: 'ele-List', orderNo: 1, isKeepAlive: true }),
      menu({ id: '942', pid: '904', type: 2, name: 'AdminExport', path: 'export', component: 'export/Index', title: '导出中心', icon: 'ele-Download', orderNo: 2 }),
    ],
  }),
  menu({
    id: '905', type: 1, title: '工作', icon: 'ele-Briefcase', orderNo: 5,
    children: [
      menu({ id: '951', pid: '905', type: 2, name: 'AdminWorkBugs', path: 'work-bugs', component: 'workBugs/Index', title: 'Bug 记录', icon: 'ele-Monitor', orderNo: 1, isKeepAlive: true }),
    ],
  }),
]

export function normalizePath(path: string | null | undefined): string {
  return (path ?? '').replace(/^\/+|\/+$/g, '')
}

/** 菜单节点相对 /admin 的路由路径，prefix 为父级链路 */
export function menuFullPath(node: MenuItem, prefix = ''): string {
  return `${prefix}/${normalizePath(node.path)}`.replace(/\/{2,}/g, '/')
}

/** 只保留用户拥有的菜单分支（按钮必须被直接分配才保留） */
export function filterTreeByOwned(nodes: MenuItem[], ownedIds: Set<string>): MenuItem[] {
  const result: MenuItem[] = []
  for (const node of nodes) {
    if (node.type === 3) {
      if (ownedIds.has(node.id)) result.push({ ...node, children: [] })
      continue
    }
    const children = filterTreeByOwned(node.children ?? [], ownedIds)
    if (ownedIds.has(node.id) || children.length > 0) {
      result.push({ ...node, children })
    }
  }
  return result
}

/** 收集树中所有按钮（type=3）的权限标识 */
export function collectPermissions(tree: MenuItem[]): string[] {
  const list: string[] = []
  const walk = (nodes: MenuItem[]) => {
    for (const node of nodes) {
      if (node.type === 3 && node.permission) list.push(node.permission)
      walk(node.children ?? [])
    }
  }
  walk(tree)
  return list
}

/** 按目标路由路径查找菜单链路（用于面包屑），如 ["权限管理", "用户管理"] 对应节点 */
export function findMenuChain(tree: MenuItem[], target: string, prefix = ''): MenuItem[] {
  for (const node of tree) {
    const full = menuFullPath(node, prefix)
    if (node.type === 2 && `/admin/${normalizePath(full)}` === target) return [node]
    const sub = findMenuChain(node.children ?? [], target, full)
    if (sub.length > 0) return [node, ...sub]
  }
  return []
}

/** 解析 component 字符串到 views 目录下的真实组件 */
function resolveComponent(component: string): () => Promise<RouteComponent> {
  const candidates = [
    `../views/${component}.vue`,
    `../views/${component}/Index.vue`,
    `../views/admin/${component}.vue`,
    `../views/admin/${component}/Index.vue`,
  ]
  for (const key of candidates) {
    const loader = modules[key]
    if (loader) return loader as () => Promise<RouteComponent>
  }
  console.warn(`[router] 未找到菜单组件 "${component}"，已回退到占位页`)
  return () => import('@/views/error/404.vue')
}

/**
 * 菜单树 → /admin 下的扁平子路由列表。
 * 目录（type=1）只参与路径拼接与侧边栏分组，不生成嵌套 router-view。
 */
export function buildRoutes(tree: MenuItem[], prefix = ''): RouteRecordRaw[] {
  const routes: RouteRecordRaw[] = []
  for (const node of tree) {
    if (node.status === 0) continue
    if (node.type === 1) {
      routes.push(...buildRoutes(node.children ?? [], menuFullPath(node, prefix)))
      continue
    }
    if (node.type !== 2 || !normalizePath(node.path)) continue
    const full = menuFullPath(node, prefix)
    routes.push({
      path: normalizePath(full),
      name: node.name || `menu-${node.id}`,
      component: resolveComponent(node.component),
      meta: {
        title: node.title,
        icon: node.icon,
        isHide: node.isHide,
        isKeepAlive: node.isKeepAlive,
        isAffix: node.isAffix,
        permission: node.permission,
      },
    })
  }
  return routes
}
