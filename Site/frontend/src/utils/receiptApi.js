/**
 * Нормализация ответов API квитанций (поддержка camelCase и PascalCase от .NET).
 */

function mapReceiptLine(l) {
  return {
    receiptLineId: l.receiptLineId ?? l.ReceiptLineId,
    serviceCode: l.serviceCode ?? l.ServiceCode,
    objectType: l.objectType ?? l.ObjectType,
    objectId: l.objectId ?? l.ObjectId,
    areaSqm: l.areaSqm ?? l.AreaSqm,
    ratePerSqm: l.ratePerSqm ?? l.RatePerSqm,
    amount: l.amount ?? l.Amount
  }
}

export function mapReceiptListItem(x) {
  return {
    receiptId: x.receiptId ?? x.ReceiptId,
    billingMonth: x.billingMonth ?? x.BillingMonth,
    totalAmount: x.totalAmount ?? x.TotalAmount,
    createdAt: x.createdAt ?? x.CreatedAt,
    paymentStatus: x.paymentStatus ?? x.PaymentStatus ?? 'Unpaid',
    paymentDueDate: x.paymentDueDate ?? x.PaymentDueDate,
    paidAt: x.paidAt ?? x.PaidAt,
    paymentReference: x.paymentReference ?? x.PaymentReference
  }
}

export function mapReceiptDetail(d) {
  return {
    receiptId: d.receiptId ?? d.ReceiptId,
    billingMonth: d.billingMonth ?? d.BillingMonth,
    totalAmount: d.totalAmount ?? d.TotalAmount,
    createdAt: d.createdAt ?? d.CreatedAt,
    paymentStatus: d.paymentStatus ?? d.PaymentStatus ?? 'Unpaid',
    paymentDueDate: d.paymentDueDate ?? d.PaymentDueDate,
    paidAt: d.paidAt ?? d.PaidAt,
    paymentReference: d.paymentReference ?? d.PaymentReference,
    payerName: d.payerName ?? d.PayerName ?? '',
    payerEmail: d.payerEmail ?? d.PayerEmail ?? '',
    lines: (d.lines ?? d.Lines ?? []).map(mapReceiptLine)
  }
}
