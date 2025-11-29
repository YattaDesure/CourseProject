<template>
  <div class="layout">
    <header class="header">
      <div class="header-content">
        <h1 class="logo">🏠 Green Quarter</h1>
        <nav class="nav">
          <router-link to="/apartments" class="nav-link">Apartments</router-link>
          <router-link to="/parking" class="nav-link">Parking</router-link>
          <router-link to="/storage" class="nav-link">Storage</router-link>
          <router-link v-if="authStore.isAdmin" to="/users" class="nav-link">Users</router-link>
          <router-link to="/account" class="nav-link">My Account</router-link>
          <button @click="handleLogout" class="btn btn-secondary">Logout</button>
        </nav>
      </div>
    </header>
    <main class="main">
      <router-view />
    </main>
    <footer class="footer">
      <div class="footer-content">
        <div class="footer-section">
          <h3>Green Quarter</h3>
          <p>Property Management System</p>
          <p>Жилой комплекс "Зеленый Квартал"</p>
        </div>
        <div class="footer-section">
          <h4>Контакты</h4>
          <p>📞 Телефон: +7 (495) 123-45-67</p>
          <p>📧 Email: info@greenquarter.ru</p>
          <p>📍 Адрес: г. Москва, ул. Зеленая, д. 1</p>
        </div>
        <div class="footer-section">
          <h4>Управляющая компания</h4>
          <p>ООО "УК Зеленый Квартал"</p>
          <p>Режим работы: Пн-Пт 9:00 - 18:00</p>
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
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
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
  padding: 0 24px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.header-content {
  max-width: 1400px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
}

.logo {
  font-size: 20px;
  color: var(--primary);
  font-weight: 700;
}

.nav {
  display: flex;
  gap: 16px;
  align-items: center;
}

.nav-link {
  color: var(--text);
  text-decoration: none;
  padding: 8px 12px;
  border-radius: 6px;
  transition: all 0.2s;
  font-weight: 500;
}

.nav-link:hover {
  background: var(--bg);
  color: var(--primary);
}

.nav-link.router-link-active {
  background: rgba(15, 157, 88, 0.1);
  color: var(--primary);
}

.main {
  flex: 1;
  padding: 32px 24px;
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
}

.footer {
  background: var(--card);
  border-top: 1px solid var(--border);
  margin-top: auto;
  padding: 40px 24px 20px;
}

.footer-content {
  max-width: 1400px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 32px;
  margin-bottom: 24px;
}

.footer-section h3 {
  color: var(--primary);
  font-size: 20px;
  margin-bottom: 12px;
}

.footer-section h4 {
  color: var(--text);
  font-size: 16px;
  margin-bottom: 12px;
  font-weight: 600;
}

.footer-section p {
  color: var(--text-muted);
  font-size: 14px;
  margin: 8px 0;
  line-height: 1.6;
}

.footer-bottom {
  max-width: 1400px;
  margin: 0 auto;
  padding-top: 24px;
  border-top: 1px solid var(--border);
  text-align: center;
}

.footer-bottom p {
  color: var(--text-muted);
  font-size: 12px;
  margin: 0;
}
</style>

