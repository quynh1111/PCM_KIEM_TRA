<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Giải đấu</h1>
      <p class="section-subtitle">Đăng ký tham gia và theo dõi bracket theo thời gian thực.</p>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">Danh sách giải đấu</div>
        <div v-if="error" class="alert">{{ error }}</div>
        <div class="grid">
          <div v-for="t in tournaments" :key="t.id" class="stat-card">
            <div class="stat-label">{{ t.name }}</div>
            <div class="stat-value">{{ t.format }} • {{ t.type }}</div>
            <div class="muted">Phí: {{ formatCurrency(t.entryFee) }} | Quỹ: {{ formatCurrency(t.prizePool) }}</div>
            <div class="muted">Đã tham gia: {{ t.currentParticipants }}/{{ t.maxParticipants }}</div>
            <div style="display:flex; gap:10px; margin-top:10px;">
              <button class="btn btn-primary" @click="joinTournament(t.id)" :disabled="loading">Đăng ký</button>
              <button class="btn btn-ghost" @click="loadBracket(t.id)">Xem bracket</button>
            </div>
          </div>
        </div>
      </div>

      <div class="card bracket-panel">
        <div class="card-title">Bracket</div>
        <div v-if="bracket && bracket.nodes?.length" class="bracket">
          <div class="bracket-column" v-for="round in groupedRounds" :key="round.round">
            <div class="bracket-match" v-for="node in round.nodes" :key="node.id">
              <div>{{ node.team1 || 'TBD' }} vs {{ node.team2 || 'TBD' }}</div>
              <small class="muted">Round {{ node.round }}</small>
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

const tournaments = ref([]);
const bracket = ref(null);
const loading = ref(false);
const error = ref("");

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);

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
  tournaments.value = await tournamentsApi.list();
};

const joinTournament = async (id) => {
  loading.value = true;
  error.value = "";
  try {
    await tournamentsApi.join(id);
    await loadTournaments();
  } catch (err) {
    error.value = err?.response?.data?.message || "Đăng ký giải đấu thất bại.";
  } finally {
    loading.value = false;
  }
};

const loadBracket = async (id) => {
  error.value = "";
  try {
    bracket.value = await tournamentsApi.bracket(id);
  } catch (err) {
    error.value = "Không tải được bracket.";
  }
};

onMounted(loadTournaments);
</script>
