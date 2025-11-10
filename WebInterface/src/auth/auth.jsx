import { createContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from '../api/api'

const AuthContext = createContext();

export const AuthActionType = {

    LOGGEDIN_USER: "REGISTER_USER",
    LOGOUT_USER: "LOGOUT_USER",
    GUEST: "GUEST",
}

function AuthContextProvider(props) {
    const [auth, setAuth] = useState({
        user: null,
        loggedIn: false
    });

    const navigate = useNavigate();

    useEffect(() => {
        auth.getLoggedIn();
    }, []);

    const authReducer = (action) => {
        const { type, payload } = action;
        switch (type) {
            case AuthActionType.LOGIN_USER: {
                return setAuth({
                    user: payload.user,
                    loggedIn: (payload.user.name !=='Guest'),
                })
            }
            case AuthActionType.GUEST: {
                return setAuth({
                    user: {
                    firstName: 'Guest',
                    lastName: 'Guest',
                    name: 'Guest',
                    email: 'Guest@gmail.com'
                    },
                    loggedIn: true,
                })
            }
            case AuthActionType.LOGOUT_USER: {
                return setAuth({
                    user: null,
                    loggedIn: false,
                })
            }
            default:
                return auth;
        }
    }

    auth.logInAsGuest = function() {
        authReducer({
            type: AuthActionType.GUEST,
            payload: null
        });
    }

    auth.getLoggedIn = async function () {
        try {
            const response = await api.getLoggedIn();
            if (response.status === 200) {
                authReducer({
                    type: AuthActionType.LOGIN_USER,
                    payload: {
                        user: response.data.user
                    }
                });
            }
        }
        catch (err) {
            console.error(err);
        }
    }

    auth.registerUser = async function(userData, store) {
        try {
            const response = await api.registerUser(userData);      
            if (response.status === 200) {
                authReducer({
                    type: AuthActionType.LOGIN_USER,
                    payload: {
                        user: response.data.user
                    }
                })
                navigate.push('/')
                store.loadIdNamePairs();
            }
        }
        catch (err) {
            try {
                store.showAlert(err.response.data.errorMessage);
            }
            catch (error) {
                console.error(error);
            }
        }
    }

    auth.loginUser = async function(userData, store) {
        try {
            const response = await api.loginUser(userData);
            if (response.status === 200) {
                authReducer({
                    type: AuthActionType.LOGIN_USER,
                    payload: {
                        user: response.data.user
                    }
                })
                navigate.push('/')
                store.loadIdNamePairs();
            }
        }
        catch (err) {
            try {
                store.showAlert(err.response.data.errorMessage);
            }
            catch (error) {
                console.error(error);
            }
        }
    }

    auth.logoutUser = async function(store) {
        try {
            await api.logoutUser();
        }
        catch {}

        authReducer({
            type: AuthActionType.LOGOUT_USER,
            payload: null
        });
        store.logout();
        document.cookie = '';
        navigate.push("/");
    }

    return (
        <AuthContext.Provider value={{
            auth
        }}>
            {props.children}
        </AuthContext.Provider>
    );
}

export default AuthContext;
export { AuthContextProvider };
