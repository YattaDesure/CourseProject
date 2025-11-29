<template>
  <div class="page">
    <div class="page-header">
      <h2>Users</h2>
    </div>

    <div class="filters">
      <input
        v-model="search"
        type="text"
        placeholder="Search by name or email..."
        class="input"
        style="max-width: 300px;"
      />
      <select v-model="roleFilter" class="input" style="max-width: 200px;">
        <option value="">All Roles</option>
        <option value="User">User</option>
        <option value="Moderator">Moderator</option>
        <option value="Admin">Admin</option>
      </select>
    </div>

    <div class="card">
      <table class="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Role</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id">
            <td>{{ user.firstName }} {{ user.lastName }} {{ user.patronymic || '' }}</td>
            <td>{{ user.email }}</td>
            <td>{{ user.phone || '-' }}</td>
            <td>
              <span :class="getRoleBadgeClass(user.role)">{{ user.role }}</span>
            </td>
            <td>
              <span :class="user.isActive ? 'badge badge-success' : 'badge badge-warning'">
                {{ user.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td>
              <button @click="editUser(user)" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px; margin-right: 8px;">
                Edit
              </button>
              <select v-model="user.role" @change="updateUserRole(user)" class="input" style="max-width: 150px; padding: 6px; display: inline-block;">
                <option value="User">User</option>
                <option value="Moderator">Moderator</option>
                <option value="Admin">Admin</option>
              </select>
            </td>
          </tr>
          <tr v-if="filteredUsers.length === 0">
            <td colspan="6" style="text-align: center; padding: 32px; color: var(--text-muted);">
              No users found
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Edit User Modal -->
    <div v-if="showModal" class="modal-overlay" @click="showModal = false">
      <div class="modal" @click.stop>
        <h3>Edit User</h3>
        <form @submit.prevent="saveUser">
          <div class="form-group">
            <label>First Name</label>
            <input v-model="form.firstName" class="input" required />
          </div>
          <div class="form-group">
            <label>Last Name</label>
            <input v-model="form.lastName" class="input" required />
          </div>
          <div class="form-group">
            <label>Patronymic</label>
            <input v-model="form.patronymic" class="input" />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input v-model="form.email" type="email" class="input" required />
          </div>
          <div class="form-group">
            <label>Phone</label>
            <input v-model="form.phone" class="input" />
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
import api from '../services/api'

const users = ref([])
const search = ref('')
const roleFilter = ref('')
const showModal = ref(false)
const editingUser = ref(null)

const form = ref({
  firstName: '',
  lastName: '',
  patronymic: '',
  email: '',
  phone: ''
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

  return result
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

async function saveUser() {
  try {
    await api.put(`/api/users/${editingUser.value.id}`, form.value)
    showModal.value = false
    editingUser.value = null
    await loadUsers()
    alert('User updated successfully')
  } catch (error) {
    console.error('Failed to save user:', error)
    alert('Failed to save user')
  }
}

async function updateUserRole(user) {
  try {
    await api.put(`/api/users/${user.id}/role`, { role: user.role })
    alert('User role updated successfully')
  } catch (error) {
    console.error('Failed to update user role:', error)
    alert('Failed to update user role')
    await loadUsers() // Reload to revert change
  }
}

function getRoleBadgeClass(role) {
  if (role === 'Admin') return 'badge badge-warning'
  if (role === 'Moderator') return 'badge badge-info'
  return 'badge badge-success'
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
</style>
