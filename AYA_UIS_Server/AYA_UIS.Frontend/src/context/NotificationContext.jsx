// src/context/NotificationContext.jsx
import { createContext, useContext, useState, useCallback } from "react";

const NotificationContext = createContext(null);

/* ─── Mock notifications per role ─── */
const MOCK = {
  student: [
    {
      id: 1, type: "grade_approved", read: false,
      title: "Assignment Graded ✅",
      body: "Your submission for 'Neural Network from Scratch' has been approved.",
      detail: {
        course: "Artificial Intelligence",
        assignment: "Neural Network from Scratch",
        grade: 18, max: 20,
        submittedAt: "Mar 14, 2025 · 11:45 PM",
        file: "ahmed_neural.pdf",
        note: "Great work! Clean implementation.",
      },
      time: "2 min ago",
    },
    {
      id: 2, type: "grade_rejected", read: false,
      title: "Assignment Returned ❌",
      body: "Your submission for 'Knowledge Base Design' was returned.",
      detail: {
        course: "Expert Systems",
        assignment: "Knowledge Base Design",
        grade: null, max: 20,
        submittedAt: "Mar 20, 2025 · 09:10 PM",
        file: "knowledge_v1.pdf",
        reason: "Suspected AI-generated content",
        note: "Please resubmit with original work.",
      },
      time: "1 hr ago",
    },
    {
      id: 3, type: "quiz_available", read: true,
      title: "New Quiz Available 📝",
      body: "Quiz 3 — ML Fundamentals is now open. Deadline: Mar 25.",
      detail: {
        course: "Artificial Intelligence",
        quiz: "Quiz 3 — ML Fundamentals",
        deadline: "Mar 25, 2025",
        duration: "20 min", questions: 10,
      },
      time: "3 hr ago",
    },
    {
      id: 4, type: "lecture_uploaded", read: true,
      title: "New Lecture Uploaded 🎬",
      body: "Dr. Farouk uploaded Week 6: Deep Learning & CNNs.",
      detail: {
        course: "Artificial Intelligence",
        lecture: "Week 6 — Deep Learning & CNNs",
        uploadedAt: "Mar 17, 2025",
        duration: "63 min", size: "380 MB",
      },
      time: "Yesterday",
    },
  ],
  instructor: [
    {
      id: 1, type: "submission_new", read: false,
      title: "New Submission 📬",
      body: "Ahmed Hassan submitted 'Neural Network from Scratch' — AI course.",
      detail: {
        student: "Ahmed Hassan", studentId: "20210001",
        course: "Artificial Intelligence",
        assignment: "Neural Network from Scratch",
        submittedAt: "Mar 14, 2025 · 11:45 PM",
        file: "ahmed_neural.pdf",
      },
      time: "5 min ago",
    },
    {
      id: 2, type: "submission_new", read: false,
      title: "New Submission 📬",
      body: "Sara Mohamed submitted 'Knowledge Base Design' — Expert Systems.",
      detail: {
        student: "Sara Mohamed", studentId: "20210002",
        course: "Expert Systems",
        assignment: "Knowledge Base Design",
        submittedAt: "Mar 20, 2025 · 09:10 PM",
        file: "sara_kb.pdf",
      },
      time: "1 hr ago",
    },
    {
      id: 3, type: "quiz_ended", read: true,
      title: "Quiz Ended 📊",
      body: "Quiz 2 has ended. 28 students attempted out of 35.",
      detail: {
        course: "Artificial Intelligence",
        quiz: "Quiz 2 — Knowledge Representation",
        attempts: 28, total: 35,
        avgScore: "7.4 / 10",
        endedAt: "Mar 10, 2025",
      },
      time: "2 days ago",
    },
  ],
  admin: [
    {
      id: 1, type: "user_registered", read: false,
      title: "New User Registered 👤",
      body: "Nour Ibrahim (Student) registered and awaits activation.",
      detail: {
        name: "Nour Ibrahim", email: "nour@uni.edu",
        role: "Student", registeredAt: "Mar 6, 2025 · 08:30 AM",
        status: "Pending Activation",
      },
      time: "10 min ago",
    },
    {
      id: 2, type: "system_alert", read: false,
      title: "System Alert ⚠️",
      body: "Storage usage reached 82%. Consider cleanup.",
      detail: {
        type: "Storage Warning",
        used: "82 GB", total: "100 GB",
        recommendation: "Archive old lecture files.",
        timestamp: "Mar 6, 2025",
      },
      time: "1 hr ago",
    },
    {
      id: 3, type: "user_registered", read: true,
      title: "Instructor Account Created 🎓",
      body: "Dr. Rania Hassan account activated successfully.",
      detail: {
        name: "Dr. Rania Hassan", email: "rania@uni.edu",
        role: "Instructor", registeredAt: "Mar 5, 2025",
        status: "Active",
      },
      time: "Yesterday",
    },
  ],
};

export function NotificationProvider({ children }) {
  const [notifications, setNotifications] = useState(MOCK);

  const getNotifs = useCallback((role) => notifications[role] || [], [notifications]);

  const markRead = useCallback((role, id) => {
    setNotifications(prev => ({
      ...prev,
      [role]: prev[role].map(n => n.id === id ? { ...n, read: true } : n),
    }));
  }, []);

  const markAllRead = useCallback((role) => {
    setNotifications(prev => ({
      ...prev,
      [role]: prev[role].map(n => ({ ...n, read: true })),
    }));
  }, []);

  const addNotification = useCallback((role, notif) => {
    setNotifications(prev => ({
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