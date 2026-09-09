import { lazy, Suspense } from 'react'
import { Box, CircularProgress } from '@mui/material'
import { Route, Routes } from 'react-router-dom'
import PublicLayout from './components/PublicLayout'
import ProtectedRoute from './components/ProtectedRoute'

const AdminPage = lazy(() => import('./pages/AdminPage'))
const ClubDetailPage = lazy(() => import('./pages/ClubDetailPage'))
const ClubsPage = lazy(() => import('./pages/ClubsPage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const HomePage = lazy(() => import('./pages/HomePage'))
const MatchDetailPage = lazy(() => import('./pages/MatchDetailPage'))
const MatchesPage = lazy(() => import('./pages/MatchesPage'))
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'))
const PlayerDetailPage = lazy(() => import('./pages/PlayerDetailPage'))
const PlayersPage = lazy(() => import('./pages/PlayersPage'))
const CompetitionDetailPage = lazy(() => import('./pages/competitions/CompetitionDetailPage'))
const CompetitionsPage = lazy(() => import('./pages/competitions/CompetitionsPage'))
const NotificationsPage = lazy(() => import('./pages/notifications/NotificationsPage'))
const PlayerComparisonPage = lazy(() => import('./pages/players/PlayerComparisonPage'))
const MyTryoutApplicationsPage = lazy(() => import('./pages/tryouts/MyTryoutApplicationsPage'))
const TryoutDetailPage = lazy(() => import('./pages/tryouts/TryoutDetailPage'))
const TryoutsPage = lazy(() => import('./pages/tryouts/TryoutsPage'))

const LoginPage = lazy(() => import('./pages/AuthPages').then((module) => ({ default: module.LoginPage })))
const RegisterPage = lazy(() => import('./pages/AuthPages').then((module) => ({ default: module.RegisterPage })))
const VerifyEmailPage = lazy(() => import('./pages/AuthPages').then((module) => ({ default: module.VerifyEmailPage })))
const MyClubsPage = lazy(() => import('./pages/CoachPages').then((module) => ({ default: module.MyClubsPage })))
const ManageClubPage = lazy(() => import('./pages/CoachPages').then((module) => ({ default: module.ManageClubPage })))
const ManageMatchPage = lazy(() => import('./pages/CoachPages').then((module) => ({ default: module.ManageMatchPage })))
const ScoutingReportsPage = lazy(() => import('./pages/CoachPages').then((module) => ({ default: module.ScoutingReportsPage })))
const WatchlistPage = lazy(() => import('./pages/CoachPages').then((module) => ({ default: module.WatchlistPage })))
const MyProfilePage = lazy(() => import('./pages/PlayerAccountPages').then((module) => ({ default: module.MyProfilePage })))
const InvitationsPage = lazy(() => import('./pages/PlayerAccountPages').then((module) => ({ default: module.InvitationsPage })))
const MyStatisticsPage = lazy(() => import('./pages/PlayerAccountPages').then((module) => ({ default: module.MyStatisticsPage })))

const protectedPage = (element, roles) => (
  <ProtectedRoute roles={roles}>{element}</ProtectedRoute>
)

export default function App() {
  return (
    <Suspense fallback={(
      <Box sx={{ minHeight: '50vh', display: 'grid', placeItems: 'center' }}>
        <CircularProgress color="secondary" />
      </Box>
    )}>
      <Routes>
        <Route element={<PublicLayout />}>
        <Route index element={<HomePage />} />
        <Route path="igraci" element={<PlayersPage />} />
        <Route path="igraci/:id" element={<PlayerDetailPage />} />
        <Route path="klubovi" element={<ClubsPage />} />
        <Route path="klubovi/:id" element={<ClubDetailPage />} />
        <Route path="utakmice" element={<MatchesPage />} />
        <Route path="utakmice/:id" element={<MatchDetailPage />} />
        <Route path="takmicenja" element={<CompetitionsPage />} />
        <Route path="takmicenja/:id" element={<CompetitionDetailPage />} />
        <Route path="probe" element={<TryoutsPage />} />
        <Route path="probe/:id" element={<TryoutDetailPage />} />
        <Route path="poredjenje-igraca" element={<PlayerComparisonPage />} />
        <Route path="prijava" element={<LoginPage />} />
        <Route path="registracija" element={<RegisterPage />} />
        <Route path="potvrda-emaila" element={<VerifyEmailPage />} />

        <Route path="kontrolna-tabla" element={protectedPage(<DashboardPage />)} />
        <Route path="moj-profil" element={protectedPage(<MyProfilePage />, ['Player'])} />
        <Route path="moji-pozivi" element={protectedPage(<InvitationsPage />, ['Player'])} />
        <Route path="moja-statistika" element={protectedPage(<MyStatisticsPage />, ['Player'])} />
        <Route path="moje-prijave-za-probe" element={protectedPage(<MyTryoutApplicationsPage />, ['Player'])} />
        <Route path="obavestenja" element={protectedPage(<NotificationsPage />)} />

        <Route path="moji-klubovi" element={protectedPage(<MyClubsPage />, ['CoachScout'])} />
        <Route path="moji-klubovi/:id" element={protectedPage(<ManageClubPage />, ['CoachScout'])} />
        <Route path="upravljanje-utakmicom/:id" element={protectedPage(<ManageMatchPage />, ['CoachScout'])} />
        <Route path="skautski-izvestaji" element={protectedPage(<ScoutingReportsPage />, ['CoachScout'])} />
        <Route path="lista-pracenja" element={protectedPage(<WatchlistPage />, ['CoachScout'])} />

        <Route path="admin" element={protectedPage(<AdminPage />, ['Admin'])} />
        <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </Suspense>
  )
}
