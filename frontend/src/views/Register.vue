<template>
  <div class="auth-shell">
    <section class="auth-panel">
      <div>
        <span class="tag">Thành viên PCM</span>
        <h1 class="section-title">Gia nhập CLB</h1>
        <p class="section-subtitle">Tạo hồ sơ số, theo dõi ranking và ví điện tử ngay khi hoàn tất đăng ký.</p>
      </div>
      <form class="form-grid" @submit.prevent="handleRegister">
        <label>
          <span class="muted">Họ và tên</span>
          <input v-model="form.fullName" type="text" class="form-control" required />
        </label>
        <label>
          <span class="muted">Email</span>
          <input v-model="form.email" type="email" class="form-control" required />
        </label>
        <label>
          <span class="muted">Số điện thoại</span>
          <input v-model="form.phoneNumber" type="tel" class="form-control" required />
        </label>
        <label>
          <span class="muted">Mật khẩu</span>
          <input v-model="form.password" type="password" class="form-control" required />
        </label>
        <label>
          <span class="muted">Ngày sinh (không bắt buộc)</span>
          <input v-model="form.dateOfBirth" type="date" class="form-control" />
        </label>
        <div v-if="error" class="alert">
          {{ error }}
        </div>
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Đang xử lý...' : 'Đăng ký' }}
        </button>
        <router-link to="/login" class="btn btn-ghost">Đã có tài khoản</router-link>
      </form>
    </section>
    <aside class="auth-aside">
      <span class="auth-chip">Tài khoản bảo mật</span>
      <h2>Hồ sơ số &amp; ví nội bộ</h2>
      <p>Hệ thống tự động cập nhật ELO, kiểm soát ví không âm và thông báo realtime.</p>
      <div class="card">
        <div class="card-title">Quyền lợi</div>
        <p class="muted">- Đặt sân linh hoạt</p>
        <p class="muted">- Theo dõi lịch sử ví</p>
        <p class="muted">- Tham gia giải đấu</p>
      </div>
    </aside>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const form = ref({
  fullName: '',
  email: '',
  phoneNumber: '',
  password: '',
  dateOfBirth: null,
  initialRankELO: 1200
})

const error = ref('')
const loading = ref(false)

async function handleRegister() {
  try {
    loading.value = true
    error.value = ''

    await authStore.register(form.value)
    router.push('/dashboard')
  } catch (err) {
    error.value = err.response?.data?.message || 'Đăng ký thất bại'
  } finally {
    loading.value = false
  }
}
</script>
