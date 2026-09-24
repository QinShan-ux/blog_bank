<script setup lang="ts">
import type { MenuItem } from '@/types/api'
import { menuFullPath, normalizePath } from '@/router/dynamicRoutes'

defineOptions({ name: 'SidebarItem' })

const props = defineProps<{
  node: MenuItem
  /** 父级链路的相对路径（不含开头的 /admin） */
  prefix?: string
}>()

/** "ele-Menu" → "Menu"，配合全局注册的 Element Plus 图标组件 */
function resolveIcon(icon: string): string {
  return icon.startsWith('ele-') ? icon.slice(4) : icon
}

function relPath(node: MenuItem): string {
  return menuFullPath(node, props.prefix ?? '')
}

function absPath(node: MenuItem): string {
  return `/admin/${normalizePath(relPath(node))}`
}
</script>

<template>
  <!-- 目录：递归渲染子级 -->
  <el-sub-menu v-if="node.type === 1 && (node.children?.length ?? 0) > 0" :index="node.id">
    <template #title>
      <el-icon v-if="node.icon">
        <component :is="resolveIcon(node.icon)" />
      </el-icon>
      <span>{{ node.title }}</span>
    </template>
    <SidebarItem
      v-for="child in node.children"
      :key="child.id"
      :node="child"
      :prefix="relPath(node)"
    />
  </el-sub-menu>

  <!-- 菜单：index 即路由路径，配合 el-menu 的 router 模式 -->
  <el-menu-item v-else-if="node.type === 2" :index="absPath(node)">
    <el-icon v-if="node.icon">
      <component :is="resolveIcon(node.icon)" />
    </el-icon>
    <template #title>{{ node.title }}</template>
  </el-menu-item>
</template>
