import { Avatar, Button, Card, CardContent, Chip, Stack, Typography } from '@mui/material'
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined'
import { Link } from 'react-router-dom'
import { cityLabels } from '../utils/labels'

export default function ClubCard({ club }) {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent sx={{ p: 3 }}>
        <Stack direction="row" spacing={2} alignItems="center">
          <Avatar src={club.logoUrl} variant="rounded" sx={{ width: 62, height: 62 }}>
            {club.name?.[0]}
          </Avatar>
          <div>
            <Typography variant="h6">{club.name}</Typography>
            <Typography color="text.secondary">{cityLabels[club.city]}</Typography>
          </div>
        </Stack>
        <Stack direction="row" spacing={1} sx={{ my: 2.5 }}>
          <Chip size="small" label={`${club.playerCount} igrača`} />
          {club.foundedYear && <Chip size="small" label={`Osnovan ${club.foundedYear}.`} />}
        </Stack>
        <Button
          component={Link}
          to={`/klubovi/${club.id}`}
          startIcon={<GroupsOutlinedIcon />}
          variant="outlined"
          fullWidth
        >
          Pogledaj klub
        </Button>
      </CardContent>
    </Card>
  )
}
