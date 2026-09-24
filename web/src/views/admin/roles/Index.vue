<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { createRole, deleteRole, getRoles, updateRole } from '@/api/roles'
import { formatDateTime } from '@/utils/format'
import type { RoleItem, RolePayload } from '@/types/api'

defineOptions({ name: 'AdminRoles' })

const list = ref<RoleItem[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    list.value = await getRoles()
  } finally {
    loading.value = false
  }
}

const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref<string | null>(null)
const formRef = ref<FormInstance>()
const form = reactive<RolePayload>({
  code: '',
  name: '',
  description: '',
})

const rules: FormRules = {
  code: [{ required: true, message: '请输入角色编码（如 admin）', trigger: 'blur' }],
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
}

function openCreate() {
  editingId.value = null
  Object.assign(form, { code: '', name: '', description: '' })
  dialogVisible.value = true
}

function openEdit(row: RoleItem) {
  editingId.value = row.id
  Object.assign(form, { code: row.code, name: row.name, description: row.description })
  dialogVisible.value = true
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (editingId.value !== null) {
      await updateRole(editingId.value, { ...form })
      ElMessage.success('更新成功')
    } else {
      await createRole({ ...form })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

function handleDelete(row: RoleItem) {
  ElMessageBox.confirm(`确定删除角色「${row.name}」吗？其用户关联会一并删除。`, '警告', {
    type: 'warning',
  })
    .then(async () => {
      await deleteRole(row.id)
      ElMessage.success('删除成功')
      load()
    })
    .catch(() => {})
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <div class="table-toolbar">
      <span class="toolbar-title">角色管理</span>
      <el-button type="primary" @click="openCreate">新增角色</el-button>
    </div>

    <el-table :data="list" v-loading="loading" stripe>
      <el-table-column prop="code" label="编码" min-width="120">
        <template #default="{ row }">
          <el-tag size="small">{{ row.code }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="name" label="名称" min-width="120" />
      <el-table-column prop="description" label="描述" min-width="220" show-overflow-tooltip />
      <el-table-column label="创建时间" width="170">
        <template #default="{ row }">{{ formatDateTime(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="140" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId !== null ? '编辑角色' : '新增角色'"
      width="480px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="编码" prop="code">
          <el-input v-model="form.code" maxlength="50" placeholder="如 admin" />
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" maxlength="100" placeholder="如 管理员" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" maxlength="500" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="submit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.toolbar-title {
  font-size: 16px;
  font-weight: 600;
}
</style>
