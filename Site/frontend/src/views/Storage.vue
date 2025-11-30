<template>
  <div class="page">
    <div class="page-header">
      <h2>Кладовые</h2>
      <div style="display: flex; gap: 8px;">
        <button @click="exportToExcel" class="btn btn-secondary" style="padding: 8px 16px;">
          📊 Excel
        </button>
        <button @click="exportToCsv" class="btn btn-secondary" style="padding: 8px 16px;">
          📄 CSV
        </button>
        <button v-if="authStore.isModerator" @click="showModal = true" class="btn btn-primary">
          + Добавить кладовую
        </button>
      </div>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Поиск по номеру..."
        class="input"
        style="max-width: 300px;"
      />
      <select v-model="statusFilter" class="input" style="max-width: 200px;">
        <option value="">Все статусы</option>
        <option value="Available">Свободна</option>
        <option value="Occupied">Занята</option>
      </select>
    </div>

    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>Номер</th>
            <th>Площадь (м²)</th>
            <th>Статус</th>
            <th>Владелец</th>
            <th v-if="authStore.isModerator">Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="room in filteredRooms" :key="room.id">
            <td>{{ room.label }}</td>
            <td>{{ parseFloat(room.area).toFixed(2) }}</td>
            <td>
              <span :class="getStatusBadgeClass(room.status)">{{ getStatusText(room.status) }}</span>
            </td>
            <td>
              <span v-for="(user, idx) in room.users" :key="idx">
                {{ user.firstName }} {{ user.lastName }}{{ idx < room.users.length - 1 ? ', ' : '' }}
              </span>
              <span v-if="room.users.length === 0">-</span>
            </td>
            <td v-if="authStore.isModerator">
              <button @click="editRoom(room)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px;">
                Редактировать
              </button>
            </td>
          </tr>
          <tr v-if="filteredRooms.length === 0">
            <td colspan="5" style="text-align: center; padding: 32px; color: var(--text-muted);">
              Кладовые не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>{{ editingRoom ? 'Редактировать' : 'Добавить' }} кладовую</h3>
        <form @submit.prevent="saveRoom">
          <div class="form-group">
            <label>Номер</label>
            <input v-model="form.label" class="input" required />
          </div>
          <div class="form-group">
            <label>Площадь (м²)</label>
            <input v-model.number="form.area" type="number" step="0.1" class="input" required />
          </div>
          <div class="form-group">
            <label>Владелец</label>
            <select v-model.number="form.ownerId" class="input">
              <option :value="null">Нет владельца (Свободна)</option>
              <option v-for="resident in residents" :key="resident.id" :value="resident.id">
                {{ resident.firstName }} {{ resident.lastName }} ({{ resident.email }})
              </option>
            </select>
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
import { useAuthStore } from '../stores/auth'
import api from '../services/api'

const authStore = useAuthStore()
const rooms = ref([])
const residents = ref([])
const search = ref('')
const statusFilter = ref('')
const showModal = ref(false)
const editingRoom = ref(null)

const form = ref({
  label: '',
  area: 0,
  ownerId: null
})

const filteredRooms = computed(() => {
  let result = rooms.value

  if (search.value) {
    const query = search.value.toLowerCase()
    result = result.filter(r => 
      (r.label && r.label.toString().toLowerCase().includes(query))
    )
  }

  if (statusFilter.value) {
    result = result.filter(r => r.status === statusFilter.value)
  }

  return result
})

async function loadRooms() {
  try {
    const params = {}
    if (search.value) params.search = search.value
    if (statusFilter.value) params.status = statusFilter.value

    const response = await api.get('/api/storagerooms', { params })
    rooms.value = response.data
  } catch (error) {
    console.error('Failed to load storage rooms:', error)
  }
}

async function loadResidents() {
  try {
    const response = await api.get('/api/users/residents')
    residents.value = response.data || []
  } catch (error) {
    console.error('Failed to load residents:', error)
  }
}

function editRoom(room) {
  editingRoom.value = room
  form.value = {
    label: room.label || '',
    area: room.area || 0,
    ownerId: room.ownerId || null
  }
  showModal.value = true
}

async function saveRoom() {
  try {
    if (editingRoom.value) {
      await api.put(`/api/storagerooms/${editingRoom.value.id}`, form.value)
    } else {
      await api.post('/api/storagerooms', form.value)
    }
    showModal.value = false
    editingRoom.value = null
    form.value = { label: '', area: 0, ownerId: null }
    await loadRooms()
  } catch (error) {
    console.error('Failed to save storage room:', error)
    alert('Ошибка при сохранении кладовой')
  }
}

function getStatusText(status) {
  if (status === 'Occupied') return 'Занята'
  if (status === 'Available') return 'Свободна'
  return status
}

function getStatusBadgeClass(status) {
  if (status === 'Occupied') return 'badge badge-warning'
  if (status === 'Available') return 'badge badge-success'
  return 'badge badge-info'
}

async function exportToExcel() {
  try {
    const response = await api.get('/api/storagerooms/export/excel', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `Кладовые_${new Date().toISOString().slice(0, 10)}.xlsx`)
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
    const response = await api.get('/api/storagerooms/export/csv', {
      responseType: 'blob'
    })
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `Кладовые_${new Date().toISOString().slice(0, 10)}.csv`)
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
  loadRooms()
  loadResidents()
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
</style>

