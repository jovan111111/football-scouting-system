import { Chip } from '@mui/material'

export default function StatusChip({ label, status }) {
  const color = {
    Approved: 'success',
    Active: 'success',
    Open: 'success',
    Completed: 'success',
    Accepted: 'success',
    Pending: 'warning',
    Scheduled: 'info',
    Planned: 'info',
    Rejected: 'error',
    Cancelled: 'default',
    Ended: 'default',
    Closed: 'default',
  }[status] || 'default'

  return <Chip label={label} color={color} size="small" variant="outlined" />
}
