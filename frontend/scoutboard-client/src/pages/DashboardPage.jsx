import {
  Box,
  Card,
  CardActionArea,
  CardContent,
  Container,
  Grid,
  Typography,
} from '@mui/material'
import BadgeOutlinedIcon from '@mui/icons-material/BadgeOutlined'
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined'
import MailOutlineIcon from '@mui/icons-material/MailOutlined'
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer'
import AssessmentOutlinedIcon from '@mui/icons-material/AssessmentOutlined'
import BookmarkBorderIcon from '@mui/icons-material/BookmarkBorder'
import AdminPanelSettingsOutlinedIcon from '@mui/icons-material/AdminPanelSettingsOutlined'
import CompareArrowsOutlinedIcon from '@mui/icons-material/CompareArrowsOutlined'
import EmojiEventsOutlinedIcon from '@mui/icons-material/EmojiEventsOutlined'
import EventAvailableOutlinedIcon from '@mui/icons-material/EventAvailableOutlined'
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone'
import { Link } from 'react-router-dom'
import PageHeader from '../components/PageHeader'
import { useAuth } from '../context/AuthContext'
import { roleLabels } from '../utils/labels'

const roleLinks = {
  Player: [
    ['Moj profil', 'Dopunite fudbalske podatke i status.', '/moj-profil', BadgeOutlinedIcon],
    ['Pozivi klubova', 'Prihvatite ili odbijte poziv.', '/moji-pozivi', MailOutlineIcon],
    ['Moja statistika', 'Pregled nastupa i rezultata.', '/moja-statistika', AssessmentOutlinedIcon],
    ['Prijave za probe', 'Pratite odgovore klubova.', '/moje-prijave-za-probe', EventAvailableOutlinedIcon],
    ['Poređenje igrača', 'Uporedite statistiku i profile.', '/poredjenje-igraca', CompareArrowsOutlinedIcon],
    ['Obaveštenja', 'Pregled važnih promena i odgovora.', '/obavestenja', NotificationsNoneIcon],
  ],
  CoachScout: [
    ['Moji klubovi', 'Upravljajte klubovima, sastavom i utakmicama.', '/moji-klubovi', GroupsOutlinedIcon],
    ['Skautski izveštaji', 'Pišite i uređujte procene igrača.', '/skautski-izvestaji', AssessmentOutlinedIcon],
    ['Lista praćenja', 'Pratite zanimljive igrače.', '/lista-pracenja', BookmarkBorderIcon],
    ['Sve utakmice', 'Pregled lokalnog rasporeda i rezultata.', '/utakmice', SportsSoccerIcon],
    ['Takmičenja', 'Kreirajte lige i pratite tabelu.', '/takmicenja', EmojiEventsOutlinedIcon],
    ['Fudbalske probe', 'Objavite termine i obradite prijave.', '/probe', EventAvailableOutlinedIcon],
    ['Poređenje igrača', 'Uporedite profile i učinak igrača.', '/poredjenje-igraca', CompareArrowsOutlinedIcon],
    ['Obaveštenja', 'Pregled prijava, poziva i zahteva.', '/obavestenja', NotificationsNoneIcon],
  ],
  Admin: [
    ['Administracija', 'Odobrite klubove i upravljajte korisnicima.', '/admin', AdminPanelSettingsOutlinedIcon],
    ['Javni klubovi', 'Pregled odobrenih klubova.', '/klubovi', GroupsOutlinedIcon],
    ['Utakmice', 'Pregled svih utakmica.', '/utakmice', SportsSoccerIcon],
    ['Takmičenja', 'Kreirajte i pratite lokalna takmičenja.', '/takmicenja', EmojiEventsOutlinedIcon],
    ['Obaveštenja', 'Pregled važnih promena sistema.', '/obavestenja', NotificationsNoneIcon],
  ],
}

export default function DashboardPage() {
  const { user } = useAuth()
  const links = roleLinks[user.role] || []

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader
        eyebrow={roleLabels[user.role]}
        title={`Zdravo, ${user.firstName}`}
        description="Odaberite deo ScoutBoard sistema kojim želite da upravljate."
      />
      <Grid container spacing={3}>
        {links.map(([title, description, path, Icon]) => (
          <Grid key={path} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={path} sx={{ height: '100%' }}>
                <CardContent sx={{ p: 4 }}>
                  <Box sx={{ width: 52, height: 52, borderRadius: 3, bgcolor: 'secondary.main', color: 'primary.main', display: 'grid', placeItems: 'center', mb: 3 }}>
                    <Icon />
                  </Box>
                  <Typography variant="h5">{title}</Typography>
                  <Typography color="text.secondary" sx={{ mt: 1 }}>{description}</Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Container>
  )
}
