import { useEffect, useState } from 'react'
import { Alert, Container, Grid, MenuItem, Stack, TextField } from '@mui/material'
import api from '../api/client'
import ClubCard from '../components/ClubCard'
import PageHeader from '../components/PageHeader'
import { cities, cityLabels } from '../utils/labels'

export default function ClubsPage() {
  const [clubs, setClubs] = useState([])
  const [filters, setFilters] = useState({ search: '', city: '' })

  useEffect(() => {
    const timer = setTimeout(() => {
      const params = Object.fromEntries(Object.entries(filters).filter(([, value]) => value))
      api.get('/clubs', { params }).then(({ data }) => setClubs(data)).catch(() => setClubs([]))
    }, 250)
    return () => clearTimeout(timer)
  }, [filters])

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Lokalni klubovi"
        title="Klubovi Novog Pazara i okoline"
        description="Pregled odobrenih amaterskih klubova i njihovih sastava."
      />
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 4 }}>
        <TextField
          label="Naziv kluba"
          value={filters.search}
          onChange={(event) => setFilters((value) => ({ ...value, search: event.target.value }))}
          sx={{ minWidth: 260 }}
        />
        <TextField
          select
          label="Mesto"
          value={filters.city}
          onChange={(event) => setFilters((value) => ({ ...value, city: event.target.value }))}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">Sva mesta</MenuItem>
          {cities.map((city) => <MenuItem key={city} value={city}>{cityLabels[city]}</MenuItem>)}
        </TextField>
      </Stack>
      {clubs.length === 0 && <Alert severity="info">Nema klubova za izabrane filtere.</Alert>}
      <Grid container spacing={3}>
        {clubs.map((club) => (
          <Grid key={club.id} size={{ xs: 12, sm: 6, lg: 4 }}><ClubCard club={club} /></Grid>
        ))}
      </Grid>
    </Container>
  )
}
