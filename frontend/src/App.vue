<template>
  <div id="app" :class="{ 'auth-layout': isAuth }">
    <ToastContainer />
    <header v-if="!isAuth" class="topbar">
      <div class="brand">
        <span class="brand-badge">PCM PRO</span>
        <div class="brand-title">Vợt Thủ Phố Núi</div>
        <div class="brand-sub">Quản lý CLB Pickleball</div>
      </div>
      <nav class="nav-links">
        <RouterLink to="/dashboard">Tổng quan</RouterLink>
        <RouterLink v-if="canManageTreasury" to="/treasury">Quỹ</RouterLink>
        <RouterLink v-if="canManageNews" to="/news">Tin tức</RouterLink>
        <RouterLink v-if="isAdmin" to="/courts">Sân</RouterLink>
        <RouterLink to="/bookings">Đặt sân</RouterLink>
        <RouterLink to="/wallet">Ví điện tử</RouterLink>
        <RouterLink to="/tournaments">Giải đấu</RouterLink>
        <RouterLink to="/leaderboard">BXH</RouterLink>
      </nav>
      <div class="nav-actions">
        <span v-if="isAuthenticated" class="role-pill">{{ roleLabel }}</span>
        <RouterLink to="/profile" class="pill">Hồ sơ</RouterLink>
        <button v-if="isAuthenticated" class="btn btn-ghost" @click="logout">Đăng xuất</button>
      </div>
    </header>

    <main class="page">
      <RouterView />
    </main>

    <footer v-if="!isAuth" class="footer">
      <span>PCM Pro Edition - Vợt Thủ Phố Núi</span>
      <span>Thời gian thực - Bảo mật - Hiệu năng</span>
    </footer>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { RouterView, RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import ToastContainer from '@/components/ToastContainer.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const isAuth = computed(() => route.meta.layout === 'auth')
const isAuthenticated = computed(() => authStore.isAuthenticated)
const isAdmin = computed(() => authStore.hasRole('Admin'))
const canManageTreasury = computed(() => authStore.hasRole('Admin') || authStore.hasRole('Treasurer'))
const canManageNews = computed(() => authStore.hasRole('Admin') || authStore.hasRole('Treasurer'))
const roleLabel = computed(() => {
  const map = { Admin: 'Quản trị', Treasurer: 'Thủ quỹ', Member: 'Thành viên' }
  if (!authStore.roles?.length) return map.Member
  return authStore.roles.map((role) => map[role] || role).join(', ')
})

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
