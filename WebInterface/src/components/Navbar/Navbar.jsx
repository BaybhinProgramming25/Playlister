import './Navbar.css'
import { useState } from 'react'

import HamburgerToggle from './HamburgerToggle.jsx'

const Navbar = () => {
    
    const [hamburgerMenuOpen, setHamburgerMenuOpen] = useState(false)

    return (
        <div>
            <header className='top-grid'>

                <h1 className='tg-header'>Playlister</h1>
                <div className='tg-contact'>+1 (800) PLAYLISTER</div>
            </header>
            <header className='bottom-grid'>
                <nav className='middle-tabs'>
                    <button className='hamburger-button' onClick={() => setHamburgerMenuOpen(!hamburgerMenuOpen)}>
                        <HamburgerToggle isOpen={hamburgerMenuOpen}/>
                    </button>
                    <div className='hamburger-content'>
                        <a href="/">Home</a>
                        <a href="/">About</a>
                        <a href="/">Contact Us</a>
                        <a href="/">Login</a>
                        <a href="/">Sign Up</a>
                    </div>
                </nav>
            </header>
        </div>
    )
}

export default Navbar;