# BlogBank 前端（web）

Vue 3 + TypeScript + Element Plus + Pinia + Vue Router 前端，覆盖 BlogBank.Api 全部接口：
博客前台（文章/随笔浏览）+ 管理后台（动态菜单、用户/角色/菜单/日志管理、导出中心）。

## 启动

```bash
npm install
npm run dev     # http://localhost:8848（后端 CORS 白名单指定端口）
npm run build   # 类型检查 + 打包
```

开发环境需同时启动后端：`dotnet run --project ../src/BlogBank.Api`（http profile，:5000）。
vite 已将 `/api`、`/hubs` 代理到 `http://localhost:5000`。

## 登录

- 前台浏览无需登录；`/admin` 需要登录（未登录自动跳转 `/login`）。
- 种子账号：`superadmin / Admin@123`（见 appsettings.Development.json `Seed` 配置）。

## 动态菜单说明

登录后路由守卫会拉取 `/api/menus/tree` + `/api/user-menus/user/{userId}`：

- 用户有菜单权限记录时，只渲染/注册被分配的菜单（按钮级权限走 `v-permission` 指令）。
- **数据库未配置任何菜单时，回退到前端内置菜单**（`src/router/dynamicRoutes.ts` 的
  `DEFAULT_MENU_TREE`），保证控制台开箱可用。菜单的 `component` 字段填
  `users/Index`、`articles/Index` 等相对 `src/views/admin/` 的路径即可映射到页面。

## 关键约定

- 雪花 ID 全程字符串（后端已配置 `NumberHandling.AllowReadingFromString`，写入侧可提交字符串数字）。
- 后端 `GET /api/articles` 不返回 total，分页用「满页即有下一页」策略。
- 401 时自动用 refreshToken 无感刷新并重放请求；刷新失败跳登录页。
- 导出中心：SignalR（`/hubs/export`，`ExportDone` 事件）+ 1.5s 轮询 `GET /api/export/task` 兜底；
  导出文件为后端静态文件，下载地址取自 `.env` 的 `VITE_API_ORIGIN`。
