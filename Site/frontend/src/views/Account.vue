<template>
  <div class="page">
    <div class="page-header">
      <h2>My Account</h2>
    </div>

    <div v-if="loading" style="text-align: center; padding: 48px;">
      Loading...
    </div>

    <div v-else-if="account" class="account-content">
      <div class="card" style="margin-bottom: 24px;">
        <h3 style="margin-bottom: 16px;">Personal Information</h3>
        <div class="info-grid">
          <div>
            <label>Name</label>
            <p>{{ account.firstName }} {{ account.lastName }} {{ account.patronymic || '' }}</p>
          </div>
          <div>
            <label>Email</label>
            <p>{{ account.email }}</p>
          </div>
          <div>
            <label>Role</label>
            <p><span :class="getRoleBadgeClass(account.role)">{{ account.role }}</span></p>
          </div>
          <div>
            <label>Member Since</label>
            <p>{{ new Date(account.createdAt).toLocaleDateString() }}</p>
          </div>
        </div>
      </div>

      <div class="card" v-if="account.apartments && account.apartments.length > 0">
        <h3 style="margin-bottom: 16px;">My Apartments</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Number</th>
              <th>Entrance</th>
              <th>Floor</th>
              <th>Area (m²)</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="apt in account.apartments" :key="apt.id">
              <td>{{ apt.number }}</td>
              <td>{{ apt.entrance }}</td>
              <td>{{ apt.floor }}</td>
              <td>{{ parseFloat(apt.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(apt.status)">{{ apt.status }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card" v-if="account.parkingSpaces && account.parkingSpaces.length > 0">
        <h3 style="margin-bottom: 16px;">My Parking Spaces</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Slot Number</th>
              <th>Area (m²)</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ps in account.parkingSpaces" :key="ps.id">
              <td>{{ ps.slotNumber }}</td>
              <td>{{ parseFloat(ps.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(ps.status)">{{ ps.status }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card" v-if="account.storageRooms && account.storageRooms.length > 0">
        <h3 style="margin-bottom: 16px;">My Storage Rooms</h3>
        <table class="table">
          <thead>
            <tr>
              <th>Label</th>
              <th>Area (m²)</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="sr in account.storageRooms" :key="sr.id">
              <td>{{ sr.label }}</td>
              <td>{{ parseFloat(sr.area).toFixed(2) }}</td>
              <td>
                <span :class="getStatusBadgeClass(sr.status)">{{ sr.status }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="!account.apartments?.length && !account.parkingSpaces?.length && !account.storageRooms?.length" class="card">
        <p style="text-align: center; color: var(--text-muted); padding: 32px;">
          You don't have any properties assigned yet.
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
