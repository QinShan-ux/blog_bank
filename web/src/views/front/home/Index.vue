<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getArticles } from '@/api/articles'
import type { ArticleItem } from '@/types/api'

defineOptions({ name: 'FrontHome' })

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

onMounted(load)
</script>

<template>
  <div class="home-page">
    <template v-if="list.length > 0">
      <article v-for="item in list" :key="item.id" class="article-card">
        <h2 class="article-title">
          <router-link :to="`/article/${item.id}`">{{ item.title }}</router-link>
        </h2>
        <div class="article-meta">
          <span>{{ item.date }}</span>
          <el-tag size="small" effect="plain">{{ item.category }}</el-tag>
          <span>{{ item.readTime }}</span>
        </div>
        <p class="article-excerpt">{{ item.excerpt }}</p>
        <div class="article-footer">
          <div class="article-tags">
            <el-tag v-for="tag in item.tags" :key="tag" size="small" type="info" effect="plain">
              # {{ tag }}
            </el-tag>
          </div>
          <router-link :to="`/article/${item.id}`">
            <el-button link type="primary">
              阅读全文
              <el-icon><ArrowRight /></el-icon>
            </el-button>
          </router-link>
        </div>
      </article>

      <div class="pager">
        <el-button :disabled="page <= 1 || loading" @click="prev">上一页</el-button>
        <span class="pager-info">第 {{ page }} 页</span>
        <el-button :disabled="!hasNext || loading" @click="next">下一页</el-button>
      </div>
    </template>

    <el-empty v-else-if="!loading" description="暂无文章" />
    <div v-else v-loading="loading" class="loading-holder" />
  </div>
</template>

<style scoped lang="scss">
.article-card {
  background: #fff;
  border-radius: 10px;
  padding: 24px 28px;
  margin-bottom: 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);
  transition: box-shadow 0.2s;

  &:hover {
    box-shadow: 0 4px 14px rgba(0, 0, 0, 0.1);
  }
}

.article-title {
  margin: 0 0 10px;
  font-size: 20px;

  a {
    color: #303133;
    text-decoration: none;

    &:hover {
      color: #409eff;
    }
  }
}

.article-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #909399;
  font-size: 13px;
  margin-bottom: 12px;
}

.article-excerpt {
  color: #606266;
  margin: 0 0 16px;
  line-height: 1.7;
}

.article-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.article-tags {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 24px;
}

.pager-info {
  color: #909399;
  font-size: 14px;
}

.loading-holder {
  min-height: 200px;
}
</style>
