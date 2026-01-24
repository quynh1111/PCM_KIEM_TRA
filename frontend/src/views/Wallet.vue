<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Ví điện tử</h1>
      <p class="section-subtitle">Nạp tiền, xem lịch sử và theo dõi số dư theo thời gian thực.</p>
    </section>

    <section class="grid grid-2">
      <div class="card reveal">
        <div class="card-title">Số dư hiện tại</div>
        <div class="stat-value">{{ formatCurrency(balance?.balance) }}</div>
        <p class="muted">Hệ thống không cho phép số dư âm.</p>
        <div v-if="walletError" class="alert">{{ walletError }}</div>
      </div>

      <div class="card reveal">
        <div class="card-title">Nạp tiền</div>
        <div class="form-grid">
          <label>
            <span class="muted">Số tiền</span>
            <input v-model.number="deposit.amount" type="number" min="10000" class="form-control" />
          </label>
          <label>
            <span class="muted">Link ảnh chuyển khoản</span>
            <input v-model="deposit.proofImageUrl" type="url" class="form-control" placeholder="https://..." />
          </label>
          <label>
            <span class="muted">Ghi chú</span>
            <input v-model="deposit.description" type="text" class="form-control" placeholder="Nạp cho booking" />
          </label>
          <button class="btn btn-primary" @click="submitDeposit" :disabled="depositLoading">Gửi yêu cầu nạp</button>
        </div>
        <div v-if="depositResult" class="alert">Đã gửi yêu cầu nạp tiền. Trạng thái: {{ depositResult.status }}</div>
      </div>
    </section>

    <section class="card reveal">
      <div class="card-title">Lịch sử giao dịch</div>
      <table class="table">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>Loại</th>
            <th>Số tiền</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in transactions" :key="t.id">
            <td>{{ formatDate(t.date) }}</td>
            <td>{{ t.type }}</td>
            <td>{{ formatCurrency(t.amount) }}</td>
            <td><span class="badge">{{ t.status }}</span></td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { walletApi } from "@/api/wallet";

const balance = ref(null);
const transactions = ref([]);
const depositLoading = ref(false);
const walletError = ref("");
const depositResult = ref(null);

const deposit = ref({
  amount: 100000,
  proofImageUrl: "",
  description: "",
});

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);
const formatDate = (value) => (value ? new Date(value).toLocaleString("vi-VN") : "--");

const loadWallet = async () => {
  walletError.value = "";
  try {
    balance.value = await walletApi.balance();
    transactions.value = await walletApi.transactions();
  } catch (err) {
    walletError.value = "Không tải được dữ liệu ví.";
  }
};

const submitDeposit = async () => {
  depositLoading.value = true;
  walletError.value = "";
  try {
    depositResult.value = await walletApi.deposit(deposit.value);
    await loadWallet();
  } catch (err) {
    walletError.value = err?.response?.data?.message || "Gửi yêu cầu nạp thất bại.";
  } finally {
    depositLoading.value = false;
  }
};

onMounted(async () => {
  await loadWallet();
});
</script>
