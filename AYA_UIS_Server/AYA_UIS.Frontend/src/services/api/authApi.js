import api from "./axiosInstance";

/**
 * POST /api/auth/login
 * Body: { email, password, role }
 * Response: { success, data: { user, token } }
 */
export const loginUser = async ({ email, password, role }) => {
  const res = await api.post("/auth/login", { email, password, role });
  // Backend returns { success, data: { user, token } }
  return res.data.data; // { user, token }
};

/**
 * GET /api/auth/me
 * Response: { success, data: { user } }
 */
export const getCurrentUser = async () => {
  const res = await api.get("/auth/me");
  return res.data.data.user;
};

/**
 * POST /api/auth/logout
 * Invalidates the current token server-side.
 */
export const logoutUser = async () => {
  try {
    await api.post("/auth/logout");
  } catch {
    // If server is down or token already expired, still clear locally
  }
};

/**
 * GET /api/user/profile
 * Response: { success, data: { user } }
 */
export const getUserProfile = async () => {
  const res = await api.get("/user/profile");
  return res.data.data.user;
};

export const updateUserProfile = async (dto) => {
  const res = await api.put("/user/profile", dto);
  return res.data.data;
};

export const changePassword = async (dto) => {
  const res = await api.post("/authentication/change-password", dto);
  return res.data;
};

export const forgotPasswordRequest = async (email) => {
  const res = await api.post("/authentication/forgot-password/request", { email });
  return res.data;
};

export const forgotPasswordVerify = async (dto) => {
  const res = await api.post("/authentication/forgot-password/verify", dto);
  return res.data;
};