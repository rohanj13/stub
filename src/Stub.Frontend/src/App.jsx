import { useState } from 'react'
import './App.css'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5077'

const initialForm = {
  merchantName: 'Demo Store',
  merchantAccount: 'sq-account-001',
  webhookMerchantId: '',
  transactionId: 'txn-1001',
  orderId: 'order-1001',
  total: '42.50',
  receiptId: '',
  customerId: 'customer-123',
  lookupCustomerId: 'customer-123',
}

function App() {
  const [form, setForm] = useState(initialForm)
  const [output, setOutput] = useState('Ready.')

  const updateField = (key, value) => {
    setForm((current) => ({ ...current, [key]: value }))
  }

  const callApi = async (path, options) => {
    const response = await fetch(`${apiBaseUrl}${path}`, options)
    let data
    try {
      data = await response.json()
    } catch {
      data = await response.text()
    }

    if (!response.ok) {
      const errorMessage =
        typeof data === 'string' ? data : JSON.stringify(data, null, 2)
      throw new Error(errorMessage)
    }

    return data
  }

  const show = (data) => {
    setOutput(typeof data === 'string' ? data : JSON.stringify(data, null, 2))
  }

  const createMerchant = async () => {
    try {
      const data = await callApi('/api/merchants', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: form.merchantName,
          posAccountId: form.merchantAccount,
        }),
      })
      updateField('webhookMerchantId', data.id ?? '')
      show(data)
    } catch (error) {
      show(error.message)
    }
  }

  const createReceipt = async () => {
    const total = Number(form.total)

    try {
      const data = await callApi('/api/webhooks/square/transactions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          merchantId: form.webhookMerchantId,
          transactionId: form.transactionId,
          orderId: form.orderId,
          total,
          currency: 'USD',
          items: [{ name: 'Sample Item', unitPrice: total, quantity: 1 }],
        }),
      })
      updateField('receiptId', data.id ?? '')
      show(data)
    } catch (error) {
      show(error.message)
    }
  }

  const assignCustomer = async () => {
    try {
      const data = await callApi(
        `/api/receipts/${form.receiptId}/assign-customer`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ customerId: form.customerId }),
        },
      )
      show(data)
    } catch (error) {
      show(error.message)
    }
  }

  const lookupReceipts = async () => {
    try {
      const data = await callApi(
        `/api/customers/${encodeURIComponent(form.lookupCustomerId)}/receipts`,
      )
      show(data)
    } catch (error) {
      show(error.message)
    }
  }

  return (
    <main>
      <h1>Stub Digital Receipts (Boilerplate)</h1>
      <p>
        React frontend running separately from the API backend at
        <code>{apiBaseUrl}</code>.
      </p>

      <section>
        <h2>1) Create Merchant</h2>
        <input
          value={form.merchantName}
          onChange={(event) => updateField('merchantName', event.target.value)}
          placeholder="Merchant Name"
        />
        <input
          value={form.merchantAccount}
          onChange={(event) =>
            updateField('merchantAccount', event.target.value)
          }
          placeholder="Square Account Id"
        />
        <button type="button" onClick={createMerchant}>
          Create Merchant
        </button>
      </section>

      <section>
        <h2>2) Simulate Square Transaction Webhook</h2>
        <input
          value={form.webhookMerchantId}
          onChange={(event) =>
            updateField('webhookMerchantId', event.target.value)
          }
          placeholder="Merchant Id"
        />
        <input
          value={form.transactionId}
          onChange={(event) =>
            updateField('transactionId', event.target.value)
          }
          placeholder="Transaction Id"
        />
        <input
          value={form.orderId}
          onChange={(event) => updateField('orderId', event.target.value)}
          placeholder="Order Id"
        />
        <input
          value={form.total}
          onChange={(event) => updateField('total', event.target.value)}
          placeholder="Total"
        />
        <button type="button" onClick={createReceipt}>
          Generate Receipt
        </button>
      </section>

      <section>
        <h2>3) Assign Stub Card (Customer) to Receipt</h2>
        <input
          value={form.receiptId}
          onChange={(event) => updateField('receiptId', event.target.value)}
          placeholder="Receipt Id"
        />
        <input
          value={form.customerId}
          onChange={(event) => updateField('customerId', event.target.value)}
          placeholder="Customer Id from card scan"
        />
        <button type="button" onClick={assignCustomer}>
          Assign Customer
        </button>
      </section>

      <section>
        <h2>4) Get Receipt History by Customer</h2>
        <input
          value={form.lookupCustomerId}
          onChange={(event) =>
            updateField('lookupCustomerId', event.target.value)
          }
          placeholder="Customer Id"
        />
        <button type="button" onClick={lookupReceipts}>
          Lookup Receipts
        </button>
      </section>

      <h2>Result</h2>
      <pre>{output}</pre>
    </main>
  )
}

export default App
