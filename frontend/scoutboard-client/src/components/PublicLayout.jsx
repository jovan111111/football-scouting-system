import { useEffect, useState } from 'react'
import {
  AppBar,
  Badge,
  Box,
  Button,
  Container,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemText,
  Stack,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import MenuIcon from '@mui/icons-material/Menu'
import SportsSoccerIcon from '@mui/icons-material/SportsSoccer'
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone'
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import api from '../api/client'

const links = [
  ['Početna', '/'],
  ['Igrači', '/igraci'],
  ['Klubovi', '/klubovi'],
  ['Utakmice', '/utakmice'],
  ['Takmičenja', '/takmicenja'],
  ['Probe', '/probe'],
  ['Poređenje', '/poredjenje-igraca'],
]

export default function PublicLayout() {
  const [open, setOpen] = useState(false)
  const [unreadCount, setUnreadCount] = useState(0)
  const theme = useTheme()
  const desktop = useMediaQuery(theme.breakpoints.up('lg'))
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  useEffect(() => {
    if (!user) {
      setUnreadCount(0)
      return
    }
    api.get('/notifications/unread-count')
      .then(({ data }) => setUnreadCount(data.count))
      .catch(() => setUnreadCount(0))
  }, [user, location.pathname])

  const signOut = () => {
    logout()
    navigate('/')
  }

  const navigation = (
    <>
      {links.map(([label, path]) => (
        <Button
          key={path}
          component={NavLink}
          to={path}
          onClick={() => setOpen(false)}
          color="inherit"
          sx={{
            opacity: 0.82,
            '&.active': { opacity: 1, color: 'secondary.main' },
          }}
        >
          {label}
        </Button>
      ))}
    </>
  )

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <AppBar position="sticky" color="primary" elevation={0}>
        <Toolbar sx={{ minHeight: 70 }}>
          <Container
            maxWidth="xl"
            sx={{ display: 'flex', alignItems: 'center', px: { xs: 0, sm: 2 } }}
          >
            <Stack
              component={Link}
              to="/"
              direction="row"
              alignItems="center"
              spacing={1}
              sx={{ color: 'inherit', textDecoration: 'none' }}
            >
              <Box
                sx={{
                  width: 38,
                  height: 38,
                  borderRadius: 2,
                  display: 'grid',
                  placeItems: 'center',
                  bgcolor: 'secondary.main',
                  color: 'primary.main',
                }}
              >
                <SportsSoccerIcon />
              </Box>
              <Typography variant="h6" sx={{ fontWeight: 900 }}>
                ScoutBoard
              </Typography>
            </Stack>

            {desktop ? (
              <>
                <Stack direction="row" spacing={0.5} sx={{ ml: 5 }}>
                  {navigation}
                </Stack>
                <Stack direction="row" spacing={1} sx={{ ml: 'auto' }}>
                  {user ? (
                    <>
                      <IconButton component={Link} to="/obavestenja" color="inherit" aria-label="Obaveštenja">
                        <Badge badgeContent={unreadCount} color="secondary">
                          <NotificationsNoneIcon />
                        </Badge>
                      </IconButton>
                      <Button
                        component={Link}
                        to="/kontrolna-tabla"
                        variant="contained"
                        color="secondary"
                      >
                        Kontrolna tabla
                      </Button>
                      <Button color="inherit" onClick={signOut}>Odjava</Button>
                    </>
                  ) : (
                    <>
                      <Button component={Link} to="/prijava" color="inherit">
                        Prijava
                      </Button>
                      <Button
                        component={Link}
                        to="/registracija"
                        variant="contained"
                        color="secondary"
                      >
                        Registracija
                      </Button>
                    </>
                  )}
                </Stack>
              </>
            ) : (
              <IconButton color="inherit" sx={{ ml: 'auto' }} onClick={() => setOpen(true)}>
                <MenuIcon />
              </IconButton>
            )}
          </Container>
        </Toolbar>
      </AppBar>

      <Drawer anchor="right" open={open} onClose={() => setOpen(false)}>
        <Box sx={{ width: 280, p: 2 }}>
          <Typography variant="h6" sx={{ px: 2, py: 1 }}>ScoutBoard</Typography>
          <List>
            {links.map(([label, path]) => (
              <ListItemButton
                key={path}
                component={Link}
                to={path}
                onClick={() => setOpen(false)}
              >
                <ListItemText primary={label} />
              </ListItemButton>
            ))}
          </List>
          <Divider sx={{ my: 1 }} />
          {user ? (
            <>
              <ListItemButton component={Link} to="/kontrolna-tabla" onClick={() => setOpen(false)}>
                <ListItemText primary="Kontrolna tabla" />
              </ListItemButton>
              <ListItemButton component={Link} to="/obavestenja" onClick={() => setOpen(false)}>
                <ListItemText primary={`Obaveštenja${unreadCount ? ` (${unreadCount})` : ''}`} />
              </ListItemButton>
              <ListItemButton onClick={signOut}><ListItemText primary="Odjava" /></ListItemButton>
            </>
          ) : (
            <>
              <ListItemButton component={Link} to="/prijava" onClick={() => setOpen(false)}>
                <ListItemText primary="Prijava" />
              </ListItemButton>
              <ListItemButton component={Link} to="/registracija" onClick={() => setOpen(false)}>
                <ListItemText primary="Registracija" />
              </ListItemButton>
            </>
          )}
        </Box>
      </Drawer>

      <Box component="main" sx={{ flex: 1 }}>
        <Outlet />
      </Box>

      <Box component="footer" sx={{ bgcolor: 'primary.main', color: 'white', py: 4, mt: 8 }}>
        <Container maxWidth="xl">
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            justifyContent="space-between"
            gap={1}
          >
            <Typography sx={{ fontWeight: 700 }}>ScoutBoard</Typography>
            <Typography sx={{ opacity: 0.7 }}>
              Lokalna platforma za amaterski fudbal Novog Pazara i okoline
            </Typography>
          </Stack>
        </Container>
      </Box>
    </Box>
  )
}
