# 认证接口文档

Base URL: `http://localhost:5000`
所有请求 `Content-Type: application/json`

---

## 目录

- [登录](#1-登录)
- [刷新 Token](#2-刷新-token)
- [登出](#3-登出)
- [鉴权说明](#鉴权说明)
- [错误格式](#错误格式)

---

## 1. 登录

**`POST /api/auth/login`**

验证用户名和密码，成功后返回 Access Token 和 Refresh Token。

### 请求体

```json
{
  "username": "superadmin",
  "password": "Admin@123"
}
```

| 字段       | 类型   | 必填 | 说明   |
| ---------- | ------ | ---- | ------ |
| `username` | string | ✅   | 用户名 |
| `password` | string | ✅   | 密码   |

### 响应

**200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedtoken...",
  "expiresAt": "2026-02-28T10:15:00Z"
}
```

| 字段           | 类型   | 说明                           |
| -------------- | ------ | ------------------------------ |
| `accessToken`  | string | JWT，有效期 15 分钟            |
| `refreshToken` | string | 不透明令牌，有效期 7 天        |
| `expiresAt`    | string | Access Token 过期时间（UTC ISO 8601） |

**401 Unauthorized**

```json
{ "message": "用户名或密码错误。" }
```

```json
{ "message": "账号已禁用。" }
```

---

## 2. 刷新 Token

**`POST /api/auth/refresh`**

用有效的 Refresh Token 换取新的双 Token，**旧 Refresh Token 同时撤销**（单次有效）。

### 请求体

```json
{
  "refreshToken": "base64encodedtoken..."
}
```

| 字段           | 类型   | 必填 | 说明               |
| -------------- | ------ | ---- | ------------------ |
| `refreshToken` | string | ✅   | 登录或上次刷新所得 |

### 响应

**200 OK**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "newbase64encodedtoken...",
  "expiresAt": "2026-02-28T10:30:00Z"
}
```

响应结构与登录相同，返回全新的双 Token。

**401 Unauthorized**

```json
{ "message": "Refresh Token 无效或已过期。" }
```

```json
{ "message": "用户不存在或已禁用。" }
```

---

## 3. 登出

**`POST /api/auth/logout`**

撤销 Refresh Token，使其立即失效。Access Token 在其自然过期前仍有效。

### 请求体

```json
{
  "refreshToken": "base64encodedtoken..."
}
```

| 字段           | 类型   | 必填 | 说明             |
| -------------- | ------ | ---- | ---------------- |
| `refreshToken` | string | ✅   | 当前持有的令牌   |

### 响应

**204 No Content**（无响应体，无论令牌是否存在均返回 204）

---

## 鉴权说明

除登录、刷新、登出外，所有其他接口均需在请求头中携带 Access Token：

```
Authorization: Bearer <accessToken>
```

### 前端推荐流程

```
1. POST /api/auth/login        → 存储 accessToken + refreshToken
2. 请求业务接口                 → Header: Authorization: Bearer <accessToken>
3. 收到 401                    → POST /api/auth/refresh（携带 refreshToken）
4. 刷新成功                    → 更新本地存储的双 Token，重试原请求
5. 刷新失败（401）              → 跳转登录页
6. 用户主动退出                → POST /api/auth/logout，清除本地存储
```

### Access Token 载荷（Payload）

解码后包含以下 Claims：

| Claim      | 说明                        |
| ---------- | --------------------------- |
| `sub`      | 用户 ID（字符串形式的雪花 ID） |
| `username` | 用户名                      |
| `nickname` | 昵称                        |
| `jti`      | Token 唯一标识              |
| `exp`      | 过期时间（Unix 时间戳）      |

---

## 错误格式

业务错误统一返回：

```json
{ "message": "错误描述" }
```

| HTTP 状态码 | 含义           |
| ----------- | -------------- |
| 200         | 成功           |
| 204         | 成功（无内容） |
| 400         | 请求参数错误   |
| 401         | 认证失败       |
