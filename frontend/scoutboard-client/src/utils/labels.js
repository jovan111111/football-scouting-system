export const cityLabels = {
  NoviPazar: 'Novi Pazar',
  Tutin: 'Tutin',
  Sjenica: 'Sjenica',
  Raska: 'Raška',
}

export const positionLabels = {
  Goalkeeper: 'Golman',
  RightBack: 'Desni bek',
  CentreBack: 'Štoper',
  LeftBack: 'Levi bek',
  DefensiveMidfielder: 'Defanzivni vezni',
  CentralMidfielder: 'Centralni vezni',
  AttackingMidfielder: 'Ofanzivni vezni',
  RightWinger: 'Desno krilo',
  LeftWinger: 'Levo krilo',
  Striker: 'Napadač',
}

export const footLabels = {
  Left: 'Leva',
  Right: 'Desna',
  Both: 'Obe',
}

export const approvalLabels = {
  Pending: 'Čeka odobrenje',
  Approved: 'Odobren',
  Rejected: 'Odbijen',
}

export const membershipLabels = {
  Pending: 'Na čekanju',
  Active: 'Aktivno',
  Rejected: 'Odbijeno',
  Ended: 'Završeno',
}

export const matchStatusLabels = {
  Scheduled: 'Zakazana',
  Completed: 'Završena',
  Cancelled: 'Otkazana',
}

export const recommendationLabels = {
  NotRecommended: 'Nije preporučen',
  Follow: 'Nastaviti praćenje',
  Recommended: 'Preporučen',
}

export const watchlistLabels = {
  Noticed: 'Primećen',
  Contacted: 'Kontaktiran',
  Observed: 'Posmatran',
  Recommended: 'Preporučen',
  Rejected: 'Odbijen',
}

export const correctionLabels = {
  Pending: 'Na čekanju',
  Accepted: 'Prihvaćen',
  Rejected: 'Odbijen',
}

export const competitionStatusLabels = {
  Planned: 'Planirano',
  Active: 'Aktivno',
  Completed: 'Završeno',
}

export const tryoutStatusLabels = {
  Open: 'Prijave otvorene',
  Closed: 'Prijave zatvorene',
  Cancelled: 'Otkazano',
}

export const tryoutApplicationLabels = {
  Pending: 'Čeka odgovor',
  Accepted: 'Prihvaćena',
  Rejected: 'Odbijena',
  Cancelled: 'Otkazana',
}

export const roleLabels = {
  Player: 'Igrač',
  CoachScout: 'Trener/skaut',
  Admin: 'Administrator',
}

export const cities = Object.keys(cityLabels)
export const positions = Object.keys(positionLabels)
export const feet = Object.keys(footLabels)

export const formatDate = (value, withTime = false) => {
  if (!value) return '—'
  const date = new Date(value)
  return new Intl.DateTimeFormat('sr-RS', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    ...(withTime ? { hour: '2-digit', minute: '2-digit' } : {}),
  }).format(date)
}

export const toDateInput = (value) => value ? value.slice(0, 10) : ''
