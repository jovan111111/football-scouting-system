import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Container,
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
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined'
import { Link, useParams } from 'react-router-dom'
import api, { getErrorMessage } from '../../api/client'
import PageHeader from '../../components/PageHeader'
import StatusChip from '../../components/StatusChip'
import {
  cityLabels,
  competitionStatusLabels,
  formatDate,
} from '../../utils/labels'

export default function CompetitionDetailPage() {
  const { id } = useParams()
  const [competition, setCompetition] = useState(null)
  const [clubs, setClubs] = useState([])
  const [matches, setMatches] = useState([])
  const [clubId, setClubId] = useState('')
  const [message, setMessage] = useState('')

  const load = useCallback(async () => {
    const [competitionResponse, clubsResponse, matchesResponse] = await Promise.all([
      api.get(`/competitions/${id}`),
      api.get('/clubs'),
      api.get('/matches', { params: { competitionId: id } }),
    ])
    setCompetition(competitionResponse.data)
    setClubs(clubsResponse.data)
    setMatches(matchesResponse.data)
  }, [id])
  useEffect(() => { load().catch(() => setCompetition(false)) }, [load])

  const addClub = async () => {
    try {
      await api.post(`/competitions/${id}/clubs`, { clubId: Number(clubId) })
      setClubId('')
      setMessage('Klub je dodat u takmičenje.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const removeClub = async (selectedClubId) => {
    try {
      await api.delete(`/competitions/${id}/clubs/${selectedClubId}`)
      setMessage('Klub je uklonjen iz takmičenja.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  const changeStatus = async (status) => {
    try {
      await api.put(`/competitions/${id}`, {
        name: competition.name,
        season: competition.season,
        description: competition.description,
        startDate: competition.startDate,
        endDate: competition.endDate,
        status,
      })
      setMessage('Status takmičenja je promenjen.')
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  if (competition === null) return <Container sx={{ py: 7 }}>Učitavanje…</Container>
  if (competition === false) return <Container sx={{ py: 7 }}><Alert severity="error">Takmičenje nije pronađeno.</Alert></Container>

  const availableClubs = clubs.filter((club) => !competition.clubs.some((entry) => entry.clubId === club.id))

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow={`Sezona ${competition.season}`}
        title={competition.name}
        description={competition.description}
        action={<StatusChip label={competitionStatusLabels[competition.status]} status={competition.status} />}
      />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}

      {competition.canManage && (
        <Card sx={{ mb: 4 }}>
          <CardContent sx={{ p: 3 }}>
            <Typography variant="h6" sx={{ mb: 2 }}>Upravljanje takmičenjem</Typography>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
              <TextField select label="Dodaj odobren klub" value={clubId} onChange={(event) => setClubId(event.target.value)} sx={{ minWidth: 260 }}>
                {availableClubs.map((club) => <MenuItem key={club.id} value={club.id}>{club.name}</MenuItem>)}
              </TextField>
              <Button variant="contained" disabled={!clubId} onClick={addClub}>Dodaj klub</Button>
              <TextField select label="Status takmičenja" value={competition.status} onChange={(event) => changeStatus(event.target.value)} sx={{ minWidth: 220 }}>
                {Object.entries(competitionStatusLabels).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}
              </TextField>
            </Stack>
          </CardContent>
        </Card>
      )}

      <Typography variant="h4" sx={{ mb: 2 }}>Tabela</Typography>
      <TableContainer component={Card} sx={{ mb: 5 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>#</TableCell><TableCell>Klub</TableCell><TableCell>UT</TableCell>
              <TableCell>P</TableCell><TableCell>N</TableCell><TableCell>I</TableCell>
              <TableCell>DG</TableCell><TableCell>PG</TableCell><TableCell>GR</TableCell>
              <TableCell><strong>Bodovi</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {competition.standings.map((row) => (
              <TableRow key={row.clubId}>
                <TableCell>{row.position}</TableCell>
                <TableCell><Button component={Link} to={`/klubovi/${row.clubId}`}>{row.clubName}</Button></TableCell>
                <TableCell>{row.played}</TableCell><TableCell>{row.won}</TableCell>
                <TableCell>{row.drawn}</TableCell><TableCell>{row.lost}</TableCell>
                <TableCell>{row.goalsFor}</TableCell><TableCell>{row.goalsAgainst}</TableCell>
                <TableCell>{row.goalDifference}</TableCell><TableCell><strong>{row.points}</strong></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      {competition.standings.length === 0 && <Alert severity="info" sx={{ mb: 4 }}>Dodajte klubove da bi se formirala tabela.</Alert>}

      <Grid container spacing={4}>
        <Grid size={{ xs: 12, lg: 5 }}>
          <Typography variant="h4" sx={{ mb: 2 }}>Učesnici</Typography>
          <Stack spacing={1.5}>
            {competition.clubs.map((club) => (
              <Card key={club.clubId}><CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
                <Stack direction="row" alignItems="center" justifyContent="space-between">
                  <Box>
                    <Typography fontWeight={700}>{club.clubName}</Typography>
                    <Typography color="text.secondary">{cityLabels[club.city]}</Typography>
                  </Box>
                  {competition.canManage && (
                    <Button color="error" startIcon={<DeleteOutlineIcon />} onClick={() => removeClub(club.clubId)}>Ukloni</Button>
                  )}
                </Stack>
              </CardContent></Card>
            ))}
          </Stack>
        </Grid>
        <Grid size={{ xs: 12, lg: 7 }}>
          <Typography variant="h4" sx={{ mb: 2 }}>Utakmice</Typography>
          <Stack spacing={1.5}>
            {matches.map((match) => (
              <Card key={match.id}>
                <CardActionArea component={Link} to={`/utakmice/${match.id}`}>
                  <CardContent>
                    <Stack direction="row" justifyContent="space-between" gap={2}>
                      <Box>
                        <Typography fontWeight={700}>{match.clubName} — {match.opponentName}</Typography>
                        <Typography color="text.secondary">{formatDate(match.matchDate, true)} · {match.venue}</Typography>
                      </Box>
                      <Typography variant="h5">{match.goalsScored ?? '–'} : {match.goalsConceded ?? '–'}</Typography>
                    </Stack>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
            {matches.length === 0 && <Alert severity="info">Još nema utakmica u ovom takmičenju.</Alert>}
          </Stack>
        </Grid>
      </Grid>
    </Container>
  )
}
