import React, { useContext, useEffect } from 'react';
import { Button, Box, Typography, InputBase, Menu, MenuItem } from '@mui/material';
import HomeIcon from '@mui/icons-material/Home';
import GroupsIcon from '@mui/icons-material/Groups';
import PersonIcon from '@mui/icons-material/Person';
import FunctionsIcon from '@mui/icons-material/Functions';
import SortIcon from '@mui/icons-material/Sort';
import List from '@mui/material/List';
import './HomeScreen.css';

const HomeScreen = () => {
    const [anchorEl, setAnchorEl] = React.useState(null);
    const open = Boolean(anchorEl);

    useEffect(() => {
        // Load initial data here
    }, []);

    const handleLoadHome = async () => {
        console.log('Loading home view');
        // Add your home loading logic here
    };

    const handleLoadGroup = async () => {
        console.log('Loading group view');
        // Add your group loading logic here
    };

    const handleLoadUser = async () => {
        console.log('Loading user view');
        // Add your user loading logic here
    };

    const handleLoadCommunity = async () => {
        console.log('Loading community view');
        // Add your community loading logic here
    };

    const handleFilterList = (event) => {
        console.log('Filtering:', event.target.value);
        // Add your filter logic here
    };

    const handleClick = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleSortByNewest = async () => {
        handleClose();
        console.log('Sorting by newest');
        // Add your sort logic here
    };

    const handleSortByOldest = async () => {
        handleClose();
        console.log('Sorting by oldest');
        // Add your sort logic here
    };

    const handleSortByViews = async () => {
        handleClose();
        console.log('Sorting by views');
        // Add your sort logic here
    };

    const handleSortByLikes = async () => {
        handleClose();
        console.log('Sorting by likes');
        // Add your sort logic here
    };

    const handleSortByDislikes = async () => {
        handleClose();
        console.log('Sorting by dislikes');
        // Add your sort logic here
    };

    return (
        <div className="home-container">
            <div className="home-box">
                <section className="hero-section">
                    <div id="list-selector-heading">
                        <Button
                            color="primary"
                            aria-label="home"
                            id="home-button"
                            onClick={handleLoadHome}
                        >
                            <HomeIcon />
                        </Button>
                        <Button
                            color="primary"
                            aria-label="group"
                            id="group-button"
                            onClick={handleLoadGroup}
                        >
                            <GroupsIcon />
                        </Button>
                        <Button
                            color="primary"
                            aria-label="user"
                            id="user-button"
                            onClick={handleLoadUser}
                        >
                            <PersonIcon />
                        </Button>
                        <Button
                            color="primary"
                            aria-label="community"
                            id="community-button"
                            onClick={handleLoadCommunity}
                        >
                            <FunctionsIcon />
                        </Button>
                        <InputBase
                            sx={{ width: '30%', maxWidth: '30%' }}
                            id="search"
                            placeholder=" Search…"
                            onChange={handleFilterList}
                        />
                        <Box sx={{ flexGrow: 1 }} />
                        <Box sx={{ display: { xs: 'none', md: 'flex' } }}>
                            <Box component="span" sx={{ p: 2 }}>
                                <Typography variant="h6" id="sort-text">
                                    SORT BY
                                </Typography>
                            </Box>
                            <Button
                                color="primary"
                                aria-label="sort"
                                id="sort-button"
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
                                MenuListProps={{
                                    'aria-labelledby': 'basic-button',
                                }}
                            >
                                <MenuItem onClick={handleSortByNewest}>
                                    Published Date (Newest)
                                </MenuItem>
                                <MenuItem onClick={handleSortByOldest}>
                                    Published Date (Oldest)
                                </MenuItem>
                                <MenuItem onClick={handleSortByViews}>Views</MenuItem>
                                <MenuItem onClick={handleSortByLikes}>Likes</MenuItem>
                                <MenuItem onClick={handleSortByDislikes}>Dislikes</MenuItem>
                            </Menu>
                        </Box>
                    </div>
                </section>

                <section className="feature-section">
                    <div className="feature-card">
                        <div className="feature-image">
                            <img
                                src="https://images.unsplash.com/photo-1569336415962-a4bd9f69cd83?w=800&q=80"
                                alt="Main feature"
                            />
                        </div>
                        <h2>Welcome to Our Platform</h2>
                        <p>
                            Discover amazing content and connect with others. Our platform makes it
                            easy to browse, search, and organize your favorite items.
                        </p>
                        <p>
                            Use the navigation buttons above to explore different views - your personal
                            home, group collections, user profiles, or the community feed.
                        </p>
                    </div>

                    <div className="feature-card">
                        <div className="feature-image">
                            <img
                                src="https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&q=80"
                                alt="Getting started"
                            />
                        </div>
                        <h2>Getting Started</h2>
                        <p>
                            Start by selecting a view using the icon buttons at the top. Each view
                            offers a unique perspective on the content available.
                        </p>
                        <ul>
                            <li>Home: Your personalized content</li>
                            <li>Groups: Shared collections</li>
                            <li>Users: Browse by creator</li>
                            <li>Community: Popular and trending</li>
                        </ul>
                    </div>

                    <div className="feature-card">
                        <div className="feature-image">
                            <img
                                src="https://images.unsplash.com/photo-1488190211105-8b0e65b80b4e?w=800&q=80"
                                alt="Features"
                            />
                        </div>
                        <h2>Powerful Features</h2>
                        <p>
                            Sort your content by newest, oldest, views, likes, or dislikes. Use the
                            search bar to quickly find exactly what you're looking for.
                        </p>
                        <p>
                            Our intuitive interface makes browsing and discovering content a breeze.
                            Start exploring today!
                        </p>
                    </div>
                </section>

                <section className="cta-section">
                    <h2>Ready to Start?</h2>
                    <p>Select a view above to begin exploring content.</p>
                </section>
            </div>
        </div>
    );
};

export default HomeScreen;