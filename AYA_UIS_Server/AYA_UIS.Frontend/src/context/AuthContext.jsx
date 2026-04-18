import { createContext, useState, useEffect, useCallback } from "react";
import { getCurrentUser, logoutUser } from "../services/api/authApi";

export const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser]       = useState(null);
  const [token, setToken]     = useState(null);
  const [loading, setLoading] = useState(true);

  // On mount: if we have a saved token, hydrate user from /api/auth/me
  useEffect(() => {
    const savedToken = localStorage.getItem("token");
    if (!savedToken) {
      setLoading(false);
      return;
    }

    setToken(savedToken);

    // Try to hydrate from saved user first (instant), then refresh from API
    const savedUser = localStorage.getItem("user");
    if (savedUser) {
      try { setUser(JSON.parse(savedUser)); } catch { /* ignore */ }
    }

    getCurrentUser()
      .then((freshUser) => {
        setUser(freshUser);
        localStorage.setItem("user", JSON.stringify(freshUser));
      })
      .catch(() => {
        // Token expired or invalid — clear everything
        setUser(null);
        setToken(null);
        localStorage.removeItem("token");
        localStorage.removeItem("user");
      })
      .finally(() => setLoading(false));
  }, []);

  const login = useCallback((userData, userToken) => {
    setUser(userData);
    setToken(userToken);
    localStorage.setItem("token", userToken);
    localStorage.setItem("user", JSON.stringify(userData));
  }, []);

  const logout = useCallback(async () => {
    // Call backend to invalidate token
    await logoutUser();
    setUser(null);
    setToken(null);
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  }, []);

  return (
    <AuthContext.Provider value={{ user, token, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}