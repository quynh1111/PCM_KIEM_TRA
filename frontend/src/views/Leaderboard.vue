<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Bảng xếp hạng ELO</h1>
      <p class="section-subtitle">Cập nhật realtime từ Redis Sorted Sets.</p>
    </section>

    <section class="card reveal">
      <div class="card-title">Top vận động viên</div>
      <ol>
        <li v-for="(m, idx) in leaderboard" :key="m.id">
          #{{ idx + 1 }} {{ m.fullName }} — {{ m.rankELO }}
        </li>
      </ol>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { membersApi } from "@/api/members";

const leaderboard = ref([]);

onMounted(async () => {
  leaderboard.value = await membersApi.topRanking(10);
});
</script>
