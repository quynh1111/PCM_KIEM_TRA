import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/dashboard'
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/Login.vue'),
      meta: { requiresGuest: true, layout: 'auth' }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/Register.vue'),
      meta: { requiresGuest: true, layout: 'auth' }
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: () => import('@/views/Dashboard.vue'),
      meta: { requiresAuth: true, layout: 'app' }
    },
    {
      path: '/bookings',
      name: 'bookings',
      component: () => import('@/views/Bookings.vue'),
      meta: { requiresAuth: true, layout: 'app' }
    },
    {
      path: '/wallet',
      name: 'wallet',
      component: () => import('@/views/Wallet.vue'),
      meta: { requiresAuth: true, layout: 'app' }
    },
    {
      path: '/tournaments',
      name: 'tournaments',
      component: () => import('@/views/Tournaments.vue'),
      meta: { requiresAuth: true, layout: 'app' }
    },
    {
      path: '/leaderboard',
      name: 'leaderboard',
      component: () => import('@/views/Leaderboard.vue'),
      meta: { layout: 'app', requiresAuth: true }
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('@/views/Profile.vue'),
      meta: { requiresAuth: true, layout: 'app' }
    }
  ]
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresGuest && authStore.isAuthenticated) {
    next('/dashboard')
  } else {
    next()
  }
})

export default router
