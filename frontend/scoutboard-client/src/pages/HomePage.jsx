import { useEffect, useState } from 'react'
import {
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Grid,
  Stack,
  Typography,
} from '@mui/material'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined'
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer'
import TravelExploreIcon from '@mui/icons-material/TravelExplore'
import { Link } from 'react-router-dom'
import api from '../api/client'
import PlayerCard from '../components/PlayerCard'
import ClubCard from '../components/ClubCard'
import { formatDate, matchStatusLabels } from '../utils/labels'

export default function HomePage() {
  const [players, setPlayers] = useState([])
  const [clubs, setClubs] = useState([])
  const [matches, setMatches] = useState([])

  useEffect(() => {
    Promise.all([
      api.get('/players?lookingForClub=true'),
      api.get('/clubs'),
      api.get('/matches'),
    ]).then(([playersResponse, clubsResponse, matchesResponse]) => {
      setPlayers(playersResponse.data.slice(0, 3))
      setClubs(clubsResponse.data.slice(0, 3))
      setMatches(matchesResponse.data.slice(0, 3))
    }).catch(() => {})
  }, [])

  return (
    <>
      <Box
        sx={{
          bgcolor: 'primary.main',
          color: 'white',
          overflow: 'hidden',
          position: 'relative',
          '&::after': {
            content: '""',
            position: 'absolute',
            width: 520,
            height: 520,
            border: '90px solid rgba(212,167,44,0.12)',
            borderRadius: '50%',
            right: -160,
            top: -190,
          },
        }}
      >
        <Container maxWidth="xl" sx={{ py: { xs: 9, md: 14 }, position: 'relative', zIndex: 1 }}>
          <Grid container spacing={5} alignItems="center">
            <Grid size={{ xs: 12, md: 8 }}>
              <Typography
                variant="overline"
                color="secondary.main"
                sx={{ fontWeight: 900, letterSpacing: 2 }}
              >
                Lokalni fudbal. Vidljiv talent.
              </Typography>
              <Typography variant="h1" sx={{ mt: 1, fontSize: { xs: '3rem', md: '5.2rem' }, maxWidth: 900 }}>
                Otkrij igrače koji zaslužuju priliku.
              </Typography>
              <Typography sx={{ mt: 3, maxWidth: 680, fontSize: '1.15rem', opacity: 0.82 }}>
                ScoutBoard povezuje amaterske igrače, trenere i klubove iz Novog Pazara,
                Tutina, Sjenice i Raške.
              </Typography>
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mt: 4 }}>
                <Button
                  component={Link}
                  to="/igraci"
                  variant="contained"
                  color="secondary"
                  size="large"
                  endIcon={<ArrowForwardIcon />}
                >
                  Pronađi igrača
                </Button>
                <Button component={Link} to="/registracija" variant="outlined" color="inherit" size="large">
                  Otvori nalog
                </Button>
              </Stack>
            </Grid>
          </Grid>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: 8 }}>
        <Grid container spacing={2} sx={{ mt: -12, position: 'relative', zIndex: 2 }}>
          {[
            [players.length, 'Igrači koji traže klub', TravelExploreIcon],
            [clubs.length, 'Lokalni klubovi', GroupsOutlinedIcon],
            [matches.length, 'Najnovije utakmice', SportsSoccerIcon],
          ].map(([value, label, Icon]) => (
            <Grid key={label} size={{ xs: 12, md: 4 }}>
              <Card>
                <CardContent sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Box sx={{ color: 'secondary.dark' }}><Icon /></Box>
                  <Box>
                    <Typography variant="h4">{value}</Typography>
                    <Typography color="text.secondary">{label}</Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        <SectionTitle
          title="Igrači koji traže klub"
          description="Pregledaj lokalne fudbalere dostupne za nove prilike."
          href="/igraci"
        />
        <Grid container spacing={3}>
          {players.map((player) => (
            <Grid key={player.id} size={{ xs: 12, md: 4 }}><PlayerCard player={player} /></Grid>
          ))}
        </Grid>

        <SectionTitle
          title="Klubovi iz regiona"
          description="Upoznaj amaterske klubove koji razvijaju lokalne igrače."
          href="/klubovi"
        />
        <Grid container spacing={3}>
          {clubs.map((club) => (
            <Grid key={club.id} size={{ xs: 12, md: 4 }}><ClubCard club={club} /></Grid>
          ))}
        </Grid>

        <SectionTitle
          title="Poslednje utakmice"
          description="Rezultati i predstojeći termini lokalnih klubova."
          href="/utakmice"
        />
        <Grid container spacing={2}>
          {matches.map((match) => (
            <Grid key={match.id} size={{ xs: 12, md: 4 }}>
              <Card>
                <CardContent sx={{ p: 3 }}>
                  <Typography variant="overline" color="secondary.dark">
                    {matchStatusLabels[match.status]} · {formatDate(match.matchDate)}
                  </Typography>
                  <Typography variant="h6" sx={{ mt: 1 }}>
                    {match.clubName} — {match.opponentName}
                  </Typography>
                  <Typography variant="h4" sx={{ my: 2 }}>
                    {match.goalsScored ?? '–'} : {match.goalsConceded ?? '–'}
                  </Typography>
                  <Button component={Link} to={`/utakmice/${match.id}`}>Detalji utakmice</Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
    </>
  )
}

function SectionTitle({ title, description, href }) {
  return (
    <Stack
      direction={{ xs: 'column', sm: 'row' }}
      alignItems={{ xs: 'flex-start', sm: 'end' }}
      justifyContent="space-between"
      gap={2}
      sx={{ mt: 10, mb: 3 }}
    >
      <Box>
        <Typography variant="h3">{title}</Typography>
        <Typography color="text.secondary" sx={{ mt: 1 }}>{description}</Typography>
      </Box>
      <Button component={Link} to={href} endIcon={<ArrowForwardIcon />}>Prikaži sve</Button>
    </Stack>
  )
}
