/**
 * Форматирование квитанций и строк начислений (список, бланк оплаты).
 */

export function formatReceiptMonth(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString('ru-RU', { year: 'numeric', month: 'long' })
}

export function formatReceiptDate(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString('ru-RU')
}

export function formatReceiptDateTime(iso) {
  if (!iso) return ''
  return new Date(iso).toLocaleString('ru-RU')
}

export function formatReceiptNumber(n) {
  const v = Number(n)
  return Number.isFinite(v) ? v.toFixed(2) : '0.00'
}

export function formatReceiptMoney(n) {
  const v = Number(n)
  const safe = Number.isFinite(v) ? v : 0
  return new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB' }).format(safe)
}

export function receiptServiceLabel(code) {
  if (code === 'CapRepair') return 'Капремонт'
  if (code === 'Electricity') return 'Электроэнергия'
  return code || ''
}

/** Подпись объекта в квитанции (краткий формат). */
export function receiptObjectLabelShort(type, id) {
  if (type === 'Apartment') return `Кв. №${id}`
  if (type === 'StorageRoom') return `Кладовая №${id}`
  if (type === 'ParkingRoom') return `Паркинг №${id}`
  return `${type} #${id}`
}

export function receiptQuantityUnit(code) {
  return code === 'Electricity' ? 'кВт·ч' : 'м²'
}

export function receiptRateUnit(code) {
  return code === 'Electricity' ? '₽/кВт·ч' : '₽/м²'
}

export function receiptPaymentStatusLabel(status) {
  return status === 'Paid' ? 'Оплачено' : 'К оплате'
}
