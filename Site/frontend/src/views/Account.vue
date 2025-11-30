<template>
  <div class="page">
    <div class="page-header">
      <h2>Мой аккаунт</h2>
      <div class="tabs">
        <button 
          @click="activeTab = 'info'" 
          :class="['tab', { active: activeTab === 'info' }]"
        >
          Информация
        </button>
        <button 
          @click="activeTab = 'settings'" 
          :class="['tab', { active: activeTab === 'settings' }]"
        >
          Настройки
        </button>
      </div>
    </div>

    <div v-if="loading" style="text-align: center; padding: 48px;">
      Загрузка...
    </div>

    <!-- Вкладка "Информация" -->
    <div v-else-if="account && activeTab === 'info'" class="account-content">
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

    <!-- Вкладка "Настройки" -->
    <div v-else-if="account && activeTab === 'settings'" class="settings-content">
      <!-- Редактирование личных данных -->
      <div class="card" style="margin-bottom: 24px;">
        <h3 style="margin-bottom: 16px;">Личные данные</h3>
        <form @submit.prevent="updateProfile" class="form">
          <div class="form-row">
            <div class="form-group">
              <label>Имя *</label>
              <input v-model="profileForm.firstName" type="text" class="input" required />
            </div>
            <div class="form-group">
              <label>Фамилия *</label>
              <input v-model="profileForm.lastName" type="text" class="input" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Отчество</label>
              <input v-model="profileForm.patronymic" type="text" class="input" />
            </div>
            <div class="form-group">
              <label>Телефон</label>
              <input v-model="profileForm.phone" type="tel" class="input" placeholder="+7 (900) 123-45-67" />
            </div>
          </div>
          <div class="form-group">
            <label>Email *</label>
            <input v-model="profileForm.email" type="email" class="input" required />
          </div>
          <div v-if="profileError" class="error-message">{{ profileError }}</div>
          <div v-if="profileSuccess" class="success-message">{{ profileSuccess }}</div>
          <button type="submit" class="btn btn-primary" :disabled="profileLoading">
            {{ profileLoading ? 'Сохранение...' : 'Сохранить изменения' }}
          </button>
        </form>
      </div>

      <!-- Смена пароля -->
      <div class="card">
        <h3 style="margin-bottom: 16px;">Смена пароля</h3>
        <form @submit.prevent="changePassword" class="form">
          <div class="form-group">
            <label>Текущий пароль *</label>
            <input v-model="passwordForm.oldPassword" type="password" class="input" required />
          </div>
          <div class="form-group">
            <label>Новый пароль *</label>
            <input v-model="passwordForm.newPassword" type="password" class="input" required minlength="6" />
            <small style="color: var(--text-muted); font-size: 12px;">Минимум 6 символов</small>
          </div>
          <div class="form-group">
            <label>Подтвердите новый пароль *</label>
            <input v-model="passwordForm.confirmPassword" type="password" class="input" required minlength="6" />
          </div>
          <div v-if="passwordError" class="error-message">{{ passwordError }}</div>
          <div v-if="passwordSuccess" class="success-message">{{ passwordSuccess }}</div>
          <button type="submit" class="btn btn-primary" :disabled="passwordLoading">
            {{ passwordLoading ? 'Изменение...' : 'Изменить пароль' }}
          </button>
        </form>
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
const activeTab = ref('info')

const profileLoading = ref(false)
const passwordLoading = ref(false)
const profileError = ref('')
const profileSuccess = ref('')
const passwordError = ref('')
const passwordSuccess = ref('')

const profileForm = ref({
  firstName: '',
  lastName: '',
  patronymic: '',
  phone: '',
  email: ''
})

const passwordForm = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

async function loadAccount() {
  try {
    const response = await api.get('/api/account/me')
    account.value = response.data
    
    // Заполнить форму профиля
    profileForm.value = {
      firstName: account.value.firstName || '',
      lastName: account.value.lastName || '',
      patronymic: account.value.patronymic || '',
      phone: account.value.phone || '',
      email: account.value.email || ''
    }
    
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

async function updateProfile() {
  profileError.value = ''
  profileSuccess.value = ''
  profileLoading.value = true

  try {
    await api.put('/api/account/profile', profileForm.value)
    profileSuccess.value = 'Профиль успешно обновлен!'
    
    // Обновить данные аккаунта
    await loadAccount()
    
    // Обновить данные в store
    if (authStore.user) {
      authStore.user.email = profileForm.value.email
      authStore.user.firstName = profileForm.value.firstName
      authStore.user.lastName = profileForm.value.lastName
    }
    
    // Очистить сообщение через 3 секунды
    setTimeout(() => {
      profileSuccess.value = ''
    }, 3000)
  } catch (error) {
    profileError.value = error.response?.data?.message || 'Ошибка при обновлении профиля'
  } finally {
    profileLoading.value = false
  }
}

async function changePassword() {
  passwordError.value = ''
  passwordSuccess.value = ''

  // Валидация
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    passwordError.value = 'Новые пароли не совпадают'
    return
  }

  if (passwordForm.value.newPassword.length < 6) {
    passwordError.value = 'Новый пароль должен содержать минимум 6 символов'
    return
  }

  passwordLoading.value = true

  try {
    await api.put('/api/account/password', {
      oldPassword: passwordForm.value.oldPassword,
      newPassword: passwordForm.value.newPassword
    })
    
    passwordSuccess.value = 'Пароль успешно изменен!'
    
    // Очистить форму
    passwordForm.value = {
      oldPassword: '',
      newPassword: '',
      confirmPassword: ''
    }
    
    // Очистить сообщение через 3 секунды
    setTimeout(() => {
      passwordSuccess.value = ''
    }, 3000)
  } catch (error) {
    passwordError.value = error.response?.data?.message || 'Ошибка при изменении пароля'
  } finally {
    passwordLoading.value = false
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

.tabs {
  display: flex;
  gap: 8px;
  margin-top: 16px;
  border-bottom: 2px solid var(--border);
}

.tab {
  padding: 12px 24px;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--text-muted);
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
  margin-bottom: -2px;
}

.tab:hover {
  color: var(--text);
}

.tab.active {
  color: var(--primary);
  border-bottom-color: var(--primary);
}

.settings-content {
  max-width: 800px;
  margin: 0 auto;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 500;
  color: var(--text);
}

.form-group small {
  margin-top: -4px;
}

.error-message {
  padding: 12px;
  background: #fee;
  border: 1px solid #fcc;
  border-radius: 8px;
  color: #c33;
}

.success-message {
  padding: 12px;
  background: #efe;
  border: 1px solid #cfc;
  border-radius: 8px;
  color: #3c3;
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
