import React, { useContext, useEffect } from 'react'
import { GlobalStoreContext } from '../../store/store.jsx'

import { Button, Box, Typography, InputBase, Menu, MenuItem } from '@mui/material'
import HomeIcon from '@mui/icons-material/Home';
import GroupsIcon from '@mui/icons-material/Groups';
import PersonIcon from '@mui/icons-material/Person';
import FunctionsIcon from '@mui/icons-material/Functions';
import SortIcon from '@mui/icons-material/Sort';
import List from '@mui/material/List';

import ListCard from '../ListCard/ListCard.jsx'

import DeleteModal from '../DeleteModal/DeleteModal.jsx'
import AuthContext from '../../auth/auth.jsx'

import './LandingPage.css';


const LandingPage = () => {

    const { store } = useContext(GlobalStoreContext);
    const { auth } = useContext(AuthContext);

    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);

    useEffect(() => {
        store.loadIdNamePairs();
    }, []);

    async function handleLoadHome() {
        if (auth.user.name !== 'Guest') {
            store.loadHomeView();
        }
    }

    async function handleLoadGroup() {
        store.loadGroupView();
    }

    async function handleLoadUser() {
        store.loadUserView();
    }

    async function handleLoadCommunity() {
        store.loadCommunityView();
    }

    async function handleFiltList(event) {
        store.filter(event.target.value);
    }

    const handleClick = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    async function handleSortByNewest() {
        handleClose();
        store.sort(1);
    }

    async function handleSortByOldest() {
        handleClose();
        store.sort(0);
    }

    async function handleSortByViews() {
        handleClose();
        store.sort(2);
    }

    async function handleSortByLikes() {
        handleClose();
        store.sort(3);
    }

    async function handleSortByDislikes() {
        handleClose();
        store.sort(4);
    }

    let listCard = "";

    if (store) {
        listCard =
            <List className="home-list-container">
            {
                store.idNamePairs.map((pair) => (
                    <div key={pair._id}>
                        <ListCard
                            idNamePair={pair}
                            selected={false}
                        />
                        <List className="list-spacer"></List>
                    </div>
                ))
            }
            </List>;
    }
    
    return (
        <div id="top5-list-selector">
            <DeleteModal />
            <div id="list-selector-heading">
                <Button
                    color="primary" 
                    aria-label="home"
                    id="home-button"
                    className="nav-button"
                    disabled={auth.user.name === 'Guest'}
                    onClick={() => { handleLoadHome() }}
                >
                    <HomeIcon />
                </Button>
                <Button
                    color="primary" 
                    aria-label="group"
                    id="group-button"
                    className="nav-button"
                    onClick={() => { handleLoadGroup() }}
                >
                    <GroupsIcon />
                </Button>
                <Button
                    color="primary" 
                    aria-label="user"
                    id="user-button"
                    className="nav-button"
                    onClick={() => { handleLoadUser() }}
                >
                    <PersonIcon />
                </Button>
                <Button
                    color='primary'
                    aria-label='community'
                    id='community-button'
                    className="nav-button"
                    onClick={() => { handleLoadCommunity() }}
                >
                    <FunctionsIcon />
                </Button>
                <InputBase 
                    className="search-input"
                    id='search'
                    placeholder=" Search…"
                    onChange={handleFiltList}
                >
                </InputBase>
                <Box className="flex-grow" />
                <Box className="sort-container">
                    <Box component="span" className="sort-text-box">
                        <Typography variant='h6' id='sort-text'>SORT BY</Typography>
                    </Box>
                    <Button
                        color='primary'
                        aria-label='sort'
                        id='sort-button'
                        className="nav-button"
                        onClick={handleClick}
                    >
                        <SortIcon />
                    </Button>
                    <Menu
                        id="sort"
                        anchorEl={anchorEl}
                        anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'left',
                        }}
                        transformOrigin={{
                            vertical: 'top',
                            horizontal: 'right',
                        }}
                        open={open}
                        onClose={handleClose}
                        slotProps={{
                            paper: {
                                'aria-labelledby': 'basic-button',
                            }
                        }}
                    >
                        <MenuItem onClick={handleSortByNewest}>Published Date (Newest)</MenuItem>
                        <MenuItem onClick={handleSortByOldest}>Published Date (Oldest)</MenuItem>
                        <MenuItem onClick={handleSortByViews}>Views</MenuItem>
                        <MenuItem onClick={handleSortByLikes}>Likes</MenuItem>
                        <MenuItem onClick={handleSortByDislikes}>Dislikes</MenuItem>
                    </Menu>
                </Box>
            </div>
            <div id="list-selector-list">
                {listCard}
            </div>
        </div>
    )
}

export default LandingPage;