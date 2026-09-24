<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { deleteAuditLog, getAuditLogs } from '@/api/auditLogs'
import { formatDateTime } from '@/utils/format'
import type { AuditLogItem } from '@/types/api'

defineOptions({ name: 'AdminAuditLogs' })

const items = ref<AuditLogItem[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)

const filters = reactive({
  userId: '',
  action: '',
  tableName: '',
  range: null as [string, string] | null,
})

async function load() {
  loading.value = true
  try {
    const res = await getAuditLogs({
      page: page.value,
      pageSize: pageSize.value,
      userId: filters.userId.trim(),
      action: filters.action.trim(),
      tableName: filters.tableName.trim(),
      startTime: filters.range?.[0],
      endTime: filters.range?.[1],
    })
    items.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function search() {
  page.value = 1
  load()
}

function reset() {
  filters.userId = ''
  filters.action = ''
  filters.tableName = ''
  filters.range = null
  search()
}

// ---------- 详情 ----------

const detailVisible = ref(false)
const current = ref<AuditLogItem | null>(null)

function prettyJson(raw: string): string {
  if (!raw) return '-'
  try {
    return JSON.stringify(JSON.parse(raw), null, 2)
  } catch {
    return raw
  }
}

function viewDetail(row: AuditLogItem) {
  current.value = row
  detailVisible.value = true
}

async function handleDelete(row: AuditLogItem) {
  try {
    await ElMessageBox.confirm(`确定删除该条日志（ID: ${row.id}）吗？`, '警告', { type: 'warning' })
  } catch {
    return
  }
  await deleteAuditLog(row.id)
  ElMessage.success('删除成功')
  // 当前页删空时回退一页
  if (items.value.length === 1 && page.value > 1) page.value--
  load()
}

onMounted(load)
</script>

<template>
  <div class="page-card">
    <el-form inline class="filter-bar" @submit.prevent>
      <el-form-item label="操作人ID">
        <el-input v-model="filters.userId" placeholder="userId" clearable style="width: 160px" />
      </el-form-item>
      <el-form-item label="操作类型">
        <el-input v-model="filters.action" placeholder="如 新增/修改/删除" clearable style="width: 140px" />
      </el-form-item>
      <el-form-item label="表名">
        <el-input v-model="filters.tableName" placeholder="如 Users" clearable style="width: 140px" />
      </el-form-item>
      <el-form-item label="时间范围">
        <el-date-picker
          v-model="filters.range"
          type="datetimerange"
          value-format="YYYY-MM-DDTHH:mm:ss"
          :default-time="[new Date(2000, 0, 1, 0, 0, 0), new Date(2000, 0, 1, 23, 59, 59)]"
          start-placeholder="开始时间"
          end-placeholder="结束时间"
          style="width: 340px"
        />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="search">查询</el-button>
        <el-button @click="reset">重置</el-button>
      </el-form-item>
    </el-form>

    <el-table :data="items" v-loading="loading" stripe>
      <el-table-column prop="id" label="ID" width="70" />
      <el-table-column prop="userName" label="操作人" width="110" show-overflow-tooltip />
      <el-table-column prop="action" label="操作" width="80" />
      <el-table-column prop="tableName" label="表名" width="110" show-overflow-tooltip />
      <el-table-column prop="entityId" label="数据ID" width="170" show-overflow-tooltip />
      <el-table-column prop="httpMethod" label="方法" width="80" />
      <el-table-column prop="requestUrl" label="请求地址" min-width="200" show-overflow-tooltip />
      <el-table-column prop="ipAddress" label="IP" width="130" show-overflow-tooltip />
      <el-table-column label="操作时间" width="170">
        <template #default="{ row }">{{ formatDateTime(row.operatedAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="130" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="viewDetail(row)">详情</el-button>
          <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-model:current-page="page"
      v-model:page-size="pageSize"
      :total="total"
      :page-sizes="[10, 20, 50, 100]"
      layout="total, sizes, prev, pager, next"
      class="pager"
      @current-change="load"
      @size-change="search"
    />

    <el-drawer v-model="detailVisible" title="日志详情" size="42%">
      <template v-if="current">
        <el-descriptions :column="1" border>
          <el-descriptions-item label="ID">{{ current.id }}</el-descriptions-item>
          <el-descriptions-item label="TraceId">{{ current.traceId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="操作人">{{ current.userName }}（{{ current.userId }}）</el-descriptions-item>
          <el-descriptions-item label="操作">{{ current.action }} · {{ current.tableName }}（{{ current.entityId }}）</el-descriptions-item>
          <el-descriptions-item label="请求">{{ current.httpMethod }} {{ current.requestUrl }}</el-descriptions-item>
          <el-descriptions-item label="IP">{{ current.ipAddress || '-' }}</el-descriptions-item>
          <el-descriptions-item label="时间">{{ formatDateTime(current.operatedAt) }}</el-descriptions-item>
        </el-descriptions>

        <h4 class="json-title">变更前（OldValues）</h4>
        <pre class="json-block">{{ prettyJson(current.oldValues) }}</pre>

        <h4 class="json-title">变更后（NewValues）</h4>
        <pre class="json-block">{{ prettyJson(current.newValues) }}</pre>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped lang="scss">
.filter-bar {
  margin-bottom: 4px;
}

.pager {
  margin-top: 14px;
  justify-content: flex-end;
}

.json-title {
  margin: 18px 0 8px;
}

.json-block {
  background: #282c34;
  color: #abb2bf;
  padding: 14px;
  border-radius: 8px;
  font-size: 12.5px;
  line-height: 1.6;
  max-height: 260px;
  overflow: auto;
  margin: 0;
}
</style>
