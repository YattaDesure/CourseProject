<template>
  <div class="login-container">
    <div class="login-card">
      <h1 class="login-title">🏠 Зеленый Квартал</h1>
      <p class="login-subtitle">Система управления недвижимостью</p>
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-group">
          <label>Email</label>
          <input v-model="email" type="email" class="input" required />
        </div>
        <div class="form-group">
          <label>Пароль</label>
          <input v-model="password" type="password" class="input" required />
        </div>
        <div v-if="error" class="error-message">{{ error }}</div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Вход...' : 'Войти' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const router = useRouter()
const authStore = useAuthStore()

async function handleLogin() {
  error.value = ''
  loading.value = true
  const result = await authStore.login(email.value, password.value)
  loading.value = false

  if (result.success) {
    router.push('/apartments')
  } else {
    error.value = result.error
  }
}
</script>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary) 0%, #8bd8a0 100%);
  padding: 24px;
}

.login-card {
  background: var(--card);
  border-radius: 16px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.login-title {
  text-align: center;
  color: var(--primary);
  margin-bottom: 8px;
  font-size: 28px;
}

.login-subtitle {
  text-align: center;
  color: var(--text-muted);
  margin-bottom: 32px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 600;
  color: var(--text);
  font-size: 14px;
}

.error-message {
  background: rgba(220, 53, 69, 0.1);
  color: var(--error);
  padding: 12px;
  border-radius: 8px;
  font-size: 14px;
  text-align: center;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

