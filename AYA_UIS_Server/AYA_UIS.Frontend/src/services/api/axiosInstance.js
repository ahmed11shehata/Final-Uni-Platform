import axios from "axios";

const BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "https://localhost:7121/api";

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 30000,
  headers: { "Content-Type": "application/json" },
});

// Request: attach JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response: global error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (!error.response) {
      // Network error / server down
      return Promise.reject(
        new Error("Cannot connect to server. Make sure the backend is running.")
      );
    }

    const { status, data } = error.response;

    if (status === 401) {
      // On login page: pass through backend error (e.g. "Invalid email or password")
      const isLoginPage = window.location.pathname === "/login";
      const backendMsg =
        data?.error?.message || data?.message || null;

      if (isLoginPage) {
        const eLogin = new Error(backendMsg || "Invalid email or password.");
        eLogin.response = error.response;
        return Promise.reject(eLogin);
      }

      // Elsewhere: token expired — clear and redirect
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login";
      const e401 = new Error("Session expired. Please log in again.");
      e401.response = error.response;
      return Promise.reject(e401);
    }

    if (status === 403) {
      const e403 = new Error("You do not have permission to perform this action.");
      e403.response = error.response;
      return Promise.reject(e403);
    }

    if (status === 404) {
      const e404 = new Error(data?.message || "Resource not found.");
      e404.response = error.response;
      return Promise.reject(e404);
    }

    if (status === 429) {
      const e429 = new Error("Too many requests. Please wait a moment and try again.");
      e429.response = error.response;
      return Promise.reject(e429);
    }

    // Extract error message from backend response shapes
    const message =
      data?.errors ||
      data?.error?.message ||
      data?.message ||
      data?.title ||
      `Server error (${status})`;

    const eGeneral = new Error(Array.isArray(message) ? message.join(" - ") : message);
    eGeneral.response = error.response;
    return Promise.reject(eGeneral);
  }
);

export default api;
