<template>
  <div id="app" :class="{ 'auth-layout': isAuth }">
    <header v-if="!isAuth" class="topbar">
      <div class="brand">
        <span class="brand-badge">PCM PRO</span>
        <div class="brand-title">Vợt Thủ Phố Núi</div>
        <div class="brand-sub">Pickleball Club Management</div>
      </div>
      <nav class="nav-links">
        <RouterLink to="/dashboard">Tổng quan</RouterLink>
        <RouterLink to="/bookings">Đặt sân</RouterLink>
        <RouterLink to="/wallet">Ví điện tử</RouterLink>
        <RouterLink to="/tournaments">Giải đấu</RouterLink>
        <RouterLink to="/leaderboard">BXH</RouterLink>
      </nav>
      <div class="nav-actions">
        <RouterLink to="/profile" class="pill">Hồ sơ</RouterLink>
        <button v-if="isAuthenticated" class="btn btn-ghost" @click="logout">Đăng xuất</button>
      </div>
    </header>

    <main class="page">
      <RouterView />
    </main>

    <footer v-if="!isAuth" class="footer">
      <span>PCM Pro Edition • Vợt Thủ Phố Núi</span>
      <span>Real-time • Secure • Performance</span>
    </footer>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { RouterView, RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const isAuth = computed(() => route.meta.layout === 'auth')
const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  if (authStore.isAuthenticated && !authStore.user) {
    await authStore.fetchUserProfile()
  }
})

const logout = async () => {
  await authStore.logout()
  router.push('/login')
}
</script>
