<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div class="front-layout">
    <header class="front-header">
      <div class="front-container front-header-inner">
        <router-link to="/" class="front-logo">📝 BlogBank</router-link>
        <nav class="front-nav">
          <router-link to="/" :class="{ active: route.path === '/' }">首页</router-link>
          <router-link to="/essays" :class="{ active: route.path === '/essays' }">随笔</router-link>
          <router-link to="/admin">后台管理</router-link>
        </nav>
      </div>
    </header>

    <main class="front-main">
      <div class="front-container">
        <router-view />
      </div>
    </main>

    <footer class="front-footer">BlogBank · Vue3 + .NET 博客系统</footer>
  </div>
</template>

<style scoped lang="scss">
.front-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.front-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(8px);
  border-bottom: 1px solid #ececec;
}

.front-header-inner {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.front-logo {
  font-size: 20px;
  font-weight: 700;
  color: #303133;
  text-decoration: none;
}

.front-nav {
  display: flex;
  gap: 24px;

  a {
    color: #606266;
    text-decoration: none;
    font-size: 15px;
    padding: 4px 2px;

    &:hover,
    &.active {
      color: #409eff;
    }
  }
}

.front-main {
  flex: 1;
  padding: 32px 0;
}

.front-footer {
  padding: 20px 0;
  text-align: center;
  color: #909399;
  font-size: 13px;
  border-top: 1px solid #ececec;
}
</style>
