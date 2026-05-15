<template>
  <div v-if="receipt" class="payment-slip" :class="{ 'is-paid': receipt.paymentStatus === 'Paid' }">
    <header class="slip-header">
      <div class="slip-org">
        <div class="slip-org-name">ТСН «Зеленый Квартал»</div>
        <div class="slip-org-meta">
          г. Архангельск, ул. Вологодская, д. 30 · info@greenquarter.ru
        </div>
      </div>
      <div class="slip-status" :class="statusClass">{{ statusText }}</div>
    </header>

    <h2 class="slip-title">Квитанция на оплату</h2>
    <p class="slip-period">Период начисления: <strong>{{ formatReceiptMonth(receipt.billingMonth) }}</strong></p>

    <section class="slip-payer">
      <div><span class="label">Плательщик</span> {{ receipt.payerName || '—' }}</div>
      <div v-if="receipt.payerEmail"><span class="label">Email</span> {{ receipt.payerEmail }}</div>
      <div><span class="label">№ квитанции</span> {{ receipt.paymentReference || receipt.receiptId }}</div>
      <div v-if="receipt.paymentDueDate && receipt.paymentStatus !== 'Paid'">
        <span class="label">Оплатить до</span> {{ formatReceiptDate(receipt.paymentDueDate) }}
      </div>
      <div v-if="receipt.paidAt && receipt.paymentStatus === 'Paid'">
        <span class="label">Оплачено</span> {{ formatReceiptDateTime(receipt.paidAt) }}
      </div>
    </section>

    <div class="table-wrap slip-table-wrap">
      <table class="slip-table">
        <thead>
          <tr>
            <th>Услуга</th>
            <th>Объект</th>
            <th>Кол-во</th>
            <th>Тариф</th>
            <th class="num">Сумма</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="line in receipt.lines" :key="line.receiptLineId">
            <td>{{ receiptServiceLabel(line.serviceCode) }}</td>
            <td>{{ receiptObjectLabelShort(line.objectType, line.objectId) }}</td>
            <td>{{ formatReceiptNumber(line.areaSqm) }} {{ receiptQuantityUnit(line.serviceCode) }}</td>
            <td>{{ formatReceiptNumber(line.ratePerSqm) }} {{ receiptRateUnit(line.serviceCode) }}</td>
            <td class="num">{{ formatReceiptMoney(line.amount) }}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colspan="4" class="num total-label">Итого к оплате</td>
            <td class="num total-value">{{ formatReceiptMoney(receipt.totalAmount) }}</td>
          </tr>
        </tfoot>
      </table>
    </div>

    <section class="slip-pay-block">
      <div class="slip-pay-left">
        <div class="slip-pay-title">Реквизиты для оплаты</div>
        <div class="slip-requisites">
          <div>Получатель: ТСН «Зеленый Квартал»</div>
          <div>ИНН: 2900000000 (демо)</div>
          <div>Банк: ПАО «Демо-Банк»</div>
          <div>Р/с: 40702810XXXXXXXXXXXX</div>
          <div>Назначение: {{ receipt.paymentReference || receipt.receiptId }}</div>
        </div>
        <p class="slip-note">
          Оплата через QR — демонстрационный режим. Реальный перевод не выполняется.
        </p>
      </div>
      <div class="slip-qr-wrap">
        <div class="slip-qr" aria-label="QR-код (заглушка)">
          <div class="qr-grid" />
          <span class="qr-caption">QR для оплаты</span>
        </div>
        <div class="slip-qr-amount">{{ formatReceiptMoney(receipt.totalAmount) }}</div>
      </div>
    </section>

    <div v-if="receipt.paymentStatus !== 'Paid'" class="slip-actions no-print">
      <button type="button" class="btn btn-primary" :disabled="marking" @click="markPaid">
        {{ marking ? 'Сохранение...' : 'Отметить как оплачено' }}
      </button>
      <span class="slip-hint">Только для учёта в системе, без списания в банке.</span>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'
import api from '../services/api'
import {
  formatReceiptMonth,
  formatReceiptDate,
  formatReceiptDateTime,
  formatReceiptNumber,
  formatReceiptMoney,
  receiptServiceLabel,
  receiptObjectLabelShort,
  receiptQuantityUnit,
  receiptRateUnit
} from '../utils/receiptFormat'

const props = defineProps({
  receipt: { type: Object, default: null }
})

const emit = defineEmits(['paid'])

const marking = ref(false)

const statusText = computed(() => {
  if (!props.receipt) return ''
  return props.receipt.paymentStatus === 'Paid' ? 'Оплачено' : 'К оплате'
})

const statusClass = computed(() => {
  if (!props.receipt) return ''
  return props.receipt.paymentStatus === 'Paid' ? 'status-paid' : 'status-unpaid'
})

/** Демо-отметка оплаты (без интеграции с банком). */
async function markPaid() {
  if (!props.receipt?.receiptId) return
  marking.value = true
  try {
    await api.post(`/api/receipts/my/${props.receipt.receiptId}/mark-paid`)
    emit('paid')
  } catch (e) {
    alert(e.response?.data?.message || 'Не удалось отметить оплату')
  } finally {
    marking.value = false
  }
}
</script>

<style scoped>
.payment-slip {
  max-width: 720px;
  margin: 0 auto;
  padding: 20px;
  background: #fff;
  border: 1px solid var(--border);
  border-radius: 12px;
  color: var(--text);
}

.slip-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  padding-bottom: 16px;
  border-bottom: 2px solid var(--primary);
}

.slip-org-name {
  font-weight: 700;
  font-size: 18px;
  color: var(--primary);
}

.slip-org-meta {
  font-size: 12px;
  color: var(--text-muted);
  margin-top: 4px;
}

.slip-status {
  padding: 6px 14px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 700;
  white-space: nowrap;
  flex-shrink: 0;
}

.status-unpaid {
  background: #fff3e0;
  color: #e65100;
}

.status-paid {
  background: #e8f5e9;
  color: #2e7d32;
}

.slip-title {
  margin: 20px 0 8px;
  font-size: 22px;
}

.slip-period {
  color: var(--text-muted);
  margin-bottom: 16px;
}

.slip-payer {
  display: grid;
  gap: 6px;
  font-size: 14px;
  margin-bottom: 20px;
}

.slip-payer .label {
  color: var(--text-muted);
  margin-right: 6px;
}

.slip-table-wrap {
  margin-bottom: 24px;
}

.slip-table {
  width: 100%;
  min-width: 480px;
  border-collapse: collapse;
  font-size: 13px;
}

.slip-table th,
.slip-table td {
  border: 1px solid var(--border);
  padding: 8px 10px;
  text-align: left;
}

.slip-table th {
  background: var(--bg);
  font-weight: 600;
}

.slip-table .num {
  text-align: right;
  white-space: nowrap;
}

.total-label {
  font-weight: 700;
}

.total-value {
  font-weight: 700;
  font-size: 15px;
  color: var(--primary);
}

.slip-pay-block {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 24px;
  align-items: start;
  padding: 16px;
  background: var(--bg);
  border-radius: 10px;
}

.slip-pay-title {
  font-weight: 700;
  margin-bottom: 8px;
}

.slip-requisites {
  font-size: 13px;
  line-height: 1.7;
  color: var(--text);
}

.slip-note {
  margin-top: 12px;
  font-size: 11px;
  color: var(--text-muted);
}

.slip-qr-wrap {
  text-align: center;
}

.slip-qr {
  width: 140px;
  height: 140px;
  border: 2px dashed var(--border);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #fff;
  position: relative;
  overflow: hidden;
}

.qr-grid {
  width: 100px;
  height: 100px;
  background-image:
    linear-gradient(90deg, #102a17 50%, transparent 50%),
    linear-gradient(#102a17 50%, transparent 50%);
  background-size: 8px 8px;
  opacity: 0.85;
}

.qr-caption {
  position: absolute;
  bottom: 6px;
  font-size: 9px;
  color: var(--text-muted);
  background: rgba(255, 255, 255, 0.9);
  padding: 2px 4px;
}

.slip-qr-amount {
  margin-top: 8px;
  font-weight: 700;
  font-size: 16px;
}

.slip-actions {
  margin-top: 20px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}

.slip-hint {
  font-size: 12px;
  color: var(--text-muted);
  flex: 1 1 200px;
}

.is-paid .slip-qr {
  opacity: 0.5;
}

@media print {
  .no-print {
    display: none !important;
  }

  .payment-slip {
    border: none;
    box-shadow: none;
    max-width: 100%;
    padding: 0;
  }
}

@media (max-width: 768px) {
  .payment-slip {
    padding: 14px;
  }

  .slip-header {
    flex-direction: column;
    align-items: stretch;
  }

  .slip-status {
    align-self: flex-start;
  }

  .slip-title {
    font-size: 1.25rem;
  }

  .slip-pay-block {
    grid-template-columns: 1fr;
    justify-items: stretch;
  }

  .slip-qr-wrap {
    justify-self: center;
  }

  .slip-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .slip-actions .btn {
    width: 100%;
  }
}
</style>
