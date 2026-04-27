// src/pages/admin/AdminDashboard.jsx
// Redesigned admin dashboard — student-dashboard inspired layout
import { useEffect, useMemo, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { useRegistration } from "../../context/RegistrationContext";
import { getAdminStats, getEmails } from "../../services/api/adminApi";
import styles from "./AdminDashboard.module.css";

const ease = [0.22, 1, 0.36, 1];

function Counter({ to = 0, duration = 1.15 }) {
  const [val, setVal] = useState(0);

  useEffect(() => {
    const target = Number(to) || 0;
    if (target <= 0) {
      setVal(0);
      return undefined;
    }

    let frame;
    const startedAt = performance.now();

    const tick = (now) => {
      const progress = Math.min((now - startedAt) / (duration * 1000), 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      setVal(Math.round(target * eased));
      if (progress < 1) frame = requestAnimationFrame(tick);
    };

    frame = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frame);
  }, [to, duration]);

  return <>{val.toLocaleString()}</>;
}

function Skeleton({ height = 18, width = "100%", radius = 12 }) {
  return <span className={styles.skeleton} style={{ height, width, borderRadius: radius }} />;
}

function RingMeter({ value = 0, color = "#22c55e", label = "Ready" }) {
  const safeValue = Math.max(0, Math.min(100, Number(value) || 0));
  const radius = 46;
  const circumference = 2 * Math.PI * radius;

  return (
    <div className={styles.ringWrap}>
      <svg className={styles.ring} viewBox="0 0 112 112" aria-hidden="true">
        <circle className={styles.ringTrack} cx="56" cy="56" r={radius} />
        <motion.circle
          className={styles.ringValue}
          cx="56"
          cy="56"
          r={radius}
          stroke={color}
          strokeDasharray={circumference}
          initial={{ strokeDashoffset: circumference }}
          animate={{ strokeDashoffset: circumference - (safeValue / 100) * circumference }}
          transition={{ duration: 1.15, ease, delay: 0.25 }}
        />
      </svg>
      <div className={styles.ringCenter}>
        <strong>{safeValue}%</strong>
        <span>{label}</span>
      </div>
    </div>
  );
}

const CORE_ACTIONS = [
  {
    label: "Manage Users",
    desc: "Open student, instructor, and admin profiles and edit academic data",
    path: "/admin/manage-users",
    icon: "👥",
    color: "#8b5cf6",
    tag: "Accounts",
  },
  {
    label: "Email Manager",
    desc: "Create academic emails and show the new student code inline",
    path: "/admin/email-manager",
    icon: "✉️",
    color: "#06b6d4",
    tag: "Emails",
  },
  {
    label: "Registration Control",
    desc: "Open, close, and monitor student course registration",
    path: "/admin/registration",
    icon: "📋",
    color: "#f59e0b",
    tag: "Courses",
  },
  {
    label: "Instructor Control",
    desc: "Assign instructors and course responsibilities",
    path: "/admin/instructor-control",
    icon: "🎓",
    color: "#22c55e",
    tag: "Faculty",
  },
  {
    label: "Final Grade Audit",
    desc: "Classify students, review grades, and publish all results",
    path: "/admin/final-grades",
    icon: "🏁",
    color: "#a855f7",
    tag: "Grades",
    featured: true,
  },
  {
    label: "Reset System",
    desc: "Run the academic year reset wizard safely after review",
    path: "/admin/academic-year-reset",
    icon: "🔁",
    color: "#14b8a6",
    tag: "Academic",
    featured: true,
  },
  {
    label: "Schedule Manager",
    desc: "Build lecture, exam, and academic schedules",
    path: "/admin/schedule",
    icon: "🗓️",
    color: "#3b82f6",
    tag: "Planning",
  },
  {
    label: "Theme Studio",
    desc: "Customize platform appearance and themes",
    path: "/admin/themes",
    icon: "🎨",
    color: "#ec4899",
    tag: "Design",
  },
];

const CREDIT_RULES = [
  {
    title: "New Student",
    range: "First Year · Semester One",
    hours: 21,
    note: "No GPA warnings yet",
    color: "#06b6d4",
  },
  {
    title: "Strong Standing",
    range: "GPA ≥ 3.0",
    hours: 21,
    note: "Full registration load",
    color: "#22c55e",
  },
  {
    title: "Good Standing",
    range: "2.0 ≤ GPA < 3.0",
    hours: 18,
    note: "Standard controlled load",
    color: "#8b5cf6",
  },
  {
    title: "Warning",
    range: "1.0 ≤ GPA < 2.0",
    hours: 12,
    note: "Reduced course load",
    color: "#f59e0b",
  },
  {
    title: "Probation",
    range: "GPA < 1.0",
    hours: 9,
    note: "Retake-first policy",
    color: "#ef4444",
  },
];

const fadeUp = {
  hidden: { opacity: 0, y: 18 },
  show: { opacity: 1, y: 0, transition: { duration: 0.45, ease } },
};

const stagger = {
  hidden: {},
  show: { transition: { staggerChildren: 0.055 } },
};

function formatDate(date) {
  return date.toLocaleDateString("en-US", {
    weekday: "long",
    month: "long",
    day: "numeric",
    year: "numeric",
  });
}

export default function AdminDashboard() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { regWindow } = useRegistration();

  const [now, setNow] = useState(new Date());
  const [stats, setStats] = useState(null);
  const [emailCounts, setEmailCounts] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    let active = true;

    async function loadDashboard() {
      try {
        setLoading(true);
        const [statsData, emailsData] = await Promise.all([getAdminStats(), getEmails()]);
        if (!active) return;
        setStats(statsData ?? null);
        setEmailCounts(emailsData?.counts ?? null);
        setError(null);
      } catch (err) {
        if (!active) return;
        setError(err?.message || "Failed to load dashboard data");
      } finally {
        if (active) setLoading(false);
      }
    }

    loadDashboard();
    return () => {
      active = false;
    };
  }, []);

  const firstName = useMemo(() => {
    const display = user?.name || user?.displayName || user?.email || "Admin";
    return String(display).split(" ")[0];
  }, [user]);

  const greeting = useMemo(() => {
    const hour = now.getHours();
    if (hour < 12) return "Good Morning";
    if (hour < 17) return "Good Afternoon";
    return "Good Evening";
  }, [now]);

  const daysLeft = regWindow?.deadline
    ? Math.max(0, Math.ceil((new Date(regWindow.deadline).getTime() - now.getTime()) / 86400000))
    : null;

  const registrationRate = Math.max(0, Math.min(100, Number(stats?.registrationRate) || 0));
  const featuredActions = CORE_ACTIONS.filter((item) => item.featured);
  const regularActions = CORE_ACTIONS;

  const statCards = useMemo(
    () => [
      {
        label: "Students",
        value: stats?.totalStudents ?? 0,
        sub: "Active academic accounts",
        icon: "👥",
        color: "#8b5cf6",
      },
      {
        label: "Registered",
        value: stats?.totalRegistered ?? 0,
        sub: "Current course registrations",
        icon: "✅",
        color: "#22c55e",
      },
      {
        label: "Instructors",
        value: stats?.totalInstructors ?? 0,
        sub: "Faculty members",
        icon: "🎓",
        color: "#3b82f6",
      },
      {
        label: "Active Courses",
        value: stats?.activeCourses ?? 0,
        sub: "Courses in curriculum",
        icon: "📚",
        color: "#f59e0b",
      },
    ],
    [stats]
  );

  const accountDistribution = useMemo(() => {
    if (!emailCounts) return [];
    const total = Number(emailCounts.total) || 0;
    return [
      { label: "Students", value: emailCounts.student ?? 0, color: "#8b5cf6" },
      { label: "Instructors", value: emailCounts.instructor ?? 0, color: "#22c55e" },
      { label: "Admins", value: emailCounts.admin ?? 0, color: "#f59e0b" },
      { label: "Suspended", value: emailCounts.suspended ?? 0, color: "#ef4444" },
    ].map((item) => ({
      ...item,
      pct: total > 0 ? Math.round((Number(item.value || 0) / total) * 100) : 0,
    }));
  }, [emailCounts]);

  const openPage = (path) => navigate(path);

  return (
    <main className={styles.page}>
      <AnimatePresence>
        {error && (
          <motion.div
            className={styles.errorBanner}
            initial={{ opacity: 0, y: -14 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
          >
            <span>⚠️ {error}</span>
            <button type="button" onClick={() => setError(null)} aria-label="Dismiss dashboard error">
              ✕
            </button>
          </motion.div>
        )}
      </AnimatePresence>

      <motion.section
        className={styles.hero}
        initial={{ opacity: 0, y: -18 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease }}
      >
        <div className={styles.heroCopy}>
          <div className={styles.heroBadge}>
            <span />
            Admin Workspace
          </div>
          <h1>
            {greeting}, <em>{firstName}</em>
          </h1>
          <p>
            Monitor registration, grade publishing, academic reset workflows, and account operations from one clean control center.
          </p>
          <div className={styles.heroMeta}>
            <strong>{formatDate(now)}</strong>
            <span />
            <strong>{now.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit", second: "2-digit" })}</strong>
          </div>
        </div>

        <motion.div
          className={`${styles.registrationCard} ${regWindow?.isOpen ? styles.registrationOpen : styles.registrationClosed}`}
          whileHover={{ y: -4 }}
        >
          <div className={styles.registrationIcon}>{regWindow?.isOpen ? "🟢" : "🔴"}</div>
          <div>
            <span>Course Registration</span>
            <strong>{regWindow?.isOpen ? "Open Now" : "Closed"}</strong>
            <p>
              {regWindow?.isOpen && daysLeft !== null
                ? daysLeft === 0
                  ? "Closes today"
                  : `${daysLeft} day${daysLeft === 1 ? "" : "s"} left${regWindow?.semester ? ` · ${regWindow.semester}` : ""}`
                : "Students cannot register courses right now"}
            </p>
          </div>
          <button type="button" onClick={() => openPage("/admin/registration")}>
            Control
          </button>
        </motion.div>
      </motion.section>

      <motion.section className={styles.statsGrid} variants={stagger} initial="hidden" animate="show">
        {statCards.map((card) => (
          <motion.article
            key={card.label}
            className={styles.statCard}
            variants={fadeUp}
            whileHover={{ y: -6 }}
            style={{ "--card-accent": card.color }}
          >
            <div className={styles.statIcon}>{card.icon}</div>
            <div>
              <span>{card.label}</span>
              <strong>{loading ? <Skeleton width={76} height={34} /> : <Counter to={card.value} />}</strong>
              <p>{card.sub}</p>
            </div>
          </motion.article>
        ))}
      </motion.section>

      <section className={styles.mainGrid}>
        <motion.div
          className={`${styles.panel} ${styles.featurePanel}`}
          initial={{ opacity: 0, x: -18 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: 0.2, duration: 0.45, ease }}
        >
          <div className={styles.panelHead}>
            <div>
              <span className={styles.sectionKicker}>New academic tools</span>
              <h2>Important workflows</h2>
              <p>Use these for final grade publishing and semester/year progression.</p>
            </div>
          </div>

          <div className={styles.featureGrid}>
            {featuredActions.map((item, index) => (
              <motion.button
                key={item.label}
                type="button"
                className={styles.featureBtn}
                style={{ "--accent": item.color }}
                onClick={() => openPage(item.path)}
                initial={{ opacity: 0, y: 14 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.28 + index * 0.08, ease }}
                whileHover={{ y: -5, scale: 1.01 }}
                whileTap={{ scale: 0.98 }}
              >
                <span className={styles.featureIcon}>{item.icon}</span>
                <span className={styles.featureText}>
                  <small>{item.tag}</small>
                  <strong>{item.label}</strong>
                  <em>{item.desc}</em>
                </span>
                <span className={styles.launchPill}>Open →</span>
              </motion.button>
            ))}
          </div>
        </motion.div>

        <motion.aside
          className={styles.panel}
          initial={{ opacity: 0, x: 18 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ delay: 0.28, duration: 0.45, ease }}
        >
          <div className={styles.panelHeadCompact}>
            <div>
              <span className={styles.sectionKicker}>Live health</span>
              <h2>System overview</h2>
            </div>
            <span className={styles.liveBadge}>Live</span>
          </div>

          {loading ? (
            <div className={styles.skeletonStack}>
              <Skeleton height={112} />
              <Skeleton height={15} />
              <Skeleton height={15} />
              <Skeleton height={15} />
            </div>
          ) : (
            <div className={styles.overviewBody}>
              <RingMeter value={registrationRate} color="#22c55e" label="Reg Rate" />
              <div className={styles.overviewList}>
                {statCards.map((item) => (
                  <div key={item.label} className={styles.overviewRow}>
                    <span style={{ "--dot": item.color }} />
                    <p>{item.label}</p>
                    <strong style={{ color: item.color }}>{Number(item.value || 0).toLocaleString()}</strong>
                  </div>
                ))}
              </div>
            </div>
          )}
        </motion.aside>
      </section>

      <section className={styles.navigationSection}>
        <div className={styles.panelHeadInline}>
          <div>
            <span className={styles.sectionKicker}>Navigation</span>
            <h2>Admin pages</h2>
            <p>Clear shortcuts to the active admin modules, including Manage Users, Final Grade Audit, and Reset System.</p>
          </div>
        </div>

        <motion.div className={styles.actionGrid} variants={stagger} initial="hidden" animate="show">
          {regularActions.map((item) => (
            <motion.button
              key={item.label}
              type="button"
              className={styles.actionCard}
              variants={fadeUp}
              style={{ "--accent": item.color }}
              onClick={() => openPage(item.path)}
              whileHover={{ y: -5 }}
              whileTap={{ scale: 0.98 }}
            >
              <span className={styles.actionIcon}>{item.icon}</span>
              <span className={styles.actionBody}>
                <small>{item.tag}</small>
                <strong>{item.label}</strong>
                <em>{item.desc}</em>
              </span>
              <span className={styles.actionArrow}>›</span>
            </motion.button>
          ))}
        </motion.div>
      </section>

      <section className={styles.bottomGrid}>
        <motion.div
          className={styles.panel}
          initial={{ opacity: 0, y: 18 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.38, duration: 0.45, ease }}
        >
          <div className={styles.panelHeadCompact}>
            <div>
              <span className={styles.sectionKicker}>Accounts</span>
              <h2>Account distribution</h2>
            </div>
            {emailCounts && <span className={styles.totalBadge}>{emailCounts.total ?? 0} total</span>}
          </div>

          {loading ? (
            <div className={styles.skeletonStack}>
              {[1, 2, 3, 4].map((key) => (
                <Skeleton key={key} height={42} />
              ))}
            </div>
          ) : accountDistribution.length ? (
            <div className={styles.distributionList}>
              {accountDistribution.map((item) => (
                <div key={item.label} className={styles.distributionRow}>
                  <div className={styles.distributionInfo}>
                    <span style={{ background: item.color }} />
                    <strong>{item.label}</strong>
                  </div>
                  <div className={styles.distributionMeta}>
                    <b style={{ color: item.color }}>{item.value}</b>
                    <em>{item.pct}%</em>
                  </div>
                  <div className={styles.distributionTrack}>
                    <motion.i
                      style={{ background: item.color }}
                      initial={{ scaleX: 0 }}
                      animate={{ scaleX: item.pct / 100 }}
                      transition={{ duration: 0.8, ease, delay: 0.3 }}
                    />
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <p className={styles.emptyText}>No account data available.</p>
          )}
        </motion.div>

        <motion.div
          className={styles.panel}
          initial={{ opacity: 0, y: 18 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.45, duration: 0.45, ease }}
        >
          <div className={styles.panelHeadCompact}>
            <div>
              <span className={styles.sectionKicker}>Academic policy</span>
              <h2>Credit hour rules</h2>
              <p>First semester students start with 21 hours. GPA rules begin from First Year / Semester Two.</p>
            </div>
          </div>

          <div className={styles.creditGrid}>
            {CREDIT_RULES.map((rule, index) => (
              <motion.article
                key={rule.title}
                className={styles.creditCard}
                style={{ "--accent": rule.color }}
                initial={{ opacity: 0, y: 14 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.5 + index * 0.05, ease }}
              >
                <strong>{rule.hours}</strong>
                <span>hrs</span>
                <h3>{rule.title}</h3>
                <p>{rule.range}</p>
                <em>{rule.note}</em>
              </motion.article>
            ))}
          </div>
        </motion.div>
      </section>
    </main>
  );
}
