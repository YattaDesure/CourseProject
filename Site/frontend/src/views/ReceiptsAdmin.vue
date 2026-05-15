<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h2>Начисления и квитанции</h2>
        <div style="color: var(--text-muted); margin-top: 6px;">
          В квитанцию входят капремонт (₽/м² по площади объектов) и электроэнергия по счётчикам
          (разница показаний за месяц × тариф ₽/кВт·ч, если есть предыдущее показание).
        </div>
      </div>
    </div>

    <div class="card">
      <h3 style="margin: 0 0 16px;">Сформировать квитанции</h3>

      <div class="form-row">
        <div class="form-group">
          <label>Месяц</label>
          <input v-model="month" type="month" class="input" />
          <small style="color: var(--text-muted); font-size: 12px;">
            Будет использован первый день месяца (YYYY-MM-01)
          </small>
        </div>
        <div class="form-group" style="align-self:end;">
          <button class="btn btn-primary" @click="generate" :disabled="loading">
            {{ loading ? 'Формирование...' : 'Сформировать' }}
          </button>
        </div>
      </div>

      <div v-if="error" class="error-message" style="margin-top: 12px;">{{ error }}</div>
      <div v-if="result" class="success-message" style="margin-top: 12px;">
        Готово за {{ formatMonth(result.billingMonth) }}. Создано: {{ result.created }}, обновлено: {{ result.updated }},
        пользователей с объектами: {{ result.residents }}.
        Тарифы: капремонт {{ result.capRepairRatePerSqm ?? result.ratePerSqm }} ₽/м²,
        электричество {{ result.electricityRatePerKwh ?? '—' }} ₽/кВт·ч.
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import api from '../services/api'

function currentMonthValue() {
  const d = new Date()
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

function formatMonth(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleDateString('ru-RU', { year: 'numeric', month: 'long' })
}

const month = ref(currentMonthValue())
const loading = ref(false)
const error = ref('')
const result = ref(null)

async function generate() {
  error.value = ''
  result.value = null
  loading.value = true
  try {
    const res = await api.post('/api/receipts/generate', null, { params: { billingMonth: month.value } })
    result.value = res.data
  } catch (e) {
    error.value = e.response?.data?.message || 'Не удалось сформировать квитанции'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.form-row {
  display: grid;
  grid-template-columns: 1fr 220px;
  gap: 16px;
  align-items: start;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 500;
  color: var(--text);
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>

