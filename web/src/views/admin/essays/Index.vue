<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { createEssay, deleteEssay, getEssays, updateEssay } from '@/api/essays'
import { today } from '@/utils/format'
import type { EssayItem, EssayPayload } from '@/types/api'

defineOptions({ name: 'AdminEssays' })

const list = ref<EssayItem[]>([])
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    list.value = await getEssays()
  } finally {
    loading.value = false
  }
}

// ---------- 新增 / 编辑 ----------

const dialogVisible = ref(false)
const saving = ref(false)
const editingId = ref('')
const formRef = ref<FormInstance>()
const form = reactive<EssayPayload>({
  title: '',
  date: '',
  mood: '',
  moodIcon: '',
  weather: '',
  weatherIcon: '',
  location: '',
  tags: [],
  excerpt: '',
  content: '',
  bgColor: '#fef3c7',
})

const rules: FormRules = {
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  date: [{ required: true, message: '请选择日期', trigger: 'change' }],
  mood: [{ required: true, message: '请输入心情描述', trigger: 'blur' }],
  moodIcon: [{ required: true, message: '请输入心情 Emoji', trigger: 'blur' }],
  weather: [{ required: true, message: '请输入天气描述', trigger: 'blur' }],
  weatherIcon: [{ required: true, message: '请输入天气 Emoji', trigger: 'blur' }],
  location: [{ required: true, message: '请输入地点', trigger: 'blur' }],
  tags: [{ required: true, type: 'array', min: 1, message: '至少一个标签', trigger: 'change' }],
  excerpt: [{ required: true, message: '请输入摘要', trigger: 'blur' }],
  content: [{ required: true, message: '请输入正文', trigger: 'blur' }],
  bgColor: [{ required: true, message: '请选择背景色', trigger: 'change' }],
}

function openCreate() {
  editingId.value = ''
  Object.assign(form, {
    title: '',
    date: today(),
    mood: '',
    moodIcon: '😌',
    weather: '',
    weatherIcon: '☀️',
    location: '',
    tags: [],
    excerpt: '',
    content: '',
    bgColor: '#fef3c7',
  })
  dialogVisible.value = true
}

function openEdit(row: EssayItem) {
  editingId.value = row.id
  Object.assign(form, {
    title: row.title,
    date: row.date,
    mood: row.mood,
    moodIcon: row.moodIcon,
    weather: row.weather,
    weatherIcon: row.weatherIcon,
    location: row.location,
    tags: [...row.tags],
    excerpt: row.excerpt,
    content: row.content,
    bgColor: row.bgColor || '#fef3c7',
  })
  dialogVisible.value = true
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const payload: EssayPayload = { ...form, tags: form.tags.filter((t) => t.trim()) }
    if (editingId.value) {
      await updateEssay(editingId.value, payload)
      ElMessage.success('更新成功')
    } else {
      await createEssay(payload)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

function handleDelete(row: EssayItem) {
  ElMessageBox.confirm(`确定删除随笔「${row.title}」吗？`, '警告', { type: 'warning' })
    .then(async () => {
      await deleteEssay(row.id)
      ElMessage.success('删除成功')
      load()
    })
    .catch(() => {})
}

// ---------- 标签动态输入 ----------

const tagInputVisible = ref(false)
const tagInputValue = ref('')

function handleTagClose(tag: string) {
  form.tags.splice(form.tags.indexOf(tag), 1)
}

function showTagInput() {
  tagInputVisible.value = true
}

function handleTagInputConfirm() {
  const value = tagInputValue.value.trim()
  if (value && !form.tags.includes(value)) form.tags.push(value)
  tagInputVisible.value = false
  tagInputValue.value = ''
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <div class="table-toolbar">
      <span class="toolbar-title">随笔管理</span>
      <el-button type="primary" @click="openCreate">新增随笔</el-button>
    </div>

    <el-table :data="list" v-loading="loading" stripe>
      <el-table-column prop="title" label="标题" min-width="180" show-overflow-tooltip />
      <el-table-column prop="date" label="日期" width="110" />
      <el-table-column label="心情" width="100">
        <template #default="{ row }">{{ row.moodIcon }} {{ row.mood }}</template>
      </el-table-column>
      <el-table-column label="天气" width="100">
        <template #default="{ row }">{{ row.weatherIcon }} {{ row.weather }}</template>
      </el-table-column>
      <el-table-column prop="location" label="地点" min-width="120" show-overflow-tooltip />
      <el-table-column label="背景色" width="90">
        <template #default="{ row }">
          <span class="color-dot" :style="{ background: row.bgColor }" />
        </template>
      </el-table-column>
      <el-table-column label="标签" min-width="140">
        <template #default="{ row }">
          <el-tag v-for="tag in row.tags" :key="tag" size="small" type="info" style="margin-right: 4px">
            {{ tag }}
          </el-tag>
        </template>
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
      :title="editingId ? '编辑随笔' : '新增随笔'"
      width="640px"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="标题" prop="title">
          <el-input v-model="form.title" maxlength="200" />
        </el-form-item>
        <el-form-item label="日期" prop="date">
          <el-date-picker
            v-model="form.date"
            type="date"
            value-format="YYYY-MM-DD"
            placeholder="选择日期"
            style="width: 100%"
          />
        </el-form-item>
        <el-row>
          <el-col :span="12">
            <el-form-item label="心情" prop="mood">
              <el-input v-model="form.mood" placeholder="如：惬意" />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="图标" prop="moodIcon" label-width="52px">
              <el-input v-model="form.moodIcon" placeholder="😌" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row>
          <el-col :span="12">
            <el-form-item label="天气" prop="weather">
              <el-input v-model="form.weather" placeholder="如：晴" />
            </el-form-item>
          </el-col>
          <el-col :span="10" :offset="2">
            <el-form-item label="图标" prop="weatherIcon" label-width="52px">
              <el-input v-model="form.weatherIcon" placeholder="☀️" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="地点" prop="location">
          <el-input v-model="form.location" placeholder="如：街角咖啡馆" />
        </el-form-item>
        <el-form-item label="背景色" prop="bgColor">
          <el-color-picker v-model="form.bgColor" />
        </el-form-item>
        <el-form-item label="标签" prop="tags">
          <div>
            <el-tag
              v-for="tag in form.tags"
              :key="tag"
              closable
              style="margin-right: 6px"
              @close="handleTagClose(tag)"
            >
              {{ tag }}
            </el-tag>
            <el-input
              v-if="tagInputVisible"
              v-model="tagInputValue"
              size="small"
              style="width: 90px"
              @keyup.enter="handleTagInputConfirm"
              @blur="handleTagInputConfirm"
            />
            <el-button v-else size="small" @click="showTagInput">+ 新标签</el-button>
          </div>
        </el-form-item>
        <el-form-item label="摘要" prop="excerpt">
          <el-input v-model="form.excerpt" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="正文" prop="content">
          <el-input v-model="form.content" type="textarea" :rows="8" placeholder="纯文本正文" />
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

.color-dot {
  display: inline-block;
  width: 22px;
  height: 22px;
  border-radius: 6px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  vertical-align: middle;
}
</style>
