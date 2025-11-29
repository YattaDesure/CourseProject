import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('../layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: '/apartments'
      },
      {
        path: 'apartments',
        name: 'Apartments',
        component: () => import('../views/Apartments.vue')
      },
      {
        path: 'parking',
        name: 'Parking',
        component: () => import('../views/Parking.vue')
      },
      {
        path: 'storage',
        name: 'Storage',
        component: () => import('../views/Storage.vue')
      },
      {
        path: 'users',
        name: 'Users',
        component: () => import('../views/Users.vue'),
        meta: { requiresAdmin: true }
      },
      {
        path: 'account',
        name: 'Account',
        component: () => import('../views/Account.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const isAuthenticated = authStore.isAuthenticated
  const userRole = authStore.user?.role

  if (to.meta.requiresAuth && !isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresAdmin && userRole !== 'Admin') {
    next('/apartments')
  } else if (to.path === '/login' && isAuthenticated) {
    next('/apartments')
  } else {
    next()
  }
})

export default router

