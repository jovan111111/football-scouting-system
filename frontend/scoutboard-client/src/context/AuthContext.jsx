/* oxlint-disable react/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import api from '../api/client'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const stored = sessionStorage.getItem('scoutboard_user')
    return stored ? JSON.parse(stored) : null
  })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = sessionStorage.getItem('scoutboard_token')
    if (!token) {
      setLoading(false)
      return
    }

    api.get('/auth/me')
      .then(({ data }) => {
        setUser(data)
        sessionStorage.setItem('scoutboard_user', JSON.stringify(data))
      })
      .catch(() => {
        sessionStorage.removeItem('scoutboard_token')
        sessionStorage.removeItem('scoutboard_user')
        setUser(null)
      })
      .finally(() => setLoading(false))
  }, [])

  const login = async (credentials) => {
    const { data } = await api.post('/auth/login', credentials)
    sessionStorage.setItem('scoutboard_token', data.token)
    sessionStorage.setItem('scoutboard_user', JSON.stringify(data.user))
    setUser(data.user)
    return data
  }

  const logout = () => {
    sessionStorage.removeItem('scoutboard_token')
    sessionStorage.removeItem('scoutboard_user')
    setUser(null)
  }

  const value = useMemo(
    () => ({ user, loading, login, logout, setUser }),
    [user, loading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => useContext(AuthContext)
