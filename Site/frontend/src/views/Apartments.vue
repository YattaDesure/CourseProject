<template>
  <div class="page">
    <div class="page-header">
      <h2>Apartments</h2>
      <button v-if="authStore.isModerator" @click="showModal = true" class="btn btn-primary">
        + Add Apartment
      </button>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Search by number or entrance..."
        class="input"
        style="max-width: 300px;"
      />
      <select v-model="statusFilter" class="input" style="max-width: 200px;">
        <option value="">All Status</option>
        <option value="Available">Available</option>
        <option value="Occupied">Occupied</option>
      </select>
    </div>

    <div class="card">
      <div v-if="loading" style="text-align: center; padding: 32px;">
        Loading apartments...
      </div>
      <table v-else class="table">
        <thead>
          <tr>
            <th>Number</th>
            <th>Entrance</th>
            <th>Floor</th>
            <th>Area (m²)</th>
            <th>Status</th>
            <th>Owners</th>
            <th v-if="authStore.isModerator">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="apt in filteredApartments" :key="apt.id">
            <td>{{ apt.number }}</td>
            <td>{{ apt.entrance }}</td>
            <td>{{ apt.floor }}</td>
            <td>{{ parseFloat(apt.area).toFixed(2) }}</td>
            <td>
              <span :class="getStatusBadgeClass(apt.status)">{{ apt.status }}</span>
            </td>
            <td>
              <span v-for="(owner, idx) in apt.owners" :key="idx">
                {{ owner.firstName }} {{ owner.lastName }}{{ idx < apt.owners.length - 1 ? ', ' : '' }}
              </span>
              <span v-if="apt.owners.length === 0">-</span>
            </td>
            <td v-if="authStore.isModerator">
              <button @click="editApartment(apt)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px;">
                Edit
              </button>
            </td>
          </tr>
          <tr v-if="apartments.length === 0 && !loading">
            <td colspan="7" style="text-align: center; padding: 32px; color: var(--text-muted);">
              No apartments found
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>{{ editingApartment ? 'Edit' : 'Add' }} Apartment</h3>
        <form @submit.prevent="saveApartment">
          <div class="form-group">
            <label>Number</label>
            <input v-model="form.number" class="input" required />
          </div>
          <div class="form-group">
            <label>Entrance</label>
            <input v-model="form.entrance" class="input" required />
          </div>
          <div class="form-group">
            <label>Floor</label>
            <input v-model.number="form.floor" type="number" class="input" required />
          </div>
          <div class="form-group">
            <label>Area (m²)</label>
            <input v-model.number="form.area" type="number" step="0.1" class="input" required />
          </div>
          <div class="form-group">
            <label>Owner</label>
            <select v-model.number="form.residentId" class="input">
              <option :value="null">No Owner (Available)</option>
              <option v-for="resident in residents" :key="resident.id" :value="resident.id">
                {{ resident.firstName }} {{ resident.lastName }} ({{ resident.email }})
              </option>
            </select>
          </div>
          <div class="modal-actions">
            <button type="button" @click="showModal = false" class="btn btn-secondary">Cancel</button>
            <button type="submit" class="btn btn-primary">Save</button>
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
    alert('Failed to save apartment')
  }
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

