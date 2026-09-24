<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { FormInstance, FormRules } from 'element-plus'
import { login } from '@/api/auth'
import { useMenuStore } from '@/stores/menu'
import { useTokenStore } from '@/stores/token'
import { useUserStore } from '@/stores/user'

defineOptions({ name: 'Login' })

const router = useRouter()
const route = useRoute()
const tokenStore = useTokenStore()
const userStore = useUserStore()
const menuStore = useMenuStore()

const formRef = ref<FormInstance>()
const loading = ref(false)
const form = reactive({
  account: '',
  password: '',
})

const rules: FormRules = {
  account: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
}

async function handleLogin() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  loading.value = true
  try {
    const res = await login({ ...form })
    tokenStore.save(res.accessToken, res.refreshToken, res.expiresAt)
    userStore.loadFromToken()
    menuStore.reset()
    router.push((route.query.redirect as string) || '/admin')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <h2 class="login-title">BlogBank 管理后台</h2>
      <el-form ref="formRef" :model="form" :rules="rules" size="large" @keyup.enter="handleLogin">
        <el-form-item prop="account">
          <el-input v-model="form.account" placeholder="用户名" :prefix-icon="'User'" clearable />
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="form.password"
            type="password"
            placeholder="密码"
            :prefix-icon="'Lock'"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" class="login-btn" :loading="loading" @click="handleLogin">
            登 录
          </el-button>
        </el-form-item>
      </el-form>
      <p class="login-tip">前台浏览无需登录，登录后进入管理控制台</p>
    </div>
  </div>
</template>

<style scoped lang="scss">
.login-page {
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #1f2d3d 0%, #2b4a6f 60%, #3a7bd5 100%);
}

.login-card {
  width: 380px;
  padding: 36px 32px 24px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.25);
}

.login-title {
  margin: 0 0 24px;
  text-align: center;
  font-size: 20px;
  color: #303133;
}

.login-btn {
  width: 100%;
}

.login-tip {
  margin: 4px 0 0;
  text-align: center;
  color: #909399;
  font-size: 12px;
}
</style>
