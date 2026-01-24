<template>
  <div class="auth-shell">
    <section class="auth-panel">
      <div>
        <span class="tag">PCM Pro Access</span>
        <h1 class="section-title">Chào mừng trở lại</h1>
        <p class="section-subtitle">Đăng nhập để quản lý lịch đặt sân, ví điện tử và bảng xếp hạng thời gian thực.</p>
      </div>
      <form class="form-grid" @submit.prevent="handleLogin">
        <label>
          <span class="muted">Email</span>
          <input
            v-model="email"
            type="email"
            class="form-control"
            placeholder="email@example.com"
            required
          />
        </label>
        <label>
          <span class="muted">Mật khẩu</span>
          <input
            v-model="password"
            type="password"
            class="form-control"
            placeholder="Nhập mật khẩu"
            required
          />
        </label>
        <div v-if="error" class="alert">
          {{ error }}
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Đang xử lý...' : 'Đăng nhập' }}
        </button>
        <router-link to="/register" class="btn btn-ghost">Tạo tài khoản mới</router-link>
      </form>
    </section>
    <aside class="auth-aside">
      <span class="auth-chip">Realtime Booking</span>
      <h2>Vợt Thủ Phố Núi</h2>
      <p>Hệ thống quản lý CLB Pickleball với booking thông minh, ví điện tử và tournament bracket chuyên nghiệp.</p>
    </aside>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleLogin() {
  try {
    loading.value = true
    error.value = ''
    
    await authStore.login(email.value, password.value)
    router.push('/dashboard')
  } catch (err) {
    error.value = err.response?.data?.message || 'Đăng nhập thất bại'
  } finally {
    loading.value = false
  }
}
</script>
