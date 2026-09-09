import {
  Avatar,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Stack,
  Typography,
} from '@mui/material'
import LocationOnOutlinedIcon from '@mui/icons-material/LocationOnOutlined'
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer'
import { Link } from 'react-router-dom'
import { cityLabels, positionLabels } from '../utils/labels'

export default function PlayerCard({ player }) {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Avatar
            src={player.profileImageUrl}
            sx={{ width: 62, height: 62, bgcolor: 'primary.main', fontWeight: 800 }}
          >
            {player.firstName?.[0]}{player.lastName?.[0]}
          </Avatar>
          <Box sx={{ minWidth: 0 }}>
            <Typography variant="h6" noWrap>
              {player.firstName} {player.lastName}
            </Typography>
            <Typography color="text.secondary">
              {positionLabels[player.primaryPosition] || 'Pozicija nije uneta'}
            </Typography>
          </Box>
        </Stack>

        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ mt: 2.5 }}>
          {player.city && (
            <Chip
              size="small"
              icon={<LocationOnOutlinedIcon />}
              label={cityLabels[player.city]}
            />
          )}
          {player.lookingForClub && (
            <Chip size="small" color="secondary" label="Traži klub" />
          )}
        </Stack>

        <Typography color="text.secondary" sx={{ mt: 2 }}>
          {player.currentClubName || 'Bez kluba'}
        </Typography>

        <Stack direction="row" spacing={2} sx={{ my: 2.5 }}>
          <Box>
            <Typography variant="h6">{player.appearances}</Typography>
            <Typography variant="caption" color="text.secondary">nastupa</Typography>
          </Box>
          <Box>
            <Typography variant="h6">{player.goals}</Typography>
            <Typography variant="caption" color="text.secondary">golova</Typography>
          </Box>
          <Box>
            <Typography variant="h6">{player.assists}</Typography>
            <Typography variant="caption" color="text.secondary">asistencija</Typography>
          </Box>
        </Stack>

        <Button
          component={Link}
          to={`/igraci/${player.id}`}
          startIcon={<SportsSoccerIcon />}
          fullWidth
          variant="outlined"
        >
          Pogledaj profil
        </Button>
      </CardContent>
    </Card>
  )
}
