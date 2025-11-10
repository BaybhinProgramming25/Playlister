import axios from 'axios'
axios.defaults.withCredentials = true;

const api = axios.create({
    baseURL: 'http://localhost:4000/api',
})

// Playlister APIs
export const createTop5List = (payload) => api.post(`/top5list/`, payload)
export const getTop5Lists = () => api.get(`/top5lists/`)
export const updateTop5ListById = (id, payload) => api.put(`/top5list/${id}`, payload)
export const publishTop5ListById = (id, payload) => api.put(`/publishtop5list/${id}`, payload)
export const deleteTop5ListById = (id) => api.delete(`/top5list/${id}`)
export const getTop5ListById = (id) => api.get(`/top5list/${id}`)
export const getCommunityLists = () => api.get(`/communitylists/`)
export const getCommunityListById = (id) => api.get(`/communitylist/${id}`)
export const updateTop5ListViews = (id) => api.get(`/top5listviews/${id}`)
export const updateTop5ListLikes = (id) => api.get(`/top5listlikes/${id}`)
export const updateTop5ListDislikes = (id) => api.get(`/top5listdislikes/${id}`)
export const updateCommunityListViews = (id) => api.get(`/communitylistviews/${id}`)
export const updateCommunityListLikes = (id) => api.get(`/communitylistlikes/${id}`)
export const updateCommunityListDislikes = (id) => api.get(`/communitylistdislikes/${id}`)
export const updateTop5ListComment = (id, payload) => api.put(`/commenttop5list/${id}`, payload)
export const updateCommunityListComment = (id, payload) => api.put(`/commentcommunitylist/${id}`, payload)

// User APIs
export const getLoggedIn = () => api.get(`/loggedIn/`);
export const registerUser = (payload) => api.post(`/register/`, payload)
export const loginUser = (payload) => api.post(`/login/`, payload)
export const logoutUser = () => api.get(`/logout/`)

// List all apis and export 
const apis = {
    createTop5List,
    getTop5Lists,
    updateTop5ListById,
    deleteTop5ListById,
    getTop5ListById,
    getCommunityLists,
    getCommunityListById,
    publishTop5ListById,
    updateTop5ListViews,
    updateTop5ListLikes,
    updateTop5ListDislikes,
    updateCommunityListViews,
    updateCommunityListLikes,
    updateCommunityListDislikes,
    updateTop5ListComment,
    updateCommunityListComment,
    getLoggedIn,
    registerUser,
    loginUser,
    logoutUser
}

export default apis
