import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
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
  Grid,
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
import AddIcon from '@mui/icons-material/Add'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import api, { getErrorMessage } from '../api/client'
import PageHeader from '../components/PageHeader'
import StatusChip from '../components/StatusChip'
import {
  approvalLabels,
  cities,
  cityLabels,
  correctionLabels,
  formatDate,
  matchStatusLabels,
  positionLabels,
  positions,
  recommendationLabels,
  watchlistLabels,
} from '../utils/labels'

const emptyClub = {
  name: '', city: 'NoviPazar', foundedYear: '', description: '',
  stadiumName: '', address: '', logoUrl: '',
}

export function MyClubsPage() {
  const [clubs, setClubs] = useState([])
  const [form, setForm] = useState(emptyClub)
  const [editingId, setEditingId] = useState(null)
  const [open, setOpen] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const load = () => api.get('/clubs/mine').then(({ data }) => setClubs(data))
  useEffect(load, [])

  const openCreate = () => {
    setEditingId(null)
    setForm(emptyClub)
    setOpen(true)
  }

  const openEdit = (club) => {
    setEditingId(club.id)
    setForm({
      name: club.name,
      city: club.city,
      foundedYear: club.foundedYear || '',
      description: club.description,
      stadiumName: club.stadiumName || '',
      address: club.address || '',
      logoUrl: club.logoUrl || '',
    })
    setOpen(true)
  }

  const save = async () => {
    try {
      const payload = { ...form, foundedYear: form.foundedYear ? Number(form.foundedYear) : null, logoUrl: form.logoUrl || null }
      if (editingId) await api.put(`/clubs/${editingId}`, payload)
      else await api.post('/clubs', payload)
      setMessage(editingId ? 'Klub je izmenjen.' : 'Klub je poslat administratoru na odobravanje.')
      setOpen(false)
      load()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        title="Moji klubovi"
        description="Novi klub postaje javno vidljiv nakon administratorskog odobrenja."
        action={<Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>Kreiraj klub</Button>}
      />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
      <Grid container spacing={3}>
        {clubs.map((club) => (
          <Grid key={club.id} size={{ xs: 12, md: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardContent sx={{ p: 3 }}>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <div>
                    <Typography variant="h5">{club.name}</Typography>
                    <Typography color="text.secondary">{cityLabels[club.city]}</Typography>
                  </div>
                  <StatusChip label={approvalLabels[club.approvalStatus]} status={club.approvalStatus} />
                </Stack>
                {club.adminNote && <Alert severity="warning" sx={{ mt: 2 }}>{club.adminNote}</Alert>}
                <Typography color="text.secondary" sx={{ my: 3 }}>{club.playerCount} aktivnih igrača</Typography>
                <Stack direction="row" spacing={1}>
                  <Button startIcon={<EditOutlinedIcon />} onClick={() => openEdit(club)}>Izmeni</Button>
                  {club.approvalStatus === 'Approved' && (
                    <Button component={Link} to={`/moji-klubovi/${club.id}`} variant="contained">Upravljaj</Button>
                  )}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
      {clubs.length === 0 && <Alert severity="info">Još niste kreirali klub.</Alert>}

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{editingId ? 'Izmena kluba' : 'Novi klub'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField required label="Naziv kluba" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            <Stack direction="row" spacing={2}>
              <TextField fullWidth select label="Mesto" value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })}>
                {cities.map((city) => <MenuItem key={city} value={city}>{cityLabels[city]}</MenuItem>)}
              </TextField>
              <TextField fullWidth type="number" label="Godina osnivanja" value={form.foundedYear} onChange={(e) => setForm({ ...form, foundedYear: e.target.value })} />
            </Stack>
            <TextField required multiline minRows={3} label="Opis" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <TextField label="Naziv terena" value={form.stadiumName} onChange={(e) => setForm({ ...form, stadiumName: e.target.value })} />
            <TextField label="Adresa" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
            <TextField label="URL grba" value={form.logoUrl} onChange={(e) => setForm({ ...form, logoUrl: e.target.value })} />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Odustani</Button>
          <Button variant="contained" onClick={save} disabled={!form.name.trim() || !form.description.trim()}>Sačuvaj</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}

export function ManageClubPage() {
  const { id } = useParams()
  const [club, setClub] = useState(null)
  const [roster, setRoster] = useState([])
  const [allPlayers, setAllPlayers] = useState([])
  const [allClubs, setAllClubs] = useState([])
  const [competitions, setCompetitions] = useState([])
  const [matches, setMatches] = useState([])
  const [corrections, setCorrections] = useState([])
  const [invitePlayerId, setInvitePlayerId] = useState('')
  const [matchOpen, setMatchOpen] = useState(false)
  const [message, setMessage] = useState('')
  const [matchForm, setMatchForm] = useState({
    competitionId: '', opponentClubId: '', opponentName: '',
    matchDate: '', venue: '', isHomeMatch: true,
  })

  const load = useCallback(async () => {
    const [clubsResponse, publicClubsResponse, competitionsResponse, rosterResponse, playersResponse, matchesResponse, correctionsResponse] = await Promise.all([
      api.get('/clubs/mine'),
      api.get('/clubs'),
      api.get('/competitions'),
      api.get(`/clubs/${id}/players`),
      api.get('/players'),
      api.get('/matches', { params: { clubId: id } }),
      api.get(`/clubs/${id}/correction-requests`),
    ])
    setClub(clubsResponse.data.find((item) => item.id === Number(id)))
    setAllClubs(publicClubsResponse.data)
    setCompetitions(competitionsResponse.data)
    setRoster(rosterResponse.data)
    setAllPlayers(playersResponse.data)
    setMatches(matchesResponse.data)
    setCorrections(correctionsResponse.data)
  }, [id])
  useEffect(() => { load().catch(() => setClub(false)) }, [load])

  const invite = async () => {
    try {
      const { data } = await api.post(`/clubs/${id}/invitations`, { playerProfileId: Number(invitePlayerId) })
      setMessage(`Poziv je poslat igraču ${data.playerName}.`)
      setInvitePlayerId('')
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const createMatch = async () => {
    try {
      await api.post(`/clubs/${id}/matches`, {
        competitionId: matchForm.competitionId ? Number(matchForm.competitionId) : null,
        opponentClubId: matchForm.opponentClubId ? Number(matchForm.opponentClubId) : null,
        opponentName: matchForm.opponentClubId ? null : matchForm.opponentName,
        matchDate: new Date(matchForm.matchDate).toISOString(),
        venue: matchForm.venue,
        isHomeMatch: matchForm.isHomeMatch,
        goalsScored: null,
        goalsConceded: null,
        status: 'Scheduled',
        matchReport: null,
      })
      setMatchOpen(false)
      setMessage('Utakmica je uspešno zakazana.')
      setMatchForm({
        competitionId: '', opponentClubId: '', opponentName: '',
        matchDate: '', venue: '', isHomeMatch: true,
      })
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const resolveCorrection = async (correction, accepted) => {
    const coachResponse = window.prompt('Unesite odgovor igraču:')
    if (!coachResponse) return
    await api.put(`/correction-requests/${correction.id}/resolve`, { accepted, coachResponse })
    setMessage('Zahtev je obrađen.')
    load()
  }

  if (club === null) return <Container sx={{ py: 7 }}>Učitavanje…</Container>
  if (club === false) return <Container sx={{ py: 7 }}><Alert severity="error">Klub nije pronađen.</Alert></Container>

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow={cityLabels[club.city]}
        title={club.name}
        description="Upravljanje sastavom, utakmicama i zahtevima za ispravku."
        action={<Button variant="contained" startIcon={<AddIcon />} onClick={() => setMatchOpen(true)}>Zakaži utakmicu</Button>}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}

      <Typography variant="h4" sx={{ mb: 2 }}>Sastav i pozivi</Typography>
      <Card sx={{ mb: 5 }}>
        <CardContent sx={{ p: 3 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mb: 3 }}>
            <TextField select label="Izaberite igrača" value={invitePlayerId} onChange={(e) => setInvitePlayerId(e.target.value)} sx={{ minWidth: 280 }}>
              {allPlayers.filter((player) => !roster.some((member) => member.id === player.id)).map((player) => (
                <MenuItem key={player.id} value={player.id}>{player.firstName} {player.lastName}</MenuItem>
              ))}
            </TextField>
            <Button variant="contained" disabled={!invitePlayerId} onClick={invite}>Pošalji poziv</Button>
          </Stack>
          <Table size="small">
            <TableHead><TableRow><TableCell>Igrač</TableCell><TableCell>Pozicija</TableCell><TableCell>Mesto</TableCell><TableCell>Nastupi</TableCell></TableRow></TableHead>
            <TableBody>
              {roster.map((player) => (
                <TableRow key={player.id}>
                  <TableCell><Button component={Link} to={`/igraci/${player.id}`}>{player.firstName} {player.lastName}</Button></TableCell>
                  <TableCell>{positionLabels[player.primaryPosition] || '—'}</TableCell>
                  <TableCell>{cityLabels[player.city] || '—'}</TableCell>
                  <TableCell>{player.appearances}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Typography variant="h4" sx={{ mb: 2 }}>Utakmice</Typography>
      <Grid container spacing={2} sx={{ mb: 6 }}>
        {matches.map((match) => (
          <Grid key={match.id} size={{ xs: 12, md: 6, lg: 4 }}>
            <Card><CardContent>
              <Stack direction="row" justifyContent="space-between">
                <Typography color="text.secondary">{formatDate(match.matchDate)}</Typography>
                <StatusChip label={matchStatusLabels[match.status]} status={match.status} />
              </Stack>
              <Typography variant="h6" sx={{ my: 2 }}>{match.clubName} — {match.opponentName}</Typography>
              <Typography variant="h4">{match.goalsScored ?? '–'} : {match.goalsConceded ?? '–'}</Typography>
              <Button component={Link} to={`/upravljanje-utakmicom/${match.id}`} sx={{ mt: 2 }}>Upravljaj utakmicom</Button>
            </CardContent></Card>
          </Grid>
        ))}
      </Grid>

      <Typography variant="h4" sx={{ mb: 2 }}>Zahtevi za ispravku</Typography>
      {corrections.length === 0 && <Alert severity="info">Nema zahteva za ispravku statistike.</Alert>}
      <Stack spacing={2}>
        {corrections.map((item) => (
          <Card key={item.id}><CardContent>
            <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={2}>
              <div>
                <Typography fontWeight={700}>{item.playerName} · {item.matchDescription}</Typography>
                <Typography color="text.secondary">{item.reason}</Typography>
              </div>
              {item.status === 'Pending' ? (
                <Stack direction="row" spacing={1}>
                  <Button color="error" onClick={() => resolveCorrection(item, false)}>Odbij</Button>
                  <Button variant="contained" onClick={() => resolveCorrection(item, true)}>Prihvati</Button>
                </Stack>
              ) : <StatusChip label={correctionLabels[item.status]} status={item.status} />}
            </Stack>
          </CardContent></Card>
        ))}
      </Stack>

      <Dialog open={matchOpen} onClose={() => setMatchOpen(false)} fullWidth>
        <DialogTitle>Nova utakmica</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField select label="Takmičenje (opciono)" value={matchForm.competitionId} onChange={(e) => setMatchForm({ ...matchForm, competitionId: e.target.value, opponentName: e.target.value ? '' : matchForm.opponentName })}>
              <MenuItem value="">Prijateljska utakmica</MenuItem>
              {competitions.map((competition) => <MenuItem key={competition.id} value={competition.id}>{competition.name} · {competition.season}</MenuItem>)}
            </TextField>
            <TextField select label="Registrovani protivnik" value={matchForm.opponentClubId} onChange={(e) => setMatchForm({ ...matchForm, opponentClubId: e.target.value, opponentName: e.target.value ? '' : matchForm.opponentName })}>
              <MenuItem value="">Ručno unesite protivnika</MenuItem>
              {allClubs.filter((item) => item.id !== Number(id)).map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}
            </TextField>
            <TextField disabled={Boolean(matchForm.opponentClubId)} label="Naziv neregistrovanog protivnika" value={matchForm.opponentName} onChange={(e) => setMatchForm({ ...matchForm, opponentName: e.target.value })} />
            <TextField type="datetime-local" label="Datum i vreme" value={matchForm.matchDate} onChange={(e) => setMatchForm({ ...matchForm, matchDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            <TextField label="Mesto odigravanja" value={matchForm.venue} onChange={(e) => setMatchForm({ ...matchForm, venue: e.target.value })} />
            <FormControlLabel control={<Checkbox checked={matchForm.isHomeMatch} onChange={(e) => setMatchForm({ ...matchForm, isHomeMatch: e.target.checked })} />} label="Domaća utakmica" />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMatchOpen(false)}>Odustani</Button>
          <Button variant="contained" disabled={(!matchForm.opponentClubId && !matchForm.opponentName) || !matchForm.matchDate || !matchForm.venue} onClick={createMatch}>Sačuvaj</Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}

export function ManageMatchPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [match, setMatch] = useState(null)
  const [form, setForm] = useState(null)
  const [rows, setRows] = useState([])
  const [message, setMessage] = useState('')

  useEffect(() => {
    api.get(`/matches/${id}`).then(async ({ data }) => {
      setMatch(data)
      setForm({
        competitionId: data.competitionId,
        opponentClubId: data.opponentClubId,
        opponentName: data.opponentClubId ? null : data.opponentName,
        matchDate: data.matchDate.slice(0, 16),
        venue: data.venue,
        isHomeMatch: data.isHomeMatch,
        goalsScored: data.goalsScored ?? '',
        goalsConceded: data.goalsConceded ?? '',
        status: data.status,
        matchReport: data.matchReport || '',
      })
      const rosterResponse = await api.get(`/clubs/${data.clubId}/players`)
      setRows(rosterResponse.data.map((player) => {
        const existing = data.playerStatistics.find((item) => item.playerProfileId === player.id)
        return {
          playerProfileId: player.id,
          playerName: `${player.firstName} ${player.lastName}`,
          selected: Boolean(existing),
          wasStarter: existing?.wasStarter || false,
          minutesPlayed: existing?.minutesPlayed || 0,
          goals: existing?.goals || 0,
          assists: existing?.assists || 0,
          yellowCards: existing?.yellowCards || 0,
          redCard: existing?.redCard || false,
          rating: existing?.rating || '',
        }
      }))
    }).catch(() => setMatch(false))
  }, [id])

  const saveMatch = async () => {
    try {
      const { data } = await api.put(`/matches/${id}`, {
        ...form,
        matchDate: new Date(form.matchDate).toISOString(),
        goalsScored: form.goalsScored === '' ? null : Number(form.goalsScored),
        goalsConceded: form.goalsConceded === '' ? null : Number(form.goalsConceded),
      })
      setMatch(data)
      setMessage('Podaci o utakmici su sačuvani.')
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const saveStatistics = async () => {
    try {
      await api.put(`/matches/${id}/statistics`, {
        players: rows.filter((row) => row.selected).map((row) => ({
          playerProfileId: row.playerProfileId,
          wasStarter: row.wasStarter,
          minutesPlayed: Number(row.minutesPlayed),
          goals: Number(row.goals),
          assists: Number(row.assists),
          yellowCards: Number(row.yellowCards),
          redCard: row.redCard,
          rating: row.rating === '' ? null : Number(row.rating),
        })),
      })
      setMessage('Statistika igrača je sačuvana.')
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const updateRow = (index, field, value) =>
    setRows((current) => current.map((row, rowIndex) => rowIndex === index ? { ...row, [field]: value } : row))

  if (match === null) return <Container sx={{ py: 7 }}>Učitavanje…</Container>
  if (match === false) return <Container sx={{ py: 7 }}><Alert severity="error">Utakmica nije pronađena.</Alert></Container>

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        title={`${match.clubName} — ${match.opponentName}`}
        description="Unesite rezultat, izveštaj i individualnu statistiku."
        action={<Button onClick={() => navigate(`/moji-klubovi/${match.clubId}`)}>Nazad na klub</Button>}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      <Card sx={{ mb: 5 }}><CardContent sx={{ p: 3 }}>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth type="datetime-local" label="Datum i vreme" value={form.matchDate} onChange={(e) => setForm({ ...form, matchDate: e.target.value })} InputLabelProps={{ shrink: true }} /></Grid>
          <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Mesto" value={form.venue} onChange={(e) => setForm({ ...form, venue: e.target.value })} /></Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <TextField fullWidth select label="Status" value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
              {Object.entries(matchStatusLabels).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}
            </TextField>
          </Grid>
          <Grid size={{ xs: 6, md: 2 }}><TextField fullWidth type="number" label="Golovi kluba" value={form.goalsScored} onChange={(e) => setForm({ ...form, goalsScored: e.target.value })} /></Grid>
          <Grid size={{ xs: 6, md: 2 }}><TextField fullWidth type="number" label="Golovi protivnika" value={form.goalsConceded} onChange={(e) => setForm({ ...form, goalsConceded: e.target.value })} /></Grid>
          <Grid size={{ xs: 12, md: 8 }}><TextField fullWidth label="Izveštaj sa utakmice" value={form.matchReport} onChange={(e) => setForm({ ...form, matchReport: e.target.value })} /></Grid>
        </Grid>
        <Button variant="contained" sx={{ mt: 3 }} onClick={saveMatch}>Sačuvaj utakmicu</Button>
      </CardContent></Card>

      <Typography variant="h4" sx={{ mb: 2 }}>Statistika igrača</Typography>
      {form.status !== 'Completed' && <Alert severity="info" sx={{ mb: 2 }}>Prvo označite utakmicu kao završenu i sačuvajte rezultat.</Alert>}
      <TableContainer component={Card}>
        <Table size="small">
          <TableHead><TableRow><TableCell>Igrao</TableCell><TableCell>Igrač</TableCell><TableCell>Starter</TableCell><TableCell>Min.</TableCell><TableCell>Golovi</TableCell><TableCell>Asist.</TableCell><TableCell>Žuti</TableCell><TableCell>Ocena</TableCell><TableCell>Crveni</TableCell></TableRow></TableHead>
          <TableBody>
            {rows.map((row, index) => (
              <TableRow key={row.playerProfileId}>
                <TableCell><Checkbox checked={row.selected} onChange={(e) => updateRow(index, 'selected', e.target.checked)} /></TableCell>
                <TableCell>{row.playerName}</TableCell>
                <TableCell><Checkbox disabled={!row.selected} checked={row.wasStarter} onChange={(e) => updateRow(index, 'wasStarter', e.target.checked)} /></TableCell>
                {['minutesPlayed', 'goals', 'assists', 'yellowCards'].map((field) => (
                  <TableCell key={field}><TextField disabled={!row.selected} type="number" value={row[field]} onChange={(e) => updateRow(index, field, e.target.value)} sx={{ width: 76 }} /></TableCell>
                ))}
                <TableCell><TextField disabled={!row.selected} type="number" value={row.rating} inputProps={{ min: 1, max: 10, step: 0.1 }} onChange={(e) => updateRow(index, 'rating', e.target.value)} sx={{ width: 76 }} /></TableCell>
                <TableCell><Checkbox disabled={!row.selected} checked={row.redCard} onChange={(e) => updateRow(index, 'redCard', e.target.checked)} /></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <Button variant="contained" disabled={form.status !== 'Completed'} sx={{ mt: 2 }} onClick={saveStatistics}>Sačuvaj statistiku</Button>
    </Container>
  )
}

const emptyReport = {
  technique: 5, speed: 5, passing: 5, shooting: 5, defending: 5,
  physicalCondition: 5, gameVision: 5, teamwork: 5,
  strengths: '', weaknesses: '', comment: '', recommendedPosition: '',
  recommendation: 'Follow', visibility: 'Public',
}

export function ScoutingReportsPage() {
  const [searchParams] = useSearchParams()
  const [players, setPlayers] = useState([])
  const [reports, setReports] = useState([])
  const [playerId, setPlayerId] = useState(searchParams.get('playerId') || '')
  const [form, setForm] = useState(emptyReport)
  const [editingId, setEditingId] = useState(null)
  const [open, setOpen] = useState(Boolean(searchParams.get('playerId')))
  const [message, setMessage] = useState('')

  const load = () => Promise.all([api.get('/players'), api.get('/scouting-reports/mine')])
    .then(([playersResponse, reportsResponse]) => {
      setPlayers(playersResponse.data)
      setReports(reportsResponse.data)
    })
  useEffect(load, [])

  const openCreate = () => {
    setEditingId(null)
    setForm(emptyReport)
    setOpen(true)
  }

  const openEdit = (report) => {
    setEditingId(report.id)
    setPlayerId(String(report.playerProfileId))
    setForm({
      technique: report.technique,
      speed: report.speed,
      passing: report.passing,
      shooting: report.shooting,
      defending: report.defending,
      physicalCondition: report.physicalCondition,
      gameVision: report.gameVision,
      teamwork: report.teamwork,
      strengths: report.strengths,
      weaknesses: report.weaknesses,
      comment: report.comment,
      recommendedPosition: report.recommendedPosition || '',
      recommendation: report.recommendation,
      visibility: report.visibility,
    })
    setOpen(true)
  }

  const save = async () => {
    try {
      const payload = {
        ...form,
        recommendedPosition: form.recommendedPosition || null,
      }
      if (editingId) await api.put(`/scouting-reports/${editingId}`, payload)
      else await api.post(`/players/${playerId}/scouting-reports`, payload)
      setOpen(false)
      setEditingId(null)
      setForm(emptyReport)
      setMessage(editingId ? 'Skautski izveštaj je izmenjen.' : 'Skautski izveštaj je sačuvan.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const remove = async (id) => {
    await api.delete(`/scouting-reports/${id}`)
    setMessage('Izveštaj je obrisan.')
    load()
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader title="Skautski izveštaji" description="Privatni izveštaj vidi samo njegov autor." action={<Button variant="contained" startIcon={<AddIcon />} onClick={openCreate}>Novi izveštaj</Button>} />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      <Grid container spacing={3}>
        {reports.map((report) => (
          <Grid key={report.id} size={{ xs: 12, md: 6 }}>
            <Card><CardContent sx={{ p: 3 }}>
              <Stack direction="row" justifyContent="space-between">
                <div><Typography variant="h6">{report.playerName}</Typography><Typography color="text.secondary">{recommendationLabels[report.recommendation]}</Typography></div>
                <StatusChip label={report.visibility === 'Public' ? 'Javno' : 'Privatno'} status={report.visibility} />
              </Stack>
              <Typography sx={{ mt: 2 }}>{report.comment}</Typography>
              <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
                <Button startIcon={<EditOutlinedIcon />} onClick={() => openEdit(report)}>Izmeni</Button>
                <Button color="error" startIcon={<DeleteOutlineIcon />} onClick={() => remove(report.id)}>Obriši</Button>
              </Stack>
            </CardContent></Card>
          </Grid>
        ))}
      </Grid>
      {reports.length === 0 && <Alert severity="info">Još niste napisali nijedan izveštaj.</Alert>}

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="md">
        <DialogTitle>{editingId ? 'Izmena skautskog izveštaja' : 'Novi skautski izveštaj'}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField disabled={Boolean(editingId)} select label="Igrač" value={playerId} onChange={(e) => setPlayerId(e.target.value)}>
              {players.map((player) => <MenuItem key={player.id} value={player.id}>{player.firstName} {player.lastName}</MenuItem>)}
            </TextField>
            <Grid container spacing={2}>
              {[
                ['technique', 'Tehnika'], ['speed', 'Brzina'], ['passing', 'Dodavanje'],
                ['shooting', 'Šut'], ['defending', 'Odbrana'], ['physicalCondition', 'Fizička sprema'],
                ['gameVision', 'Pregled igre'], ['teamwork', 'Timski rad'],
              ].map(([field, label]) => (
                <Grid key={field} size={{ xs: 6, md: 3 }}>
                  <TextField fullWidth type="number" label={label} value={form[field]} inputProps={{ min: 1, max: 10 }} onChange={(e) => setForm({ ...form, [field]: Number(e.target.value) })} />
                </Grid>
              ))}
            </Grid>
            <TextField multiline minRows={2} label="Prednosti" value={form.strengths} onChange={(e) => setForm({ ...form, strengths: e.target.value })} />
            <TextField multiline minRows={2} label="Slabosti" value={form.weaknesses} onChange={(e) => setForm({ ...form, weaknesses: e.target.value })} />
            <TextField multiline minRows={3} label="Komentar" value={form.comment} onChange={(e) => setForm({ ...form, comment: e.target.value })} />
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField fullWidth select label="Preporučena pozicija" value={form.recommendedPosition} onChange={(e) => setForm({ ...form, recommendedPosition: e.target.value })}>
                <MenuItem value="">Nije određena</MenuItem>
                {positions.map((position) => <MenuItem key={position} value={position}>{positionLabels[position]}</MenuItem>)}
              </TextField>
              <TextField fullWidth select label="Preporuka" value={form.recommendation} onChange={(e) => setForm({ ...form, recommendation: e.target.value })}>
                {Object.entries(recommendationLabels).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}
              </TextField>
              <TextField fullWidth select label="Vidljivost" value={form.visibility} onChange={(e) => setForm({ ...form, visibility: e.target.value })}>
                <MenuItem value="Public">Javno</MenuItem>
                <MenuItem value="Private">Privatno</MenuItem>
              </TextField>
            </Stack>
          </Stack>
        </DialogContent>
        <DialogActions><Button onClick={() => setOpen(false)}>Odustani</Button><Button variant="contained" disabled={!playerId || !form.strengths || !form.weaknesses || !form.comment} onClick={save}>Sačuvaj</Button></DialogActions>
      </Dialog>
    </Container>
  )
}

export function WatchlistPage() {
  const [items, setItems] = useState([])
  const [message, setMessage] = useState('')
  const load = () => api.get('/watchlist').then(({ data }) => setItems(data))
  useEffect(load, [])

  const update = async (item) => {
    await api.put(`/watchlist/${item.id}`, { status: item.status, privateNote: item.privateNote || null })
    setMessage('Stavka je sačuvana.')
  }
  const remove = async (id) => {
    await api.delete(`/watchlist/${id}`)
    setMessage('Igrač je uklonjen sa liste.')
    load()
  }
  const change = (id, field, value) => setItems((current) => current.map((item) => item.id === id ? { ...item, [field]: value } : item))

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader title="Lista praćenja" description="Statusi i beleške sa ove stranice vidljivi su samo vama." />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      <Grid container spacing={3}>
        {items.map((item) => (
          <Grid key={item.id} size={{ xs: 12, md: 6 }}>
            <Card><CardContent sx={{ p: 3 }}>
              <Typography variant="h6">{item.playerName}</Typography>
              <Typography color="text.secondary">{positionLabels[item.position] || 'Pozicija nije uneta'} · {cityLabels[item.city] || 'Mesto nije uneto'}</Typography>
              <Stack spacing={2} sx={{ mt: 3 }}>
                <TextField select label="Status" value={item.status} onChange={(e) => change(item.id, 'status', e.target.value)}>
                  {Object.entries(watchlistLabels).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}
                </TextField>
                <TextField multiline minRows={2} label="Privatna beleška" value={item.privateNote || ''} onChange={(e) => change(item.id, 'privateNote', e.target.value)} />
                <Stack direction="row" spacing={1}>
                  <Button variant="contained" onClick={() => update(item)}>Sačuvaj</Button>
                  <Button color="error" onClick={() => remove(item.id)}>Ukloni</Button>
                  <Button component={Link} to={`/igraci/${item.playerProfileId}`}>Profil</Button>
                </Stack>
              </Stack>
            </CardContent></Card>
          </Grid>
        ))}
      </Grid>
      {items.length === 0 && <Alert severity="info">Lista praćenja je prazna.</Alert>}
    </Container>
  )
}
