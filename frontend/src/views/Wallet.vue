<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Ví điện tử</h1>
      <p class="section-subtitle">Nạp tiền, xem lịch sử ví và theo dõi số dư theo thời gian thực.</p>
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
            <input v-model="deposit.description" type="text" class="form-control" placeholder="Nạp cho đặt sân" />
          </label>
          <button class="btn btn-primary" @click="submitDeposit" :disabled="depositLoading">Gửi yêu cầu nạp</button>
        </div>
        <div v-if="depositResult" class="alert">Đã gửi yêu cầu nạp tiền. Trạng thái: {{ formatStatus(depositResult.status) }}</div>
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
            <td>{{ formatType(t.type) }}</td>
            <td>{{ formatCurrency(t.amount) }}</td>
            <td><span class="badge">{{ formatStatus(t.status) }}</span></td>
          </tr>
        </tbody>
      </table>
    </section>

    <section v-if="canApprove" class="card reveal">
      <div class="card-title">Duyệt nạp tiền</div>
      <div v-if="pendingLoading" class="muted">Đang tải danh sách...</div>
      <div v-else-if="!pendingDeposits.length" class="muted">Không có yêu cầu đang chờ.</div>
      <table v-else class="table">
        <thead>
          <tr>
            <th>Thành viên</th>
            <th>Số tiền</th>
            <th>Minh chứng</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in pendingDeposits" :key="item.id">
            <td>{{ item.memberName }}</td>
            <td>{{ formatCurrency(item.amount) }}</td>
            <td>
              <a v-if="item.proofImageUrl" :href="item.proofImageUrl" target="_blank" rel="noreferrer">Xem ảnh</a>
              <span v-else class="muted">Không có</span>
            </td>
            <td style="display:flex; gap:8px;">
              <button class="btn btn-primary" @click="approveDeposit(item.id, true)" :disabled="pendingLoading">Duyệt</button>
              <button class="btn btn-ghost" @click="approveDeposit(item.id, false)" :disabled="pendingLoading">Từ chối</button>
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from "vue";
import { walletApi } from "@/api/wallet";
import { useAuthStore } from "@/stores/auth";
import { useNotificationsStore } from "@/stores/notifications";

const balance = ref(null);
const transactions = ref([]);
const depositLoading = ref(false);
const walletError = ref("");
const depositResult = ref(null);
const pendingDeposits = ref([]);
const pendingLoading = ref(false);
const authStore = useAuthStore();
const notifications = useNotificationsStore();
const canApprove = computed(
  () => authStore.hasRole("Admin") || authStore.hasRole("Treasurer")
);

const deposit = ref({
  amount: 100000,
  proofImageUrl: "",
  description: "",
});

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);
const formatDate = (value) => (value ? new Date(value).toLocaleString("vi-VN") : "--");
const formatType = (value) =>
  ({
    Deposit: "Nạp tiền",
    PayBooking: "Phí đặt sân",
    PayTournament: "Phí giải đấu",
    ReceivePrize: "Nhận thưởng",
    Refund: "Hoàn tiền",
    Adjustment: "Điều chỉnh",
  }[value] || value);
const formatStatus = (value) =>
  ({
    Pending: "Chờ duyệt",
    Success: "Thành công",
    Failed: "Thất bại",
    Cancelled: "Đã hủy",
  }[value] || value);

const loadWallet = async () => {
  walletError.value = "";
  try {
    balance.value = await walletApi.balance();
    transactions.value = await walletApi.transactions();
  } catch (err) {
    walletError.value = "Không tải được dữ liệu ví.";
    notifications.push(walletError.value, "error");
  }
};

const submitDeposit = async () => {
  depositLoading.value = true;
  walletError.value = "";
  try {
    depositResult.value = await walletApi.deposit(deposit.value);
    notifications.push("Gửi yêu cầu nạp tiền thành công.", "success");
    await loadWallet();
  } catch (err) {
    walletError.value = err?.response?.data?.message || "Gửi yêu cầu nạp thất bại.";
    notifications.push(walletError.value, "error");
  } finally {
    depositLoading.value = false;
  }
};

const loadPendingDeposits = async () => {
  if (!canApprove.value) return;
  pendingLoading.value = true;
  try {
    pendingDeposits.value = await walletApi.pending();
  } catch (err) {
    walletError.value = "Không tải được danh sách chờ duyệt.";
    notifications.push(walletError.value, "error");
  } finally {
    pendingLoading.value = false;
  }
};

const approveDeposit = async (transactionId, approved) => {
  pendingLoading.value = true;
  walletError.value = "";
  try {
    await walletApi.approve({ transactionId, approved });
    notifications.push(approved ? "Đã duyệt nạp tiền." : "Đã từ chối nạp tiền.", "success");
    await loadPendingDeposits();
    await loadWallet();
  } catch (err) {
    walletError.value = err?.response?.data?.message || "Duyệt nạp tiền thất bại.";
    notifications.push(walletError.value, "error");
  } finally {
    pendingLoading.value = false;
  }
};

onMounted(async () => {
  await loadWallet();
  await loadPendingDeposits();
});

watch(canApprove, async (value) => {
  if (value) await loadPendingDeposits();
});
</script>
