# 商业化计费与物理隔离实施流水线

- `[x]` **Phase 1: 底层财务系统建设 (Backend Database & API)**
  - `[x]` 修改 `src/db.js` 以增加 `redemptions` (兑换码) 表，并且在 `users` 表预留 `balance`。
  - `[x]` 开发 `src/routes/finance.js` 充值结算微服务：
    - `[x]` 面向管理员的 `/generate` (生成CDKey)
    - `[x]` 面向客户端的 `/redeem` (兑换金额充值)
  - `[x]` 将 `finance.js` 注册入 `src/server.js`。
  - `[x]` 升级 `src/routes/relay.js`：从直接计算 Token 改为**实时扣费**体系，如余额不足则 402 Payment Required 拦截。

- `[x]` **Phase 2: 网页工程级安全切片 (Vite MPA)**
  - `[x]` 备份并拆分根目录 `index.html` 为 `client.html`，创建新的 `admin.html`。
  - `[x]` 分拆 Vue 挂载入口，建立 `src/main.js` (Client 用) 与 `src/main-admin.js` (Admin 用)。
  - `[x]` 分离并剥离路由：`src/router/client.js` 与 `src/router/admin.js`。
  - `[x]` 修改 `vite.config.js`，开启 `build.rollupOptions` MPA 多入口编译特性。

- `[x]` **Phase 3: 客户端财务 UI 模块实现 (Client App)**
  - `[x]` 在客户端控制台侧边栏添加【经费充值与账单】菜单 `views/Finance/Recharge.vue`。
  - `[x]` 实现兑换卡密交互体系及当前余额 (Balance - 美元计价版) 获取展示逻辑。

- `[x]` **Phase 4: 服务器发卡 UI 模块实现 (Admin App)**
  - `[x]` 在 Admin 端左侧加装【礼品码发卡与审计】菜单 `views/Admin/Redemption.vue`。
  - `[x]` 实现前端批量生成 CD-Key 及查阅核销记录功能供站长淘宝发卡。

- `[ ]` **Phase 5: 服务可用性测试**
  - `[ ]` 全链路验证发卡、加款、调用接口扣次是否畅通无死锁。
