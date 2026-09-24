<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import SidebarMenu from './components/SidebarMenu.vue'
import { logout } from '@/api/auth'
import { findMenuChain } from '@/router/dynamicRoutes'
import { useMenuStore } from '@/stores/menu'
import { useTokenStore } from '@/stores/token'
import { useUserStore } from '@/stores/user'
import type { MenuItem } from '@/types/api'

const route = useRoute()
const router = useRouter()
const menuStore = useMenuStore()
const tokenStore = useTokenStore()
const userStore = useUserStore()

const isCollapse = ref(false)

const breadcrumbs = computed(() =>
  findMenuChain(menuStore.menuTree, route.path).map((node: MenuItem) => node.title),
)

async function handleDropdown(command: string) {
  if (command === 'front') {
    router.push('/')
    return
  }
  if (command !== 'logout') return
  try {
    await ElMessageBox.confirm('确定退出登录吗？', '提示', { type: 'warning' })
  } catch {
    return
  }
  try {
    await logout(tokenStore.accessToken, tokenStore.refreshToken)
  } catch {
    // 登出接口失败也继续清理本地状态
  }
  tokenStore.clear()
  menuStore.reset()
  router.push('/login')
}
</script>

<template>
  <el-container class="admin-layout">
    <el-aside :width="isCollapse ? '64px' : '220px'" class="admin-aside">
      <div class="admin-logo">
        <span v-if="!isCollapse">BlogBank 管理后台</span>
        <span v-else>BB</span>
      </div>
      <SidebarMenu :collapse="isCollapse" />
    </el-aside>

    <el-container>
      <el-header class="admin-header">
        <div class="admin-header-left">
          <el-icon class="collapse-btn" :size="18" @click="isCollapse = !isCollapse">
            <Fold v-if="!isCollapse" />
            <Expand v-else />
          </el-icon>
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/admin' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-for="item in breadcrumbs" :key="item">
              {{ item }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <el-dropdown @command="handleDropdown">
          <span class="admin-user">
            <el-icon><UserFilled /></el-icon>
            <span>{{ userStore.nickname || userStore.account || '用户' }}</span>
            <el-icon><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="front">前往前台</el-dropdown-item>
              <el-dropdown-item command="logout" divided>退出登录</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </el-header>

      <el-main class="admin-main">
        <router-view v-slot="{ Component }">
          <keep-alive :include="menuStore.cachedNames">
            <component :is="Component" />
          </keep-alive>
        </router-view>
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped lang="scss">
.admin-layout {
  height: 100vh;
}

.admin-aside {
  background-color: #001529;
  transition: width 0.2s;
  overflow: hidden;
}

.admin-logo {
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 16px;
  font-weight: 600;
  white-space: nowrap;
}

.admin-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
}

.admin-header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.collapse-btn {
  cursor: pointer;
}

.admin-user {
  display: flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
  color: #303133;
}

.admin-main {
  background: #f5f7fa;
  overflow-y: auto;
}
</style>
