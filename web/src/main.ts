import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import 'element-plus/dist/index.css'
import App from './App.vue'
import router from './router'
import { setupPermission } from './directives/permission'
import '@/styles/index.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(ElementPlus, { locale: zhCn })

// 全局注册 Element Plus 图标，配合菜单 icon 字段 "ele-XXX" 动态渲染
for (const [name, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(name, component)
}

setupPermission(app)

app.mount('#app')
