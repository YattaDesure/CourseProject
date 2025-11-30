<template>
  <div class="page">
    <div class="page-header">
      <h2>Паркинг</h2>
      <button v-if="authStore.isModerator" @click="showModal = true" class="btn btn-primary">
        + Добавить место
      </button>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Поиск по номеру места..."
        class="input"
        style="max-width: 300px;"
      />
      <select v-model="statusFilter" class="input" style="max-width: 200px;">
        <option value="">Все статусы</option>
        <option value="Available">Свободно</option>
        <option value="Occupied">Занято</option>
      </select>
    </div>

    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>Номер места</th>
            <th>Площадь (м²)</th>
            <th>Статус</th>
            <th>Назначено</th>
            <th v-if="authStore.isModerator">Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="space in filteredSpaces" :key="space.id">
            <td>{{ space.slotNumber }}</td>
            <td>{{ parseFloat(space.area).toFixed(2) }}</td>
            <td>
              <span :class="getStatusBadgeClass(space.status)">{{ getStatusText(space.status) }}</span>
            </td>
            <td>
              <span v-for="(user, idx) in space.users" :key="idx">
                {{ user.firstName }} {{ user.lastName }}{{ idx < space.users.length - 1 ? ', ' : '' }}
              </span>
              <span v-if="space.users.length === 0">-</span>
            </td>
            <td v-if="authStore.isModerator">
              <button @click="editSpace(space)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px;">
                Редактировать
              </button>
            </td>
          </tr>
          <tr v-if="filteredSpaces.length === 0">
            <td colspan="5" style="text-align: center; padding: 32px; color: var(--text-muted);">
              Места не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>{{ editingSpace ? 'Редактировать' : 'Добавить' }} место</h3>
        <form @submit.prevent="saveSpace">
          <div class="form-group">
            <label>Номер места</label>
            <input v-model="form.slotNumber" class="input" required />
          </div>
          <div class="form-group">
            <label>Площадь (м²)</label>
            <input v-model.number="form.area" type="number" step="0.1" class="input" required />
          </div>
          <div class="form-group">
            <label>Владелец</label>
            <select v-model.number="form.ownerId" class="input">
              <option :value="null">Нет владельца (Свободно)</option>
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
const spaces = ref([])
const residents = ref([])
const search = ref('')
const statusFilter = ref('')
const showModal = ref(false)
const editingSpace = ref(null)

const form = ref({
  slotNumber: '',
  area: 0,
  ownerId: null
})

const filteredSpaces = computed(() => {
  let result = spaces.value

  if (search.value) {
    const query = search.value.toLowerCase()
    result = result.filter(s => 
      (s.slotNumber && s.slotNumber.toString().toLowerCase().includes(query))
    )
  }

  if (statusFilter.value) {
    result = result.filter(s => s.status === statusFilter.value)
  }

  return result
})

async function loadSpaces() {
  try {
    const params = {}
    if (search.value) params.search = search.value
    if (statusFilter.value) params.status = statusFilter.value

    const response = await api.get('/api/parking', { params })
    spaces.value = response.data
  } catch (error) {
    console.error('Failed to load parking spaces:', error)
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

function editSpace(space) {
  editingSpace.value = space
  form.value = {
    slotNumber: space.slotNumber || '',
    area: space.area || 0,
    ownerId: space.ownerId || null
  }
  showModal.value = true
}

async function saveSpace() {
  try {
    if (editingSpace.value) {
      await api.put(`/api/parking/${editingSpace.value.id}`, form.value)
    } else {
      await api.post('/api/parking', form.value)
    }
    showModal.value = false
    editingSpace.value = null
    form.value = { slotNumber: '', area: 0, ownerId: null }
    await loadSpaces()
  } catch (error) {
    console.error('Failed to save parking space:', error)
    alert('Ошибка при сохранении места')
  }
}

function getStatusText(status) {
  if (status === 'Occupied') return 'Занято'
  if (status === 'Available') return 'Свободно'
  return status
}

function getStatusBadgeClass(status) {
  if (status === 'Occupied') return 'badge badge-warning'
  if (status === 'Available') return 'badge badge-success'
  return 'badge badge-info'
}

onMounted(() => {
  loadSpaces()
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

