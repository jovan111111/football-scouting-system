import { Box, Typography } from '@mui/material'

export default function PageHeader({ eyebrow, title, description, action }) {
  return (
    <Box
      sx={{
        mb: 4,
        display: 'flex',
        alignItems: { xs: 'flex-start', md: 'center' },
        justifyContent: 'space-between',
        gap: 2,
        flexDirection: { xs: 'column', md: 'row' },
      }}
    >
      <Box>
        {eyebrow && (
          <Typography
            variant="overline"
            color="secondary.dark"
            sx={{ fontWeight: 800, letterSpacing: 1.4 }}
          >
            {eyebrow}
          </Typography>
        )}
        <Typography variant="h3" component="h1">{title}</Typography>
        {description && (
          <Typography color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>
            {description}
          </Typography>
        )}
      </Box>
      {action}
    </Box>
  )
}
