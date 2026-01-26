<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Quỹ CLB</h1>
      <p class="section-subtitle">Theo dõi thu chi, quản lý danh mục và giao dịch.</p>
    </section>

    <section class="grid grid-3">
      <div class="stat-card">
        <div class="stat-label">Tổng thu</div>
        <div class="stat-value">{{ formatCurrency(summary?.totalIncome) }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Tổng chi</div>
        <div class="stat-value">{{ formatCurrency(summary?.totalExpense) }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Số dư</div>
        <div class="stat-value" :class="summary?.balance < 0 ? 'text-danger' : 'text-success'">
          {{ formatCurrency(summary?.balance) }}
        </div>
      </div>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">Tạo danh mục</div>
        <div class="form-grid">
          <label>
            <span class="muted">Tên danh mục</span>
            <input v-model="categoryForm.name" type="text" class="form-control" />
          </label>
          <label>
            <span class="muted">Loại</span>
            <select v-model="categoryForm.type" class="form-control">
              <option value="Income">Thu</option>
              <option value="Expense">Chi</option>
            </select>
          </label>
          <button class="btn btn-primary" @click="createCategory" :disabled="loading">
            Thêm danh mục
          </button>
        </div>
        <div style="margin-top:16px;">
          <div class="card-title">Danh mục hiện có</div>
          <table class="table">
            <thead>
              <tr>
                <th>Tên</th>
                <th>Loại</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="cat in categories" :key="cat.id">
                <td>{{ cat.name }}</td>
                <td>{{ cat.type === 'Income' ? 'Thu' : 'Chi' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="card">
        <div class="card-title">Tạo giao dịch</div>
        <div class="form-grid">
          <label>
            <span class="muted">Ngày</span>
            <input v-model="transactionForm.date" type="date" class="form-control" />
          </label>
          <label>
            <span class="muted">Danh mục</span>
            <select v-model="transactionForm.categoryId" class="form-control">
              <option disabled value="">Chọn danh mục</option>
              <option v-for="cat in categories" :key="cat.id" :value="cat.id">
                {{ cat.name }} ({{ cat.type === 'Income' ? 'Thu' : 'Chi' }})
              </option>
            </select>
          </label>
          <label>
            <span class="muted">Loại giao dịch</span>
            <select v-model="transactionForm.type" class="form-control">
              <option value="Income">Thu</option>
              <option value="Expense">Chi</option>
            </select>
          </label>
          <label>
            <span class="muted">Số tiền</span>
            <input v-model.number="transactionForm.amount" type="number" min="1" class="form-control" />
          </label>
          <label>
            <span class="muted">Mô tả</span>
            <input v-model="transactionForm.description" type="text" class="form-control" />
          </label>
          <button class="btn btn-primary" @click="createTransaction" :disabled="loading">
            Thêm giao dịch
          </button>
        </div>
      </div>
    </section>

    <section class="card">
      <div class="card-title">Lịch sử giao dịch</div>
      <table class="table">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>Danh mục</th>
            <th>Số tiền</th>
            <th>Mô tả</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="trx in transactions" :key="trx.id">
            <td>{{ formatDate(trx.date) }}</td>
            <td>{{ trx.categoryName }}</td>
            <td>{{ formatCurrency(trx.amount) }}</td>
            <td>{{ trx.description || "--" }}</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { treasuryApi } from "@/api/treasury";
import { useNotificationsStore } from "@/stores/notifications";

const summary = ref(null);
const categories = ref([]);
const transactions = ref([]);
const loading = ref(false);
const notifications = useNotificationsStore();

const categoryForm = ref({
  name: "",
  type: "Income",
});

const transactionForm = ref({
  date: new Date().toISOString().slice(0, 10),
  categoryId: "",
  type: "Income",
  amount: 100000,
  description: "",
});

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);

const formatDate = (value) => (value ? new Date(value).toLocaleDateString("vi-VN") : "--");

const loadTreasury = async () => {
  try {
    summary.value = await treasuryApi.summary();
    categories.value = await treasuryApi.listCategories();
    transactions.value = await treasuryApi.listTransactions();
  } catch (err) {
    notifications.push("Không tải được dữ liệu quỹ.", "error");
  }
};

const createCategory = async () => {
  if (!categoryForm.value.name) {
    notifications.push("Vui lòng nhập tên danh mục.", "warning");
    return;
  }

  loading.value = true;
  try {
    await treasuryApi.createCategory({
      name: categoryForm.value.name,
      type: categoryForm.value.type,
      scope: "Treasury",
    });
    notifications.push("Tạo danh mục thành công.", "success");
    categoryForm.value.name = "";
    await loadTreasury();
  } catch (err) {
    const message = err?.response?.data?.message || "Không thể tạo danh mục.";
    notifications.push(message, "error");
  } finally {
    loading.value = false;
  }
};

const createTransaction = async () => {
  if (!transactionForm.value.categoryId) {
    notifications.push("Vui lòng chọn danh mục.", "warning");
    return;
  }

  const amount =
    transactionForm.value.type === "Expense"
      ? -Math.abs(transactionForm.value.amount)
      : Math.abs(transactionForm.value.amount);

  loading.value = true;
  try {
    await treasuryApi.createTransaction({
      date: transactionForm.value.date,
      amount,
      description: transactionForm.value.description,
      categoryId: transactionForm.value.categoryId,
    });
    notifications.push("Thêm giao dịch thành công.", "success");
    transactionForm.value.description = "";
    await loadTreasury();
  } catch (err) {
    const message = err?.response?.data?.message || "Không thể tạo giao dịch.";
    notifications.push(message, "error");
  } finally {
    loading.value = false;
  }
};

onMounted(loadTreasury);
</script>
