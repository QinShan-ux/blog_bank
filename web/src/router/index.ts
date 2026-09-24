import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import AdminLayout from '@/layout/AdminLayout.vue'
import FrontLayout from '@/layout/FrontLayout.vue'
import { buildRoutes } from './dynamicRoutes'
import { useMenuStore } from '@/stores/menu'
import { useTokenStore } from '@/stores/token'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    icon?: string
    isHide?: boolean
    isKeepAlive?: boolean
    isAffix?: boolean
    permission?: string
  }
}

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/login/Index.vue'),
    meta: { title: '登录' },
  },
  {
    path: '/',
    component: FrontLayout,
    children: [
      { path: '', name: 'home', component: () => import('@/views/front/home/Index.vue'), meta: { title: '首页' } },
      { path: 'article/:id', name: 'article-detail', component: () => import('@/views/front/article/Detail.vue'), meta: { title: '文章详情' } },
      { path: 'essays', name: 'front-essays', component: () => import('@/views/front/essays/Index.vue'), meta: { title: '随笔' } },
    ],
  },
  {
    // 后台壳；子路由在路由守卫中根据后端菜单树动态注册。进入后台默认打开仪表板
    path: '/admin',
    name: 'admin',
    component: AdminLayout,
    redirect: '/admin/dashboard',
    children: [],
  },
  {
    // 404 兜底。Vue Router 按路径优先级匹配，动态路由注册后仍优先于通配符命中
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/error/404.vue'),
    meta: { title: '页面不存在' },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const tokenStore = useTokenStore()
  const menuStore = useMenuStore()

  if (to.path === '/login') {
    return tokenStore.isLogged ? { path: '/admin' } : true
  }

  // 前台页面公开访问，仅后台需要登录
  if (!to.path.startsWith('/admin')) return true

  if (!tokenStore.isLogged) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  if (!menuStore.registered) {
    try {
      await menuStore.load()
    } catch {
      tokenStore.clear()
      menuStore.reset()
      return { path: '/login', query: { redirect: to.fullPath } }
    }
    for (const route of buildRoutes(menuStore.menuTree)) {
      router.addRoute('admin', route)
    }
    // 重新导航一次，使命中刚注册的动态路由。
    // 不能直接展开 to：首次未命中时 name 已是 'not-found'，而 RouteLocationRaw 中
    // name 优先于 path，展开会把重导航固定在 404 上；只带 path/query/hash 才会重新解析
    return { path: to.path, query: to.query, hash: to.hash, replace: true }
  }

  return true
})

export default router
