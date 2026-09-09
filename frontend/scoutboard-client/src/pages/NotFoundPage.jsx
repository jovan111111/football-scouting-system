import { Button, Container, Typography } from '@mui/material'
import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  return (
    <Container sx={{ py: 12, textAlign: 'center' }}>
      <Typography variant="h1" color="secondary.dark">404</Typography>
      <Typography variant="h4" sx={{ mt: 2 }}>Stranica nije pronađena.</Typography>
      <Button component={Link} to="/" variant="contained" sx={{ mt: 4 }}>Povratak na početnu</Button>
    </Container>
  )
}
