<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { createWorkBug, deleteWorkBug, getWorkBugs, updateWorkBug } from '@/api/workBugs'
import { today } from '@/utils/format'
import type { BugSeverity, BugStatus, WorkBugItem, WorkBugPayload } from '@/types/api'

defineOptions({ name: 'AdminWorkBugs' })

// ---------- 状态 / 严重程度文案与标签样式 ----------

const STATUS_TEXT: Record<BugStatus, string> = { 0: '待处理', 1: '处理中', 2: '已解决' }
const STATUS_TAG: Record<BugStatus, 'info' | 'warning' | 'success'> = { 0: 'info', 1: 'warning', 2: 'success' }
const SEVERITY_TEXT: Record<BugSeverity, string> = { 0: '低', 1: '中', 2: '高' }
const SEVERITY_TAG: Record<BugSeverity, 'info' | 'warning' | 'danger'> = { 0: 'info', 1: 'warning', 2: 'danger' }

const statusOptions = (Object.keys(STATUS_TEXT) as unknown as string[]).map((v) => ({
  value: Number(v) as BugStatus,
  label: STATUS_TEXT[Number(v) as BugStatus],
}))
const severityOptions = (Object.keys(SEVERITY_TEXT) as unknown as string[]).map((v) => ({
  value: Number(v) as BugSeverity,
  label: SEVERITY_TEXT[Number(v) as BugSeverity],
}))

// ---------- 列表与筛选 ----------

const list = ref<WorkBugItem[]>([])
const loading = ref(false)

const filters = reactive({
  status: '' as BugStatus | '',
  severity: '' as BugSeverity | '',
  project: '',
  keyword: '',
})

async function load() {
  loading.value = true
  try {
    list.value = await getWorkBugs({
      status: filters.status === '' ? undefined : filters.status,
      severity: filters.severity === '' ? undefined : filters.severity,
      project: filters.project.trim() || undefined,
      keyword: filters.keyword.trim() || undefined,
    })
  } finally {
    loading.value = false
  }
}

function search() {
  load()
}

function reset() {
  filters.status = ''
  filters.severity = ''
  filters.project = ''
  filters.keyword = ''
  load()
}

// ---------- 详情抽屉 ----------

const detailVisible = ref(false)
const current = ref<WorkBugItem | null>(null)

function viewDetail(row: WorkBugItem) {
  current.value = row
  detailVisible.value = true
}

// ---------- 新增 / 编辑 ----------

const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref('')
const formRef = ref<FormInstance>()
const form = reactive<WorkBugPayload>({
  title: '',
  description: '',
  rootCause: null,
  solution: '',
  severity: 1,
  status: 0,
  project: '',
  environment: null,
  occurredDate: '',
  solvedDate: null,
})

const isResolved = computed(() => form.status === 2)

// ---------- 处理方法：编辑 / 预览切换（HTML 格式，与文章正文一致） ----------

const solutionPreview = ref(false)
const solutionPreviewHtml = computed(() =>
  form.solution || '<p style="color:#909399">（暂无内容）</p>',
)

const rules: FormRules = {
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  description: [{ required: true, message: '请输入 bug 现象描述', trigger: 'blur' }],
  solution: [{ required: true, message: '请输入处理方法', trigger: 'blur' }],
  project: [{ required: true, message: '请输入所属项目/模块', trigger: 'blur' }],
  occurredDate: [{ required: true, message: '请选择发生日期', trigger: 'change' }],
}

function openCreate() {
  editingId.value = ''
  Object.assign(form, {
    title: '',
    description: '',
    rootCause: null,
    solution: '',
    severity: 1,
    status: 0,
    project: '',
    environment: null,
    occurredDate: today(),
    solvedDate: null,
  })
  dialogVisible.value = true
}

function openEdit(row: WorkBugItem) {
  editingId.value = row.id
  Object.assign(form, {
    title: row.title,
    description: row.description,
    rootCause: row.rootCause,
    solution: row.solution,
    severity: row.severity,
    status: row.status,
    project: row.project,
    environment: row.environment,
    occurredDate: row.occurredDate,
    solvedDate: row.solvedDate,
  })
  dialogVisible.value = true
}

/** 状态切到已解决且未填解决日期时，表单里先展示今天（保存时后端也会兜底） */
function onStatusChange() {
  if (isResolved.value && !form.solvedDate) form.solvedDate = today()
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    if (editingId.value) {
      await updateWorkBug(editingId.value, { ...form })
      ElMessage.success('更新成功')
    } else {
      await createWorkBug({ ...form })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

function handleDelete(row: WorkBugItem) {
  ElMessageBox.confirm(`确定删除 bug「${row.title}」吗？`, '警告', { type: 'warning' })
    .then(async () => {
      await deleteWorkBug(row.id)
      ElMessage.success('删除成功')
      load()
    })
    .catch(() => {})
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <el-form inline class="filter-bar" @submit.prevent>
      <el-form-item label="状态">
        <el-select v-model="filters.status" placeholder="全部" clearable style="width: 120px">
          <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
      </el-form-item>
      <el-form-item label="严重程度">
        <el-select v-model="filters.severity" placeholder="全部" clearable style="width: 120px">
          <el-option v-for="opt in severityOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
      </el-form-item>
      <el-form-item label="项目">
        <el-input v-model="filters.project" placeholder="项目/模块" clearable style="width: 150px" />
      </el-form-item>
      <el-form-item label="关键字">
        <el-input
          v-model="filters.keyword"
          placeholder="标题/现象/处理方法/原因"
          clearable
          style="width: 200px"
          @keyup.enter="search"
        />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="search">查询</el-button>
        <el-button @click="reset">重置</el-button>
      </el-form-item>
    </el-form>

    <div class="table-toolbar">
      <span class="toolbar-title">工作 Bug 记录</span>
      <el-button type="primary" @click="openCreate">记录 Bug</el-button>
    </div>

    <el-table :data="list" v-loading="loading" stripe>
      <el-table-column label="标题" min-width="200" show-overflow-tooltip>
        <template #default="{ row }">
          <el-link type="primary" :underline="false" @click="viewDetail(row)">{{ row.title }}</el-link>
        </template>
      </el-table-column>
      <el-table-column label="严重程度" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="SEVERITY_TAG[row.severity as BugSeverity]" size="small">
            {{ SEVERITY_TEXT[row.severity as BugSeverity] }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="STATUS_TAG[row.status as BugStatus]" size="small">
            {{ STATUS_TEXT[row.status as BugStatus] }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="project" label="项目/模块" min-width="110" show-overflow-tooltip />
      <el-table-column prop="environment" label="环境" min-width="100" show-overflow-tooltip>
        <template #default="{ row }">{{ row.environment || '-' }}</template>
      </el-table-column>
      <el-table-column prop="occurredDate" label="发生日期" width="110" />
      <el-table-column label="解决日期" width="110">
        <template #default="{ row }">{{ row.solvedDate || '-' }}</template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="viewDetail(row)">详情</el-button>
          <el-button link type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 详情抽屉 -->
    <el-drawer v-model="detailVisible" :title="current?.title" size="480px">
      <template v-if="current">
        <div class="detail-meta">
          <el-tag :type="STATUS_TAG[current.status]" size="small">{{ STATUS_TEXT[current.status] }}</el-tag>
          <el-tag :type="SEVERITY_TAG[current.severity]" size="small" style="margin-left: 8px">
            {{ SEVERITY_TEXT[current.severity] }}严重
          </el-tag>
          <span class="detail-meta-text">
            {{ current.project }}<template v-if="current.environment"> · {{ current.environment }}</template>
          </span>
        </div>
        <div class="detail-meta-text detail-dates">
          发生 {{ current.occurredDate }}
          <template v-if="current.solvedDate"> · 解决 {{ current.solvedDate }}</template>
        </div>

        <div class="detail-section">
          <div class="detail-label">现象描述</div>
          <div class="detail-content">{{ current.description }}</div>
        </div>
        <div class="detail-section">
          <div class="detail-label">原因分析</div>
          <div class="detail-content">{{ current.rootCause || '未记录' }}</div>
        </div>
        <div class="detail-section">
          <div class="detail-label">处理方法</div>
          <!-- solution 为 HTML 格式，与文章正文同方式渲染；description/rootCause 保持纯文本 -->
          <div class="detail-content rich-content" v-html="current.solution" />
        </div>
      </template>
    </el-drawer>

    <!-- 新增 / 编辑弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="editingId ? '编辑 Bug 记录' : '记录 Bug'"
      width="640px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="标题" prop="title">
          <el-input v-model="form.title" maxlength="200" placeholder="简明概括问题" />
        </el-form-item>
        <el-row>
          <el-col :span="12">
            <el-form-item label="严重程度" prop="severity">
              <el-select v-model="form.severity" style="width: 100%">
                <el-option v-for="opt in severityOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="状态" prop="status" label-width="52px">
              <el-select v-model="form.status" style="width: 100%" @change="onStatusChange">
                <el-option v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="12">
            <el-form-item label="项目/模块" prop="project">
              <el-input v-model="form.project" placeholder="如：订单系统" />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="环境" prop="environment" label-width="52px">
              <el-input v-model="form.environment" placeholder="如：生产环境" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="12">
            <el-form-item label="发生日期" prop="occurredDate">
              <el-date-picker
                v-model="form.occurredDate"
                type="date"
                value-format="YYYY-MM-DD"
                placeholder="选择日期"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="解决日期" label-width="76px" prop="solvedDate">
              <el-date-picker
                v-model="form.solvedDate"
                type="date"
                value-format="YYYY-MM-DD"
                placeholder="未解决留空"
                style="width: 100%"
                :disabled="!isResolved"
              />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="现象描述" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="4"
            placeholder="报错信息、复现步骤等"
          />
        </el-form-item>
        <el-form-item label="原因分析" prop="rootCause">
          <el-input v-model="form.rootCause" type="textarea" :rows="3" placeholder="排查后定位的根本原因，未定位可留空" />
        </el-form-item>
        <el-form-item label="处理方法" prop="solution">
          <div class="solution-editor">
            <el-radio-group v-model="solutionPreview" size="small" class="solution-toggle">
              <el-radio-button :value="false">编辑</el-radio-button>
              <el-radio-button :value="true">预览</el-radio-button>
            </el-radio-group>
            <el-input
              v-if="!solutionPreview"
              v-model="form.solution"
              type="textarea"
              :rows="6"
              placeholder="支持 HTML（与文章正文一致），如 <pre><code>代码块</code></pre>、<strong>加粗</strong>、<ol><li>列表</li></ol>"
            />
            <!-- 预览：按富文本渲染当前 HTML 源码，不改动表单内容 -->
            <div v-else class="rich-content solution-preview" v-html="solutionPreviewHtml" />
          </div>
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

.detail-meta {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
}

.detail-meta-text {
  color: #909399;
  font-size: 13px;
}

.detail-dates {
  margin-bottom: 16px;
}

.detail-section {
  margin-bottom: 16px;
}

.detail-label {
  font-weight: 600;
  margin-bottom: 6px;
}

.detail-content {
  white-space: pre-wrap;
  line-height: 1.7;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  padding: 10px 12px;
}

/* HTML 富文本渲染容器（处理方法预览/详情），覆盖纯文本的 pre-wrap */
.rich-content {
  white-space: normal;

  :deep(p) {
    margin: 0 0 8px;
  }

  :deep(h1),
  :deep(h2),
  :deep(h3),
  :deep(h4) {
    margin: 12px 0 8px;
  }

  :deep(pre) {
    background: #f5f7fa;
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 10px 12px;
    margin: 8px 0;
    overflow-x: auto;
  }

  :deep(code) {
    font-family: Menlo, Consolas, monospace;
    font-size: 13px;
  }

  :deep(:not(pre) > code) {
    background: #f5f7fa;
    border-radius: 3px;
    padding: 1px 5px;
  }

  :deep(ul),
  :deep(ol) {
    margin: 8px 0;
    padding-left: 22px;
  }

  :deep(blockquote) {
    margin: 8px 0;
    padding: 4px 12px;
    border-left: 3px solid #dcdfe6;
    color: #606266;
  }

  :deep(img) {
    max-width: 100%;
  }

  :deep(a) {
    color: var(--el-color-primary);
  }

  :deep(table) {
    border-collapse: collapse;
    margin: 8px 0;
  }

  :deep(th),
  :deep(td) {
    border: 1px solid #e4e7ed;
    padding: 4px 10px;
  }
}

.solution-editor {
  width: 100%;
}

.solution-toggle {
  margin-bottom: 6px;
}

.solution-preview {
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  padding: 10px 12px;
  min-height: 140px;
}
</style>
