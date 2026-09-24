<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getArticleTitles } from '@/api/articles'
import { getEssays } from '@/api/essays'
import { getRoles } from '@/api/roles'
import { getUsers } from '@/api/users'
import { useUserStore } from '@/stores/user'

defineOptions({ name: 'AdminDashboard' })

const userStore = useUserStore()
const stats = ref({ users: 0, roles: 0, articles: 0, essays: 0 })

onMounted(async () => {
  // 统计数据允许部分失败，成功的照常展示
  const [users, roles, titles, essays] = await Promise.allSettled([
    getUsers(),
    getRoles(),
    getArticleTitles(),
    getEssays(),
  ])
  if (users.status === 'fulfilled') stats.value.users = users.value.length
  if (roles.status === 'fulfilled') stats.value.roles = roles.value.length
  if (titles.status === 'fulfilled') stats.value.articles = titles.value.length
  if (essays.status === 'fulfilled') stats.value.essays = essays.value.length
})
</script>

<template>
  <div>
    <el-card shadow="never" class="welcome-card">
      <h2 class="welcome-title">你好，{{ userStore.nickname || userStore.account || '管理员' }} 👋</h2>
      <p class="welcome-sub">欢迎回来，这是 BlogBank 管理控制台。</p>
    </el-card>

    <el-row :gutter="16">
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-icon" style="background: #e8f4ff; color: #409eff">
              <el-icon :size="26"><User /></el-icon>
            </div>
            <div>
              <div class="stat-value">{{ stats.users }}</div>
              <div class="stat-label">注册用户</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-icon" style="background: #eef9f0; color: #67c23a">
              <el-icon :size="26"><Avatar /></el-icon>
            </div>
            <div>
              <div class="stat-value">{{ stats.roles }}</div>
              <div class="stat-label">角色</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-icon" style="background: #fdf6ec; color: #e6a23c">
              <el-icon :size="26"><Document /></el-icon>
            </div>
            <div>
              <div class="stat-value">{{ stats.articles }}</div>
              <div class="stat-label">文章</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover">
          <div class="stat-card">
            <div class="stat-icon" style="background: #f4e8ff; color: #9c59f0">
              <el-icon :size="26"><EditPen /></el-icon>
            </div>
            <div>
              <div class="stat-value">{{ stats.essays }}</div>
              <div class="stat-label">随笔</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped lang="scss">
.welcome-card {
  margin-bottom: 16px;
}

.welcome-title {
  margin: 0 0 8px;
}

.welcome-sub {
  margin: 0;
  color: #909399;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-value {
  font-size: 26px;
  font-weight: 700;
  line-height: 1.2;
}

.stat-label {
  color: #909399;
  font-size: 13px;
}
</style>
