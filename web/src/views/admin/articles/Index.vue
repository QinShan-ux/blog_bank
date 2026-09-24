<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { createArticle, deleteArticle, getArticles, updateArticle } from '@/api/articles'
import { today } from '@/utils/format'
import type { ArticleItem, ArticlePayload } from '@/types/api'

defineOptions({ name: 'AdminArticles' })

const SIZE = 10
const page = ref(1)
const list = ref<ArticleItem[]>([])
const hasNext = ref(false)
const loading = ref(false)

/** 后端不返回 total：返回条数等于 size 时认为可能还有下一页 */
async function load() {
  loading.value = true
  try {
    list.value = await getArticles(page.value, SIZE)
    hasNext.value = list.value.length === SIZE
  } finally {
    loading.value = false
  }
}

function prev() {
  if (page.value > 1) {
    page.value--
    load()
  }
}

function next() {
  if (hasNext.value) {
    page.value++
    load()
  }
}

// ---------- 新增 / 编辑 ----------

const drawerVisible = ref(false)
const saving = ref(false)
const editingId = ref('')
const formRef = ref<FormInstance>()
const form = reactive<ArticlePayload>({
  title: '',
  date: '',
  category: '',
  readTime: '',
  excerpt: '',
  tags: [],
  content: '',
  contentType: 'html',
})

const rules: FormRules = {
  title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
  date: [{ required: true, message: '请选择发布日期', trigger: 'change' }],
  category: [{ required: true, message: '请输入分类', trigger: 'blur' }],
  readTime: [{ required: true, message: '请输入预计阅读时长', trigger: 'blur' }],
  excerpt: [{ required: true, message: '请输入摘要', trigger: 'blur' }],
  content: [{ required: true, message: '请输入正文', trigger: 'blur' }],
}

function openCreate() {
  editingId.value = ''
  Object.assign(form, {
    title: '',
    date: today(),
    category: '',
    readTime: '',
    excerpt: '',
    tags: [],
    content: '',
    contentType: 'html',
  })
  drawerVisible.value = true
}

function openEdit(row: ArticleItem) {
  editingId.value = row.id
  Object.assign(form, {
    title: row.title,
    date: row.date,
    category: row.category,
    readTime: row.readTime,
    excerpt: row.excerpt,
    tags: [...row.tags],
    content: row.content,
    contentType: row.contentType || 'html',
  })
  drawerVisible.value = true
}

async function submit() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return
  saving.value = true
  try {
    const payload: ArticlePayload = { ...form, tags: form.tags.filter((t) => t.trim()) }
    if (editingId.value) {
      await updateArticle(editingId.value, payload)
      ElMessage.success('更新成功')
    } else {
      await createArticle(payload)
      ElMessage.success('创建成功')
    }
    drawerVisible.value = false
    load()
  } finally {
    saving.value = false
  }
}

function handleDelete(row: ArticleItem) {
  ElMessageBox.confirm(`确定删除文章「${row.title}」吗？`, '警告', { type: 'warning' })
    .then(async () => {
      await deleteArticle(row.id)
      ElMessage.success('删除成功')
      if (list.value.length === 1 && page.value > 1) page.value--
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
      <span class="toolbar-title">文章管理</span>
      <el-button type="primary" @click="openCreate">新增文章</el-button>
    </div>

    <el-table :data="list" v-loading="loading" stripe>
      <el-table-column prop="title" label="标题" min-width="220" show-overflow-tooltip />
      <el-table-column prop="date" label="发布日期" width="110" />
      <el-table-column prop="category" label="分类" width="120">
        <template #default="{ row }">
          <el-tag size="small" effect="plain">{{ row.category }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="readTime" label="阅读时长" width="110" />
      <el-table-column label="内容类型" width="100">
        <template #default="{ row }">
          <el-tag size="small" :type="row.contentType === 'markdown' ? 'warning' : 'info'" effect="plain">
            {{ row.contentType === 'markdown' ? 'Markdown' : 'HTML' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="标签" min-width="160">
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

    <div class="pager">
      <el-button :disabled="page <= 1 || loading" @click="prev">上一页</el-button>
      <span class="pager-info">第 {{ page }} 页</span>
      <el-button :disabled="!hasNext || loading" @click="next">下一页</el-button>
    </div>

    <el-drawer
      v-model="drawerVisible"
      :title="editingId ? '编辑文章' : '新增文章'"
      size="46%"
      destroy-on-close
    >
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px">
        <el-form-item label="标题" prop="title">
          <el-input v-model="form.title" maxlength="200" />
        </el-form-item>
        <el-form-item label="发布日期" prop="date">
          <el-date-picker
            v-model="form.date"
            type="date"
            value-format="YYYY-MM-DD"
            placeholder="选择日期"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="分类" prop="category">
          <el-input v-model="form.category" placeholder="如：前端开发" />
        </el-form-item>
        <el-form-item label="阅读时长" prop="readTime">
          <el-input v-model="form.readTime" placeholder="如：5 分钟阅读" />
        </el-form-item>
        <el-form-item label="摘要" prop="excerpt">
          <el-input v-model="form.excerpt" type="textarea" :rows="2" />
        </el-form-item>
        <el-form-item label="标签">
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
        <el-form-item label="内容类型" prop="contentType">
          <el-radio-group v-model="form.contentType">
            <el-radio value="html">HTML</el-radio>
            <el-radio value="markdown">Markdown</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="正文" prop="content">
          <el-input
            v-model="form.content"
            type="textarea"
            :rows="16"
            class="mono-textarea"
            :placeholder="
              form.contentType === 'markdown'
                ? 'Markdown 格式正文，如 ## 标题、**加粗**、- 列表…'
                : 'HTML 格式正文，如 <h2>标题</h2><p>段落…</p>'
            "
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :loading="saving" @click="submit">保存</el-button>
        </el-form-item>
      </el-form>
    </el-drawer>
  </div>
</template>

<style scoped lang="scss">
.toolbar-title {
  font-size: 16px;
  font-weight: 600;
}

.pager {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 14px;
}

.pager-info {
  color: #909399;
  font-size: 13px;
}
</style>
