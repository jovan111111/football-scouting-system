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
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import ScaleOutlinedIcon from '@mui/icons-material/ScaleOutlined'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import {
  cityLabels,
  footLabels,
  positionLabels,
} from '../../utils/labels'

const comparisonMetrics = [
  ['appearances', 'Nastupi', 30],
  ['minutesPlayed', 'Minuti', 2700],
  ['goals', 'Golovi', 30],
  ['assists', 'Asistencije', 30],
  ['yellowCards', 'Žuti kartoni', 15],
  ['redCards', 'Crveni kartoni', 5],
  ['averageRating', 'Prosečna ocena', 10],
]

function PlayerComparisonCard({ player, color }) {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Avatar src={player.profileImageUrl} sx={{ width: 62, height: 62 }}>{player.fullName[0]}</Avatar>
          <Box>
            <Typography variant="h5">{player.fullName}</Typography>
            <Typography color="text.secondary">
              {positionLabels[player.position] || 'Pozicija nije uneta'} · {cityLabels[player.city] || 'Mesto nije uneto'}
            </Typography>
          </Box>
        </Stack>
        <Stack direction="row" spacing={1} sx={{ my: 3 }} flexWrap="wrap">
          <Chip label={`${player.heightCm || '—'} cm`} />
          <Chip label={`Noga: ${footLabels[player.dominantFoot] || '—'}`} />
        </Stack>
        <Stack spacing={2.5}>
          {comparisonMetrics.map(([field, label, maximum]) => {
            const value = player.statistics[field] ?? 0
            return (
              <Box key={field}>
                <Stack direction="row" justifyContent="space-between">
                  <Typography color="text.secondary">{label}</Typography>
                  <Typography fontWeight={800}>{value || '—'}</Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={Math.min(100, Number(value) / maximum * 100)}
                  color={color}
                  sx={{ mt: 0.75, height: 7, borderRadius: 5 }}
                />
              </Box>
            )
          })}
        </Stack>
      </CardContent>
    </Card>
  )
}

export default function PlayerComparisonPage() {
  const [players, setPlayers] = useState([])
  const [firstPlayerId, setFirstPlayerId] = useState('')
  const [secondPlayerId, setSecondPlayerId] = useState('')
  const [season, setSeason] = useState('')
  const [comparison, setComparison] = useState(null)
  const [message, setMessage] = useState('')

  useEffect(() => { api.get('/players').then(({ data }) => setPlayers(data)) }, [])

  const compare = async () => {
    try {
      const { data } = await api.get('/players/compare', {
        params: {
          firstPlayerId,
          secondPlayerId,
          season: season || undefined,
        },
      })
      setComparison(data)
      setMessage('')
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="lg" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Statistička analiza"
        title="Poređenje igrača"
        description="Uporedite profile i zbirnu statistiku, ukupno ili za određenu sezonu."
      />
      <Card sx={{ mb: 4 }}><CardContent sx={{ p: 3 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField fullWidth select label="Prvi igrač" value={firstPlayerId} onChange={(event) => setFirstPlayerId(event.target.value)}>
            {players.map((player) => <MenuItem key={player.id} value={player.id}>{player.firstName} {player.lastName}</MenuItem>)}
          </TextField>
          <TextField fullWidth select label="Drugi igrač" value={secondPlayerId} onChange={(event) => setSecondPlayerId(event.target.value)}>
            {players.filter((player) => String(player.id) !== String(firstPlayerId)).map((player) => <MenuItem key={player.id} value={player.id}>{player.firstName} {player.lastName}</MenuItem>)}
          </TextField>
          <TextField label="Sezona (opciono)" placeholder="2026" value={season} onChange={(event) => setSeason(event.target.value)} sx={{ minWidth: 170 }} />
          <Button variant="contained" startIcon={<ScaleOutlinedIcon />} disabled={!firstPlayerId || !secondPlayerId} onClick={compare}>Uporedi</Button>
        </Stack>
      </CardContent></Card>
      {message && <Alert severity="error" sx={{ mb: 3 }}>{message}</Alert>}
      {comparison && (
        <>
          {comparison.season && <Alert severity="info" sx={{ mb: 3 }}>Prikazana je statistika za sezonu {comparison.season}.</Alert>}
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <PlayerComparisonCard player={comparison.firstPlayer} color="primary" />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <PlayerComparisonCard player={comparison.secondPlayer} color="secondary" />
            </Grid>
          </Grid>
        </>
      )}
    </Container>
  )
}
