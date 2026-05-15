<template>
  <div class="layout">
    <header class="header">
      <div class="header-content">
        <router-link to="/" class="logo-link" @click="closeNav">
          <h1 class="logo">🏠 Зеленый Квартал</h1>
        </router-link>

        <button
          type="button"
          class="nav-toggle"
          :aria-expanded="navOpen"
          aria-controls="main-nav"
          @click="navOpen = !navOpen"
        >
          <span class="nav-toggle-bar" />
          <span class="nav-toggle-bar" />
          <span class="nav-toggle-bar" />
          <span class="sr-only">Меню</span>
        </button>

        <nav id="main-nav" class="nav" :class="{ open: navOpen }">
          <router-link to="/" class="nav-link" @click="closeNav">Главная</router-link>
          <router-link to="/apartments" class="nav-link" @click="closeNav">Квартиры</router-link>
          <router-link to="/parking" class="nav-link" @click="closeNav">Парковка</router-link>
          <router-link to="/storage" class="nav-link" @click="closeNav">Кладовые</router-link>
          <router-link
            v-if="authStore.isAdmin"
            to="/electric-readings"
            class="nav-link nav-link-badge"
            @click="closeNav"
          >
            Показания
            <span
              v-if="readingBadgeCount > 0"
              class="nav-badge"
              :title="`Не внесены показания: ${readingBadgeCount}`"
            >
              {{ readingBadgeCount }}
            </span>
          </router-link>
          <router-link v-if="authStore.isAdmin" to="/receipts-admin" class="nav-link" @click="closeNav">
            Начисления
          </router-link>
          <router-link v-if="authStore.isAdmin" to="/users" class="nav-link" @click="closeNav">
            Пользователи
          </router-link>
          <router-link to="/account" class="nav-link" @click="closeNav">Мой аккаунт</router-link>
          <button type="button" class="btn btn-secondary nav-logout" @click="handleLogout">Выйти</button>
        </nav>
      </div>
    </header>

    <div v-if="navOpen" class="nav-backdrop" @click="closeNav" />

    <main class="main">
      <router-view />
    </main>

    <footer class="footer">
      <div class="footer-content">
        <div class="footer-section">
          <h3>Зеленый Квартал</h3>
          <p>Система управления недвижимостью</p>
          <p>Жилой комплекс «Зеленый Квартал»</p>
        </div>
        <div class="footer-section">
          <h4>Контакты</h4>
          <p>📞 Телефон: +7 (900) 920-21-28</p>
          <p>📧 Email: info@greenquarter.ru</p>
          <p>📍 Адрес: г. Архангельск, ул. Вологодская, д. 30</p>
        </div>
        <div class="footer-section">
          <h4>Управляющая компания</h4>
          <p>ТСН «Зеленый Квартал»</p>
          <p>Режим работы: Пн–Пт 9:00–17:00</p>
          <p>Экстренная служба: +7 (495) 123-45-68</p>
        </div>
      </div>
      <div class="footer-bottom">
        <p>&copy; 2025 Green Quarter. Все права защищены.</p>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'
import { useRouter, useRoute } from 'vue-router'
import { onMounted, ref, watch } from 'vue'
import api from '../services/api'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()
const readingBadgeCount = ref(0)
const navOpen = ref(false)

function closeNav() {
  navOpen.value = false
}

function handleLogout() {
  closeNav()
  authStore.logout()
  router.push('/login')
}

function monthValue(date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

/** Счётчик квартир без показаний счётчиков (с 15-го числа месяца). */
async function loadReadingBadge() {
  try {
    if (!authStore.isAdmin) {
      readingBadgeCount.value = 0
      return
    }
    const today = new Date()
    if (today.getDate() < 15) {
      readingBadgeCount.value = 0
      return
    }
    const res = await api.get('/api/electric-meters/missing-count', {
      params: { readingMonth: monthValue(today) }
    })
    readingBadgeCount.value = res.data?.missingTotal || 0
  } catch {
    readingBadgeCount.value = 0
  }
}

watch(
  () => route.path,
  () => closeNav()
)

onMounted(() => {
  loadReadingBadge()
})
</script>

<style scoped>
.layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  background: var(--card);
  border-bottom: 1px solid var(--border);
  padding: 0 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  position: relative;
  z-index: 100;
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  min-height: 64px;
  gap: 12px;
}

.logo-link {
  text-decoration: none;
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.logo {
  font-size: 18px;
  color: var(--primary);
  font-weight: 700;
  margin: 0;
  transition: opacity 0.2s;
}

.logo-link:hover .logo {
  opacity: 0.8;
}

.nav-toggle {
  display: none;
  flex-direction: column;
  justify-content: center;
  gap: 5px;
  width: 44px;
  height: 44px;
  padding: 10px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--card);
  cursor: pointer;
}

.nav-toggle-bar {
  display: block;
  height: 2px;
  width: 100%;
  background: var(--text);
  border-radius: 1px;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  border: 0;
}

.nav {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.nav-link {
  color: var(--text);
  text-decoration: none;
  padding: 8px 10px;
  border-radius: 6px;
  transition: all 0.2s;
  font-weight: 500;
  font-size: 14px;
  white-space: nowrap;
}

.nav-link:hover {
  background: var(--bg);
  color: var(--primary);
}

.nav-link.router-link-active {
  background: rgba(15, 157, 88, 0.1);
  color: var(--primary);
}

.nav-link-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.nav-badge {
  min-width: 18px;
  height: 18px;
  padding: 0 6px;
  border-radius: 999px;
  background: #d93025;
  color: white;
  font-size: 11px;
  font-weight: 700;
  line-height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.nav-logout {
  margin-left: 4px;
}

.nav-backdrop {
  display: none;
}

.main {
  flex: 1;
  padding: 24px 16px;
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
}

.footer {
  background: var(--card);
  border-top: 1px solid var(--border);
  margin-top: auto;
  padding: 32px 16px 16px;
}

.footer-content {
  max-width: 1400px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 24px;
  margin-bottom: 20px;
}

.footer-section h3 {
  color: var(--primary);
  font-size: 18px;
  margin-bottom: 10px;
}

.footer-section h4 {
  color: var(--text);
  font-size: 15px;
  margin-bottom: 10px;
  font-weight: 600;
}

.footer-section p {
  color: var(--text-muted);
  font-size: 13px;
  margin: 6px 0;
  line-height: 1.6;
}

.footer-bottom {
  max-width: 1400px;
  margin: 0 auto;
  padding-top: 16px;
  border-top: 1px solid var(--border);
  text-align: center;
}

.footer-bottom p {
  color: var(--text-muted);
  font-size: 12px;
  margin: 0;
}

@media (max-width: 900px) {
  .nav-toggle {
    display: flex;
  }

  .nav-backdrop {
    display: block;
    position: fixed;
    inset: 64px 0 0;
    background: rgba(16, 42, 23, 0.35);
    z-index: 90;
  }

  .nav {
    display: none;
    position: fixed;
    top: 64px;
    right: 0;
    left: 0;
    flex-direction: column;
    align-items: stretch;
    gap: 0;
    background: var(--card);
    border-bottom: 1px solid var(--border);
    padding: 8px 12px 16px;
    max-height: calc(100vh - 64px);
    overflow-y: auto;
    z-index: 95;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  }

  .nav.open {
    display: flex;
  }

  .nav-link {
    padding: 12px 14px;
    border-radius: 8px;
  }

  .nav-logout {
    margin: 8px 0 0;
    width: 100%;
  }

  .logo {
    font-size: 16px;
  }

  .main {
    padding: 16px 12px;
  }
}
</style>
