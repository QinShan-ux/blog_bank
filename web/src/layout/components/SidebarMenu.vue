<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import SidebarItem from './SidebarItem.vue'
import { useMenuStore } from '@/stores/menu'
import type { MenuItem } from '@/types/api'

defineProps<{ collapse: boolean }>()

const menuStore = useMenuStore()
const route = useRoute()

const visibleTree = computed<MenuItem[]>(() =>
  menuStore.menuTree.filter((node) => node.status === 1 && !node.isHide),
)
</script>

<template>
  <el-menu
    :default-active="route.path"
    :collapse="collapse"
    :collapse-transition="false"
    router
    background-color="#001529"
    text-color="rgba(255, 255, 255, 0.68)"
    active-text-color="#ffffff"
    class="sidebar-menu"
  >
    <SidebarItem v-for="node in visibleTree" :key="node.id" :node="node" />
  </el-menu>
</template>

<style scoped lang="scss">
.sidebar-menu {
  border-right: none;
  height: calc(100% - 56px);
  overflow-y: auto;
}
</style>
