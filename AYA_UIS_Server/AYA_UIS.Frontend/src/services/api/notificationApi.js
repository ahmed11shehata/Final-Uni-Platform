// src/services/api/notificationApi.js
// Shared notification endpoints — work for all roles (Student, Instructor, Admin)
import api from "./axiosInstance";

/**
 * GET /api/notifications
 * Returns all notifications for the currently authenticated user.
 */
export const getNotifications = async () => {
  const res = await api.get("/notifications");
  return res.data.data;
};

/**
 * PUT /api/notifications/{id}/read
 * Marks a single notification as read.
 */
export const markNotificationRead = async (id) => {
  await api.put(`/notifications/${id}/read`);
};

/**
 * PUT /api/notifications/read-all
 * Marks all notifications as read for the current user.
 */
export const markAllNotificationsRead = async () => {
  await api.put("/notifications/read-all");
};

/**
 * GET /api/notifications/unread-count
 * Returns { count: number }
 */
export const getUnreadCount = async () => {
  const res = await api.get("/notifications/unread-count");
  return res.data.data.count;
};
