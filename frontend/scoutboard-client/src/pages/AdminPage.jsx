import { useEffect, useState } from 'react'
import {
  Alert,
  Button,
  Card,
  CardContent,
  Container,
  Grid,
  Stack,
  Switch,
  Tab,
  Tabs,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import api, { getErrorMessage } from '../api/client'
import PageHeader from '../components/PageHeader'
import StatusChip from '../components/StatusChip'
import { approvalLabels, cityLabels, formatDate, roleLabels } from '../utils/labels'

export default function AdminPage() {
  const [dashboard, setDashboard] = useState(null)
  const [clubs, setClubs] = useState([])
  const [users, setUsers] = useState([])
  const [tab, setTab] = useState(0)
  const [message, setMessage] = useState('')

  const load = () => Promise.all([
    api.get('/admin/dashboard'),
    api.get('/admin/clubs'),
    api.get('/admin/users'),
  ]).then(([dashboardResponse, clubsResponse, usersResponse]) => {
    setDashboard(dashboardResponse.data)
    setClubs(clubsResponse.data)
    setUsers(usersResponse.data)
  }).catch((error) => setMessage(getErrorMessage(error)))

  useEffect(load, [])

  const approve = async (id) => {
    const { data } = await api.put(`/admin/clubs/${id}/approve`)
    setMessage(data.message)
    load()
  }

  const reject = async (id) => {
    const adminNote = window.prompt('Navedite razlog odbijanja:')
    if (!adminNote) return
    const { data } = await api.put(`/admin/clubs/${id}/reject`, { adminNote })
    setMessage(data.message)
    load()
  }

  const changeUserStatus = async (user) => {
    try {
      const { data } = await api.put(`/admin/users/${user.id}/status`, { isActive: !user.isActive })
      setMessage(data.message)
      load()
    } catch (error) {
      setMessage(getErrorMessage(error))
    }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 7 }}>
      <PageHeader title="Administracija" description="Odobravanje klubova i upravljanje korisničkim nalozima." />
      {message && <Alert sx={{ mb: 3 }} onClose={() => setMessage('')}>{message}</Alert>}
      {dashboard && (
        <Grid container spacing={2} sx={{ mb: 5 }}>
          {[
            [dashboard.userCount, 'Korisnici'],
            [dashboard.playerCount, 'Igrači'],
            [dashboard.clubCount, 'Klubovi'],
            [dashboard.pendingClubCount, 'Čekaju odobrenje'],
            [dashboard.matchCount, 'Utakmice'],
          ].map(([value, label]) => (
            <Grid key={label} size={{ xs: 6, md: 2.4 }}>
              <Card><CardContent><Typography variant="h4">{value}</Typography><Typography color="text.secondary">{label}</Typography></CardContent></Card>
            </Grid>
          ))}
        </Grid>
      )}

      <Card>
        <Tabs value={tab} onChange={(_, value) => setTab(value)} sx={{ px: 2, borderBottom: 1, borderColor: 'divider' }}>
          <Tab label="Klubovi" />
          <Tab label="Korisnici" />
        </Tabs>
        {tab === 0 && (
          <TableContainer>
            <Table>
              <TableHead><TableRow><TableCell>Klub</TableCell><TableCell>Mesto</TableCell><TableCell>Vlasnik</TableCell><TableCell>Status</TableCell><TableCell>Akcije</TableCell></TableRow></TableHead>
              <TableBody>
                {clubs.map((club) => (
                  <TableRow key={club.id}>
                    <TableCell><Typography fontWeight={700}>{club.name}</Typography><Typography variant="caption" color="text.secondary">{club.description.slice(0, 80)}</Typography></TableCell>
                    <TableCell>{cityLabels[club.city]}</TableCell>
                    <TableCell>{club.ownerName}</TableCell>
                    <TableCell><StatusChip label={approvalLabels[club.approvalStatus]} status={club.approvalStatus} /></TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={1}>
                        <Button size="small" variant="contained" onClick={() => approve(club.id)}>Odobri</Button>
                        <Button size="small" color="error" onClick={() => reject(club.id)}>Odbij</Button>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
        {tab === 1 && (
          <TableContainer>
            <Table>
              <TableHead><TableRow><TableCell>Korisnik</TableCell><TableCell>Uloga</TableCell><TableCell>E-mail</TableCell><TableCell>Registracija</TableCell><TableCell>Aktivan</TableCell></TableRow></TableHead>
              <TableBody>
                {users.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>{user.firstName} {user.lastName}</TableCell>
                    <TableCell>{roleLabels[user.role]}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{formatDate(user.createdAt)}</TableCell>
                    <TableCell><Switch checked={user.isActive} onChange={() => changeUserStatus(user)} /></TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Card>
    </Container>
  )
}
