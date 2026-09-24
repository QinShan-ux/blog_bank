<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue'
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { fileUrlToDownloadUrl, getExportTask, startArticleExport } from '@/api/export'
import { getAccessToken } from '@/utils/auth'
import type { ExportTaskItem } from '@/types/api'

defineOptions({ name: 'AdminExport' })

const task = ref<ExportTaskItem | null>(null)
const running = ref(false)
const errorMessage = ref('')

const STATUS_TEXT: Record<number, string> = {
  0: '等待中',
  1: '处理中',
  2: '已完成',
  3: '失败',
}

const downloadUrl = computed(() =>
  task.value?.fileUrl ? fileUrlToDownloadUrl(task.value.fileUrl) : '',
)

let pollTimer: number | null = null
let connection: HubConnection | null = null

/** 发起导出：SignalR 接收完成推送 + 1.5s 轮询兜底 */
async function startExport() {
  errorMessage.value = ''
  running.value = true
  try {
    const { id } = await startArticleExport()
    task.value = await getExportTask(id)
    connectSignalR(id)
    startPolling(id)
  } catch {
    running.value = false
  }
}

function startPolling(id: string) {
  stopPolling()
  pollTimer = window.setInterval(async () => {
    try {
      const latest = await getExportTask(id)
      task.value = latest
      if (latest.status === 2 || latest.status === 3) {
        if (latest.status === 3) errorMessage.value = latest.errorMessage || '导出失败'
        stopAll()
      }
    } catch {
      // 单次轮询失败忽略，等待下一轮
    }
  }, 1500)
}

function stopPolling() {
  if (pollTimer !== null) {
    clearInterval(pollTimer)
    pollTimer = null
  }
}

async function connectSignalR(taskId: string) {
  try {
    connection = new HubConnectionBuilder()
      .withUrl(`/hubs/export?access_token=${encodeURIComponent(getAccessToken())}`)
      .configureLogging(LogLevel.Warning)
      .withAutomaticReconnect()
      .build()
    // 后端 ExportProcessor 推送：Clients.User(operatorId).SendAsync("ExportDone", new { fileUrl })
    connection.on('ExportDone', async () => {
      try {
        task.value = await getExportTask(taskId)
      } finally {
        stopAll()
      }
    })
    await connection.start()
  } catch {
    // SignalR 不可用时由轮询兜底
  }
}

function stopAll() {
  stopPolling()
  running.value = false
  connection?.stop().catch(() => {})
  connection = null
}

onBeforeUnmount(stopAll)
</script>

<template>
  <div class="page-card">
    <div class="table-toolbar">
      <span class="toolbar-title">导出中心</span>
      <el-button type="primary" :loading="running" @click="startExport">
        {{ running ? '导出中…' : '导出全部文章' }}
      </el-button>
    </div>

    <el-alert
      title="导出为 Excel（.xlsx），通过后台任务异步生成；完成后可在此下载。"
      type="info"
      :closable="false"
      show-icon
      class="tip"
    />

    <template v-if="task">
      <div class="task-panel">
        <div class="task-row">
          <span class="task-label">任务 ID</span>
          <span>{{ task.id }}</span>
        </div>
        <div class="task-row">
          <span class="task-label">状态</span>
          <el-tag
            :type="task.status === 2 ? 'success' : task.status === 3 ? 'danger' : 'warning'"
            size="small"
          >
            {{ STATUS_TEXT[task.status] ?? task.status }}
          </el-tag>
        </div>
        <div class="task-row">
          <span class="task-label">进度</span>
          <el-progress
            class="task-progress"
            :percentage="task.progress"
            :status="task.status === 3 ? 'exception' : task.status === 2 ? 'success' : undefined"
          />
        </div>
        <div v-if="task.completedAt" class="task-row">
          <span class="task-label">完成时间</span>
          <span>{{ task.completedAt }}</span>
        </div>
        <div v-if="errorMessage" class="task-row">
          <span class="task-label">错误</span>
          <span class="task-error">{{ errorMessage }}</span>
        </div>
        <div v-if="task.status === 2 && task.fileUrl" class="task-row">
          <span class="task-label">文件</span>
          <a :href="downloadUrl" :download="task.fileUrl" class="download-link">
            <el-button type="success" size="small">
              下载 {{ task.fileUrl }}
            </el-button>
          </a>
        </div>
      </div>
    </template>

    <el-empty v-else description="暂无导出任务，点击右上角按钮发起导出" :image-size="80" />
  </div>
</template>

<style scoped lang="scss">
.toolbar-title {
  font-size: 16px;
  font-weight: 600;
}

.tip {
  margin-bottom: 16px;
}

.task-panel {
  max-width: 560px;
}

.task-row {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 14px;
}

.task-label {
  width: 72px;
  color: #909399;
  font-size: 13px;
  flex-shrink: 0;
}

.task-progress {
  flex: 1;
}

.task-error {
  color: #f56c6c;
  font-size: 13px;
}

.download-link {
  text-decoration: none;
}
</style>
