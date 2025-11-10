const HamburgerToggle = ({ isOpen }) => {

    if (isOpen) {
        return (
            <div className='open'>
                <span></span>
                <span></span>
                <span></span>
            </div>
        )
    }
    
    return (
        <>
            <span></span>
            <span></span>
            <span></span>
        </>
    )
}

export default HamburgerToggle;