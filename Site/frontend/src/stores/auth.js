import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')
  const isModerator = computed(() => user.value?.role === 'Moderator' || isAdmin.value)

  async function login(email, password) {
    try {
      const response = await api.post('/api/auth/login', { email, password })
      token.value = response.data.token
      user.value = {
        email: response.data.email,
        role: response.data.role,
        fullName: response.data.fullName
      }
      localStorage.setItem('token', token.value)
      localStorage.setItem('user', JSON.stringify(user.value))
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      return { success: true }
    } catch (error) {
      return { success: false, error: error.response?.data?.message || 'Ошибка входа' }
    }
  }

  function logout() {
    token.value = ''
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    delete api.defaults.headers.common['Authorization']
  }

  // Initialize axios header if token exists
  if (token.value) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
  }

  return {
    token,
    user,
    isAuthenticated,
    isAdmin,
    isModerator,
    login,
    logout
  }
})

