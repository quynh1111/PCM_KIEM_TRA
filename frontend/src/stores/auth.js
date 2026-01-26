import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import apiClient from '@/api/http'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref(localStorage.getItem('accessToken') || null)
  const refreshToken = ref(localStorage.getItem('refreshToken') || null)
  const user = ref(null)
  const roles = ref([])

  const isAuthenticated = computed(() => !!accessToken.value)

  const parseRoles = (token) => {
    if (!token) return []
    try {
      const payload = token.split('.')[1]
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
      const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
      const decoded = JSON.parse(atob(padded))
      const claim =
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        decoded.role ||
        decoded.roles
      if (!claim) return []
      return Array.isArray(claim) ? claim : [claim]
    } catch (error) {
      console.error('Role parse error:', error)
      return []
    }
  }

  const setRolesFromToken = (token) => {
    roles.value = parseRoles(token)
  }

  if (accessToken.value) {
    setRolesFromToken(accessToken.value)
  }

  async function login(email, password) {
    try {
      const response = await apiClient.post('/api/auth/login', { email, password })
      const { accessToken: token, refreshToken: refresh } = response.data

      accessToken.value = token
      refreshToken.value = refresh
      setRolesFromToken(token)
      
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
      setRolesFromToken(token)
      
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
      roles.value = []
      
      localStorage.clear()
    }
  }

  async function fetchUserProfile() {
    try {
      const response = await apiClient.get('/api/members/me')
      user.value = response.data
      const me = await apiClient.get('/api/auth/me')
      if (me?.data?.roles) {
        roles.value = me.data.roles
      }
    } catch (error) {
      console.error('Fetch profile error:', error)
    }
  }

  return {
    accessToken,
    refreshToken,
    user,
    roles,
    isAuthenticated,
    login,
    register,
    logout,
    fetchUserProfile,
    hasRole: (role) => roles.value.includes(role)
  }
})
