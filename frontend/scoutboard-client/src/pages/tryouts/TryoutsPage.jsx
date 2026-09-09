import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EventAvailableOutlinedIcon from '@mui/icons-material/EventAvailableOutlined'
import { Link } from 'react-router-dom'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import StatusChip from '../../components/StatusChip'
import { useAuth } from '../../context/AuthContext'
import {
  cityLabels,
  formatDate,
  positionLabels,
  positions,
  tryoutStatusLabels,
} from '../../utils/labels'

const createEmptyTryout = () => ({
  clubId: '',
  title: '',
  description: '',
  tryoutDate: '',
  venue: '',
  position: '',
  minimumAge: '',
  maximumAge: '',
  status: 'Open',
})

export default function TryoutsPage() {
  const { user } = useAuth()
  const [tryouts, setTryouts] = useState([])
  const [clubs, setClubs] = useState([])
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState(createEmptyTryout)
  const [message, setMessage] = useState('')

  const load = useCallback(async () => {
    const { data } = await api.get('/tryouts')
    setTryouts(data)
    if (user?.role === 'CoachScout') {
      const clubsResponse = await api.get('/clubs/mine')
      setClubs(clubsResponse.data.filter((club) => club.approvalStatus === 'Approved'))
    }
  }, [user])
  useEffect(() => { load() }, [load])

  const create = async () => {
    try {
      await api.post(`/clubs/${form.clubId}/tryouts`, {
        ...form,
        tryoutDate: new Date(form.tryoutDate).toISOString(),
        position: form.position || null,
        minimumAge: form.minimumAge ? Number(form.minimumAge) : null,
        maximumAge: form.maximumAge ? Number(form.maximumAge) : null,
      })
      setOpen(false)
      setForm(createEmptyTryout())
      setMessage('Proba je objavljena.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Prilika za igrače"
        title="Fudbalske probe"
        description="Pronađite otvorene termine i prijavite se direktno klubu."
        action={user?.role === 'CoachScout' && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpen(true)}>Objavi probu</Button>
        )}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      <Grid container spacing={3}>
        {tryouts.map((tryout) => (
          <Grid key={tryout.id} size={{ xs: 12, md: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={`/probe/${tryout.id}`} sx={{ height: '100%' }}>
                <CardContent sx={{ p: 3 }}>
                  <Stack direction="row" justifyContent="space-between">
                    <EventAvailableOutlinedIcon color="secondary" />
                    <StatusChip label={tryoutStatusLabels[tryout.status]} status={tryout.status} />
                  </Stack>
                  <Typography variant="h5" sx={{ mt: 3 }}>{tryout.title}</Typography>
                  <Typography color="text.secondary">{tryout.clubName} · {cityLabels[tryout.city]}</Typography>
                  <Divider sx={{ my: 2 }} />
                  <Typography>{formatDate(tryout.tryoutDate, true)}</Typography>
                  <Typography color="text.secondary">{tryout.venue}</Typography>
                  <Stack direction="row" spacing={1} sx={{ mt: 2 }} flexWrap="wrap">
                    <Chip size="small" label={tryout.position ? positionLabels[tryout.position] : 'Sve pozicije'} />
                    <Chip size="small" label={`${tryout.applicationCount} prijava`} />
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Objavi fudbalsku probu</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField select label="Klub" value={form.clubId} onChange={(event) => setForm({ ...form, clubId: event.target.value })}>
              {clubs.map((club) => <MenuItem key={club.id} value={club.id}>{club.name}</MenuItem>)}
            </TextField>
            <TextField label="Naslov" value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
            <TextField multiline minRows={3} label="Opis i potrebna oprema" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            <TextField type="datetime-local" label="Datum i vreme" value={form.tryoutDate} onChange={(event) => setForm({ ...form, tryoutDate: event.target.value })} InputLabelProps={{ shrink: true }} />
            <TextField label="Lokacija" value={form.venue} onChange={(event) => setForm({ ...form, venue: event.target.value })} />
            <TextField select label="Tražena pozicija" value={form.position} onChange={(event) => setForm({ ...form, position: event.target.value })}>
              <MenuItem value="">Sve pozicije</MenuItem>
              {positions.map((position) => <MenuItem key={position} value={position}>{positionLabels[position]}</MenuItem>)}
            </TextField>
            <Stack direction="row" spacing={2}>
              <TextField fullWidth type="number" label="Minimalni uzrast" value={form.minimumAge} onChange={(event) => setForm({ ...form, minimumAge: event.target.value })} />
              <TextField fullWidth type="number" label="Maksimalni uzrast" value={form.maximumAge} onChange={(event) => setForm({ ...form, maximumAge: event.target.value })} />
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Odustani</Button>
          <Button variant="contained" disabled={!form.clubId || !form.title || !form.description || !form.tryoutDate || !form.venue} onClick={create}>Objavi</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
