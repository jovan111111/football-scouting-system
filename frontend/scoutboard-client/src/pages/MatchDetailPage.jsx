import { useEffect, useState } from 'react'
import {
  Alert,
  Card,
  CardContent,
  Container,
  Grid,
  LinearProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { Link, useParams } from 'react-router-dom'
import api from '../api/client'
import StatusChip from '../components/StatusChip'
import { formatDate, matchStatusLabels } from '../utils/labels'

export default function MatchDetailPage() {
  const { id } = useParams()
  const [match, setMatch] = useState(null)

  useEffect(() => {
    api.get(`/matches/${id}`).then(({ data }) => setMatch(data)).catch(() => setMatch(false))
  }, [id])

  if (match === null) return <Container sx={{ py: 8 }}><LinearProgress /></Container>
  if (match === false) return <Container sx={{ py: 8 }}><Alert severity="error">Utakmica nije pronađena.</Alert></Container>

  return (
    <Container maxWidth="lg" sx={{ py: 7 }}>
      <Card>
        <CardContent sx={{ p: { xs: 3, md: 5 }, textAlign: 'center' }}>
          <Stack direction="row" justifyContent="center" sx={{ mb: 2 }}>
            <StatusChip label={matchStatusLabels[match.status]} status={match.status} />
          </Stack>
          {match.competitionId && (
            <Typography component={Link} to={`/takmicenja/${match.competitionId}`} color="secondary.main" sx={{ textDecoration: 'none', fontWeight: 700 }}>
              {match.competitionName}
            </Typography>
          )}
          <Typography color="text.secondary">{formatDate(match.matchDate, true)} · {match.venue}</Typography>
          <Grid container spacing={3} alignItems="center" sx={{ my: 4 }}>
            <Grid size={{ xs: 5 }}><Typography variant="h4">{match.clubName}</Typography></Grid>
            <Grid size={{ xs: 2 }}><Typography variant="h3">{match.goalsScored ?? '–'}:{match.goalsConceded ?? '–'}</Typography></Grid>
            <Grid size={{ xs: 5 }}><Typography variant="h4">{match.opponentName}</Typography></Grid>
          </Grid>
          {match.matchReport && <Typography color="text.secondary">{match.matchReport}</Typography>}
        </CardContent>
      </Card>

      <Typography variant="h4" sx={{ mt: 6, mb: 2 }}>Statistika igrača</Typography>
      {match.playerStatistics.length === 0 ? (
        <Alert severity="info">Statistika igrača još nije uneta.</Alert>
      ) : (
        <TableContainer component={Card}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Igrač</TableCell>
                <TableCell>Starter</TableCell>
                <TableCell>Min.</TableCell>
                <TableCell>Golovi</TableCell>
                <TableCell>Asist.</TableCell>
                <TableCell>Žuti</TableCell>
                <TableCell>Ocena</TableCell>
                <TableCell>Crveni</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {match.playerStatistics.map((stat) => (
                <TableRow key={stat.id}>
                  <TableCell>{stat.playerName}</TableCell>
                  <TableCell>{stat.wasStarter ? 'Da' : 'Ne'}</TableCell>
                  <TableCell>{stat.minutesPlayed}</TableCell>
                  <TableCell>{stat.goals}</TableCell>
                  <TableCell>{stat.assists}</TableCell>
                  <TableCell>{stat.yellowCards}</TableCell>
                  <TableCell>{stat.rating ?? '—'}</TableCell>
                  <TableCell>{stat.redCard ? 'Da' : 'Ne'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Container>
  )
}
