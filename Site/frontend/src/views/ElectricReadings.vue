<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h1>Показания электросчётчиков</h1>
        <p class="subtitle">Ввод показаний списком. Новое значение не может быть меньше предыдущего.</p>
      </div>

      <div class="controls">
        <div class="control">
          <label>Месяц</label>
          <div class="month-row">
            <input v-model="selectedMonth" type="month" class="input" />
            <button class="btn btn-secondary btn-small" @click="setCurrentMonth" type="button">Текущий</button>
          </div>
        </div>

        <div class="control">
          <label>Режим</label>
          <label class="toggle">
            <input type="checkbox" v-model="sequenceMode" />
            <span>ввод подряд</span>
          </label>
        </div>

        <button class="btn btn-primary" @click="reload" :disabled="loading">
          Обновить
        </button>
      </div>
    </div>

    <div class="tabs">
      <button
        class="tab"
        :class="{ active: objectType === 'apartment' }"
        @click="setType('apartment')"
      >
        Квартиры
      </button>
      <button
        class="tab"
        :class="{ active: objectType === 'storage' }"
        @click="setType('storage')"
      >
        Кладовые
      </button>
      <button
        class="tab"
        :class="{ active: objectType === 'parking' }"
        @click="setType('parking')"
      >
        Паркинг
      </button>
    </div>

    <div v-if="error" class="alert alert-error">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>Загрузка...</p>
    </div>

    <div v-else class="card">
      <div class="table-tools">
        <input v-model="search" class="input search" placeholder="Поиск по номеру..." />
        <div class="hint">
          Enter — сохранить. <span v-if="sequenceMode">После сохранения фокус перейдёт на следующую строку.</span>
        </div>
      </div>

      <div v-if="missingCount && missingCount.missingTotal > 0" class="alert alert-warn" style="margin-bottom: 12px;">
        Напоминание: до 15 числа нужно внести показания. Сейчас не внесено: <b>{{ missingCount.missingTotal }}</b>
      </div>

      <div class="table-wrap">
        <table class="table">
          <thead>
            <tr>
              <th style="width: 140px;">Объект</th>
              <th style="width: 220px;">Владелец</th>
              <th style="width: 140px;">Предыдущее</th>
              <th style="width: 140px;">Месяц</th>
              <th style="width: 220px;">Новое показание</th>
              <th style="width: 160px;"></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(row, idx) in filteredRows"
              :key="row.objectId"
              :class="{
                missing: !row.hasReadingForMonth && isOverdue(),
                pending: !row.hasReadingForMonth && !isOverdue()
              }"
            >
              <td class="mono">{{ row.label }}</td>
              <td class="muted">{{ row.owner || '—' }}</td>
              <td>
                <span v-if="row.lastValue !== null" class="mono">{{ row.lastValue }}</span>
                <span v-else class="muted">—</span>
              </td>
              <td>
                <span v-if="row.lastMonth" class="mono">{{ row.lastMonth.slice(0, 7) }}</span>
                <span v-else class="muted">—</span>
              </td>
              <td>
                <input
                  :ref="(el) => setInputRef(el, idx)"
                  v-model="row.newValue"
                  class="input mono"
                  inputmode="decimal"
                  placeholder="например 1234.5"
                  @keydown.enter.prevent="saveRow(row.objectId, idx)"
                />
                <div v-if="row.rowError" class="row-error">{{ row.rowError }}</div>
                <div v-if="!row.hasReadingForMonth" class="row-hint">
                  <span v-if="isOverdue()" class="dot dot-red"></span>
                  <span v-else class="dot dot-amber"></span>
                  {{ isOverdue() ? 'Не внесено до 15 числа' : 'Показание ещё не внесено' }}
                </div>
              </td>
              <td class="actions">
                <button class="btn btn-primary" @click="saveRow(row.objectId, idx)" :disabled="row.saving">
                  {{ row.saving ? '...' : 'Сохранить' }}
                </button>
              </td>
            </tr>

            <tr v-if="filteredRows.length === 0">
              <td colspan="6" class="muted empty">Нет данных</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, ref, watch } from 'vue'
import api from '../services/api'

const loading = ref(false)
const error = ref('')
const search = ref('')

const objectType = ref('apartment') // apartment | storage | parking
const sequenceMode = ref(true)
const selectedMonth = ref(toMonthValue(new Date()))
const missingCount = ref(null)

const rows = ref([])
const inputRefs = ref([])

function setInputRef(el, idx) {
  if (!el) return
  inputRefs.value[idx] = el
}

function toMonthValue(date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

function setCurrentMonth() {
  selectedMonth.value = toMonthValue(new Date())
}

function normalizeNumberString(value) {
  if (value === null || value === undefined) return ''
  return String(value).trim().replace(',', '.')
}

function parseDecimal(value) {
  const s = normalizeNumberString(value)
  if (!s) return null
  const n = Number(s)
  if (!Number.isFinite(n)) return null
  return n
}

const filteredRows = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return rows.value
  return rows.value.filter(r => String(r.label || '').toLowerCase().includes(q))
})

function isOverdue() {
  const today = new Date()
  return today.getDate() >= 15
}

async function loadObjects() {
  if (loading.value) return
  loading.value = true
  error.value = ''
  rows.value = []
  inputRefs.value = []

  try {
    const listEndpoint =
      objectType.value === 'apartment'
        ? '/api/apartments'
        : objectType.value === 'storage'
          ? '/api/storagerooms'
          : '/api/parking'

    const listRes = await api.get(listEndpoint)
    const objects = listRes.data || []

    const mapped = objects.map(o => {
      const id = o.id
      const label =
        objectType.value === 'apartment'
          ? `№ ${o.number}`
          : objectType.value === 'storage'
            ? `№ ${o.label}`
            : `№ ${o.slotNumber}`

      const owner =
        objectType.value === 'apartment'
          ? (o.owners?.[0]?.email || o.owners?.[0]?.firstName || '')
          : (o.users?.[0]?.email || o.users?.[0]?.firstName || '')

      return {
        objectId: id,
        label,
        owner,
        lastValue: null,
        lastMonth: null,
        newValue: '',
        saving: false,
        rowError: ''
      }
    })

    rows.value = mapped

    const ids = mapped.map(r => r.objectId).join(',')
    if (ids) {
      try {
        const statusRes = await api.get('/api/electric-meters/month-status', {
          params: { objectType: objectType.value, objectIds: ids, readingMonth: selectedMonth.value }
        })
        const status = statusRes.data || []
        const byId = new Map(status.map(x => [x.objectId, x]))
        rows.value = rows.value.map(r => {
          const info = byId.get(r.objectId)
          return {
            ...r,
            lastValue: info?.lastValue ?? null,
            lastMonth: info?.lastMonth ?? null,
            hasReadingForMonth: info?.hasReadingForMonth ?? false
          }
        })
      } catch (e) {
        // Backend may be outdated; still show the list without last readings
      }
    }

    try {
      const today = new Date()
      if (today.getDate() >= 15) {
        const missRes = await api.get('/api/electric-meters/missing-count', { params: { readingMonth: selectedMonth.value } })
        missingCount.value = missRes.data || null
      } else {
        missingCount.value = null
      }
    } catch {
      missingCount.value = null
    }

    await nextTick()
    focusFirst()
  } catch (e) {
    error.value = e?.response?.data?.message || 'Не удалось загрузить список'
  } finally {
    loading.value = false
  }
}

function focusFirst() {
  const el = inputRefs.value[0]
  if (el && typeof el.focus === 'function') el.focus()
}

function focusNext(currentIdx) {
  const nextIdx = currentIdx + 1
  const el = inputRefs.value[nextIdx]
  if (el && typeof el.focus === 'function') el.focus()
}

async function saveRow(objectId, filteredIdx) {
  const realIdx = rows.value.findIndex(r => r.objectId === objectId)
  if (realIdx < 0) return

  rows.value[realIdx].rowError = ''

  const value = parseDecimal(rows.value[realIdx].newValue)
  if (value === null) {
    rows.value[realIdx].rowError = 'Введите число'
    return
  }
  if (value < 0) {
    rows.value[realIdx].rowError = 'Не может быть меньше 0'
    return
  }
  if (rows.value[realIdx].lastValue !== null && value < rows.value[realIdx].lastValue) {
    rows.value[realIdx].rowError = `Меньше предыдущего (${rows.value[realIdx].lastValue})`
    return
  }

  rows.value[realIdx].saving = true
  try {
    const res = await api.post('/api/electric-meters/readings', {
      objectType: objectType.value,
      objectId: rows.value[realIdx].objectId,
      readingMonth: selectedMonth.value,
      readingValue: value
    })

    rows.value[realIdx].lastValue = res.data?.value ?? value
    rows.value[realIdx].lastMonth = res.data?.month ?? `${selectedMonth.value}-01`
    rows.value[realIdx].hasReadingForMonth = true
    rows.value[realIdx].newValue = ''

    await nextTick()
    if (sequenceMode.value) focusNext(filteredIdx)
  } catch (e) {
    const msg = e?.response?.data?.message || 'Ошибка сохранения'
    rows.value[realIdx].rowError = msg
  } finally {
    rows.value[realIdx].saving = false
  }
}

function setType(type) {
  objectType.value = type
}

async function reload() {
  await loadObjects()
}

watch(objectType, async () => {
  search.value = ''
  await loadObjects()
})

watch(selectedMonth, async () => {
  await loadObjects()
})

onMounted(() => {
  loadObjects()
})
</script>

<style scoped>
.page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-start;
  flex-wrap: wrap;
}

h1 {
  margin: 0;
}

.subtitle {
  margin: 6px 0 0;
  color: var(--text-muted);
}

.controls {
  display: flex;
  gap: 12px;
  align-items: flex-end;
  flex-wrap: wrap;
}

.month-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.btn-small {
  padding: 10px 12px;
}

.control label {
  display: block;
  font-size: 12px;
  color: var(--text-muted);
  margin-bottom: 6px;
}

.toggle {
  display: flex;
  gap: 8px;
  align-items: center;
  user-select: none;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background: var(--card);
}

.tabs {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.tab {
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid var(--border);
  background: var(--card);
  cursor: pointer;
  font-weight: 600;
}

.tab.active {
  border-color: rgba(15, 157, 88, 0.35);
  background: rgba(15, 157, 88, 0.08);
  color: var(--primary);
}

.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 16px;
}

.table-tools {
  display: flex;
  gap: 12px;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  margin-bottom: 12px;
}

.hint {
  color: var(--text-muted);
  font-size: 12px;
}

.table-wrap {
  overflow: auto;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th,
.table td {
  padding: 10px 8px;
  border-bottom: 1px solid var(--border);
  vertical-align: top;
}

.table th {
  color: var(--text-muted);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.input {
  width: 100%;
  padding: 10px 12px;
  border-radius: 10px;
  border: 1px solid var(--border);
  background: var(--bg);
  color: var(--text);
}

.search {
  max-width: 320px;
}

.mono {
  font-variant-numeric: tabular-nums;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
}

.muted {
  color: var(--text-muted);
}

.actions {
  text-align: right;
}

.row-error {
  margin-top: 6px;
  font-size: 12px;
  color: #d93025;
}

.empty {
  padding: 20px 8px;
  text-align: center;
}

.alert {
  padding: 12px 14px;
  border-radius: 12px;
  border: 1px solid var(--border);
}

.alert-error {
  background: rgba(217, 48, 37, 0.08);
  border-color: rgba(217, 48, 37, 0.25);
  color: #b3261e;
}

.alert-warn {
  background: rgba(255, 193, 7, 0.14);
  border-color: rgba(255, 193, 7, 0.35);
  color: #6a4b00;
}

.row-hint {
  margin-top: 6px;
  font-size: 12px;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 8px;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 999px;
  display: inline-block;
}

.dot-red {
  background: #d93025;
}

.dot-amber {
  background: #f4b400;
}

tr.missing td {
  background: rgba(217, 48, 37, 0.04);
}

tr.pending td {
  background: rgba(244, 180, 0, 0.05);
}

.loading {
  display: flex;
  gap: 10px;
  align-items: center;
  color: var(--text-muted);
}

.spinner {
  width: 18px;
  height: 18px;
  border-radius: 50%;
  border: 2px solid rgba(0, 0, 0, 0.15);
  border-top-color: rgba(15, 157, 88, 0.85);
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 768px) {
  .controls {
    width: 100%;
  }
  .control {
    width: 100%;
  }
  .month-row {
    width: 100%;
  }
  .btn-small {
    width: 100%;
  }
  .search {
    max-width: 100%;
  }
  .actions {
    text-align: left;
  }
}
</style>

