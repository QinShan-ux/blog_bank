<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getEssays } from '@/api/essays'
import type { EssayItem } from '@/types/api'

defineOptions({ name: 'FrontEssays' })

const essays = ref<EssayItem[]>([])
const expanded = ref<Set<string>>(new Set())
const loading = ref(true)

async function load() {
  try {
    essays.value = await getEssays()
  } finally {
    loading.value = false
  }
}

function toggle(id: string) {
  const next = new Set(expanded.value)
  if (next.has(id)) {
    next.delete(id)
  } else {
    next.add(id)
  }
  expanded.value = next
}

onMounted(load)
</script>

<template>
  <div class="essays-page">
    <h2 class="essays-heading">随笔 · 生活碎片</h2>

    <template v-if="essays.length > 0">
      <el-timeline class="essays-timeline">
        <el-timeline-item
          v-for="item in essays"
          :key="item.id"
          :timestamp="item.date"
          placement="top"
          type="primary"
        >
          <div
            class="essay-card"
            :style="{ background: item.bgColor || '#fff' }"
          >
            <div class="essay-head">
              <h3 class="essay-title">{{ item.title }}</h3>
              <div class="essay-emoji">
                <span :title="item.mood">{{ item.moodIcon }}</span>
                <span :title="item.weather">{{ item.weatherIcon }}</span>
              </div>
            </div>

            <div class="essay-info">
              <span v-if="item.mood">心情：{{ item.mood }}</span>
              <span v-if="item.weather">天气：{{ item.weather }}</span>
              <span v-if="item.location">📍 {{ item.location }}</span>
            </div>

            <div class="essay-tags">
              <el-tag
                v-for="tag in item.tags"
                :key="tag"
                size="small"
                effect="plain"
                color="rgba(255,255,255,0.6)"
              >
                # {{ tag }}
              </el-tag>
            </div>

            <p class="essay-excerpt">{{ item.excerpt }}</p>

            <div v-if="expanded.has(item.id)" class="essay-content">{{ item.content }}</div>

            <el-button link type="primary" class="essay-toggle" @click="toggle(item.id)">
              {{ expanded.has(item.id) ? '收起' : '展开全文' }}
            </el-button>
          </div>
        </el-timeline-item>
      </el-timeline>
    </template>

    <el-empty v-else-if="!loading" description="暂无随笔" />
  </div>
</template>

<style scoped lang="scss">
.essays-heading {
  margin: 0 0 24px;
  font-size: 22px;
}

.essays-timeline {
  padding-left: 4px;
}

.essay-card {
  border-radius: 10px;
  padding: 18px 20px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}

.essay-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.essay-title {
  margin: 0;
  font-size: 17px;
}

.essay-emoji {
  font-size: 18px;
  display: flex;
  gap: 8px;
}

.essay-info {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  margin-top: 8px;
  color: #5a6b7b;
  font-size: 13px;
}

.essay-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 10px;
}

.essay-excerpt {
  margin: 12px 0 8px;
  color: #44505c;
  line-height: 1.7;
}

.essay-content {
  margin-top: 4px;
  padding-top: 12px;
  border-top: 1px dashed rgba(0, 0, 0, 0.12);
  white-space: pre-wrap;
  color: #33404c;
  line-height: 1.8;
}
</style>
