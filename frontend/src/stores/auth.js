import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import apiClient from '@/api/http'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(localStorage.getItem('accessToken') || null)
  const refreshToken = ref(localStorage.getItem('refreshToken') || null)
  const user = ref(null)

  const isAuthenticated = computed(() => !!accessToken.value)

  async function login(email, password) {
    try {
      const response = await apiClient.post('/api/auth/login', { email, password })
      const { accessToken: token, refreshToken: refresh } = response.data

      accessToken.value = token
      refreshToken.value = refresh
      
      localStorage.setItem('accessToken', token)
      localStorage.setItem('refreshToken', refresh)

      await fetchUserProfile()

      return true
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  }

  async function register(data) {
    try {
      const response = await apiClient.post('/api/auth/register', data)
      const { accessToken: token, refreshToken: refresh } = response.data

      accessToken.value = token
      refreshToken.value = refresh
      
      localStorage.setItem('accessToken', token)
      localStorage.setItem('refreshToken', refresh)

      await fetchUserProfile()

      return true
    } catch (error) {
      console.error('Register error:', error)
      throw error
    }
  }

  async function logout() {
    try {
      await apiClient.post('/api/auth/revoke')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      accessToken.value = null
      refreshToken.value = null
      user.value = null
      
      localStorage.clear()
    }
  }

  async function fetchUserProfile() {
    try {
      const response = await apiClient.get('/api/members/me')
      user.value = response.data
    } catch (error) {
      console.error('Fetch profile error:', error)
    }
  }

  return {
    accessToken,
    refreshToken,
    user,
    isAuthenticated,
    login,
    register,
    logout,
    fetchUserProfile
  }
})
