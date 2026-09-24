<script setup lang="ts">
import { useRouter } from 'vue-router'

defineOptions({ name: 'NotFound' })

const router = useRouter()
</script>

<template>
  <div class="notfound-page">
    <el-result icon="warning" title="404" sub-title="抱歉，您访问的页面不存在或未分配权限">
      <template #extra>
        <el-button type="primary" @click="router.push('/')">返回首页</el-button>
        <el-button @click="router.push('/admin')">进入后台</el-button>
      </template>
    </el-result>
  </div>
</template>

<style scoped lang="scss">
.notfound-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
