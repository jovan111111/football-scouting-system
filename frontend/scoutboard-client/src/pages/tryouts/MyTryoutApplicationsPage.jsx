import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Stack,
  Typography,
} from '@mui/material'
import { Link } from 'react-router-dom'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import StatusChip from '../../components/StatusChip'
import { formatDate, tryoutApplicationLabels } from '../../utils/labels'

export default function MyTryoutApplicationsPage() {
  const [applications, setApplications] = useState([])
  const [message, setMessage] = useState('')
  const load = useCallback(() => api.get('/tryouts/my-applications').then(({ data }) => setApplications(data)), [])
  useEffect(load, [load])

  const cancel = async (id) => {
    try {
      await api.post(`/tryouts/applications/${id}/cancel`)
      setMessage('Prijava je otkazana.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="lg" sx={{ py: 7 }}>
      <PageHeader title="Moje prijave za probe" description="Pratite odgovore klubova i predstojeće termine." />
      {message && <Alert sx={{ mb: 3 }}>{message}</Alert>}
      <Stack spacing={2}>
        {applications.map((application) => (
          <Card key={application.id}><CardContent sx={{ p: 3 }}>
            <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" gap={2}>
              <Box>
                <Typography variant="h6">{application.tryoutTitle}</Typography>
                <Typography color="text.secondary">{application.clubName} · {formatDate(application.tryoutDate, true)}</Typography>
                <Typography sx={{ mt: 1 }}>{application.venue}</Typography>
                {application.coachNote && <Alert severity="info" sx={{ mt: 2 }}>Napomena trenera: {application.coachNote}</Alert>}
              </Box>
              <Stack alignItems={{ xs: 'flex-start', md: 'flex-end' }} spacing={1}>
                <StatusChip label={tryoutApplicationLabels[application.status]} status={application.status} />
                <Button component={Link} to={`/probe/${application.tryoutId}`}>Detalji probe</Button>
                {application.status === 'Pending' && <Button color="error" onClick={() => cancel(application.id)}>Otkaži prijavu</Button>}
              </Stack>
            </Stack>
          </CardContent></Card>
        ))}
        {applications.length === 0 && <Alert severity="info">Niste se prijavili ni za jednu probu.</Alert>}
      </Stack>
    </Container>
  )
}
