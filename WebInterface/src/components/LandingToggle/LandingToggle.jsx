import { useContext } from 'react'

import LandingPage from '../LandingPage/LandingPage' 
import SplashScreen from '../SplashScreen/SplashScreen'
import AuthContext from '../../auth/auth.jsx'

const LandingToggle = () => {

    const { auth } = useContext(AuthContext);
    
    if (auth.loggedIn)
        return <LandingPage />
    else
        return <SplashScreen />
}

export default LandingToggle;