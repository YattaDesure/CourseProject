<template>
  <div class="page">
    <div class="page-header">
      <h2>Пользователи</h2>
      <div class="page-actions">
        <button @click="exportToExcel" class="btn btn-secondary" style="padding: 8px 16px;">
          📊 Excel
        </button>
        <button @click="exportToCsv" class="btn btn-secondary" style="padding: 8px 16px;">
          📄 CSV
        </button>
        <button @click="showAddModal = true" class="btn btn-primary">
          + Добавить пользователя
        </button>
      </div>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Поиск по имени или email..."
        class="input filter-field"
      />
      <select v-model="roleFilter" class="input filter-field">
        <option value="">Все роли</option>
        <option value="User">Пользователь</option>
        <option value="Moderator">Модератор</option>
        <option value="Admin">Администратор</option>
      </select>
      <select v-model="statusFilter" class="input filter-field">
        <option value="">Все статусы</option>
        <option value="active">Активен</option>
        <option value="inactive">Неактивен</option>
      </select>
      <select v-model="sortBy" class="input filter-field">
        <option value="nameAsc">Сортировка: имя ↑</option>
        <option value="nameDesc">Сортировка: имя ↓</option>
        <option value="emailAsc">Сортировка: email ↑</option>
        <option value="roleAsc">Сортировка: роль ↑</option>
      </select>
    </div>

    <div class="card">
      <div class="table-wrap">
        <table class="table">
        <thead>
          <tr>
            <th>Имя</th>
            <th>Email</th>
            <th>Телефон</th>
            <th>Роль</th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id">
            <td>{{ user.firstName }} {{ user.lastName }} {{ user.patronymic || '' }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.phone || '-' }}</td>
            <td>
              <span :class="getRoleBadgeClass(user.role)">{{ getRoleText(user.role) }}</span>
            </td>
            <td>
              <span :class="user.isActive ? 'badge badge-success' : 'badge badge-warning'">
                {{ user.isActive ? 'Активен' : 'Неактивен' }}
              </span>
            </td>
            <td>
              <button @click="editUser(user)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px; margin-right: 8px;">
                Редактировать
              </button>
              <select 
                v-model="user.role" 
                @change="updateUserRole(user)" 
                class="input" 
                style="max-width: 150px; padding: 6px; display: inline-block;"
                :disabled="user.email === authStore.user?.email && user.role === 'Admin'"
                :title="user.email === authStore.user?.email && user.role === 'Admin' ? 'Вы не можете убрать себе роль администратора' : ''"
              >
                <option value="User">Пользователь</option>
                <option value="Moderator">Модератор</option>
                <option value="Admin">Администратор</option>
              </select>
            </td>
          </tr>
          <tr v-if="filteredUsers.length === 0">
            <td colspan="6" style="text-align: center; padding: 32px; color: var(--text-muted);">
              Пользователи не найдены
            </td>
          </tr>
        </tbody>
        </table>
      </div>
    </div>

    <!-- Add User Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click="showAddModal = false">
      <div class="modal" @click.stop>
        <h3>Добавить пользователя</h3>
        <form @submit.prevent="createUser">
          <div class="form-group">
            <label>Имя *</label>
            <input v-model="addForm.firstName" class="input" required />
          </div>
          <div class="form-group">
            <label>Фамилия *</label>
            <input v-model="addForm.lastName" class="input" required />
          </div>
          <div class="form-group">
            <label>Отчество</label>
            <input v-model="addForm.patronymic" class="input" />
          </div>
          <div class="form-group">
            <label>Email *</label>
            <input v-model="addForm.email" type="email" class="input" required />
          </div>
          <div class="form-group">
            <label>Пароль *</label>
            <input v-model="addForm.password" type="password" class="input" required minlength="6" />
          </div>
          <div class="form-group">
            <label>Телефон</label>
            <input v-model="addForm.phone" class="input" />
          </div>
          <div class="form-group">
            <label>Роль *</label>
            <select v-model="addForm.role" class="input" required>
              <option value="User">Пользователь</option>
              <option value="Moderator">Модератор</option>
              <option value="Admin">Администратор</option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="button" @click="showAddModal = false" class="btn btn-secondary">Отмена</button>
            <button type="submit" class="btn btn-primary">Создать</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit User Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>Редактировать пользователя</h3>
        <form @submit.prevent="saveUser">
          <div class="form-group">
            <label>Имя</label>
            <input v-model="form.firstName" class="input" required />
          </div>
          <div class="form-group">
            <label>Фамилия</label>
            <input v-model="form.lastName" class="input" required />
          </div>
          <div class="form-group">
            <label>Отчество</label>
            <input v-model="form.patronymic" class="input" />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input v-model="form.email" type="email" class="input" required />
          </div>
          <div class="form-group">
            <label>Телефон</label>
            <input v-model="form.phone" class="input" />
          </div>
          <div class="modal-actions">
            <button type="button" @click="showModal = false" class="btn btn-secondary">Отмена</button>
            <button type="submit" class="btn btn-primary">Сохранить</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()

const users = ref([])
const search = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const sortBy = ref('nameAsc')
const showModal = ref(false)
const showAddModal = ref(false)
const editingUser = ref(null)

const form = ref({
  firstName: '',
  lastName: '',
  patronymic: '',
  email: '',
  phone: ''
})

const addForm = ref({
  firstName: '',
  lastName: '',
  patronymic: '',
  email: '',
  password: '',
  phone: '',
  role: 'User'
})

const filteredUsers = computed(() => {
  let result = users.value

  if (search.value) {
    const query = search.value.toLowerCase()
    result = result.filter(u => 
      (u.firstName && u.firstName.toLowerCase().includes(query)) || 
      (u.lastName && u.lastName.toLowerCase().includes(query)) ||
      (u.email && u.email.toLowerCase().includes(query))
    )
  }

  if (roleFilter.value) {
    result = result.filter(u => u.role === roleFilter.value)
  }

  if (statusFilter.value) {
    const active = statusFilter.value === 'active'
    result = result.filter(u => !!u.isActive === active)
  }

  const sorted = [...result]
  const roleRank = (r) => (r === 'Admin' ? 1 : r === 'Moderator' ? 2 : 3)
  sorted.sort((a, b) => {
    const an = `${a.lastName || ''} ${a.firstName || ''}`.trim()
    const bn = `${b.lastName || ''} ${b.firstName || ''}`.trim()
    if (sortBy.value === 'nameDesc') return bn.localeCompare(an)
    if (sortBy.value === 'emailAsc') return String(a.email || '').localeCompare(String(b.email || ''))
    if (sortBy.value === 'roleAsc') return roleRank(a.role) - roleRank(b.role)
    return an.localeCompare(bn)
  })

  return sorted
})

async function loadUsers() {
  try {
    const params = {}
    if (search.value) params.search = search.value
    if (roleFilter.value) params.role = roleFilter.value

    const response = await api.get('/api/users', { params })
    users.value = response.data || []
  } catch (error) {
    console.error('Failed to load users:', error)
    users.value = []
  }
}

function editUser(user) {
  editingUser.value = user
  form.value = {
    firstName: user.firstName || '',
    lastName: user.lastName || '',
    patronymic: user.patronymic || '',
    email: user.email || '',
    phone: user.phone || ''
  }
  showModal.value = true
}

async function createUser() {
  try {
    await api.post('/api/users', addForm.value)
    showAddModal.value = false
    addForm.value = {
      firstName: '',
      lastName: '',
      patronymic: '',
      email: '',
      password: '',
      phone: '',
      role: 'User'
    }
    await loadUsers()
    alert('Пользователь успешно создан')
  } catch (error) {
    console.error('Failed to create user:', error)
    const errorMsg = error.response?.data?.message || 'Ошибка при создании пользователя'
    alert(errorMsg)
  }
}

async function saveUser() {
  try {
    await api.put(`/api/users/${editingUser.value.id}`, form.value)
    showModal.value = false
    editingUser.value = null
    await loadUsers()
    alert('Пользователь успешно обновлен')
  } catch (error) {
    console.error('Failed to save user:', error)
    alert('Ошибка при сохранении пользователя')
  }
}

async function updateUserRole(user) {
  // Проверка на фронтенде: администратор не может убрать себе роль
  if (user.email === authStore.user?.email && user.role !== 'Admin') {
    alert('Вы не можете убрать себе роль администратора')
    await loadUsers() // Reload to revert change
    return
  }

  try {
    await api.put(`/api/users/${user.id}/role`, { role: user.role })
    alert('Роль пользователя успешно обновлена')
  } catch (error) {
    console.error('Failed to update user role:', error)
    const errorMsg = error.response?.data?.message || 'Ошибка при обновлении роли'
    alert(errorMsg)
    await loadUsers() // Reload to revert change
  }
}

function getRoleText(role) {
  if (role === 'Admin') return 'Администратор'
  if (role === 'Moderator') return 'Модератор'
  if (role === 'User') return 'Пользователь'
  return role
}

function getRoleBadgeClass(role) {
  if (role === 'Admin') return 'badge badge-warning'
  if (role === 'Moderator') return 'badge badge-info'
  return 'badge badge-success'
}

async function exportToExcel() {
  try {
    const response = await api.get('/api/users/export/excel', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `Пользователи_${new Date().toISOString().slice(0, 10)}.xlsx`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to export to Excel:', error)
    alert('Ошибка при экспорте в Excel')
  }
}

async function exportToCsv() {
  try {
    const response = await api.get('/api/users/export/csv', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `Пользователи_${new Date().toISOString().slice(0, 10)}.csv`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error('Failed to export to CSV:', error)
    alert('Ошибка при экспорте в CSV')
  }
}

onMounted(() => {
  loadUsers()
})
</script>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.page-header h2 {
  color: var(--text);
  font-size: 24px;
}

.filters {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
  flex-wrap: wrap;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--card);
  border-radius: 12px;
  padding: 32px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal h3 {
  margin-bottom: 24px;
  color: var(--text);
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 24px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.form-group label {
  font-weight: 600;
  color: var(--text);
  font-size: 14px;
}
</style>
