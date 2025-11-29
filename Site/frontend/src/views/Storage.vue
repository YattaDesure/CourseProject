<template>
  <div class="page">
    <div class="page-header">
      <h2>Storage Rooms</h2>
      <button v-if="authStore.isModerator" @click="showModal = true" class="btn btn-primary">
        + Add Storage Room
      </button>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Search by label..."
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
      <table class="table">
        <thead>
          <tr>
            <th>Label</th>
            <th>Area (m²)</th>
            <th>Status</th>
            <th>Assigned To</th>
            <th v-if="authStore.isModerator">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="room in filteredRooms" :key="room.id">
            <td>{{ room.label }}</td>
            <td>{{ parseFloat(room.area).toFixed(2) }}</td>
            <td>
              <span :class="getStatusBadgeClass(room.status)">{{ room.status }}</span>
            </td>
            <td>
              <span v-for="(user, idx) in room.users" :key="idx">
                {{ user.firstName }} {{ user.lastName }}{{ idx < room.users.length - 1 ? ', ' : '' }}
              </span>
              <span v-if="room.users.length === 0">-</span>
            </td>
            <td v-if="authStore.isModerator">
              <button @click="editRoom(room)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px;">
                Edit
              </button>
            </td>
          </tr>
          <tr v-if="filteredRooms.length === 0">
            <td colspan="5" style="text-align: center; padding: 32px; color: var(--text-muted);">
              No storage rooms found
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Add/Edit Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>{{ editingRoom ? 'Edit' : 'Add' }} Storage Room</h3>
        <form @submit.prevent="saveRoom">
          <div class="form-group">
            <label>Label</label>
            <input v-model="form.label" class="input" required />
          </div>
          <div class="form-group">
            <label>Level</label>
            <input v-model="form.level" class="input" required />
          </div>
          <div class="form-group">
            <label>Area (m²)</label>
            <input v-model.number="form.area" type="number" step="0.1" class="input" required />
          </div>
          <div class="form-group">
            <label>Owner</label>
            <select v-model.number="form.ownerId" class="input">
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
    alert('Failed to save storage room')
  }
}

function getStatusBadgeClass(status) {
  if (status === 'Occupied') return 'badge badge-warning'
  if (status === 'Available') return 'badge badge-success'
  return 'badge badge-info'
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

