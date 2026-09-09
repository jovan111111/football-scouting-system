import { useState } from 'react'
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Container,
  Link as MuiLink,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import api, { getErrorMessage } from '../api/client'
import { useAuth } from '../context/AuthContext'

function AuthShell({ title, description, children }) {
  return (
    <Container maxWidth="sm" sx={{ py: { xs: 6, md: 9 } }}>
      <Card>
        <CardContent sx={{ p: { xs: 3, md: 5 } }}>
          <Typography variant="h3">{title}</Typography>
          <Typography color="text.secondary" sx={{ mt: 1, mb: 4 }}>{description}</Typography>
          {children}
        </CardContent>
      </Card>
    </Container>
  )
}

export function LoginPage() {
  const [form, setForm] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const submit = async (event) => {
    event.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      await login(form)
      navigate(location.state?.from?.pathname || '/kontrolna-tabla', { replace: true })
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthShell title="Dobro došli nazad" description="Prijavite se na svoj ScoutBoard nalog.">
      <Box component="form" onSubmit={submit}>
        <Stack spacing={2.5}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            required
            type="email"
            label="E-mail adresa"
            value={form.email}
            onChange={(event) => setForm((value) => ({ ...value, email: event.target.value }))}
          />
          <TextField
            required
            type="password"
            label="Lozinka"
            value={form.password}
            onChange={(event) => setForm((value) => ({ ...value, password: event.target.value }))}
          />
          <Button type="submit" variant="contained" size="large" disabled={submitting}>
            {submitting ? 'Prijavljivanje…' : 'Prijavi se'}
          </Button>
          <Typography textAlign="center" color="text.secondary">
            Nemate nalog?{' '}
            <MuiLink component={Link} to="/registracija">Registrujte se</MuiLink>
          </Typography>
        </Stack>
      </Box>
    </AuthShell>
  )
}

export function RegisterPage() {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    role: 'Player',
  })
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()

  const change = (name) => (event) => setForm((value) => ({ ...value, [name]: event.target.value }))

  const submit = async (event) => {
    event.preventDefault()
    if (form.password !== form.confirmPassword) {
      setError('Lozinke se ne podudaraju.')
      return
    }

    setSubmitting(true)
    setError('')
    try {
      await api.post('/auth/register', {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        role: form.role,
      })
      navigate('/potvrda-emaila', { state: { email: form.email } })
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AuthShell title="Kreirajte nalog" description="Pridružite se lokalnoj fudbalskoj ScoutBoard zajednici.">
      <Box component="form" onSubmit={submit}>
        <Stack spacing={2.2}>
          {error && <Alert severity="error">{error}</Alert>}
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <TextField required fullWidth label="Ime" value={form.firstName} onChange={change('firstName')} />
            <TextField required fullWidth label="Prezime" value={form.lastName} onChange={change('lastName')} />
          </Stack>
          <TextField required type="email" label="E-mail adresa" value={form.email} onChange={change('email')} />
          <TextField select label="Registrujem se kao" value={form.role} onChange={change('role')}>
            <MenuItem value="Player">Igrač</MenuItem>
            <MenuItem value="CoachScout">Trener/skaut</MenuItem>
          </TextField>
          <TextField required type="password" label="Lozinka" helperText="Najmanje 8 karaktera, veliko i malo slovo i broj." value={form.password} onChange={change('password')} />
          <TextField required type="password" label="Potvrda lozinke" value={form.confirmPassword} onChange={change('confirmPassword')} />
          <Button type="submit" variant="contained" size="large" disabled={submitting}>
            {submitting ? 'Kreiranje naloga…' : 'Registruj se'}
          </Button>
          <Typography textAlign="center" color="text.secondary">
            Već imate nalog? <MuiLink component={Link} to="/prijava">Prijavite se</MuiLink>
          </Typography>
        </Stack>
      </Box>
    </AuthShell>
  )
}

export function VerifyEmailPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const [email, setEmail] = useState(location.state?.email || '')
  const [code, setCode] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const submit = async (event) => {
    event.preventDefault()
    setError('')
    try {
      const { data } = await api.post('/auth/verify-email', { email, code })
      setMessage(data.message)
      setTimeout(() => navigate('/prijava'), 1000)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  const resend = async () => {
    setError('')
    try {
      const { data } = await api.post('/auth/resend-otp', { email })
      setMessage(data.message)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  return (
    <AuthShell title="Potvrdite e-mail" description="Unesite šestocifreni kod koji smo poslali na vašu adresu.">
      <Box component="form" onSubmit={submit}>
        <Stack spacing={2.5}>
          {error && <Alert severity="error">{error}</Alert>}
          {message && <Alert severity="success">{message}</Alert>}
          <TextField required type="email" label="E-mail adresa" value={email} onChange={(event) => setEmail(event.target.value)} />
          <TextField
            required
            label="OTP kod"
            value={code}
            onChange={(event) => setCode(event.target.value.replace(/\D/g, '').slice(0, 6))}
            inputProps={{ inputMode: 'numeric', pattern: '[0-9]{6}' }}
          />
          <Button type="submit" variant="contained" size="large">Potvrdi adresu</Button>
          <Button onClick={resend}>Pošalji novi kod</Button>
        </Stack>
      </Box>
    </AuthShell>
  )
}
