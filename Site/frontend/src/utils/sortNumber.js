// Очень простой helper для сортировки "номеров".
// Сначала пытаемся как число (2 < 10), а если не выходит — сортируем как строку.

export function toggleDir(dir) {
  return dir === 'asc' ? 'desc' : 'asc'
}

export function parseMaybeNumber(value) {
  const s = String(value ?? '').trim().replace(',', '.')
  const n = Number(s)
  return Number.isFinite(n) ? n : null
}

export function compareNumberLike(a, b, dir = 'asc') {
  const sign = dir === 'desc' ? -1 : 1

  const an = parseMaybeNumber(a)
  const bn = parseMaybeNumber(b)
  if (an !== null && bn !== null) return (an - bn) * sign

  const as = String(a ?? '')
  const bs = String(b ?? '')
  return as.localeCompare(bs, 'ru-RU', { numeric: true }) * sign
}

