# Blog API 接口文档

> Base URL: `/api`
> 数据格式：`application/json`
> 接口请求失败时，前端自动降级使用本地静态数据。

---

## 目录

- [文章接口](#文章接口)
  - [获取文章列表](#获取文章列表)
- [随笔接口](#随笔接口)
  - [获取随笔列表](#获取随笔列表)
- [数据结构](#数据结构)
  - [Article](#article)
  - [EssayItem](#essayitem)
- [错误响应](#错误响应)

---

## 文章接口

### 获取文章列表

获取所有博客文章，按发布时间倒序排列。

```
GET /api/articles
```

**请求参数**

无

**响应示例**

```json
[
  {
    "id": "css-grid",
    "title": "用 CSS Grid 打造响应式布局的最佳实践",
    "date": "2026-02-10",
    "category": "前端开发",
    "readTime": "5 分钟阅读",
    "excerpt": "CSS Grid 是现代前端布局的核心技术之一……",
    "tags": ["CSS", "布局", "响应式"],
    "content": "<h2>为什么选择 CSS Grid？</h2><p>……</p>"
  },
  {
    "id": "js-async",
    "title": "JavaScript 异步编程：从回调到 async/await",
    "date": "2026-02-05",
    "category": "JavaScript",
    "readTime": "8 分钟阅读",
    "excerpt": "异步编程是 JavaScript 的核心概念……",
    "tags": ["JavaScript", "异步"],
    "content": "<h2>为什么 JavaScript 需要异步？</h2><p>……</p>"
  }
]
```

**响应状态码**

| 状态码 | 说明 |
| --- | --- |
| `200` | 成功，返回 `Article[]` |
| `4xx / 5xx` | 请求失败，前端降级使用本地静态数据 |

---

## 随笔接口

### 获取随笔列表

获取所有随笔，按发布时间倒序排列。

```
GET /api/essays
```

**请求参数**

无

**响应示例**

```json
[
  {
    "id": 1,
    "title": "春日午后的咖啡馆",
    "date": "2026-02-18",
    "mood": "惬意",
    "moodIcon": "😌",
    "weather": "晴",
    "weatherIcon": "☀️",
    "location": "街角咖啡馆",
    "tags": ["生活", "咖啡", "阅读"],
    "excerpt": "阳光透过落地窗洒在书页上，咖啡的香气弥漫在空气中，时间仿佛慢了下来。",
    "content": "阳光透过落地窗洒在书页上……",
    "bgColor": "#fef3c7"
  }
]
```

**响应状态码**

| 状态码 | 说明 |
| --- | --- |
| `200` | 成功，返回 `EssayItem[]` |
| `4xx / 5xx` | 请求失败，前端降级使用本地静态数据 |

---

## 数据结构

### Article

博客文章对象。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `id` | `string` | 是 | 文章唯一标识，用于路由 `/post/:id` |
| `title` | `string` | 是 | 文章标题 |
| `date` | `string` | 是 | 发布日期，格式 `YYYY-MM-DD` |
| `category` | `string` | 是 | 所属分类，如 `前端开发`、`JavaScript`、`DevOps`、`随笔` |
| `readTime` | `string` | 是 | 预计阅读时长，如 `5 分钟阅读` |
| `excerpt` | `string` | 是 | 文章摘要，用于列表页展示 |
| `tags` | `string[]` | 是 | 标签列表，用于筛选过滤 |
| `content` | `string` | 是 | 文章正文，HTML 格式字符串 |

**TypeScript 定义**

```typescript
interface Article {
  id: string;
  title: string;
  date: string;
  category: string;
  readTime: string;
  excerpt: string;
  tags: string[];
  content: string;
}
```

---

### EssayItem

随笔对象。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `id` | `number` | 是 | 随笔唯一标识 |
| `title` | `string` | 是 | 随笔标题 |
| `date` | `string` | 是 | 发布日期，格式 `YYYY-MM-DD` |
| `mood` | `string` | 是 | 心情文字描述，如 `惬意`、`专注` |
| `moodIcon` | `string` | 是 | 心情 Emoji 图标 |
| `weather` | `string` | 是 | 天气文字描述，如 `晴`、`多云` |
| `weatherIcon` | `string` | 是 | 天气 Emoji 图标 |
| `location` | `string` | 是 | 写作地点 |
| `tags` | `string[]` | 是 | 标签列表，用于筛选过滤 |
| `excerpt` | `string` | 是 | 随笔摘要，用于列表页折叠展示 |
| `content` | `string` | 是 | 随笔正文，纯文本格式 |
| `bgColor` | `string` | 是 | 卡片背景色，十六进制颜色值，如 `#fef3c7` |

**TypeScript 定义**

```typescript
interface EssayItem {
  id: number;
  title: string;
  date: string;
  mood: string;
  moodIcon: string;
  weather: string;
  weatherIcon: string;
  location: string;
  tags: string[];
  excerpt: string;
  content: string;
  bgColor: string;
}
```

---

## 错误响应

所有接口在请求失败时，前端通过 `catchError` 自动降级至本地静态数据，页面不会出现空白或报错。

后端建议在非 `2xx` 状态码时返回统一错误结构（仅供参考）：

```json
{
  "code": 500,
  "message": "Internal Server Error"
}
```
