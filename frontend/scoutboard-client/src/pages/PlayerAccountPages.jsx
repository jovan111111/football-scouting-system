import { useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  MenuItem,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material'
import api, { getErrorMessage } from '../api/client'
import PageHeader from '../components/PageHeader'
import StatusChip from '../components/StatusChip'
import {
  cities,
  cityLabels,
  correctionLabels,
  feet,
  footLabels,
  formatDate,
  positionLabels,
  positions,
  toDateInput,
} from '../utils/labels'

export function MyProfilePage() {
  const [form, setForm] = useState(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    api.get('/players/me').then(({ data }) => setForm({
      dateOfBirth: toDateInput(data.dateOfBirth),
      heightCm: data.heightCm || '',
      dominantFoot: data.dominantFoot || '',
      primaryPosition: data.primaryPosition || '',
      secondaryPosition: data.secondaryPosition || '',
      city: data.city || '',
      biography: data.biography || '',
      profileImageUrl: data.profileImageUrl || '',
      lookingForClub: data.lookingForClub,
    })).catch((requestError) => setError(getErrorMessage(requestError)))
  }, [])

  const change = (name) => (event) => setForm((value) => ({
    ...value,
    [name]: event.target.type === 'checkbox' ? event.target.checked : event.target.value,
  }))

  const submit = async (event) => {
    event.preventDefault()
    setError('')
    try {
      await api.put('/players/me', {
        ...form,
        dateOfBirth: form.dateOfBirth || null,
        heightCm: form.heightCm ? Number(form.heightCm) : null,
        dominantFoot: form.dominantFoot || null,
        primaryPosition: form.primaryPosition || null,
        secondaryPosition: form.secondaryPosition || null,
        city: form.city || null,
        profileImageUrl: form.profileImageUrl || null,
      })
      setMessage('Profil je uspešno sačuvan.')
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  if (!form) return <Container sx={{ py: 7 }}>{error ? <Alert severity="error">{error}</Alert> : 'Učitavanje…'}</Container>

  return (
    <Container maxWidth="md" sx={{ py: 7 }}>
      <PageHeader title="Moj profil" description="Podaci sa ovog obrasca prikazuju se na javnom profilu igrača." />
      <Card>
        <CardContent sx={{ p: { xs: 3, md: 5 } }}>
          <Box component="form" onSubmit={submit}>
            <Stack spacing={2.5}>
              {message && <Alert onClose={() => setMessage('')}>{message}</Alert>}
              {error && <Alert severity="error">{error}</Alert>}
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField fullWidth type="date" label="Datum rođenja" value={form.dateOfBirth} onChange={change('dateOfBirth')} InputLabelProps={{ shrink: true }} />
                <TextField fullWidth type="number" label="Visina (cm)" value={form.heightCm} onChange={change('heightCm')} />
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField fullWidth select label="Mesto" value={form.city} onChange={change('city')}>
                  <MenuItem value="">Nije izabrano</MenuItem>
                  {cities.map((city) => <MenuItem key={city} value={city}>{cityLabels[city]}</MenuItem>)}
                </TextField>
                <TextField fullWidth select label="Dominantna noga" value={form.dominantFoot} onChange={change('dominantFoot')}>
                  <MenuItem value="">Nije izabrano</MenuItem>
                  {feet.map((foot) => <MenuItem key={foot} value={foot}>{footLabels[foot]}</MenuItem>)}
                </TextField>
              </Stack>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField fullWidth select label="Primarna pozicija" value={form.primaryPosition} onChange={change('primaryPosition')}>
                  <MenuItem value="">Nije izabrano</MenuItem>
                  {positions.map((position) => <MenuItem key={position} value={position}>{positionLabels[position]}</MenuItem>)}
                </TextField>
                <TextField fullWidth select label="Sekundarna pozicija" value={form.secondaryPosition} onChange={change('secondaryPosition')}>
                  <MenuItem value="">Nema</MenuItem>
                  {positions.map((position) => <MenuItem key={position} value={position}>{positionLabels[position]}</MenuItem>)}
                </TextField>
              </Stack>
              <TextField label="URL fotografije" value={form.profileImageUrl} onChange={change('profileImageUrl')} />
              <TextField multiline minRows={4} label="Kratka biografija" value={form.biography} onChange={change('biography')} />
              <FormControlLabel control={<Checkbox checked={form.lookingForClub} onChange={change('lookingForClub')} />} label="Želim da moj profil ima oznaku „Traži klub“" />
              <Button type="submit" variant="contained" size="large">Sačuvaj profil</Button>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Container>
  )
}

export function InvitationsPage() {
  const [invitations, setInvitations] = useState([])
  const [message, setMessage] = useState('')

  const load = () => api.get('/memberships/my-invitations').then(({ data }) => setInvitations(data))
  useEffect(load, [])

  const respond = async (id, action) => {
    const { data } = await api.post(`/memberships/${id}/${action}`)
    setMessage(data.message)
    load()
  }

  return (
    <Container maxWidth="lg" sx={{ py: 7 }}>
      <PageHeader title="Pozivi klubova" description="Klub ne može da vas doda u sastav bez vašeg pristanka." />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      {invitations.length === 0 && <Alert severity="info">Trenutno nemate novih poziva.</Alert>}
      <Stack spacing={2}>
        {invitations.map((item) => (
          <Card key={item.id}>
            <CardContent sx={{ p: 3 }}>
              <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ sm: 'center' }} gap={2}>
                <div>
                  <Typography variant="h6">{item.clubName}</Typography>
                  <Typography color="text.secondary">Poziv poslat {formatDate(item.invitedAt)}</Typography>
                </div>
                <Stack direction="row" spacing={1}>
                  <Button variant="outlined" color="error" onClick={() => respond(item.id, 'reject')}>Odbij</Button>
                  <Button variant="contained" onClick={() => respond(item.id, 'accept')}>Prihvati</Button>
                </Stack>
              </Stack>
            </CardContent>
          </Card>
        ))}
      </Stack>
    </Container>
  )
}

export function MyStatisticsPage() {
  const [statistics, setStatistics] = useState([])
  const [corrections, setCorrections] = useState([])
  const [selected, setSelected] = useState(null)
  const [reason, setReason] = useState('')
  const [message, setMessage] = useState('')

  const load = () => Promise.all([
    api.get('/players/me/statistics'),
    api.get('/correction-requests/mine'),
  ]).then(([statisticsResponse, correctionsResponse]) => {
    setStatistics(statisticsResponse.data)
    setCorrections(correctionsResponse.data)
  })
  useEffect(load, [])

  const createCorrection = async () => {
    try {
      await api.post(`/player-statistics/${selected.statisticId}/correction-requests`, { reason })
      setMessage('Zahtev za ispravku je poslat.')
      setSelected(null)
      setReason('')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader title="Moja statistika" description="Zvanične podatke unosi trener. Ako uočite grešku, pošaljite zahtev za ispravku." />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      <TableContainer component={Card}>
        <Table>
          <TableHead><TableRow><TableCell>Utakmica</TableCell><TableCell>Datum</TableCell><TableCell>Min.</TableCell><TableCell>Golovi</TableCell><TableCell>Asist.</TableCell><TableCell /></TableRow></TableHead>
          <TableBody>
            {statistics.map((item) => (
              <TableRow key={item.statisticId}>
                <TableCell>{item.clubName} — {item.opponentName}</TableCell>
                <TableCell>{formatDate(item.matchDate)}</TableCell>
                <TableCell>{item.minutesPlayed}</TableCell>
                <TableCell>{item.goals}</TableCell>
                <TableCell>{item.assists}</TableCell>
                <TableCell><Button onClick={() => setSelected(item)}>Prijavi grešku</Button></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      {statistics.length === 0 && <Alert severity="info" sx={{ mt: 2 }}>Još nema evidentiranih nastupa.</Alert>}

      <Typography variant="h4" sx={{ mt: 6, mb: 2 }}>Zahtevi za ispravku</Typography>
      <Stack spacing={2}>
        {corrections.map((item) => (
          <Card key={item.id}><CardContent>
            <Stack direction="row" justifyContent="space-between" gap={2}>
              <div>
                <Typography fontWeight={700}>{item.matchDescription}</Typography>
                <Typography color="text.secondary">{item.reason}</Typography>
                {item.coachResponse && <Typography sx={{ mt: 1 }}>Odgovor: {item.coachResponse}</Typography>}
              </div>
              <StatusChip label={correctionLabels[item.status]} status={item.status} />
            </Stack>
          </CardContent></Card>
        ))}
      </Stack>

      <Dialog open={Boolean(selected)} onClose={() => setSelected(null)} fullWidth>
        <DialogTitle>Prijavi grešku u statistici</DialogTitle>
        <DialogContent>
          <TextField autoFocus multiline minRows={4} fullWidth label="Opišite šta treba ispraviti" value={reason} onChange={(event) => setReason(event.target.value)} sx={{ mt: 1 }} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSelected(null)}>Odustani</Button>
          <Button variant="contained" disabled={!reason.trim()} onClick={createCorrection}>Pošalji zahtev</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
