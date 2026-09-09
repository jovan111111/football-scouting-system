import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5272/api',
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('scoutboard_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const getErrorMessage = (error) =>
  error.response?.data?.message ||
  Object.values(error.response?.data?.errors || {}).flat().join(' ') ||
  'Došlo je do greške. Pokušajte ponovo.'

export default api
