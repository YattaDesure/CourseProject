<template>
  <div class="dashboard">
    <!-- Welcome Section -->
    <div class="welcome-section">
      <h1>Добро пожаловать, {{ userFullName }}! 👋</h1>
      <p class="welcome-subtitle">Система управления недвижимостью "Зеленый Квартал"</p>
    </div>

    <!-- Statistics Cards -->
    <div class="stats-grid" v-if="!loading">
      <div class="stat-card apartments">
        <div class="stat-icon">🏠</div>
        <div class="stat-content">
          <h3>{{ stats.apartments.total }}</h3>
          <p>Всего квартир</p>
          <div class="stat-details">
            <span class="badge badge-success">{{ stats.apartments.available }} свободно</span>
            <span class="badge badge-warning">{{ stats.apartments.occupied }} занято</span>
          </div>
        </div>
        <router-link to="/apartments" class="stat-link">Перейти →</router-link>
      </div>

      <div class="stat-card parking">
        <div class="stat-icon">🚗</div>
        <div class="stat-content">
          <h3>{{ stats.parking.total }}</h3>
          <p>Мест парковки</p>
          <div class="stat-details">
            <span class="badge badge-success">{{ stats.parking.available }} свободно</span>
            <span class="badge badge-warning">{{ stats.parking.occupied }} занято</span>
          </div>
        </div>
        <router-link to="/parking" class="stat-link">Перейти →</router-link>
      </div>

      <div class="stat-card storage">
        <div class="stat-icon">📦</div>
        <div class="stat-content">
          <h3>{{ stats.storage.total }}</h3>
          <p>Кладовых</p>
          <div class="stat-details">
            <span class="badge badge-success">{{ stats.storage.available }} свободно</span>
            <span class="badge badge-warning">{{ stats.storage.occupied }} занято</span>
          </div>
        </div>
        <router-link to="/storage" class="stat-link">Перейти →</router-link>
      </div>

      <div class="stat-card users" v-if="authStore.isAdmin">
        <div class="stat-icon">👥</div>
        <div class="stat-content">
          <h3>{{ stats.users.total }}</h3>
          <p>Пользователей</p>
          <div class="stat-details">
            <span class="badge badge-info">{{ stats.users.admins }} админов</span>
            <span class="badge badge-secondary">{{ stats.users.moderators }} модераторов</span>
          </div>
        </div>
        <router-link to="/users" class="stat-link">Перейти →</router-link>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions-section">
      <h2>Быстрые действия</h2>
      <div class="actions-grid">
        <router-link to="/apartments" class="action-card">
          <div class="action-icon">🏠</div>
          <h3>Квартиры</h3>
          <p>Просмотр и управление квартирами</p>
        </router-link>

        <router-link to="/parking" class="action-card">
          <div class="action-icon">🚗</div>
          <h3>Паркинг</h3>
          <p>Управление парковочными местами</p>
        </router-link>

        <router-link to="/storage" class="action-card">
          <div class="action-icon">📦</div>
          <h3>Кладовые</h3>
          <p>Управление кладовыми помещениями</p>
        </router-link>

        <router-link to="/account" class="action-card">
          <div class="action-icon">👤</div>
          <h3>Мой аккаунт</h3>
          <p>Личная информация и настройки</p>
        </router-link>

        <router-link v-if="authStore.isAdmin" to="/users" class="action-card">
          <div class="action-icon">👥</div>
          <h3>Пользователи</h3>
          <p>Управление пользователями системы</p>
        </router-link>
      </div>
    </div>

    <!-- User Info Card -->
    <div class="user-info-card">
      <h2>Информация о вашем аккаунте</h2>
      <div class="user-info-grid">
        <div class="info-item">
          <label>Роль</label>
          <p><span :class="getRoleBadgeClass(authStore.user?.role)">{{ getRoleText(authStore.user?.role) }}</span></p>
        </div>
        <div class="info-item">
          <label>Email</label>
          <p>{{ authStore.user?.email }}</p>
        </div>
        <div class="info-item">
          <label>Имя</label>
          <p>{{ userFullName }}</p>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Загрузка статистики...</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import api from '../services/api'

const authStore = useAuthStore()
const loading = ref(true)
const stats = ref({
  apartments: { total: 0, available: 0, occupied: 0 },
  parking: { total: 0, available: 0, occupied: 0 },
  storage: { total: 0, available: 0, occupied: 0 },
  users: { total: 0, admins: 0, moderators: 0 }
})

const userFullName = computed(() => {
  if (!authStore.user) return 'Пользователь'
  const parts = []
  if (authStore.user.firstName) parts.push(authStore.user.firstName)
  if (authStore.user.lastName) parts.push(authStore.user.lastName)
  return parts.length > 0 ? parts.join(' ') : authStore.user.email || 'Пользователь'
})

async function loadStats() {
  try {
    loading.value = true
    
    // Load apartments stats
    const apartmentsRes = await api.get('/api/apartments')
    const apartments = apartmentsRes.data || []
    stats.value.apartments.total = apartments.length
    stats.value.apartments.available = apartments.filter(a => a.status === 'Available').length
    stats.value.apartments.occupied = apartments.filter(a => a.status === 'Occupied').length

    // Load parking stats
    const parkingRes = await api.get('/api/parking')
    const parking = parkingRes.data || []
    stats.value.parking.total = parking.length
    stats.value.parking.available = parking.filter(p => p.status === 'Available').length
    stats.value.parking.occupied = parking.filter(p => p.status === 'Occupied').length

    // Load storage stats
    const storageRes = await api.get('/api/storagerooms')
    const storage = storageRes.data || []
    stats.value.storage.total = storage.length
    stats.value.storage.available = storage.filter(s => s.status === 'Available').length
    stats.value.storage.occupied = storage.filter(s => s.status === 'Occupied').length

    // Load users stats (only for admins)
    if (authStore.isAdmin) {
      try {
        const usersRes = await api.get('/api/users')
        const users = usersRes.data || []
        stats.value.users.total = users.length
        stats.value.users.admins = users.filter(u => u.role === 'Admin').length
        stats.value.users.moderators = users.filter(u => u.role === 'Moderator').length
      } catch (error) {
        console.error('Failed to load users stats:', error)
      }
    }
  } catch (error) {
    console.error('Failed to load stats:', error)
  } finally {
    loading.value = false
  }
}

function getRoleText(role) {
  if (role === 'Admin') return 'Администратор'
  if (role === 'Moderator') return 'Модератор'
  if (role === 'User') return 'Пользователь'
  return role || 'Пользователь'
}

function getRoleBadgeClass(role) {
  if (role === 'Admin') return 'badge badge-warning'
  if (role === 'Moderator') return 'badge badge-info'
  return 'badge badge-success'
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.dashboard {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.welcome-section {
  background: linear-gradient(135deg, var(--primary) 0%, #8bd8a0 100%);
  border-radius: 16px;
  padding: 48px 32px;
  margin-bottom: 32px;
  color: white;
  text-align: center;
}

.welcome-section h1 {
  font-size: 36px;
  margin-bottom: 8px;
  color: white;
}

.welcome-subtitle {
  font-size: 18px;
  opacity: 0.95;
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  margin-bottom: 48px;
}

.stat-card {
  background: var(--card);
  border-radius: 16px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
  transition: transform 0.2s, box-shadow 0.2s;
  border: 2px solid transparent;
}

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: var(--primary);
}

.stat-card.apartments::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #4CAF50, #8bd8a0);
}

.stat-card.parking::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #2196F3, #64b5f6);
}

.stat-card.storage::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #FF9800, #ffb74d);
}

.stat-card.users::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #9C27B0, #ba68c8);
}

.stat-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.stat-content h3 {
  font-size: 32px;
  margin: 0 0 8px 0;
  color: var(--text);
}

.stat-content > p {
  color: var(--text-muted);
  margin: 0 0 16px 0;
  font-size: 14px;
}

.stat-details {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.stat-link {
  margin-top: auto;
  color: var(--primary);
  text-decoration: none;
  font-weight: 600;
  font-size: 14px;
  transition: color 0.2s;
}

.stat-link:hover {
  color: var(--primary-dark);
}

.quick-actions-section {
  margin-bottom: 48px;
}

.quick-actions-section h2 {
  font-size: 24px;
  margin-bottom: 24px;
  color: var(--text);
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
}

.action-card {
  background: var(--card);
  border-radius: 12px;
  padding: 32px 24px;
  text-decoration: none;
  color: var(--text);
  text-align: center;
  transition: transform 0.2s, box-shadow 0.2s;
  border: 2px solid var(--border);
}

.action-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  border-color: var(--primary);
}

.action-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.action-card h3 {
  font-size: 20px;
  margin: 0 0 8px 0;
  color: var(--text);
}

.action-card p {
  color: var(--text-muted);
  margin: 0;
  font-size: 14px;
}

.user-info-card {
  background: var(--card);
  border-radius: 16px;
  padding: 32px;
  border: 2px solid var(--border);
}

.user-info-card h2 {
  font-size: 24px;
  margin-bottom: 24px;
  color: var(--text);
}

.user-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 24px;
}

.info-item label {
  display: block;
  font-size: 12px;
  color: var(--text-muted);
  margin-bottom: 8px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.info-item p {
  margin: 0;
  font-size: 16px;
  color: var(--text);
}

.loading-state {
  text-align: center;
  padding: 64px;
  color: var(--text-muted);
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid var(--border);
  border-top-color: var(--primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .dashboard {
    padding: 16px;
  }

  .welcome-section {
    padding: 32px 24px;
  }

  .welcome-section h1 {
    font-size: 28px;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .actions-grid {
    grid-template-columns: 1fr;
  }
}
</style>

