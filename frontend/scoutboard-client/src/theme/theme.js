import { createTheme } from '@mui/material/styles'

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#12372A',
      light: '#2E7D32',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#D4A72C',
      dark: '#A67C14',
      contrastText: '#1B241E',
    },
    background: {
      default: '#F5F7F5',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1B241E',
      secondary: '#5D6A62',
    },
  },
  shape: { borderRadius: 14 },
  typography: {
    fontFamily: '"Roboto", Arial, sans-serif',
    h1: { fontWeight: 800, letterSpacing: '-0.04em' },
    h2: { fontWeight: 800, letterSpacing: '-0.03em' },
    h3: { fontWeight: 750, letterSpacing: '-0.02em' },
    h4: { fontWeight: 750 },
    h5: { fontWeight: 700 },
    button: { fontWeight: 700, textTransform: 'none' },
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { borderRadius: 10, paddingInline: 18 } },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          border: '1px solid rgba(18, 55, 42, 0.08)',
          boxShadow: '0 14px 34px rgba(18, 55, 42, 0.07)',
        },
      },
    },
    MuiTextField: {
      defaultProps: { size: 'small' },
    },
  },
})

export default theme
