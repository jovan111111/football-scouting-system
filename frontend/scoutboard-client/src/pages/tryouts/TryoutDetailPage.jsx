import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Grid,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link, useNavigate, useParams } from 'react-router-dom'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import StatusChip from '../../components/StatusChip'
import { useAuth } from '../../context/AuthContext'
import {
  cityLabels,
  formatDate,
  positionLabels,
  tryoutApplicationLabels,
  tryoutStatusLabels,
} from '../../utils/labels'

export default function TryoutDetailPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const [tryout, setTryout] = useState(null)
  const [applications, setApplications] = useState([])
  const [applicationMessage, setApplicationMessage] = useState('')
  const [message, setMessage] = useState('')

  const load = useCallback(async () => {
    const { data } = await api.get(`/tryouts/${id}`)
    setTryout(data)
    if (data.canManage) {
      const applicationsResponse = await api.get(`/tryouts/${id}/applications`)
      setApplications(applicationsResponse.data)
    }
  }, [id])
  useEffect(() => { load().catch(() => setTryout(false)) }, [load])

  const apply = async () => {
    try {
      await api.post(`/tryouts/${id}/apply`, { message: applicationMessage })
      setMessage('Prijava je poslata klubu.')
      setApplicationMessage('')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const resolve = async (application, status) => {
    const coachNote = window.prompt('Unesite kratku napomenu za igrača:') || ''
    try {
      await api.put(`/tryouts/applications/${application.id}/resolve`, { status, coachNote })
      setMessage('Prijava je obrađena.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const closeTryout = async () => {
    try {
      await api.put(`/tryouts/${id}`, {
        title: tryout.title,
        description: tryout.description,
        tryoutDate: tryout.tryoutDate,
        venue: tryout.venue,
        position: tryout.position,
        minimumAge: tryout.minimumAge,
        maximumAge: tryout.maximumAge,
        status: 'Closed',
      })
      setMessage('Prijave za probu su zatvorene.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  if (tryout === null) return <Container sx={{ py: 7 }}>Učitavanje…</Container>
  if (tryout === false) return <Container sx={{ py: 7 }}><Alert severity="error">Proba nije pronađena.</Alert></Container>

  return (
    <Container maxWidth="lg" sx={{ py: 7 }}>
      <PageHeader
        eyebrow={`${tryout.clubName} · ${cityLabels[tryout.city]}`}
        title={tryout.title}
        description={tryout.description}
        action={<StatusChip label={tryoutStatusLabels[tryout.status]} status={tryout.status} />}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 7 }}>
          <Card><CardContent sx={{ p: 4 }}>
            <Typography variant="h6">Termin i uslovi</Typography>
            <Stack spacing={1.5} sx={{ mt: 2 }}>
              <Typography><strong>Datum:</strong> {formatDate(tryout.tryoutDate, true)}</Typography>
              <Typography><strong>Lokacija:</strong> {tryout.venue}</Typography>
              <Typography><strong>Pozicija:</strong> {tryout.position ? positionLabels[tryout.position] : 'Sve pozicije'}</Typography>
              <Typography><strong>Uzrast:</strong> {tryout.minimumAge || '—'}–{tryout.maximumAge || '—'} godina</Typography>
              <Typography><strong>Broj prijava:</strong> {tryout.applicationCount}</Typography>
            </Stack>
            {tryout.canManage && tryout.status === 'Open' && (
              <Button sx={{ mt: 3 }} onClick={closeTryout}>Zatvori prijave</Button>
            )}
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 12, md: 5 }}>
          {user?.role === 'Player' && !tryout.myApplicationStatus && tryout.status === 'Open' && (
            <Card><CardContent sx={{ p: 3 }}>
              <Typography variant="h6">Prijava za probu</Typography>
              <TextField fullWidth multiline minRows={4} label="Poruka treneru" value={applicationMessage} onChange={(event) => setApplicationMessage(event.target.value)} sx={{ my: 2 }} />
              <Button fullWidth variant="contained" disabled={!applicationMessage.trim()} onClick={apply}>Pošalji prijavu</Button>
            </CardContent></Card>
          )}
          {user?.role === 'Player' && tryout.myApplicationStatus && (
            <Alert severity={tryout.myApplicationStatus === 'Accepted' ? 'success' : 'info'}>
              Status vaše prijave: {tryoutApplicationLabels[tryout.myApplicationStatus]}
            </Alert>
          )}
          {!user && (
            <Alert severity="info" action={<Button onClick={() => navigate('/prijava')}>Prijava</Button>}>
              Prijavite se kao igrač da biste poslali prijavu.
            </Alert>
          )}
        </Grid>
      </Grid>

      {tryout.canManage && (
        <Box sx={{ mt: 6 }}>
          <Typography variant="h4" sx={{ mb: 2 }}>Prijavljeni igrači</Typography>
          <Stack spacing={2}>
            {applications.map((application) => (
              <Card key={application.id}><CardContent>
                <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={2}>
                  <Box>
                    <Button component={Link} to={`/igraci/${application.playerProfileId}`} sx={{ px: 0, fontSize: 17 }}>
                      {application.playerName}
                    </Button>
                    <Typography color="text.secondary">
                      {positionLabels[application.position] || 'Pozicija nije uneta'} · {cityLabels[application.city] || 'Mesto nije uneto'}
                    </Typography>
                    <Typography sx={{ mt: 1 }}>{application.message}</Typography>
                  </Box>
                  {application.status === 'Pending' ? (
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Button color="error" onClick={() => resolve(application, 'Rejected')}>Odbij</Button>
                      <Button variant="contained" onClick={() => resolve(application, 'Accepted')}>Prihvati</Button>
                    </Stack>
                  ) : <StatusChip label={tryoutApplicationLabels[application.status]} status={application.status} />}
                </Stack>
              </CardContent></Card>
            ))}
            {applications.length === 0 && <Alert severity="info">Još nema prijavljenih igrača.</Alert>}
          </Stack>
        </Box>
      )}
    </Container>
  )
}
