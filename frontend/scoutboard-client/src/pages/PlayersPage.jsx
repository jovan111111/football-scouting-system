import { useEffect, useState } from 'react'
import {
  Alert,
  Container,
  Grid,
  MenuItem,
  Stack,
  TextField,
} from '@mui/material'
import api from '../api/client'
import PageHeader from '../components/PageHeader'
import PlayerCard from '../components/PlayerCard'
import { cities, cityLabels, positionLabels, positions } from '../utils/labels'

export default function PlayersPage() {
  const [players, setPlayers] = useState([])
  const [filters, setFilters] = useState({ search: '', city: '', position: '', lookingForClub: '' })
  const [error, setError] = useState('')

  useEffect(() => {
    const timer = setTimeout(() => {
      const params = Object.fromEntries(Object.entries(filters).filter(([, value]) => value !== ''))
      api.get('/players', { params })
        .then(({ data }) => {
          setPlayers(data)
          setError('')
        })
        .catch(() => setError('Igrači trenutno ne mogu da se učitaju.'))
    }, 250)
    return () => clearTimeout(timer)
  }, [filters])

  const setFilter = (name) => (event) =>
    setFilters((current) => ({ ...current, [name]: event.target.value }))

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Baza igrača"
        title="Pronađi lokalni talenat"
        description="Pretraži igrače iz Novog Pazara i okoline prema mestu, poziciji i statusu."
      />

      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 4 }}>
        <TextField
          label="Pretraga po imenu"
          value={filters.search}
          onChange={setFilter('search')}
          sx={{ minWidth: 260 }}
        />
        <TextField select label="Mesto" value={filters.city} onChange={setFilter('city')} sx={{ minWidth: 170 }}>
          <MenuItem value="">Sva mesta</MenuItem>
          {cities.map((city) => <MenuItem key={city} value={city}>{cityLabels[city]}</MenuItem>)}
        </TextField>
        <TextField select label="Pozicija" value={filters.position} onChange={setFilter('position')} sx={{ minWidth: 190 }}>
          <MenuItem value="">Sve pozicije</MenuItem>
          {positions.map((position) => <MenuItem key={position} value={position}>{positionLabels[position]}</MenuItem>)}
        </TextField>
        <TextField
          select
          label="Status"
          value={filters.lookingForClub}
          onChange={setFilter('lookingForClub')}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">Svi igrači</MenuItem>
          <MenuItem value="true">Traže klub</MenuItem>
          <MenuItem value="false">Ne traže klub</MenuItem>
        </TextField>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
      {!error && players.length === 0 && <Alert severity="info">Nema igrača za izabrane filtere.</Alert>}
      <Grid container spacing={3}>
        {players.map((player) => (
          <Grid key={player.id} size={{ xs: 12, sm: 6, lg: 4 }}><PlayerCard player={player} /></Grid>
        ))}
      </Grid>
    </Container>
  )
}
