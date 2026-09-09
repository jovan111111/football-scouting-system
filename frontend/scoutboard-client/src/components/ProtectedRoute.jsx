import { Box, CircularProgress, Typography } from '@mui/material'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function ProtectedRoute({ children, roles }) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'grid', placeItems: 'center' }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!user) {
    return <Navigate to="/prijava" state={{ from: location }} replace />
  }

  if (roles && !roles.includes(user.role)) {
    return (
      <Box sx={{ py: 10, textAlign: 'center' }}>
        <Typography variant="h4">Nemate dozvolu za pristup ovoj stranici.</Typography>
      </Box>
    )
  }

  return children
}
