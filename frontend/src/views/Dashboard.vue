<template>
  <div class="grid">
    <section class="hero reveal">
      <span class="tag">Tổng quan thời gian thực</span>
      <h1>Xin chào {{ displayName }}</h1>
      <p>Quản lý lịch đặt sân, ví điện tử và ranking ELO ngay tại đây.</p>
      <div class="grid grid-3">
        <div class="stat-card">
          <div class="stat-label">Doanh thu</div>
          <div class="stat-value">{{ formatCurrency(sum?.totalIncome) }}</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Chi phí</div>
          <div class="stat-value">{{ formatCurrency(sum?.totalExpense) }}</div>
        </div>
        <div class="stat-card">
          <div class="stat-label">Số dư</div>
          <div class="stat-value" :class="sum?.balance < 0 ? 'text-danger' : 'text-success'">
            {{ formatCurrency(sum?.balance) }}
          </div>
        </div>
      </div>
    </section>

    <section class="grid grid-2">
      <div class="card reveal">
        <div class="card-title">Top xếp hạng</div>
        <ol>
          <li v-for="(m, idx) in top" :key="m.id">
            #{{ idx + 1 }} {{ m.fullName }} - {{ m.rankELO ?? m.rankLevel }}
          </li>
        </ol>
      </div>
      <div class="card reveal">
        <div class="card-title">Trạng thái nhanh</div>
        <div class="grid">
          <div class="stat-card">
            <div class="stat-label">Thời gian thực</div>
            <div class="stat-value">SignalR</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Cache</div>
            <div class="stat-value">Redis</div>
          </div>
          <div class="stat-card">
            <div class="stat-label">Job nền</div>
            <div class="stat-value">Hangfire</div>
          </div>
        </div>
      </div>
    </section>

    <section class="card reveal">
      <div class="card-title">Thông báo ghim</div>
      <div v-if="pinnedNews.length" class="grid">
        <div v-for="item in pinnedNews" :key="item.id" class="stat-card">
          <div class="stat-label">{{ item.title }}</div>
          <div class="muted">{{ item.content }}</div>
        </div>
      </div>
      <div v-else class="muted">Chưa có thông báo.</div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { treasuryApi } from "../api/treasury";
import { membersApi } from "../api/members";
import { newsApi } from "../api/news";
import { useAuthStore } from "../stores/auth";

const sum = ref<any>(null);
const top = ref<any[]>([]);
const pinnedNews = ref<any[]>([]);
const authStore = useAuthStore();

const displayName = computed(() => authStore.user?.fullName || "thành viên");
const canViewTreasury = computed(
  () => authStore.hasRole("Admin") || authStore.hasRole("Treasurer")
);

const formatCurrency = (value?: number) => {
  if (value === null || value === undefined) return "--";
  return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value);
};

onMounted(async () => {
  if (canViewTreasury.value) {
    try {
      sum.value = await treasuryApi.summary();
    } catch {
      sum.value = null;
    }
  }
  top.value = await membersApi.topRanking(5);
  pinnedNews.value = await newsApi.list(true);
});
</script>
