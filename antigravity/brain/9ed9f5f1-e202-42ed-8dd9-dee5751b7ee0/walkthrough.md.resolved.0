# 核心网关系统重构落地演示 (API Gateway Refactor)

根据你的需求，我们成功抛弃了基于简单纯 JSON 的脆弱单脚本，正式按照**双流量线**构建出了一个高可用的大型 API 代理网关。

## 🎯 业务目标对齐
*   **角色 A (业务与控制台)**：提供一个现代化的管理大盘。给用户展示充值情况、统计图表，并允许极速生成自己的 `sk-custom-xxxx` 路由密钥。
*   **角色 B (代理与计费引擎)**：极速接收用户传递进来的 `sk-custom` 令牌，拦截后查缓存校验余额，然后代理至官方 OpenAI 等下游并返回数据，并在流结束后神不知鬼不觉地精确扣减 Token 余额。

---

## 🎨 角色 A: 前端控制台 (Vue 3 + TailwindSPA)

控制台部署在 `web/` 目录，目前处于开发者模式。界面设计全面参照了商用中转平台：
*   **Landing Page**: 支持暗色系/亮色系一键切换的精美落地页。
*   **注册体系**: 全新搭建了 Login 组件并与后端 SQLite 鉴权打通。首个注册的用户**自动授予最高 Admin 权限**。
*   **Dashboard 面板**: 使用 ECharts 真实接管并展示 SQLite `usage_logs` 中的请求与消费数据。
*   **一建建 Key**: `Tokens.vue` 面板提供带遮罩的安全新建功能，直接将用户的 token 写进 SQLite 鉴权池。

## ⚙️ 角色 B: 极速转发大闸 (Express + in-Memory Cache)

我们对 `server.js` 做了深度的“分层降维”重构，拆分到了 `src/routes/` 结构下。最核心的代理通道现在位于 `src/routes/relay.js`：

1. **高速鉴权**: 实现了 `src/cache.js` 内存鉴权，无需单独配置系统级的 Redis。对于已经存在的 Key，只需毫秒级的 Map 读取操作即可放通，同时内置了精准限速。
2. **SSE 极速串流拦截**: 流式数据的截流不再阻滞用户的展示体验。它在持续通过管道向外 `res.write` 的同时，悄悄聚合 `usage`；当上游无情不提供统计时，自动下沉调用 `tiktoken` 原生估算。

### 🤖 新增测试功能：真机 Playground
为了让你和你的用户能测试新建到的 Key 能否用，在面板左侧菜单加入了 `测试与接入` 菜单。
它内置了一个调用我们自身后端 `/v1/chat/completions` 的极简原生聊天框，能够演示真正的 Server-Sent Events 流式打字机效果。

## 🚀 部署指南与后续

既然项目结构已经重整完成，你可以立刻打开前台网页感受全链路闭环，当前已在本地运行：
- **前端门户**：[http://localhost:5173/](http://localhost:5173/)
- **后端进程**：跑在 `3000` 端口上（前端已使用 Vite Proxy 将 `/api` 和 `/v1` 的流量转发了过去）。

> [!TIP]
> **真实服务器上线建议 (Nginx)**: 
> 当你将代码移上 Linux 时，为了极度契合你的构架（`www` 对应静态网页，`api` 对应后端接口）：
> 1. 执行 `cd web && npm run build` 把前端打包。
> 2. 将打包后的 `dist/` 交给 Nginx 配置 `www.yourdomain.com`。
> 3. 后端执行 `pm2 start src/server.js` 跑到 `3000`。
> 4. 最后让 Nginx 把 `api.yourdomain.com` 全部 proxy_pass 到 `localhost:3000` 即可完全合体交付！
