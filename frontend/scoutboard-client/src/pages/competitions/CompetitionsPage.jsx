import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Button,
  Card,
  CardActionArea,
  CardContent,
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
import SportsScoreOutlinedIcon from '@mui/icons-material/SportsScoreOutlined'
import { Link } from 'react-router-dom'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import StatusChip from '../../components/StatusChip'
import { useAuth } from '../../context/AuthContext'
import { competitionStatusLabels, formatDate } from '../../utils/labels'

const today = () => new Date().toISOString().slice(0, 10)

const inMonths = (months) => {
  const date = new Date()
  date.setMonth(date.getMonth() + months)
  return date.toISOString().slice(0, 10)
}

const createEmptyCompetition = () => ({
  name: '',
  season: String(new Date().getFullYear()),
  description: '',
  startDate: today(),
  endDate: inMonths(6),
  status: 'Planned',
})

export default function CompetitionsPage() {
  const { user } = useAuth()
  const [competitions, setCompetitions] = useState([])
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState(createEmptyCompetition)
  const [message, setMessage] = useState('')
  const canCreate = user && ['CoachScout', 'Admin'].includes(user.role)

  const load = useCallback(() => {
    api.get('/competitions').then(({ data }) => setCompetitions(data))
  }, [])
  useEffect(load, [load])

  const create = async () => {
    try {
      await api.post('/competitions', {
        ...form,
        startDate: new Date(form.startDate).toISOString(),
        endDate: new Date(form.endDate).toISOString(),
      })
      setOpen(false)
      setForm(createEmptyCompetition())
      setMessage('Takmičenje je kreirano.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow="Sezone i rezultati"
        title="Takmičenja"
        description="Lokalne lige i turniri sa automatski obračunatom tabelom."
        action={canCreate && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpen(true)}>
            Novo takmičenje
          </Button>
        )}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      <Grid container spacing={3}>
        {competitions.map((competition) => (
          <Grid key={competition.id} size={{ xs: 12, md: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={`/takmicenja/${competition.id}`} sx={{ height: '100%' }}>
                <CardContent sx={{ p: 3 }}>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <SportsScoreOutlinedIcon color="secondary" />
                    <StatusChip label={competitionStatusLabels[competition.status]} status={competition.status} />
                  </Stack>
                  <Typography variant="h5" sx={{ mt: 3 }}>{competition.name}</Typography>
                  <Typography color="text.secondary">Sezona {competition.season}</Typography>
                  <Divider sx={{ my: 2 }} />
                  <Typography>{formatDate(competition.startDate)} – {formatDate(competition.endDate)}</Typography>
                  <Typography color="text.secondary" sx={{ mt: 1 }}>
                    {competition.clubCount} {competition.clubCount === 1 ? 'klub' : 'klubova'}
                  </Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
      {competitions.length === 0 && <Alert severity="info">Još nema kreiranih takmičenja.</Alert>}

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Novo takmičenje</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField label="Naziv" value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
            <TextField label="Sezona" value={form.season} onChange={(event) => setForm({ ...form, season: event.target.value })} />
            <TextField multiline minRows={3} label="Opis" value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField fullWidth type="date" label="Početak" value={form.startDate} onChange={(event) => setForm({ ...form, startDate: event.target.value })} InputLabelProps={{ shrink: true }} />
              <TextField fullWidth type="date" label="Završetak" value={form.endDate} onChange={(event) => setForm({ ...form, endDate: event.target.value })} InputLabelProps={{ shrink: true }} />
            </Stack>
            <TextField select label="Status" value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value })}>
              {Object.entries(competitionStatusLabels).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Odustani</Button>
          <Button variant="contained" disabled={!form.name || !form.season || !form.description} onClick={create}>Kreiraj</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
