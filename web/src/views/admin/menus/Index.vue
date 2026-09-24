<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance } from 'element-plus'
import { createMenu, deleteMenu, getMenuTree, updateMenu } from '@/api/menus'
import type { MenuItem } from '@/types/api'

defineOptions({ name: 'AdminMenus' })

const tree = ref<MenuItem[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    tree.value = await getMenuTree()
  } finally {
    loading.value = false
  }
}

const TYPE_MAP: Record<number, { label: string; tag: 'warning' | 'success' | 'info' }> = {
  1: { label: '目录', tag: 'warning' },
  2: { label: '菜单', tag: 'success' },
  3: { label: '按钮', tag: 'info' },
}

// ---------- 上级菜单选择 ----------

interface TreeOption {
  value: string
  label: string
  children?: TreeOption[]
}

/** 按钮不能作为上级；顶级用 pid=0 表示 */
function buildOptions(nodes: MenuItem[]): TreeOption[] {
  const result: TreeOption[] = []
  for (const node of nodes) {
    if (node.type === 3) continue
    const children = node.children ? buildOptions(node.children) : undefined
    result.push({ value: node.id, label: node.title, children: children?.length ? children : undefined })
  }
  return result
}

const parentOptions = computed<TreeOption[]>(() => [
  { value: '0', label: '顶级菜单', children: buildOptions(tree.value) },
])

// ---------- 新增 / 编辑 ----------

const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref('')
const formRef = ref<FormInstance>()

const form = reactive({
  pid: '0',
  type: 2 as MenuItem['type'],
  title: '',
  name: '',
  path: '',
  component: '',
  redirect: '',
  permission: '',
  icon: 'ele-Menu',
  isIframe: false,
  outLink: '',
  isHide: false,
  isKeepAlive: true,
  isAffix: false,
  orderNo: 100,
  status: 1 as MenuItem['status'],
  remark: '',
})

function openCreateChild(row: MenuItem, type: MenuItem['type'] = 2) {
  editingId.value = ''
  Object.assign(form, {
    pid: row.id,
    type,
    title: '',
    name: '',
    path: '',
    component: '',
    redirect: '',
    permission: '',
    icon: 'ele-Menu',
    isIframe: false,
    outLink: '',
    isHide: false,
    isKeepAlive: true,
    isAffix: false,
    orderNo: 100,
    status: 1,
    remark: '',
  })
  dialogVisible.value = true
}

function openCreate() {
  // 顶级目录：pid=0
  openCreateChild({ id: '0' } as MenuItem, 1)
}

function openEdit(row: MenuItem) {
  editingId.value = row.id
  Object.assign(form, {
    pid: row.pid,
    type: row.type,
    title: row.title,
    name: row.name ?? '',
    path: row.path ?? '',
    component: row.component ?? '',
    redirect: row.redirect ?? '',
    permission: row.permission ?? '',
    icon: row.icon ?? 'ele-Menu',
    isIframe: row.isIframe,
    outLink: row.outLink ?? '',
    isHide: row.isHide,
    isKeepAlive: row.isKeepAlive,
    isAffix: row.isAffix,
    orderNo: row.orderNo,
    status: row.status,
    remark: row.remark ?? '',
  })
  dialogVisible.value = true
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const payload = { ...form }
    if (editingId.value) {
      await updateMenu(editingId.value, payload)
      ElMessage.success('更新成功')
    } else {
      await createMenu(payload)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

/** 有子菜单时后端返回 400 并提示，拦截器统一展示 */
async function handleDelete(row: MenuItem) {
  try {
    await ElMessageBox.confirm(`确定删除「${row.title}」吗？`, '警告', { type: 'warning' })
  } catch {
    return
  }
  try {
    await deleteMenu(row.id)
    ElMessage.success('删除成功')
    load()
  } catch {
    // 错误信息已由拦截器提示
  }
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <div class="table-toolbar">
      <span class="toolbar-title">菜单管理</span>
      <el-button type="primary" @click="openCreate">新增顶级菜单</el-button>
    </div>

    <el-table
      :data="tree"
      v-loading="loading"
      row-key="id"
      :tree-props="{ children: 'children' }"
      default-expand-all
    >
      <el-table-column prop="title" label="名称" min-width="180" />
      <el-table-column label="类型" width="80">
        <template #default="{ row }">
          <el-tag :type="TYPE_MAP[row.type]?.tag ?? 'info'" size="small">
            {{ TYPE_MAP[row.type]?.label ?? row.type }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="name" label="路由名称" min-width="120" show-overflow-tooltip />
      <el-table-column prop="path" label="路由路径" min-width="120" show-overflow-tooltip />
      <el-table-column prop="component" label="组件" min-width="140" show-overflow-tooltip />
      <el-table-column prop="permission" label="权限标识" min-width="140" show-overflow-tooltip />
      <el-table-column prop="orderNo" label="排序" width="70" />
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.status === 1 ? 'success' : 'danger'" size="small">
            {{ row.status === 1 ? '启用' : '禁用' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="隐藏" width="70">
        <template #default="{ row }">{{ row.isHide ? '是' : '否' }}</template>
      </el-table-column>
      <el-table-column label="操作" width="210" fixed="right">
        <template #default="{ row }">
          <el-button v-if="row.type !== 3" link type="primary" @click="openCreateChild(row, row.type === 1 ? 1 : 3)">
            +子级
          </el-button>
          <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? '编辑菜单' : '新增菜单'"
      width="640px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" label-width="90px">
        <el-row>
          <el-col :span="12">
            <el-form-item label="上级菜单">
              <el-tree-select
                v-model="form.pid"
                :data="parentOptions"
                check-strictly
                default-expand-all
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="类型" label-width="52px">
              <el-radio-group v-model="form.type">
                <el-radio-button :value="1">目录</el-radio-button>
                <el-radio-button :value="2">菜单</el-radio-button>
                <el-radio-button :value="3">按钮</el-radio-button>
              </el-radio-group>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row>
          <el-col :span="12">
            <el-form-item label="标题" prop="title" required>
              <el-input v-model="form.title" maxlength="64" />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="路由名称" label-width="52px">
              <el-input v-model="form.name" maxlength="64" placeholder="如 AdminUsers" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row v-if="form.type !== 3">
          <el-col :span="12">
            <el-form-item label="路由路径">
              <el-input v-model="form.path" maxlength="128" placeholder="如 users" />
            </el-form-item>
          </el-col>
          <el-col v-if="form.type === 1" :span="10" :offset="2">
            <el-form-item label="重定向" label-width="52px">
              <el-input v-model="form.redirect" maxlength="128" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item v-if="form.type === 2" label="组件路径">
          <el-input v-model="form.component" maxlength="128" placeholder="如 users/Index" />
        </el-form-item>

        <el-form-item v-if="form.type === 3" label="权限标识">
          <el-input v-model="form.permission" maxlength="128" placeholder="如 sys:user:add" />
        </el-form-item>

        <el-row v-if="form.type !== 3">
          <el-col :span="12">
            <el-form-item label="图标">
              <el-input v-model="form.icon" maxlength="128" placeholder="ele-Menu（Element Plus 图标名）" />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="排序" label-width="52px">
              <el-input-number v-model="form.orderNo" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item v-if="form.isIframe" label="外链地址">
          <el-input v-model="form.outLink" maxlength="256" placeholder="https://..." />
        </el-form-item>

        <el-form-item label="选项">
          <el-checkbox v-model="form.isHide">侧边栏隐藏</el-checkbox>
          <el-checkbox v-model="form.isKeepAlive">页面缓存</el-checkbox>
          <el-checkbox v-model="form.isAffix">固定标签栏</el-checkbox>
          <el-checkbox v-model="form.isIframe">内嵌 iframe</el-checkbox>
        </el-form-item>

        <el-row>
          <el-col :span="12">
            <el-form-item label="状态">
              <el-radio-group v-model="form.status">
                <el-radio-button :value="1">启用</el-radio-button>
                <el-radio-button :value="0">禁用</el-radio-button>
              </el-radio-group>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="备注">
          <el-input v-model="form.remark" type="textarea" :rows="2" maxlength="256" />
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
