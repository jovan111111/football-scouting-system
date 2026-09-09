import { useEffect, useState } from 'react'
import {
  Alert,
  Button,
  Card,
  CardContent,
  Container,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link } from 'react-router-dom'
import api from '../api/client'
import PageHeader from '../components/PageHeader'
import StatusChip from '../components/StatusChip'
import { cities, cityLabels, formatDate, matchStatusLabels } from '../utils/labels'

export default function MatchesPage() {
  const [matches, setMatches] = useState([])
  const [city, setCity] = useState('')

  useEffect(() => {
    api.get('/matches', { params: city ? { city } : {} })
      .then(({ data }) => setMatches(data))
      .catch(() => setMatches([]))
  }, [city])

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Raspored i rezultati"
        title="Lokalne utakmice"
        description="Predstojeći termini i rezultati utakmica klubova iz regiona."
      />
      <TextField select label="Mesto kluba" value={city} onChange={(event) => setCity(event.target.value)} sx={{ minWidth: 220, mb: 4 }}>
        <MenuItem value="">Sva mesta</MenuItem>
        {cities.map((item) => <MenuItem key={item} value={item}>{cityLabels[item]}</MenuItem>)}
      </TextField>
      {matches.length === 0 && <Alert severity="info">Nema evidentiranih utakmica.</Alert>}
      <Grid container spacing={3}>
        {matches.map((match) => (
          <Grid key={match.id} size={{ xs: 12, md: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardContent sx={{ p: 3 }}>
                <Stack direction="row" justifyContent="space-between" alignItems="center">
                  <Typography color="text.secondary">{formatDate(match.matchDate, true)}</Typography>
                  <StatusChip label={matchStatusLabels[match.status]} status={match.status} />
                </Stack>
                <Typography variant="h6" sx={{ mt: 3 }}>{match.clubName}</Typography>
                <Typography color="text.secondary">{match.opponentName}</Typography>
                <Typography variant="h3" sx={{ my: 3 }}>
                  {match.goalsScored ?? '–'} : {match.goalsConceded ?? '–'}
                </Typography>
                <Typography color="text.secondary" sx={{ mb: 2 }}>{match.venue}</Typography>
                <Button component={Link} to={`/utakmice/${match.id}`} variant="outlined" fullWidth>
                  Detalji
                </Button>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Container>
  )
}
