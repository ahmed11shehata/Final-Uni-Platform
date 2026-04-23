// src/context/NotificationContext.jsx
import { createContext, useContext, useState, useCallback, useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { useAuth } from "../hooks/useAuth";
import {
  getNotifications,
  markNotificationRead,
  markAllNotificationsRead,
} from "../services/api/notificationApi";

const NotificationContext = createContext(null);

// ─── Hub URL (strips /api suffix from the base URL) ───────────────
const HUB_URL = (import.meta.env.VITE_API_BASE_URL || "https://localhost:7121/api")
  .replace(/\/api\/?$/, "") + "/hubs/notifications";

// ─── Map an API notification to the context shape ─────────────────
function mapApiNotif(n) {
  const d = n.detail || {};
  return {
    id:    n.id,
    type:  n.type,
    read:  n.isRead,
    title: n.title,
    body:  n.body,
    time:  n.time || "Just now",
    detail: {
      // Course
      course:   d.courseName   ?? null,
      courseId: d.courseId     ?? null,

      // Assignment
      assignment:   d.assignmentTitle ?? null,
      assignmentId: d.assignmentId    ?? null,
      grade:  d.grade          ?? null,
      max:    d.max            ?? null,
      reason: d.rejectionReason ?? null,

      // Quiz
      quiz:   d.quizTitle  ?? null,
      quizId: d.quizId     ?? null,

      // Lecture
      lecture:   d.lectureTitle ?? null,
      lectureId: d.lectureId   ?? null,

      // People
      instructor:      d.instructorName  ?? null,
      student:         d.studentName     ?? null,
      studentId:       d.studentCode     ?? null,
      targetStudentId: d.targetStudentId ?? null,

      // Timestamp
      submittedAt: d.submittedAt ?? null,
    },
  };
}

export function NotificationProvider({ children }) {
  // NotificationProvider is inside AuthProvider — safe to use useAuth
  const { user } = useAuth();

  // Notifications keyed by role: { student: [], instructor: [], admin: [] }
  const [notifications, setNotifications] = useState({ student: [], instructor: [], admin: [] });
  const connectionRef = useRef(null);

  // ── Re-run whenever the logged-in user changes (login / logout / switch) ──
  useEffect(() => {
    if (!user) {
      // User logged out — clear all notifications; cleanup fn stops SignalR
      setNotifications({ student: [], instructor: [], admin: [] });
      return;
    }

    const role = (user.role || "student").toLowerCase();

    // Fetch real notifications from the shared endpoint
    getNotifications()
      .then((data) => {
        if (Array.isArray(data)) {
          setNotifications((prev) => ({ ...prev, [role]: data.map(mapApiNotif) }));
        }
      })
      .catch(() => { /* not authenticated yet or server offline — stay with empty */ });

    // ── Connect SignalR hub ─────────────────────────────────────
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => localStorage.getItem("token") || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", (notif) => {
      const mapped = mapApiNotif(notif);
      setNotifications((prev) => ({
        ...prev,
        student:    role === "student"    ? [mapped, ...prev.student]    : prev.student,
        instructor: role === "instructor" ? [mapped, ...prev.instructor] : prev.instructor,
        admin:      role === "admin"      ? [mapped, ...prev.admin]      : prev.admin,
      }));
    });

    connection.start().catch(() => { /* SignalR failure is non-fatal */ });
    connectionRef.current = connection;

    // Cleanup: stop connection when user changes or component unmounts
    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [user?.id]); // Re-run only when the user identity changes

  // ── Public API ─────────────────────────────────────────────────
  const getNotifs = useCallback((role) => notifications[role] || [], [notifications]);

  const markRead = useCallback((role, id) => {
    setNotifications((prev) => ({
      ...prev,
      [role]: prev[role].map((n) => (n.id === id ? { ...n, read: true } : n)),
    }));
    markNotificationRead(id).catch(() => {});
  }, []);

  const markAllRead = useCallback((role) => {
    setNotifications((prev) => ({
      ...prev,
      [role]: prev[role].map((n) => ({ ...n, read: true })),
    }));
    markAllNotificationsRead().catch(() => {});
  }, []);

  const addNotification = useCallback((role, notif) => {
    setNotifications((prev) => ({
      ...prev,
      [role]: [{ ...notif, id: Date.now(), read: false, time: "Just now" }, ...(prev[role] || [])],
    }));
  }, []);

  return (
    <NotificationContext.Provider value={{ getNotifs, markRead, markAllRead, addNotification }}>
      {children}
    </NotificationContext.Provider>
  );
}

export function useNotifications() {
  const ctx = useContext(NotificationContext);
  if (!ctx) throw new Error("useNotifications must be inside NotificationProvider");
  return ctx;
}
