// src/components/layout/Topbar.jsx
import { useState, useEffect, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { useNotifications } from "../../context/NotificationContext";
import styles from "./Topbar.module.css";

/* ── Greeting ── */
function greeting() {
  const h = new Date().getHours();
  if (h < 12) return "Good morning";
  if (h < 17) return "Good afternoon";
  return "Good evening";
}

/* ── Portal label per role ── */
const PORTAL_LABEL = {
  student:    "Student Portal",
  instructor: "Instructor Portal",
  admin:      "Admin Portal",
};

/* ── Notification type icon & color ── */
function notifMeta(type) {
  const map = {
    grade_approved:  { emoji: "✅", color: "#22c55e", bg: "rgba(34,197,94,0.1)"  },
    grade_rejected:  { emoji: "❌", color: "#ef4444", bg: "rgba(239,68,68,0.1)"  },
    quiz_available:  { emoji: "📝", color: "#6366f1", bg: "rgba(99,102,241,0.1)" },
    lecture_uploaded:{ emoji: "🎬", color: "#0ea5e9", bg: "rgba(14,165,233,0.1)" },
    submission_new:  { emoji: "📬", color: "#f59e0b", bg: "rgba(245,158,11,0.1)" },
    quiz_ended:      { emoji: "📊", color: "#8b5cf6", bg: "rgba(139,92,246,0.1)" },
    user_registered: { emoji: "👤", color: "#06b6d4", bg: "rgba(6,182,212,0.1)"  },
    system_alert:    { emoji: "⚠️", color: "#f97316", bg: "rgba(249,115,22,0.1)" },
  };
  return map[type] || { emoji: "🔔", color: "#6366f1", bg: "rgba(99,102,241,0.1)" };
}

/* ── Detail Panel content ── */
function DetailContent({ notif }) {
  const meta = notifMeta(notif.type);
  const d = notif.detail || {};

  return (
    <div className={styles.detailBody}>
      {/* Header */}
      <div className={styles.detailHeader} style={{ background: meta.bg }}>
        <span className={styles.detailEmoji}>{meta.emoji}</span>
        <div>
          <div className={styles.detailTitle}>{notif.title}</div>
          <div className={styles.detailTime}>{notif.time}</div>
        </div>
      </div>

      {/* Fields */}
      <div className={styles.detailFields}>
        {d.course     && <Field icon="📚" label="Course"     value={d.course} />}
        {d.assignment && <Field icon="📋" label="Assignment" value={d.assignment} />}
        {d.quiz       && <Field icon="📝" label="Quiz"       value={d.quiz} />}
        {d.lecture    && <Field icon="🎬" label="Lecture"    value={d.lecture} />}
        {d.student    && <Field icon="👤" label="Student"    value={`${d.student} · ID: ${d.studentId}`} />}
        {d.name       && <Field icon="👤" label="Name"       value={d.name} />}
        {d.email      && <Field icon="📧" label="Email"      value={d.email} />}
        {d.role       && <Field icon="🏷️" label="Role"       value={d.role} />}
        {d.submittedAt && <Field icon="⏰" label="Submitted"  value={d.submittedAt} />}
        {d.registeredAt && <Field icon="⏰" label="Date"     value={d.registeredAt} />}
        {d.uploadedAt && <Field icon="⏰" label="Uploaded"   value={d.uploadedAt} />}
        {d.deadline   && <Field icon="⏰" label="Deadline"   value={d.deadline} />}
        {d.duration   && <Field icon="⏱" label="Duration"   value={d.duration} />}
        {d.questions  && <Field icon="🔢" label="Questions"  value={`${d.questions} questions`} />}
        {d.file       && <Field icon="📄" label="File"       value={d.file} isFile />}
        {d.status     && <Field icon="🟢" label="Status"     value={d.status} />}
        {d.used       && <Field icon="💾" label="Storage"    value={`${d.used} / ${d.total}`} />}
        {d.attempts   && <Field icon="👥" label="Attempts"   value={`${d.attempts} / ${d.total} students`} />}
        {d.avgScore   && <Field icon="📊" label="Avg Score"  value={d.avgScore} />}

        {/* Grade result */}
        {d.grade !== undefined && d.grade !== null && (
          <div className={styles.gradeResult} style={{ borderColor: `${meta.color}30`, background: meta.bg }}>
            <span className={styles.gradeResultNum} style={{ color: meta.color }}>{d.grade}</span>
            <span className={styles.gradeResultMax}>/ {d.max}</span>
            <span className={styles.gradeResultLabel}>Grade</span>
          </div>
        )}

        {/* Rejection reason */}
        {d.reason && (
          <div className={styles.rejectReason}>
            <span className={styles.rejectLabel}>Rejection Reason</span>
            <span className={styles.rejectText}>{d.reason}</span>
          </div>
        )}

        {/* Note */}
        {d.note && (
          <div className={styles.detailNote}>
            <span>💬</span> {d.note}
          </div>
        )}
        {d.recommendation && (
          <div className={styles.detailNote}>
            <span>💡</span> {d.recommendation}
          </div>
        )}
      </div>
    </div>
  );
}

function Field({ icon, label, value, isFile }) {
  return (
    <div className={styles.field}>
      <span className={styles.fieldIcon}>{icon}</span>
      <span className={styles.fieldLabel}>{label}</span>
      <span className={`${styles.fieldValue} ${isFile ? styles.fieldFile : ""}`}>{value}</span>
    </div>
  );
}

/* ── Main Topbar ── */
export default function Topbar() {
  const { user } = useAuth();
  const { getNotifs, markRead, markAllRead } = useNotifications();
  const role   = user?.role || "student";
  const notifs = getNotifs(role);
  const unread = notifs.filter(n => !n.read).length;

  const [panelOpen,   setPanelOpen]   = useState(false);
  const navigate = useNavigate();
  const [detailNotif, setDetailNotif] = useState(null);

  const panelRef = useRef(null);
  const letter   = user?.name?.charAt(0)?.toUpperCase() || "?";
  const displayName = (() => {
    const parts = (user?.name || "").trim().split(/\s+/);
    if (parts.length <= 2) return parts.join(" ");
    return `${parts[0]} ${parts[parts.length - 1]}`;
  })();

  /* Close panel/detail on outside click */
  useEffect(() => {
    if (!panelOpen && !detailNotif) return;
    const handler = (e) => {
      if (panelRef.current && !panelRef.current.contains(e.target)) {
        setPanelOpen(false);
        setDetailNotif(null);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, [panelOpen, detailNotif]);

  const openDetail = (notif) => {
    markRead(role, notif.id);
    setDetailNotif(notif);
    setPanelOpen(false);
  };

  const ACCENT = {
    student:    "#8b7cf8",
    instructor: "#5ba4cf",
    admin:      "#e07b6a",
  }[role] || "#8b7cf8";

  return (
    <>
      <header className={styles.topbar}>
        {/* Left */}
        <div className={styles.topLeft}>
          <div className={styles.portalLabel}>{PORTAL_LABEL[role] || "Portal"}</div>
          <div className={styles.greetingRow}>
            <span className={styles.greetingText}>{greeting()},</span>
            <span className={styles.greetingName} style={{ color: ACCENT }}>
              {role === "instructor" ? `Dr. ${displayName.split(" ")[0]}` : displayName}
            </span>
            <span className={styles.greetingEmoji}>
              {{ student: "🎓", instructor: "👨‍🏫", admin: "⚙️" }[role] || "👋"}
            </span>
          </div>
        </div>

        {/* Right */}
        <div className={styles.topRight} ref={panelRef}>
          {/* Bell */}
          <motion.button
            className={styles.bellBtn}
            onClick={() => { setPanelOpen(v => !v); setDetailNotif(null); }}
            whileHover={{ scale: 1.08 }}
            whileTap={{ scale: 0.92 }}
          >
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" width="20" height="20">
              <path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/>
            </svg>
            <AnimatePresence>
              {unread > 0 && (
                <motion.span
                  className={styles.badge}
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                  exit={{ scale: 0 }}
                  transition={{ type: "spring", stiffness: 500, damping: 22 }}
                >
                  {unread}
                </motion.span>
              )}
            </AnimatePresence>
          </motion.button>

          {/* Avatar — click → Profile */}
          <motion.button
            className={styles.avatar}
            title="View Profile"
            style={{ background: user?.avatar ? "transparent" : `linear-gradient(135deg, ${ACCENT}cc, ${ACCENT}88)`, border: "none", cursor: "pointer", padding: 0, overflow: "hidden" }}
            whileHover={{ scale: 1.08 }}
            whileTap={{ scale: 0.94 }}
            onClick={() => navigate(`/${role}/profile`)}
          >
            {user?.avatar
              ? <img src={user.avatar} alt="avatar" style={{ width: "100%", height: "100%", objectFit: "cover", borderRadius: "50%" }} />
              : letter
            }
          </motion.button>

          {/* ── Notification List Panel ── */}
          <AnimatePresence>
            {panelOpen && (
              <motion.div
                className={styles.notifPanel}
                initial={{ opacity: 0, x: 24, scale: 0.97 }}
                animate={{ opacity: 1, x: 0, scale: 1 }}
                exit={{ opacity: 0, x: 24, scale: 0.97 }}
                transition={{ type: "spring", stiffness: 380, damping: 30 }}
              >
                {/* Panel header */}
                <div className={styles.panelHead}>
                  <span className={styles.panelTitle}>Notifications</span>
                  {unread > 0 && (
                    <button className={styles.markAllBtn} onClick={() => markAllRead(role)}>
                      Mark all read
                    </button>
                  )}
                </div>

                {/* List */}
                <div className={styles.notifList}>
                  {notifs.length === 0 ? (
                    <div className={styles.emptyNotif}>No notifications yet</div>
                  ) : notifs.map((n, i) => {
                    const meta = notifMeta(n.type);
                    return (
                      <motion.div
                        key={n.id}
                        className={`${styles.notifItem} ${!n.read ? styles.notifUnread : ""}`}
                        initial={{ opacity: 0, x: 10 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: i * 0.04 }}
                        onClick={() => openDetail(n)}
                        whileHover={{ x: -2 }}
                      >
                        <span
                          className={styles.notifDot}
                          style={{ background: meta.bg, fontSize: 18 }}
                        >
                          {meta.emoji}
                        </span>
                        <div className={styles.notifText}>
                          <div className={styles.notifItemTitle}>{n.title}</div>
                          <div className={styles.notifBody}>{n.body}</div>
                          <div className={styles.notifTime}>{n.time}</div>
                        </div>
                        {!n.read && <span className={styles.unreadDot} style={{ background: ACCENT }} />}
                      </motion.div>
                    );
                  })}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </header>

      {/* ── Detail Panel — slides from right ── */}
      <AnimatePresence>
        {detailNotif && (
          <>
            {/* Backdrop */}
            <motion.div
              className={styles.detailBackdrop}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              onClick={() => setDetailNotif(null)}
            />

            {/* Panel */}
            <motion.div
              className={styles.detailPanel}
              initial={{ x: "100%", opacity: 0 }}
              animate={{ x: 0, opacity: 1 }}
              exit={{ x: "100%", opacity: 0 }}
              transition={{ type: "spring", stiffness: 340, damping: 32 }}
            >
              {/* Close */}
              <button className={styles.detailClose} onClick={() => setDetailNotif(null)}>
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" width="18" height="18">
                  <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>

              <DetailContent notif={detailNotif} />

              <button
                className={styles.detailDoneBtn}
                style={{ background: notifMeta(detailNotif.type).color }}
                onClick={() => setDetailNotif(null)}
              >
                Got it ✓
              </button>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </>
  );
}