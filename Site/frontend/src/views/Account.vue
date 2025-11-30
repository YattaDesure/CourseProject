<template>
  <div class="page">
    <div class="page-header">
      <h2>Мой аккаунт</h2>
    </div>

    <div v-if="loading" style="text-align: center; padding: 48px;">
      Загрузка...
    </div>

    <div v-else-if="account" class="account-content">
      <div class="card" style="margin-bottom: 24px;">
        <h3 style="margin-bottom: 16px;">Личная информация</h3>
        <div class="info-grid">
          <div>
            <label>Имя</label>
            <p>{{ account.firstName }} {{ account.lastName }} {{ account.patronymic || '' }}</p>
          </div>
          <div>
            <label>Email</label>
            <p>{{ account.email }}</p>
          </div>
          <div>
            <label>Роль</label>
            <p><span :class="getRoleBadgeClass(account.role)">{{ getRoleText(account.role) }}</span></p>
          </div>
          <div>
            <label>Участник с</label>
            <p>{{ new Date(account.createdAt).toLocaleDateString('ru-RU') }}</p>
          </div>
        </div>
      </div>

      <div class="card" v-if="account.apartments && account.apartments.length > 0">
        <h3 style="margin-bottom: 16px;">Мои квартиры</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Номер</th>
              <th>Подъезд</th>
              <th>Этаж</th>
              <th>Площадь (м²)</th>
              <th>Статус</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="apt in account.apartments" :key="apt.id">
              <td>{{ apt.number }}</td>
              <td>{{ apt.entrance }}</td>
              <td>{{ apt.floor }}</td>
              <td>{{ parseFloat(apt.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(apt.status)">{{ getStatusText(apt.status) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card" v-if="account.parkingSpaces && account.parkingSpaces.length > 0">
        <h3 style="margin-bottom: 16px;">Мои парковочные места</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Номер места</th>
              <th>Площадь (м²)</th>
              <th>Статус</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ps in account.parkingSpaces" :key="ps.id">
              <td>{{ ps.slotNumber }}</td>
              <td>{{ parseFloat(ps.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(ps.status)">{{ getStatusText(ps.status) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card" v-if="account.storageRooms && account.storageRooms.length > 0">
        <h3 style="margin-bottom: 16px;">Мои кладовые</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Номер</th>
              <th>Площадь (м²)</th>
              <th>Статус</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sr in account.storageRooms" :key="sr.id">
              <td>{{ sr.label }}</td>
              <td>{{ parseFloat(sr.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(sr.status)">{{ getStatusText(sr.status) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="!account.apartments?.length && !account.parkingSpaces?.length && !account.storageRooms?.length" class="card">
        <p style="text-align: center; color: var(--text-muted); padding: 32px;">
          У вас пока нет назначенной недвижимости.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const account = ref(null)
const loading = ref(true)

async function loadAccount() {
  try {
    const response = await api.get('/api/account/me')
    account.value = response.data
    // Update auth store with role from account if different
    if (account.value && account.value.role && authStore.user) {
      authStore.user.role = account.value.role
      localStorage.setItem('user', JSON.stringify(authStore.user))
    }
  } catch (error) {
    console.error('Failed to load account:', error)
  } finally {
    loading.value = false
  }
}

function getStatusText(status) {
  if (status === 'Occupied') return 'Занята'
  if (status === 'Available') return 'Свободна'
  if (status === 'Vacant') return 'Свободна'
  return status
}

function getRoleText(role) {
  if (role === 'Admin') return 'Администратор'
  if (role === 'Moderator') return 'Модератор'
  if (role === 'User') return 'Пользователь'
  return role
}

function getStatusBadgeClass(status) {
  if (status === 'Occupied' || status === 'Available') return 'badge badge-success'
  if (status === 'Vacant') return 'badge badge-info'
  return 'badge badge-warning'
}

function getRoleBadgeClass(role) {
  if (role === 'Admin') return 'badge badge-warning'
  if (role === 'Moderator') return 'badge badge-info'
  return 'badge badge-success'
}

onMounted(() => {
  loadAccount()
})
</script>

<style scoped>
.page-header {
  margin-bottom: 24px;
}

.page-header h2 {
  color: var(--text);
  font-size: 24px;
}

.account-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 24px;
}

.info-grid label {
  display: block;
  font-weight: 600;
  color: var(--text-muted);
  font-size: 12px;
  text-transform: uppercase;
  margin-bottom: 8px;
}

.info-grid p {
  margin: 0;
  color: var(--text);
  font-size: 16px;
}
</style>
