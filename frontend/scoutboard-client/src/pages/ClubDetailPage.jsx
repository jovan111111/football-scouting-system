import { useEffect, useState } from 'react'
import {
  Alert,
  Avatar,
  Box,
  Card,
  CardContent,
  Container,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material'
import { useParams } from 'react-router-dom'
import api from '../api/client'
import PlayerCard from '../components/PlayerCard'
import { cityLabels } from '../utils/labels'

export default function ClubDetailPage() {
  const { id } = useParams()
  const [club, setClub] = useState(null)
  const [players, setPlayers] = useState([])

  useEffect(() => {
    Promise.all([api.get(`/clubs/${id}`), api.get(`/clubs/${id}/players`)])
      .then(([clubResponse, playerResponse]) => {
        setClub(clubResponse.data)
        setPlayers(playerResponse.data)
      })
      .catch(() => setClub(false))
  }, [id])

  if (club === null) return <Container sx={{ py: 8 }}><LinearProgress /></Container>
  if (club === false) return <Container sx={{ py: 8 }}><Alert severity="error">Klub nije pronađen.</Alert></Container>

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <Card>
        <Box sx={{ height: 130, bgcolor: 'primary.main' }} />
        <CardContent sx={{ px: { xs: 3, md: 5 }, pb: 5 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={3} alignItems={{ md: 'end' }} sx={{ mt: -7 }}>
            <Avatar src={club.logoUrl} variant="rounded" sx={{ width: 126, height: 126, border: '5px solid white', bgcolor: 'secondary.main', color: 'primary.main', fontSize: 42 }}>
              {club.name[0]}
            </Avatar>
            <Box sx={{ pt: { md: 7 } }}>
              <Typography variant="h3">{club.name}</Typography>
              <Typography color="text.secondary">
                {cityLabels[club.city]} {club.foundedYear ? `· osnovan ${club.foundedYear}.` : ''}
              </Typography>
            </Box>
          </Stack>
          <Typography sx={{ mt: 3, maxWidth: 850 }}>{club.description}</Typography>
          <Grid container spacing={2} sx={{ mt: 2 }}>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography color="text.secondary">Teren</Typography>
              <Typography fontWeight={700}>{club.stadiumName || 'Nije unet'}</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography color="text.secondary">Adresa</Typography>
              <Typography fontWeight={700}>{club.address || 'Nije uneta'}</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 4 }}>
              <Typography color="text.secondary">Trener/skaut</Typography>
              <Typography fontWeight={700}>{club.ownerName}</Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Typography variant="h4" sx={{ mt: 7, mb: 3 }}>Sastav kluba</Typography>
      {players.length === 0 && <Alert severity="info">Klub trenutno nema aktivne igrače.</Alert>}
      <Grid container spacing={3}>
        {players.map((player) => (
          <Grid key={player.id} size={{ xs: 12, sm: 6, lg: 4 }}><PlayerCard player={player} /></Grid>
        ))}
      </Grid>
    </Container>
  )
}
