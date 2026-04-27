import { useState, useEffect } from "react";
import { NavLink, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";
import { useNotifications } from "../../context/NotificationContext";
import styles from "./Sidebar.module.css";

/* ── Icons ─────────────────────────────────────── */
const IC = {
  home:        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>,
  courses:     <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M4 19.5A2.5 2.5 0 016.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z"/></svg>,
  grades:      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>,
  timetable:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>,
  quizzes:     <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11"/></svg>,
  assignments: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>,
  ai:          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M12 2a2 2 0 012 2v2a2 2 0 01-2 2 2 2 0 01-2-2V4a2 2 0 012-2z"/><path d="M12 16a2 2 0 012 2v2a2 2 0 01-2 2 2 2 0 01-2-2v-2a2 2 0 012-2z"/><path d="M2 12a2 2 0 012-2h2a2 2 0 012 2 2 2 0 01-2 2H4a2 2 0 01-2-2z"/><path d="M16 12a2 2 0 012-2h2a2 2 0 012 2 2 2 0 01-2 2h-2a2 2 0 01-2-2z"/><circle cx="12" cy="12" r="2"/></svg>,
  register:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/></svg>,
  quiz_build:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 015.83 1c0 2-3 3-3 3"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>,
  upload:      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="16 16 12 12 8 16"/><line x1="12" y1="12" x2="12" y2="21"/><path d="M20.39 18.39A5 5 0 0018 9h-1.26A8 8 0 103 16.3"/></svg>,
  users:       <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"/></svg>,
  logout:      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.9" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>,
  schedule:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/><line x1="8" y1="14" x2="8" y2="14"/><line x1="12" y1="14" x2="12" y2="14"/><line x1="16" y1="14" x2="16" y2="14"/><line x1="8" y1="18" x2="8" y2="18"/><line x1="12" y1="18" x2="12" y2="18"/></svg>,
  themes:      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><circle cx="12" cy="12" r="3"/><line x1="12" y1="2" x2="12" y2="5"/><line x1="12" y1="19" x2="12" y2="22"/><line x1="2" y1="12" x2="5" y2="12"/><line x1="19" y1="12" x2="22" y2="12"/><line x1="4.93" y1="4.93" x2="7.05" y2="7.05"/><line x1="16.95" y1="16.95" x2="19.07" y2="19.07"/><line x1="19.07" y1="4.93" x2="16.95" y2="7.05"/><line x1="7.05" y1="16.95" x2="4.93" y2="19.07"/></svg>,
  instructor_ctrl: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M20 21v-2a4 4 0 00-4-4h-4"/><circle cx="9" cy="7" r="4"/><line x1="15" y1="11" x2="21" y2="11"/><line x1="18" y1="8" x2="18" y2="14"/></svg>,
  final_grade:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg>,
  reset_year:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"/></svg>,
  reset_material:<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>,
  settings:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 01-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z"/></svg>,
  chevL:       <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round"><polyline points="15 18 9 12 15 6"/></svg>,
};

/* ── Nav data
   ✅ FIXED: Manage Users has its own unique path
────────────────────────────────────────────────── */
const NAV = {
  student: [
    { label: "Home",                path: "/student/dashboard",        icon: IC.home,        badge: null  },
    { label: "My Courses",          path: "/student/courses",          icon: IC.courses,     badge: null  },
    { label: "Grades",              path: "/student/grades",           icon: IC.grades,      badge: null  },
    { label: "Timetable",           path: "/student/timetable",        icon: IC.timetable,   badge: null  },
    { label: "Schedule",            path: "/student/schedule",         icon: IC.schedule,    badge: null  },
    { label: "Quizzes",             path: "/student/quizzes",          icon: IC.quizzes,     badge: null  },
    { label: "AI Tools",            path: "/student/ai-tools",         icon: IC.ai,          badge: "NEW" },
    { label: "Course Registration", path: "/student/register-courses", icon: IC.register,    badge: null  },
    { label: "Themes",              path: "/student/themes",           icon: IC.themes,      badge: null  },
    { label: "Settings",            path: "/student/settings",         icon: IC.settings,    badge: null  },
  ],
  instructor: [
    { label: "Home",            path: "/instructor/dashboard",    icon: IC.home,        badge: null },
    { label: "Schedule",        path: "/instructor/schedule",     icon: IC.schedule,    badge: null },
    { label: "Quiz Builder",    path: "/instructor/quiz-builder", icon: IC.quiz_build,  badge: null },
    { label: "Upload Material", path: "/instructor/material",     icon: IC.upload,      badge: null },
    { label: "Grades",          path: "/instructor/grades",       icon: IC.grades,      badge: null },
    { label: "Themes",          path: "/instructor/themes",       icon: IC.themes,      badge: null },
    { label: "Settings",        path: "/instructor/settings",     icon: IC.settings,    badge: null },
  ],
  admin: [
    { label: "Home",          path: "/admin/dashboard",    icon: IC.home,     badge: null },
    { label: "Manage Users",    path: "/admin/manage-users",   icon: IC.users,    badge: null },
    { label: "Registration",      path: "/admin/registration",       icon: IC.register,        badge: null },
    { label: "Instructor Control",path: "/admin/instructor-control", icon: IC.instructor_ctrl, badge: null },
    { label: "Final Grade",       path: "/admin/final-grade",        icon: IC.final_grade,     badge: null },
    { label: "Academic Year Reset", path: "/admin/academic-year-reset", icon: IC.reset_year,    badge: null },
    { label: "Reset Material",      path: "/admin/reset-material",     icon: IC.reset_material, badge: null },
    { label: "Email Manager",     path: "/admin/email-manager",      icon: IC.courses,         badge: null },
    { label: "Schedule",          path: "/admin/schedule",           icon: IC.schedule,        badge: null },
    { label: "Themes",            path: "/admin/themes",             icon: IC.themes,          badge: null },
  ],
};

const ROLES = {
  student:    { label: "Student",    accent: "#8b7cf8" },
  instructor: { label: "Instructor", accent: "#5ba4cf" },
  admin:      { label: "Admin",      accent: "#e07b6a" },
};

/* ── Nav Item ────────────────────────────────────── */
function NavItem({ item, collapsed, index, accent }) {
  const [tip, setTip] = useState(false);
  const location = useLocation();

  // ✅ FIXED: dashboard roots never match child routes
  const EXACT_PATHS = ["/admin/dashboard", "/student/dashboard", "/instructor/dashboard"];
  const isActive = EXACT_PATHS.includes(item.path)
    ? location.pathname === item.path
    : location.pathname === item.path || location.pathname.startsWith(item.path + "/");

  return (
    <motion.div
      style={{ position: "relative" }}
      initial={{ opacity: 0, x: -20 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: 0.04 + index * 0.04, duration: 0.38, ease: [0.22, 1, 0.36, 1] }}
      onMouseEnter={() => collapsed && setTip(true)}
      onMouseLeave={() => setTip(false)}
    >
      <NavLink
        to={item.path}
        end
        className={`${styles.navItem} ${isActive ? styles.navActive : ""}`}
        style={{ "--accent": accent }}
      >
        {/* Sliding active background — ORIGINAL */}
        {isActive && (
          <motion.div
            className={styles.activeBg}
            layoutId="activeBg"
            transition={{ type: "spring", stiffness: 380, damping: 32 }}
          />
        )}

        {/* Icon */}
        <motion.div
          className={`${styles.iconWrap} ${isActive ? styles.iconActive : ""}`}
          whileHover={{ scale: 1.1 }}
          transition={{ duration: 0.2 }}
        >
          {item.icon}
        </motion.div>

        {/* Label */}
        <AnimatePresence>
          {!collapsed && (
            <motion.span
              className={styles.navLabel}
              initial={{ opacity: 0, x: -10, width: 0 }}
              animate={{ opacity: 1, x: 0, width: "auto" }}
              exit={{ opacity: 0, x: -8, width: 0 }}
              transition={{ duration: 0.2 }}
            >
              {item.label}
            </motion.span>
          )}
        </AnimatePresence>

        {/* Badge */}
        <AnimatePresence>
          {!collapsed && item.badge && (
            <motion.span
              className={`${styles.badge} ${item.badge === "NEW" ? styles.badgeNew : styles.badgeNum}`}
              initial={{ scale: 0, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0, opacity: 0 }}
              transition={{ type: "spring", stiffness: 480, damping: 22 }}
            >
              {item.badge}
            </motion.span>
          )}
        </AnimatePresence>

        {/* Active left indicator */}
        {isActive && (
          <motion.div
            className={styles.activeBar}
            layoutId="activeBar"
            transition={{ type: "spring", stiffness: 380, damping: 32 }}
          />
        )}
      </NavLink>

      {/* Tooltip when collapsed */}
      <AnimatePresence>
        {collapsed && tip && (
          <motion.div
            className={styles.tooltip}
            initial={{ opacity: 0, x: -8, scale: 0.92 }}
            animate={{ opacity: 1, x: 0, scale: 1 }}
            exit={{ opacity: 0, x: -5, scale: 0.92 }}
            transition={{ duration: 0.14 }}
          >
            {item.label}
            {item.badge && <span className={styles.tipBadge}>{item.badge}</span>}
            <div className={styles.tipArrow} />
          </motion.div>
        )}
      </AnimatePresence>
    </motion.div>
  );
}

/* ── Sidebar ─────────────────────────────────────── */
export default function Sidebar({ collapsed, onToggle }) {
  const { user, logout } = useAuth();
  const { getNotifs }    = useNotifications();
  const navigate = useNavigate();
  const [showLogout, setShowLogout] = useState(false);
  const [time, setTime] = useState(new Date());

  /* ── Dynamic badges from NotificationContext ── */
  const notifs = getNotifs(user?.role || "student");
  const unread = notifs.filter(n => !n.read);

  const dynamicBadges = {
    student: {
      "/student/quizzes":
        unread.filter(n => n.type === "quiz_available" || n.type === "quiz_published").length || null,
      "/student/courses":
        unread.filter(n => n.type === "lecture_uploaded" || n.type === "assignment_published").length || null,
      "/student/ai-tools": "NEW",
    },
    instructor: {
      "/instructor/assignments":
        unread.filter(n => n.type === "submission_new" || n.type === "submission_updated").length || null,
      "/instructor/grades":
        unread.filter(n => n.type === "quiz_ended").length || null,
    },
    admin: {
      "/admin/manage-users":
        unread.filter(n => n.type === "user_registered" || n.type === "gpa_low" || n.type === "student_failed").length || null,
      "/admin/register":
        unread.filter(n => n.type === "system_alert").length || null,
    },
  };

  const roleBadges = dynamicBadges[user?.role] || {};

  /* ── Inject dynamic badges into NAV items ── */
  const rawItems = NAV[user?.role] || [];
  const items = rawItems.map(item => ({
    ...item,
    badge: roleBadges[item.path] !== undefined ? roleBadges[item.path] : null,
  }));

  const role   = ROLES[user?.role] || ROLES.student;
  const letter = user?.name?.charAt(0)?.toUpperCase() || "?";

  // First + Last name only
  const displayName = (() => {
    const parts = (user?.name || "").trim().split(/\s+/);
    if (parts.length <= 2) return parts.join(" ");
    return `${parts[0]} ${parts[parts.length - 1]}`;
  })();

  useEffect(() => {
    const t = setInterval(() => setTime(new Date()), 30000);
    return () => clearInterval(t);
  }, []);

  // Small clock — HH:MM only, 24h
  const hhmm = time.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", hour12: false });

  return (
    <motion.aside
      className={styles.sidebar}
      style={{ "--accent": role.accent }}
      animate={{ width: collapsed ? 70 : 252 }}
      transition={{ type: "spring", stiffness: 280, damping: 30 }}
    >
      {/* Subtle top glow — same as original */}
      <div
        className={styles.topGlow}
        style={{ background: `radial-gradient(ellipse 100% 40% at 50% 0%, ${role.accent}18, transparent)` }}
      />

      {/* ══ LOGO ══ */}
      <div className={styles.logoRow}>
        <motion.div
          className={styles.logoMark}
          whileHover={{ rotate: 180, scale: 1.05 }}
          transition={{ duration: 0.55, ease: "easeInOut" }}
        >
          <svg viewBox="0 0 36 36" fill="none">
            <rect x="1" y="1" width="34" height="34" rx="10" fill="rgba(255,255,255,0.04)" stroke="rgba(255,255,255,0.1)" strokeWidth="1"/>
            <polygon points="18,8 28,14 28,22 18,28 8,22 8,14" fill="white" opacity="0.9"/>
            <circle cx="18" cy="18" r="3.5" fill={role.accent}/>
          </svg>
        </motion.div>

        <AnimatePresence>
          {!collapsed && (
            <motion.div
              className={styles.logoText}
              initial={{ opacity: 0, x: -14 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -12 }}
              transition={{ duration: 0.22 }}
            >
              <span className={styles.logoMain}>AKHBAR ELYOM</span>
              <span className={styles.logoSub}>Academy</span>
            </motion.div>
          )}
        </AnimatePresence>

        <motion.button
          className={styles.toggleBtn}
          onClick={onToggle}
          whileHover={{ scale: 1.1 }}
          whileTap={{ scale: 0.88 }}
        >
          <motion.span
            style={{ display: "flex" }}
            animate={{ rotate: collapsed ? 180 : 0 }}
            transition={{ type: "spring", stiffness: 280, damping: 24 }}
          >
            {IC.chevL}
          </motion.span>
        </motion.button>
      </div>

      {/* ══ USER CARD ══ */}
      <div className={styles.userSection}>
        <AnimatePresence mode="wait">
          {!collapsed ? (
            <motion.div
              key="full"
              className={styles.userCard}
              initial={{ opacity: 0, y: -8 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -6 }}
              transition={{ duration: 0.24 }}
            >
              <div className={styles.userCardInner}>
                {/* Avatar */}
                <motion.div
                  className={styles.avatar}
                  style={{ background: `linear-gradient(135deg, ${role.accent}cc, ${role.accent}66)` }}
                  whileHover={{ scale: 1.06 }}
                >
                  {letter}
                  <span className={styles.onlineDot} />
                </motion.div>

                {/* Info */}
                <div className={styles.userInfo}>
                  <span className={styles.userName}>{displayName}</span>
                  <span className={styles.userRole} style={{ color: role.accent }}>
                    {role.label}
                  </span>
                </div>

                {/* ✅ Small styled clock — replaces plain timeTag */}
                <motion.div
                  className={styles.clockPill}
                  style={{ borderColor: `${role.accent}30` }}
                  animate={{ opacity: [0.7, 1, 0.7] }}
                  transition={{ duration: 4, repeat: Infinity, ease: "easeInOut" }}
                >
                  <motion.span
                    className={styles.clockDot}
                    style={{ background: role.accent }}
                    animate={{ opacity: [0.4, 1, 0.4] }}
                    transition={{ duration: 1.5, repeat: Infinity }}
                  />
                  <span className={styles.clockTime} style={{ color: role.accent }}>
                    {hhmm}
                  </span>
                </motion.div>
              </div>
            </motion.div>
          ) : (
            <motion.div
              key="mini"
              className={styles.userMini}
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.8 }}
              transition={{ duration: 0.2 }}
            >
              <motion.div
                className={styles.avatarMini}
                style={{ background: `linear-gradient(135deg, ${role.accent}cc, ${role.accent}66)` }}
                whileHover={{ scale: 1.08 }}
              >
                {letter}
                <span className={styles.onlineDot} />
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* ══ SEPARATOR ══ */}
      <div className={styles.sep}>
        <motion.div
          className={styles.sepLine}
          initial={{ scaleX: 0 }} animate={{ scaleX: 1 }}
          transition={{ delay: 0.3, duration: 0.5 }}
        />
      </div>

      {/* Section label */}
      <AnimatePresence>
        {!collapsed && (
          <motion.span
            className={styles.sectionLabel}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          >
            MENU
          </motion.span>
        )}
      </AnimatePresence>

      {/* ══ NAV ══ */}
      <nav className={styles.nav}>
        {items.map((item, i) => (
          <NavItem
            key={item.path}
            item={item}
            collapsed={collapsed}
            index={i}
            accent={role.accent}
          />
        ))}
      </nav>

      {/* ══ BOTTOM ══ */}
      <div className={styles.bottom}>
        <div className={styles.sep}>
          <motion.div
            className={styles.sepLine}
            initial={{ scaleX: 0 }} animate={{ scaleX: 1 }}
            transition={{ delay: 0.45, duration: 0.5 }}
          />
        </div>

        {/* Logout confirm */}
        <AnimatePresence>
          {showLogout && (
            <motion.div
              className={styles.logoutBox}
              initial={{ opacity: 0, y: 10, scale: 0.94 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 8, scale: 0.94 }}
              transition={{ type: "spring", stiffness: 380, damping: 28 }}
            >
              {!collapsed && <p className={styles.logoutQuestion}>Sign out of your account?</p>}
              <div className={styles.logoutBtns}>
                <motion.button
                  className={styles.btnConfirm}
                  onClick={() => { logout(); navigate("/login"); }}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.96 }}
                >
                  <span className={styles.btnIcon}>{IC.logout}</span>
                  {!collapsed && "Sign Out"}
                </motion.button>
                <motion.button
                  className={styles.btnCancel}
                  onClick={() => setShowLogout(false)}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.96 }}
                >
                  {collapsed ? "✕" : "Cancel"}
                </motion.button>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Logout trigger */}
        <motion.button
          className={`${styles.logoutBtn} ${showLogout ? styles.logoutOpen : ""}`}
          onClick={() => setShowLogout(v => !v)}
          whileHover="hov"
          whileTap={{ scale: 0.96 }}
        >
          <motion.div
            className={styles.logoutHoverBg}
            variants={{ hov: { opacity: 1 } }}
            initial={{ opacity: 0 }}
            transition={{ duration: 0.18 }}
          />
          <motion.span
            className={styles.logoutIconWrap}
            variants={{ hov: { x: 2 } }}
            transition={{ duration: 0.2 }}
          >
            {IC.logout}
          </motion.span>
          <AnimatePresence>
            {!collapsed && (
              <motion.span
                className={styles.logoutLabel}
                initial={{ opacity: 0, x: -10, width: 0 }}
                animate={{ opacity: 1, x: 0, width: "auto" }}
                exit={{ opacity: 0, x: -8, width: 0 }}
                transition={{ duration: 0.18 }}
              >
                Sign Out
              </motion.span>
            )}
          </AnimatePresence>
          <motion.span
            className={styles.logoutDot}
            animate={{ opacity: [0.6, 1, 0.6] }}
            transition={{ duration: 2.5, repeat: Infinity }}
          />
        </motion.button>
      </div>
    </motion.aside>
  );
}