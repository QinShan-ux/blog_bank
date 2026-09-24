<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { createUser, deleteUser, getUsers, updateUser } from '@/api/users'
import { assignUserRole, getUserRoles, revokeUserRole } from '@/api/userRoles'
import { getRoles } from '@/api/roles'
import { formatDateTime } from '@/utils/format'
import type { RoleItem, UserItem, UserPayload } from '@/types/api'

defineOptions({ name: 'AdminUsers' })

const list = ref<UserItem[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    list.value = await getUsers()
  } finally {
    loading.value = false
  }
}

// ---------- 新增 / 编辑 ----------

const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref('')
const formRef = ref<FormInstance>()
const form = reactive<UserPayload & { password: string }>({
  account: '',
  nickname: '',
  email: '',
  password: '',
  avatar: '',
  isEnabled: true,
})

const rules: FormRules = {
  account: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  nickname: [{ required: true, message: '请输入昵称', trigger: 'blur' }],
  email: [
    { required: true, message: '请输入邮箱', trigger: 'blur' },
    { type: 'email', message: '邮箱格式不正确', trigger: ['blur', 'change'] },
  ],
  password: [
    {
      validator: (_rule, value: string, callback) => {
        // 新增时必填；编辑时留空表示不修改密码
        if (!editingId.value && !value) callback(new Error('请输入密码'))
        else callback()
      },
      trigger: 'blur',
    },
  ],
}

function openCreate() {
  editingId.value = ''
  Object.assign(form, { account: '', nickname: '', email: '', password: '', avatar: '', isEnabled: true })
  dialogVisible.value = true
}

function openEdit(row: UserItem) {
  editingId.value = row.id
  Object.assign(form, {
    account: row.username,
    nickname: row.nickname,
    email: row.email,
    password: '',
    avatar: row.avatar,
    isEnabled: row.isEnabled,
  })
  dialogVisible.value = true
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (editingId.value) {
      // password 为空字符串时后端不修改密码
      await updateUser(editingId.value, { ...form, password: form.password || undefined })
      ElMessage.success('更新成功')
    } else {
      await createUser({ ...form })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

/** 表格内直接切换启用状态 */
async function toggleEnabled(row: UserItem) {
  try {
    await updateUser(row.id, {
      account: row.username,
      nickname: row.nickname,
      email: row.email,
      isEnabled: row.isEnabled,
    })
    ElMessage.success(row.isEnabled ? '已启用' : '已禁用')
  } catch {
    row.isEnabled = !row.isEnabled // 失败回滚开关状态
  }
}

function handleDelete(row: UserItem) {
  ElMessageBox.confirm(`确定删除用户「${row.nickname || row.username}」吗？其角色关联会一并删除。`, '警告', {
    type: 'warning',
  })
    .then(async () => {
      await deleteUser(row.id)
      ElMessage.success('删除成功')
      load()
    })
    .catch(() => {})
}

// ---------- 分配角色 ----------

const roleDialogVisible = ref(false)
const roleSaving = ref(false)
const roleUserId = ref('')
const roleUserName = ref('')
const allRoles = ref<RoleItem[]>([])
const checkedRoleIds = ref<string[]>([])
const originalRoleIds = ref<string[]>([])

async function openAssignRoles(row: UserItem) {
  roleUserId.value = row.id
  roleUserName.value = row.nickname || row.username
  const [roles, owned] = await Promise.all([getRoles(), getUserRoles(row.id)])
  allRoles.value = roles
  originalRoleIds.value = owned.map((item) => String(item.roleId))
  checkedRoleIds.value = [...originalRoleIds.value]
  roleDialogVisible.value = true
}

async function saveRoles() {
  roleSaving.value = true
  try {
    for (const role of allRoles.value) {
      const checked = checkedRoleIds.value.includes(role.id)
      const had = originalRoleIds.value.includes(role.id)
      if (checked && !had) await assignUserRole(roleUserId.value, role.id)
      if (!checked && had) await revokeUserRole(roleUserId.value, role.id)
    }
    ElMessage.success('角色分配已保存')
    roleDialogVisible.value = false
    load()
  } finally {
    roleSaving.value = false
  }
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <div class="table-toolbar">
      <span class="toolbar-title">用户管理</span>
      <el-button type="primary" @click="openCreate">新增用户</el-button>
    </div>

    <el-table :data="list" v-loading="loading" stripe>
      <el-table-column prop="username" label="用户名" min-width="120" />
      <el-table-column prop="nickname" label="昵称" min-width="120" show-overflow-tooltip />
      <el-table-column prop="email" label="邮箱" min-width="180" show-overflow-tooltip />
      <el-table-column label="角色" min-width="150">
        <template #default="{ row }">
          <el-tag
            v-for="role in row.roles"
            :key="role.id"
            size="small"
            style="margin-right: 4px"
            effect="plain"
          >
            {{ role.name }}
          </el-tag>
          <span v-if="row.roles.length === 0" class="muted">未分配</span>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-switch v-model="row.isEnabled" @change="toggleEnabled(row)" />
        </template>
      </el-table-column>
      <el-table-column label="创建时间" width="170">
        <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link type="primary" @click="openAssignRoles(row)">分配角色</el-button>
          <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新增/编辑用户 -->
    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? '编辑用户' : '新增用户'"
      width="480px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="用户名" prop="account">
          <el-input v-model="form.account" maxlength="50" />
        </el-form-item>
        <el-form-item label="昵称" prop="nickname">
          <el-input v-model="form.nickname" maxlength="100" />
        </el-form-item>
        <el-form-item label="邮箱" prop="email">
          <el-input v-model="form.email" maxlength="200" />
        </el-form-item>
        <el-form-item label="密码" prop="password">
          <el-input
            v-model="form.password"
            type="password"
            show-password
            :placeholder="editingId ? '留空表示不修改密码' : '请输入密码'"
          />
        </el-form-item>
        <el-form-item label="头像 URL">
          <el-input v-model="form.avatar" maxlength="500" />
        </el-form-item>
        <el-form-item label="启用">
          <el-switch v-model="form.isEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submit">保存</el-button>
      </template>
    </el-dialog>

    <!-- 分配角色 -->
    <el-dialog v-model="roleDialogVisible" :title="`分配角色：${roleUserName}`" width="420px">
      <el-checkbox-group v-model="checkedRoleIds">
        <div class="role-check-list">
          <el-checkbox v-for="role in allRoles" :key="role.id" :value="role.id">
            {{ role.name }}
            <span class="muted">（{{ role.code }}）</span>
          </el-checkbox>
        </div>
      </el-checkbox-group>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="roleSaving" @click="saveRoles">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.toolbar-title {
  font-size: 16px;
  font-weight: 600;
}

.muted {
  color: #c0c4cc;
}

.role-check-list {
  display: flex;
  flex-direction: column;
}
</style>
