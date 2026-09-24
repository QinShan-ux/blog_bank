<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { marked } from 'marked'
import { getArticle } from '@/api/articles'
import type { ArticleItem } from '@/types/api'

defineOptions({ name: 'ArticleDetail' })

const route = useRoute()
const article = ref<ArticleItem | null>(null)
const notFound = ref(false)
const loading = ref(true)

/** html 类型直接使用后端 HTML；markdown 类型先在前端转换为 HTML 再渲染 */
const renderedContent = computed(() => {
  if (!article.value) return ''
  return article.value.contentType === 'markdown'
    ? (marked.parse(article.value.content) as string)
    : article.value.content
})

onMounted(async () => {
  try {
    article.value = await getArticle(route.params.id as string)
  } catch {
    notFound.value = true
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="detail-page">
    <div v-if="article" class="page-card">
      <h1 class="detail-title">{{ article.title }}</h1>
      <div class="detail-meta">
        <span>{{ article.date }}</span>
        <el-tag size="small" effect="plain">{{ article.category }}</el-tag>
        <span>{{ article.readTime }}</span>
        <el-tag v-for="tag in article.tags" :key="tag" size="small" type="info" effect="plain">
          # {{ tag }}
        </el-tag>
      </div>
      <el-divider />
      <!-- html 类型直接渲染后端 HTML，markdown 类型渲染 marked 转换结果 -->
      <div class="article-content" v-html="renderedContent" />
      <el-divider />
      <router-link to="/">
        <el-button link type="primary">
          <el-icon><ArrowLeft /></el-icon>
          返回列表
        </el-button>
      </router-link>
    </div>

    <el-empty v-else-if="!loading" :description="notFound ? '文章不存在或已被删除' : '加载失败'" />
    <div v-else v-loading="loading" class="loading-holder" />
  </div>
</template>

<style scoped lang="scss">
.detail-title {
  margin: 0 0 12px;
  font-size: 26px;
  line-height: 1.4;
}

.detail-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
  color: #909399;
  font-size: 13px;
}

.loading-holder {
  min-height: 240px;
}
</style>
