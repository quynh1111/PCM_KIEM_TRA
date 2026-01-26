<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Giải đấu</h1>
      <p class="section-subtitle">
        Đăng ký tham gia, tạo giải đấu mới (Admin/Treasurer) và theo dõi bracket theo thời gian thực.
      </p>
    </section>

    <section v-if="canManage" class="card">
      <div class="card-title">Tạo giải đấu mới</div>
      <div class="form-grid">
        <label>
          <span class="muted">Tên giải đấu</span>
          <input v-model="createForm.name" type="text" class="form-control" />
        </label>
        <label>
          <span class="muted">Mô tả</span>
          <input v-model="createForm.description" type="text" class="form-control" />
        </label>
        <div class="grid grid-2">
          <label>
            <span class="muted">Loại giải</span>
            <select v-model="createForm.type" class="form-control">
              <option v-for="item in typeOptions" :key="item.value" :value="item.value">
                {{ item.label }}
              </option>
            </select>
          </label>
          <label>
            <span class="muted">Thể thức</span>
            <select v-model="createForm.format" class="form-control">
              <option v-for="item in formatOptions" :key="item.value" :value="item.value">
                {{ item.label }}
              </option>
            </select>
          </label>
        </div>
        <div class="grid grid-3">
          <label>
            <span class="muted">Ngày bắt đầu</span>
            <input v-model="createForm.startDate" type="date" class="form-control" />
          </label>
          <label>
            <span class="muted">Ngày kết thúc</span>
            <input v-model="createForm.endDate" type="date" class="form-control" />
          </label>
          <label>
            <span class="muted">Hạn đăng ký</span>
            <input v-model="createForm.registrationDeadline" type="date" class="form-control" />
          </label>
        </div>
        <div class="grid grid-3">
          <label>
            <span class="muted">Phí tham gia</span>
            <input v-model.number="createForm.entryFee" type="number" min="0" class="form-control" />
          </label>
          <label>
            <span class="muted">Quỹ thưởng</span>
            <input v-model.number="createForm.prizePool" type="number" min="0" class="form-control" />
          </label>
          <label>
            <span class="muted">Số lượng tối đa</span>
            <input v-model.number="createForm.maxParticipants" type="number" min="2" class="form-control" />
          </label>
        </div>
        <button class="btn btn-primary" @click="createTournament" :disabled="adminLoading">
          Tạo giải đấu
        </button>
      </div>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">Danh sách giải đấu</div>
        <div v-if="error" class="alert">{{ error }}</div>
        <div class="grid">
          <div v-for="t in tournaments" :key="t.id" class="stat-card">
            <div class="stat-label">{{ t.name }}</div>
            <div class="stat-value">{{ formatFormat(t.format) }} - {{ formatType(t.type) }}</div>
            <div class="muted">Trạng thái: {{ formatStatus(t.status) }}</div>
            <div class="muted">Phí: {{ formatCurrency(t.entryFee) }} | Quỹ: {{ formatCurrency(t.prizePool) }}</div>
            <div class="muted">Đã tham gia: {{ t.currentParticipants }}/{{ t.maxParticipants }}</div>
            <div style="display:flex; gap:10px; margin-top:10px; flex-wrap:wrap;">
              <button
                class="btn btn-primary"
                @click="joinTournament(t.id)"
                :disabled="loading || !canJoin(t)"
              >
                Đăng ký
              </button>
              <button class="btn btn-ghost" @click="loadBracket(t.id)">Xem bracket</button>
              <button
                v-if="canManage"
                class="btn btn-secondary"
                @click="generateBracket(t.id)"
                :disabled="adminLoading"
              >
                Tạo bracket
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="card bracket-panel">
        <div class="card-title">Bracket</div>
        <div v-if="bracket && bracket.nodes?.length" class="bracket">
          <div class="bracket-column" v-for="round in groupedRounds" :key="round.round">
            <div class="bracket-match" v-for="node in round.nodes" :key="node.id">
              <div>{{ node.team1 || 'Chưa có' }} vs {{ node.team2 || 'Chưa có' }}</div>
              <small class="muted">Vòng {{ node.round }}</small>
            </div>
          </div>
        </div>
        <div v-else class="muted">Bracket chưa được tạo.</div>
      </div>
    </section>

    <section class="card">
      <div class="card-title">Quy trình</div>
      <div class="grid grid-3">
        <div class="stat-card">Check-in online trước giờ đấu</div>
        <div class="stat-card">Tạo bracket tự động</div>
        <div class="stat-card">Cập nhật live score</div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { tournamentsApi } from "@/api/tournaments";
import { useAuthStore } from "@/stores/auth";
import { useNotificationsStore } from "@/stores/notifications";

const tournaments = ref([]);
const bracket = ref(null);
const loading = ref(false);
const adminLoading = ref(false);
const error = ref("");
const notifications = useNotificationsStore();
const authStore = useAuthStore();
const canManage = computed(
  () => authStore.hasRole("Admin") || authStore.hasRole("Treasurer")
);

const typeOptions = [
  { value: "Duel", label: "Kèo thách đấu" },
  { value: "MiniGame", label: "Mini-game" },
  { value: "Professional", label: "Chuyên nghiệp" },
];

const formatOptions = [
  { value: "RoundRobin", label: "Vòng tròn" },
  { value: "Knockout", label: "Loại trực tiếp" },
  { value: "TeamBattle", label: "Đội kháng đội" },
];

const createForm = ref({
  name: "",
  description: "",
  type: "Professional",
  format: "Knockout",
  startDate: new Date().toISOString().slice(0, 10),
  endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10),
  registrationDeadline: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10),
  entryFee: 100000,
  prizePool: 1000000,
  maxParticipants: 8,
});

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);

const formatType = (value) =>
  typeOptions.find((item) => item.value === value)?.label || value;

const formatFormat = (value) =>
  formatOptions.find((item) => item.value === value)?.label || value;

const formatStatus = (value) =>
  ({ Open: "Mở đăng ký", Ongoing: "Đang diễn ra", Finished: "Kết thúc" }[value] || value);

const canJoin = (tournament) => {
  if (!tournament) return false;
  if (tournament.status !== "Open") return false;
  if (tournament.maxParticipants > 0 && tournament.currentParticipants >= tournament.maxParticipants) return false;
  return true;
};

const groupedRounds = computed(() => {
  if (!bracket.value?.nodes) return [];
  const map = new Map();
  bracket.value.nodes.forEach((node) => {
    if (!map.has(node.round)) map.set(node.round, []);
    map.get(node.round).push(node);
  });
  return Array.from(map.entries()).map(([round, nodes]) => ({ round, nodes }));
});

const loadTournaments = async () => {
  error.value = "";
  try {
    tournaments.value = await tournamentsApi.list();
  } catch (err) {
    error.value = "Không tải được danh sách giải đấu.";
    notifications.push(error.value, "error");
  }
};

const createTournament = async () => {
  if (!createForm.value.name) {
    notifications.push("Vui lòng nhập tên giải đấu.", "warning");
    return;
  }

  adminLoading.value = true;
  error.value = "";
  try {
    const registrationDeadline = createForm.value.registrationDeadline
      ? `${createForm.value.registrationDeadline}T23:59:59`
      : null;

    await tournamentsApi.create({
      name: createForm.value.name,
      description: createForm.value.description,
      type: createForm.value.type,
      format: createForm.value.format,
      startDate: createForm.value.startDate,
      endDate: createForm.value.endDate,
      registrationDeadline,
      entryFee: createForm.value.entryFee,
      prizePool: createForm.value.prizePool,
      maxParticipants: createForm.value.maxParticipants,
    });
    notifications.push("Tạo giải đấu thành công.", "success");
    await loadTournaments();
  } catch (err) {
    error.value = err?.response?.data?.message || "Tạo giải đấu thất bại.";
    notifications.push(error.value, "error");
  } finally {
    adminLoading.value = false;
  }
};

const joinTournament = async (id) => {
  loading.value = true;
  error.value = "";
  try {
    const res = await tournamentsApi.join(id);
    if (res?.joined) {
      notifications.push("Đăng ký giải đấu thành công.", "success");
    } else {
      notifications.push("Không thể đăng ký giải đấu.", "warning");
    }
    await loadTournaments();
  } catch (err) {
    const message = err?.response?.data?.message || "Đăng ký giải đấu thất bại.";
    const normalized = message.toLowerCase();
    if (normalized.includes("insufficient") || normalized.includes("không đủ") || normalized.includes("số dư")) {
      notifications.push("Số dư ví không đủ để đăng ký.", "warning");
    } else if (normalized.includes("already") || normalized.includes("đã đăng ký")) {
      notifications.push("Bạn đã đăng ký giải đấu này.", "warning");
    } else if (
      normalized.includes("not open") ||
      normalized.includes("closed") ||
      normalized.includes("chưa mở") ||
      normalized.includes("hết hạn")
    ) {
      notifications.push("Giải đấu chưa mở đăng ký.", "warning");
    } else {
      notifications.push(message, "error");
    }
    error.value = message;
  } finally {
    loading.value = false;
  }
};

const loadBracket = async (id) => {
  error.value = "";
  try {
    bracket.value = await tournamentsApi.bracket(id);
    if (!bracket.value?.nodes?.length) {
      notifications.push("Bracket chưa được tạo hoặc chưa đủ người tham gia.", "warning");
    }
  } catch (err) {
    const status = err?.response?.status;
    if (status === 404) {
      error.value = "Bracket chưa được tạo.";
      notifications.push(error.value, "warning");
    } else {
      error.value = "Không tải được bracket.";
      notifications.push(error.value, "error");
    }
  }
};

const generateBracket = async (id) => {
  adminLoading.value = true;
  error.value = "";
  try {
    const res = await tournamentsApi.generateBracket(id);
    if (res?.generated) {
      notifications.push("Tạo bracket thành công.", "success");
    } else {
      notifications.push("Chưa đủ người để tạo bracket.", "warning");
    }
    await loadBracket(id);
  } catch (err) {
    error.value = err?.response?.data?.message || "Tạo bracket thất bại.";
    notifications.push(error.value, "error");
  } finally {
    adminLoading.value = false;
  }
};

onMounted(loadTournaments);
</script>
