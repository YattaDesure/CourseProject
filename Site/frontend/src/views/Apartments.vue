<template>
  <div class="page">
    <div class="page-header">
      <h2>Квартиры</h2>
      <button v-if="authStore.isModerator" @click="showModal = true" class="btn btn-primary">
        + Добавить квартиру
      </button>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Поиск по номеру или подъезду..."
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
      <div v-if="loading" style="text-align: center; padding: 32px;">
        Загрузка квартир...
      </div>
      <table v-else class="table">
        <thead>
          <tr>
            <th>Номер</th>
            <th>Подъезд</th>
            <th>Этаж</th>
            <th>Площадь (м²)</th>
            <th>Статус</th>
            <th>Владельцы</th>
            <th v-if="authStore.isModerator">Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="apt in filteredApartments" :key="apt.id">
            <td>{{ apt.number }}</td>
            <td>{{ apt.entrance }}</td>
            <td>{{ apt.floor }}</td>
            <td>{{ parseFloat(apt.area).toFixed(2) }}</td>
            <td>
              <span :class="getStatusBadgeClass(apt.status)">{{ getStatusText(apt.status) }}</span>
            </td>
            <td>
              <span v-for="(owner, idx) in apt.owners" :key="idx">
                {{ owner.firstName }} {{ owner.lastName }}{{ idx < apt.owners.length - 1 ? ', ' : '' }}
              </span>
              <span v-if="apt.owners.length === 0">-</span>
            </td>
            <td v-if="authStore.isModerator">
              <button @click="editApartment(apt)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px;">
                Редактировать
              </button>
            </td>
          </tr>
          <tr v-if="apartments.length === 0 && !loading">
            <td colspan="7" style="text-align: center; padding: 32px; color: var(--text-muted);">
              Квартиры не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>{{ editingApartment ? 'Редактировать' : 'Добавить' }} квартиру</h3>
        <form @submit.prevent="saveApartment">
          <div class="form-group">
            <label>Номер</label>
            <input v-model="form.number" class="input" required />
          </div>
          <div class="form-group">
            <label>Подъезд</label>
            <input v-model="form.entrance" class="input" required />
          </div>
          <div class="form-group">
            <label>Этаж</label>
            <input v-model.number="form.floor" type="number" class="input" required />
          </div>
          <div class="form-group">
            <label>Площадь (м²)</label>
            <input v-model.number="form.area" type="number" step="0.1" class="input" required />
          </div>
          <div class="form-group">
            <label>Владелец</label>
            <select v-model.number="form.residentId" class="input">
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
const apartments = ref([])
const residents = ref([])
const loading = ref(false)
const search = ref('')
const statusFilter = ref('')
const showModal = ref(false)
const editingApartment = ref(null)

const form = ref({
  number: '',
  entrance: '',
  floor: 0,
  area: 0,
  residentId: null
})


const filteredApartments = computed(() => {
  let result = apartments.value

  if (search.value) {
    const query = search.value.toLowerCase()
    result = result.filter(a => 
      (a.number && a.number.toString().toLowerCase().includes(query)) || 
      (a.entrance && a.entrance.toString().toLowerCase().includes(query))
    )
  }

  if (statusFilter.value) {
    result = result.filter(a => a.status === statusFilter.value)
  }

  return result
})

async function loadApartments() {
  try {
    loading.value = true
    const params = {}
    if (search.value) params.search = search.value
    if (statusFilter.value) params.status = statusFilter.value

    const response = await api.get('/api/apartments', { params })
    apartments.value = response.data || []
  } catch (error) {
    console.error('Failed to load apartments:', error)
    apartments.value = []
  } finally {
    loading.value = false
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

function editApartment(apt) {
  editingApartment.value = apt
  form.value = {
    number: apt.number || '',
    entrance: apt.entrance || '',
    floor: apt.floor || 0,
    area: apt.area || 0,
    residentId: apt.residentId || null
  }
  showModal.value = true
}

async function saveApartment() {
  try {
    if (editingApartment.value) {
      await api.put(`/api/apartments/${editingApartment.value.id}`, form.value)
    } else {
      await api.post('/api/apartments', form.value)
    }
    showModal.value = false
    editingApartment.value = null
    form.value = { number: '', entrance: '', floor: 0, area: 0, residentId: null }
    await loadApartments()
  } catch (error) {
    console.error('Failed to save apartment:', error)
    alert('Ошибка при сохранении квартиры')
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

onMounted(() => {
  loadApartments()
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

