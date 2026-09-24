/**
 * 与后端 BlogBank.Api 响应/请求体一一对应的类型定义。
 * 注意：雪花 ID（含角色 ID）后端一律以字符串返回（防止 JS 精度丢失）。
 */

// ---------- 认证 ----------

/** POST /api/auth/login、/api/auth/refresh 成功响应 */
export interface LoginResult {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

// ---------- 用户 ----------

export interface UserRoleBrief {
  id: number
  code: string
  name: string
}

/** GET /api/users 列表项 */
export interface UserItem {
  id: string
  username: string
  nickname: string
  email: string
  avatar: string
  isEnabled: boolean
  createdAt: string
  updatedAt: string
  roles: UserRoleBrief[]
}

/** POST/PUT /api/users 请求体；更新时 password 留空表示不修改密码 */
export interface UserPayload {
  account: string
  nickname: string
  email: string
  password?: string
  avatar?: string
  isEnabled?: boolean
}

// ---------- 角色 ----------

export interface RoleItem {
  id: string
  code: string
  name: string
  description: string
  createdAt: string
}

export interface RolePayload {
  code: string
  name: string
  description?: string
}

// ---------- 菜单 ----------

/** 菜单类型：1 目录 / 2 菜单 / 3 按钮 */
export type MenuType = 1 | 2 | 3

export interface MenuItem {
  id: string
  pid: string
  type: MenuType
  /** 路由名称，同时需与页面组件 defineOptions 的 name 一致以支持 keep-alive */
  name: string
  path: string
  /** 组件路径，如 "users/Index"，对应 src/views/admin/users/Index.vue */
  component: string
  redirect: string
  /** 按钮权限标识，如 "sys:user:add" */
  permission: string
  title: string
  /** 图标名，格式 "ele-XXX"，对应 @element-plus/icons-vue 导出的组件名 */
  icon: string
  isIframe: boolean
  outLink: string
  isHide: boolean
  isKeepAlive: boolean
  isAffix: boolean
  orderNo: number
  /** 菜单状态：0 禁用 / 1 启用 */
  status: 0 | 1
  remark: string
  children?: MenuItem[]
}

/** GET /api/user-roles/user/{userId} 列表项 */
export interface UserRoleRef {
  userId: string
  roleId: number
  roleCode?: string
  roleName?: string
  assignedAt: string
}

/** GET /api/user-menus/user/{userId} 列表项 */
export interface UserMenuRef {
  userId: string
  menuId: string
  menuTitle: string
  menuType: MenuType
  assignedAt: string
}

// ---------- 文章 ----------

export interface ArticleItem {
  id: string
  title: string
  /** yyyy-MM-dd */
  date: string
  category: string
  readTime: string
  excerpt: string
  tags: string[]
  /** 正文，HTML 或 Markdown 字符串，由 contentType 决定 */
  content: string
  /** 正文内容类型："html"（默认）或 "markdown" */
  contentType?: string
}

export interface ArticlePayload {
  title: string
  date: string
  category: string
  readTime: string
  excerpt: string
  tags: string[]
  content: string
  /** 正文内容类型："html" 或 "markdown" */
  contentType?: string
}

// ---------- 随笔 ----------

export interface EssayItem {
  id: string
  date: string
  title: string
  mood: string
  moodIcon: string
  weather: string
  weatherIcon: string
  location: string
  tags: string[]
  excerpt: string
  /** 正文，纯文本 */
  content: string
  /** 卡片背景色，如 "#fef3c7" */
  bgColor: string
}

export interface EssayPayload {
  title: string
  date: string
  mood: string
  moodIcon: string
  weather: string
  weatherIcon: string
  location: string
  tags: string[]
  excerpt: string
  content: string
  bgColor: string
}

// ---------- 工作 Bug 记录 ----------

/** 处理状态：0=待处理 1=处理中 2=已解决 */
export type BugStatus = 0 | 1 | 2

/** 严重程度：0=低 1=中 2=高 */
export type BugSeverity = 0 | 1 | 2

export interface WorkBugItem {
  /** 雪花 ID，字符串形式 */
  id: string
  title: string
  description: string
  rootCause: string | null
  solution: string
  severity: BugSeverity
  status: BugStatus
  project: string
  environment: string | null
  occurredDate: string
  /** 解决日期，未解决为 null */
  solvedDate: string | null
  createdAt: string
  updatedAt: string
}

export interface WorkBugPayload {
  title: string
  description: string
  rootCause: string | null
  solution: string
  severity: BugSeverity
  status: BugStatus
  project: string
  environment: string | null
  occurredDate: string
  solvedDate: string | null
}

export interface WorkBugQuery {
  status?: BugStatus
  severity?: BugSeverity
  project?: string
  keyword?: string
}

// ---------- 审计日志 ----------

export interface AuditLogItem {
  /** 雪花 ID，字符串形式 */
  id: string
  traceId: string
  userId: string
  userName: string
  requestUrl: string
  ipAddress: string
  httpMethod: string
  tableName: string
  entityId: string
  action: string
  oldValues: string
  newValues: string
  operatedAt: string
}

export interface AuditLogQuery {
  page: number
  pageSize: number
  userId?: string
  action?: string
  tableName?: string
  startTime?: string
  endTime?: string
}

export interface PagedResult<T> {
  total: number
  page: number
  pageSize: number
  items: T[]
}

// ---------- 导出 ----------

/** 导出任务状态：0 Pending / 1 Processing / 2 Done / 3 Failed */
export type ExportTaskStatus = 0 | 1 | 2 | 3

export interface ExportTaskItem {
  /** 雪花 ID，字符串形式 */
  id: string
  status: ExportTaskStatus
  progress: number
  fileUrl: string
  errorMessage: string
  operatorId: string
  completedAt: string | null
}
