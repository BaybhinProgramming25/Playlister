import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { AuthContextProvider } from './auth/auth.jsx'
import { GlobalStoreContextProvider } from './store/store.jsx'

import Navbar from './components/Navbar/Navbar'
import Alert from './components/Alert/Alert'
import LandingToggle from './components/LandingToggle/LandingToggle'
import RegisterScreen from './components/RegisterScreen/RegisterScreen'
import WorkspaceScreen from './components/WorkspaceScreen/WorkspaceScreen'
import LoginScreen from './components/LoginScreen/LoginScreen'
import LandingPage from './components/LandingPage/LandingPage'

import './App.css'

const App = () => {

  return (
    <BrowserRouter>
      <AuthContextProvider>
          <GlobalStoreContextProvider>
            <Navbar />
            <Alert />
            <Routes>
                <Route path="/" element={<LandingToggle />} />
                <Route path="/register" element={<RegisterScreen />} />
                <Route path="/top5list/:id" element={<WorkspaceScreen />} />
                <Route path="/login/" element={<LoginScreen />} />
                <Route path="/lists/" element={<LandingPage />} />
            </Routes>
          </GlobalStoreContextProvider>
      </AuthContextProvider>
    </BrowserRouter>
  )
}

export default App;

