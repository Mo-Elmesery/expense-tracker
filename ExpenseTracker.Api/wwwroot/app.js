const apiBase = '/api/expenses';
const currencyFormatter = new Intl.NumberFormat('ar-EG', {
  style: 'currency',
  currency: 'EGP',
  maximumFractionDigits: 2,
});

const elements = {
  form: document.getElementById('expense-form'),
  formMessage: document.getElementById('form-message'),
  title: document.getElementById('title'),
  amount: document.getElementById('amount'),
  expenseDate: document.getElementById('expenseDate'),
  category: document.getElementById('category'),
  notes: document.getElementById('notes'),
  editForm: document.getElementById('edit-form'),
  editId: document.getElementById('edit-id'),
  editTitle: document.getElementById('edit-title'),
  editAmount: document.getElementById('edit-amount'),
  editExpenseDate: document.getElementById('edit-expenseDate'),
  editCategory: document.getElementById('edit-category'),
  editNotes: document.getElementById('edit-notes'),
  editFormMessage: document.getElementById('edit-form-message'),
  editModal: document.getElementById('edit-modal'),
  refreshBtn: document.getElementById('refresh-btn'),
  totalAmount: document.getElementById('total-amount'),
  expenseCount: document.getElementById('expense-count'),
  breakdownList: document.getElementById('breakdown-list'),
  expensesBody: document.getElementById('expenses-body'),
  loadingState: document.getElementById('loading-state'),
  errorState: document.getElementById('error-state'),
  categoryFilter: document.getElementById('category-filter'),
};

const defaultCategories = ['طعام', 'مواصلات', 'سكن', 'فواتير', 'صحة', 'تسوق', 'ترفيه', 'أخرى'];

document.addEventListener('DOMContentLoaded', () => {
  elements.expenseDate.value = new Date().toISOString().slice(0, 10);
  bindEvents();
  loadDashboard();
});

function bindEvents() {
  elements.form.addEventListener('submit', handleSubmit);
  elements.editForm.addEventListener('submit', handleEditSubmit);
  elements.refreshBtn.addEventListener('click', loadDashboard);
  elements.categoryFilter.addEventListener('change', loadExpenses);
}

async function handleSubmit(event) {
  event.preventDefault();
  setFormMessage('');

  const payload = {
    title: elements.title.value.trim(),
    amount: Number(elements.amount.value),
    category: elements.category.value,
    expenseDate: elements.expenseDate.value,
    notes: elements.notes.value.trim() || null,
  };

  try {
    const response = await fetch(apiBase, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      const message = error?.errors
        ? Object.values(error.errors).flat().join(' ')
        : 'تعذر حفظ المصروف.';
      throw new Error(message);
    }

    elements.form.reset();
    elements.expenseDate.value = new Date().toISOString().slice(0, 10);
    setFormMessage('تم حفظ المصروف بنجاح.', 'success');
    await loadDashboard();
  } catch (error) {
    setFormMessage(error.message || 'حدث خطأ غير متوقع.', 'error');
  }
}

async function loadDashboard() {
  await Promise.all([loadSummary(), loadExpenses()]);
}

async function loadSummary() {
  try {
    const summary = await getJson(`${apiBase}/summary`);
    elements.totalAmount.textContent = currencyFormatter.format(summary.totalAmount ?? 0);
    elements.expenseCount.textContent = String(summary.count ?? 0);
    renderBreakdown(summary.breakdown ?? []);
    syncFilterOptions(summary.breakdown ?? []);
    setError('');
  } catch (error) {
    setError(error.message || 'تعذر تحميل الملخص.');
  }
}

async function loadExpenses() {
  elements.loadingState.classList.remove('hidden');
  const selectedCategory = elements.categoryFilter.value;
  const query = selectedCategory ? `?category=${encodeURIComponent(selectedCategory)}` : '';

  try {
    const expenses = await getJson(`${apiBase}${query}`);
    renderExpenses(expenses);
    setError('');
  } catch (error) {
    setError(error.message || 'تعذر تحميل المصروفات.');
  } finally {
    elements.loadingState.classList.add('hidden');
  }
}

function renderBreakdown(items) {
  elements.breakdownList.innerHTML = '';
  if (!items.length) {
    const li = document.createElement('li');
    li.textContent = 'لا توجد بيانات حتى الآن.';
    elements.breakdownList.appendChild(li);
    return;
  }

  for (const item of items) {
    const li = document.createElement('li');
    li.innerHTML = `
      <div>
        <strong>${escapeHtml(item.category)}</strong>
        <div>${item.count} عملية</div>
      </div>
      <strong>${currencyFormatter.format(item.totalAmount)}</strong>
    `;
    elements.breakdownList.appendChild(li);
  }
}

function renderExpenses(expenses) {
  elements.expensesBody.innerHTML = '';

  if (!expenses.length) {
    const row = document.createElement('tr');
    row.innerHTML = '<td colspan="7">لا توجد مصروفات مطابقة.</td>';
    elements.expensesBody.appendChild(row);
    return;
  }

  for (const expense of expenses) {
    const row = document.createElement('tr');
    row.innerHTML = `
      <td>${escapeHtml(expense.title)}</td>
      <td>${escapeHtml(expense.category)}</td>
      <td>${currencyFormatter.format(expense.amount)}</td>
      <td>${formatDate(expense.expenseDate)}</td>
      <td>${escapeHtml(expense.notes || '-')}</td>
      <td>
        <button class="edit-btn" data-id="${expense.id}" data-title="${escapeHtml(expense.title)}" data-amount="${expense.amount}" data-category="${escapeHtml(expense.category)}" data-date="${expense.expenseDate}" data-notes="${escapeHtml(expense.notes || '')}">تعديل</button>
        <button class="delete-btn" data-id="${expense.id}">حذف</button>
      </td>
    `;
    row.querySelector('.delete-btn').addEventListener('click', () => deleteExpense(expense.id));
    row.querySelector('.edit-btn').addEventListener('click', (e) => openEditModal(e.target));
    elements.expensesBody.appendChild(row);
  }
}

async function deleteExpense(id) {
  if (!window.confirm('هل تريد حذف هذا المصروف؟')) {
    return;
  }

  try {
    const response = await fetch(`${apiBase}/${id}`, { method: 'DELETE' });
    if (!response.ok) {
      throw new Error('تعذر حذف المصروف.');
    }
    await loadDashboard();
  } catch (error) {
    setError(error.message || 'تعذر حذف المصروف.');
  }
}

function openEditModal(button) {
  const id = button.dataset.id;
  const title = button.dataset.title;
  const amount = button.dataset.amount;
  const category = button.dataset.category;
  const expenseDate = button.dataset.date;
  const notes = button.dataset.notes || '';

  elements.editId.value = id;
  elements.editTitle.value = title;
  elements.editAmount.value = amount;
  elements.editCategory.value = category;
  elements.editExpenseDate.value = expenseDate;
  elements.editNotes.value = notes;

  elements.editFormMessage.textContent = '';
  elements.editModal.classList.remove('hidden');
  elements.editTitle.focus();
}

function closeEditModal() {
  elements.editModal.classList.add('hidden');
  elements.editForm.reset();
  elements.editFormMessage.textContent = '';
}

async function handleEditSubmit(event) {
  event.preventDefault();

  const id = Number(elements.editId.value);
  const payload = {
    title: elements.editTitle.value.trim(),
    amount: Number(elements.editAmount.value),
    category: elements.editCategory.value,
    expenseDate: elements.editExpenseDate.value,
    notes: elements.editNotes.value.trim() || null,
  };

  try {
    const response = await fetch(`${apiBase}/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      const message = error?.errors
        ? Object.values(error.errors).flat().join(' ')
        : 'تعذر تحديث المصروف.';
      throw new Error(message);
    }

    closeEditModal();
    setFormMessage('تم تحديث المصروف بنجاح.', 'success');
    await loadDashboard();
  } catch (error) {
    elements.editFormMessage.textContent = error.message || 'حدث خطأ أثناء التحديث.';
    elements.editFormMessage.className = 'form-message error';
  }
}

function syncFilterOptions(breakdown) {
  const selected = elements.categoryFilter.value;
  const categories = [...new Set([...defaultCategories, ...breakdown.map(item => item.category)])];
  elements.categoryFilter.innerHTML = '<option value="">كل التصنيفات</option>';

  for (const category of categories) {
    const option = document.createElement('option');
    option.value = category;
    option.textContent = category;
    if (category === selected) {
      option.selected = true;
    }
    elements.categoryFilter.appendChild(option);
  }
}

async function getJson(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error('فشل الاتصال بالخادم.');
  }
  return response.json();
}

function setFormMessage(message, type = '') {
  elements.formMessage.textContent = message;
  elements.formMessage.className = `form-message ${type}`.trim();
}

function setError(message) {
  elements.errorState.textContent = message;
  elements.errorState.classList.toggle('hidden', !message);
}

function formatDate(dateString) {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ar-EG', { dateStyle: 'medium' }).format(date);
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}
