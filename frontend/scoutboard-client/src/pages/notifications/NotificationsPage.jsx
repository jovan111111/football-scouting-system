import { useCallback, useEffect, useState } from 'react'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  CardActionArea,
  CardContent,
  Container,
  Stack,
  Typography,
} from '@mui/material'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutlineOutlined'
import DoneAllIcon from '@mui/icons-material/DoneAll'
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone'
import { Link } from 'react-router-dom'
import api from '../../api/client'
import PageHeader from '../../components/PageHeader'
import { formatDate } from '../../utils/labels'

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState([])
  const load = useCallback(() => api.get('/notifications').then(({ data }) => setNotifications(data)), [])
  useEffect(load, [load])

  const markRead = async (notification) => {
    if (!notification.isRead) await api.put(`/notifications/${notification.id}/read`)
    load()
  }

  const markAll = async () => {
    await api.put('/notifications/read-all')
    load()
  }

  return (
    <Container maxWidth="md" sx={{ py: 7 }}>
      <PageHeader
        title="Obaveštenja"
        description="Pozivi, prijave, statistika i odluke na jednom mestu."
        action={<Button startIcon={<DoneAllIcon />} onClick={markAll}>Označi sve kao pročitano</Button>}
      />
      <Stack spacing={1.5}>
        {notifications.map((notification) => (
          <Card key={notification.id} variant={notification.isRead ? 'outlined' : 'elevation'}>
            <CardActionArea
              component={notification.link ? Link : 'button'}
              to={notification.link || undefined}
              onClick={() => markRead(notification)}
            >
              <CardContent sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
                <Avatar sx={{ bgcolor: notification.isRead ? 'grey.200' : 'secondary.main', color: 'primary.main' }}>
                  {notification.isRead ? <CheckCircleOutlineIcon /> : <NotificationsNoneIcon />}
                </Avatar>
                <Box>
                  <Typography fontWeight={notification.isRead ? 500 : 800}>{notification.title}</Typography>
                  <Typography color="text.secondary">{notification.message}</Typography>
                  <Typography variant="caption" color="text.secondary">{formatDate(notification.createdAt, true)}</Typography>
                </Box>
              </CardContent>
            </CardActionArea>
          </Card>
        ))}
        {notifications.length === 0 && <Alert severity="info">Nemate obaveštenja.</Alert>}
      </Stack>
    </Container>
  )
}
