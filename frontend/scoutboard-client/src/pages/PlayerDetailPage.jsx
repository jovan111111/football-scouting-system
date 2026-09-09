import { useEffect, useState } from 'react'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Container,
  Grid,
  LinearProgress,
  Stack,
  Typography,
} from '@mui/material'
import BookmarkAddOutlinedIcon from '@mui/icons-material/BookmarkAddOutlined'
import { Link, useParams } from 'react-router-dom'
import api, { getErrorMessage } from '../api/client'
import { useAuth } from '../context/AuthContext'
import {
  cityLabels,
  footLabels,
  formatDate,
  positionLabels,
  recommendationLabels,
} from '../utils/labels'

export default function PlayerDetailPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const [player, setPlayer] = useState(null)
  const [reports, setReports] = useState([])
  const [message, setMessage] = useState('')

  useEffect(() => {
    Promise.all([api.get(`/players/${id}`), api.get(`/players/${id}/scouting-reports`)])
      .then(([playerResponse, reportsResponse]) => {
        setPlayer(playerResponse.data)
        setReports(reportsResponse.data)
      })
      .catch(() => setPlayer(false))
  }, [id])

  const addToWatchlist = async () => {
    try {
      await api.post('/watchlist', { playerProfileId: Number(id), status: 'Noticed' })
      setMessage('Igrač je dodat na listu praćenja.')
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  if (player === null) return <Container sx={{ py: 8 }}><LinearProgress /></Container>
  if (player === false) return <Container sx={{ py: 8 }}><Alert severity="error">Igrač nije pronađen.</Alert></Container>

  const stats = [
    [player.statistics.appearances, 'Nastupi'],
    [player.statistics.minutesPlayed, 'Minuti'],
    [player.statistics.goals, 'Golovi'],
    [player.statistics.assists, 'Asistencije'],
    [player.statistics.yellowCards, 'Žuti kartoni'],
    [player.statistics.redCards, 'Crveni kartoni'],
  ]

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      <Card sx={{ overflow: 'hidden' }}>
        <Box sx={{ height: 110, bgcolor: 'primary.main' }} />
        <CardContent sx={{ px: { xs: 3, md: 5 }, pb: 5 }}>
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            alignItems={{ xs: 'flex-start', md: 'end' }}
            spacing={3}
            sx={{ mt: -7 }}
          >
            <Avatar
              src={player.profileImageUrl}
              sx={{ width: 126, height: 126, border: '5px solid white', bgcolor: 'secondary.main', color: 'primary.main', fontSize: 34 }}
            >
              {player.firstName[0]}{player.lastName[0]}
            </Avatar>
            <Box sx={{ flex: 1, pt: { xs: 0, md: 7 } }}>
              <Typography variant="h3">{player.firstName} {player.lastName}</Typography>
              <Typography color="text.secondary">
                {positionLabels[player.primaryPosition] || 'Pozicija nije uneta'} · {player.currentClubName || 'Bez kluba'}
              </Typography>
            </Box>
            {user?.role === 'CoachScout' && (
              <Button variant="contained" startIcon={<BookmarkAddOutlinedIcon />} onClick={addToWatchlist}>
                Dodaj na listu
              </Button>
            )}
          </Stack>

          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ mt: 3 }}>
            {player.city && <Chip label={cityLabels[player.city]} />}
            {player.dominantFoot && <Chip label={`${footLabels[player.dominantFoot]} noga`} />}
            {player.heightCm && <Chip label={`${player.heightCm} cm`} />}
            {player.lookingForClub && <Chip color="secondary" label="Traži klub" />}
          </Stack>

          <Typography sx={{ mt: 3, maxWidth: 850 }}>
            {player.biography || 'Igrač još nije uneo biografiju.'}
          </Typography>
        </CardContent>
      </Card>

      <Typography variant="h4" sx={{ mt: 6, mb: 2 }}>Statistika</Typography>
      <Grid container spacing={2}>
        {stats.map(([value, label]) => (
          <Grid key={label} size={{ xs: 6, md: 2 }}>
            <Card><CardContent><Typography variant="h4">{value}</Typography><Typography color="text.secondary">{label}</Typography></CardContent></Card>
          </Grid>
        ))}
      </Grid>

      <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mt: 7, mb: 2 }}>
        <Typography variant="h4">Skautski izveštaji</Typography>
        {user?.role === 'CoachScout' && (
          <Button component={Link} to={`/skautski-izvestaji?playerId=${player.id}`}>
            Napiši izveštaj
          </Button>
        )}
      </Stack>
      {reports.length === 0 && <Alert severity="info">Nema javnih skautskih izveštaja.</Alert>}
      <Grid container spacing={3}>
        {reports.map((report) => (
          <Grid key={report.id} size={{ xs: 12, lg: 6 }}>
            <Card>
              <CardContent sx={{ p: 3 }}>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Box>
                    <Typography variant="h6">{recommendationLabels[report.recommendation]}</Typography>
                    <Typography color="text.secondary">
                      {report.authorName} · {formatDate(report.createdAt)}
                    </Typography>
                  </Box>
                  <Chip label={report.visibility === 'Public' ? 'Javno' : 'Privatno'} />
                </Stack>
                <Grid container spacing={1.5} sx={{ my: 3 }}>
                  {[
                    ['Tehnika', report.technique], ['Brzina', report.speed],
                    ['Dodavanje', report.passing], ['Šut', report.shooting],
                    ['Odbrana', report.defending], ['Fizička sprema', report.physicalCondition],
                    ['Pregled igre', report.gameVision], ['Timski rad', report.teamwork],
                  ].map(([label, value]) => (
                    <Grid key={label} size={{ xs: 6 }}>
                      <Stack direction="row" justifyContent="space-between">
                        <Typography variant="body2">{label}</Typography>
                        <Typography variant="body2" fontWeight={700}>{value}/10</Typography>
                      </Stack>
                      <LinearProgress variant="determinate" value={value * 10} color="secondary" sx={{ mt: 0.5, height: 6, borderRadius: 4 }} />
                    </Grid>
                  ))}
                </Grid>
                <Typography><strong>Prednosti:</strong> {report.strengths}</Typography>
                <Typography sx={{ mt: 1 }}><strong>Za napredak:</strong> {report.weaknesses}</Typography>
                <Typography color="text.secondary" sx={{ mt: 2 }}>{report.comment}</Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Container>
  )
}
