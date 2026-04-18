// src/pages/admin/AdminDashboard.jsx — Full redesign with real backend data
import { useState, useEffect, useMemo } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { useRegistration } from "../../context/RegistrationContext";
import { getAdminStats, getEmails } from "../../services/api/adminApi";
import styles from "./AdminDashboard.module.css";

/* ── Animated counter ── */
function Counter({ to, duration = 1.4 }) {
  const [val, setVal] = useState(0);
  useEffect(() => {
    if (!to) { setVal(0); return; }
    let n = 0;
    const steps = 60;
    const inc = to / steps;
    const id = setInterval(() => {
      n += inc;
      if (n >= to) { setVal(to); clearInterval(id); }
      else setVal(Math.floor(n));
    }, (duration * 1000) / steps);
    return () => clearInterval(id);
  }, [to, duration]);
  return <>{val.toLocaleString()}</>;
}

/* ── Radial arc ── */
function ArcMeter({ pct, color, size = 104 }) {
  const r = (size - 14) / 2;
  const cx = size / 2, cy = size / 2;
  const circ = 2 * Math.PI * r;
  return (
    <svg width={size} height={size} style={{ transform: "rotate(-90deg)" }}>
      <circle cx={cx} cy={cy} r={r} fill="none" stroke="var(--border)" strokeWidth="10" />
      <motion.circle cx={cx} cy={cy} r={r} fill="none"
        stroke={color} strokeWidth="10" strokeLinecap="round"
        strokeDasharray={circ}
        initial={{ strokeDashoffset: circ }}
        animate={{ strokeDashoffset: circ - (pct / 100) * circ }}
        transition={{ delay: 0.45, duration: 1.3, ease: [0.22, 1, 0.36, 1] }}
      />
    </svg>
  );
}

/* ── Skeleton shimmer ── */
function Sk({ h = 20, w = "100%", r = 10 }) {
  return <div className={styles.skeleton} style={{ height: h, width: w, borderRadius: r }} />;
}

/* ─────────────────────────────────────── */
const ACTIONS = [
  { label: "Register User",        desc: "Add student or instructor",        path: "/admin/register",      icon: "👤", color: "#818cf8" },
  { label: "Manage Users",         desc: "View & control accounts",          path: "/admin/manage-users",  icon: "⚙️", color: "#22c55e" },
  { label: "Registration Manager", desc: "Open / close course registration", path: "/admin/registration",  icon: "📋", color: "#f59e0b" },
  { label: "Email Manager",        desc: "Create & manage emails",           path: "/admin/email-manager", icon: "✉️", color: "#ef4444" },
  { label: "Schedule Manager",     desc: "Build weekly & exam schedules",    path: "/admin/schedule",      icon: "🗓️", color: "#14b8a6" },
  { label: "Themes",               desc: "Customize appearance",             path: "/admin/themes",        icon: "🎨", color: "#ec4899" },
];

const GPA_RULES = [
  { label: "Excellent", range: "GPA ≥ 3.5", hrs: 21, bg: "linear-gradient(135deg,#14532d,#22c55e)" },
  { label: "Very Good", range: "GPA ≥ 3.0", hrs: 18, bg: "linear-gradient(135deg,#166534,#4ade80)" },
  { label: "Good",      range: "GPA ≥ 2.5", hrs: 18, bg: "linear-gradient(135deg,#3730a3,#818cf8)" },
  { label: "Pass",      range: "GPA ≥ 2.0", hrs: 15, bg: "linear-gradient(135deg,#92400e,#f59e0b)" },
  { label: "Warning",   range: "GPA ≥ 1.5", hrs: 12, bg: "linear-gradient(135deg,#7c2d12,#f97316)" },
  { label: "Probation", range: "GPA < 1.5",  hrs: 9,  bg: "linear-gradient(135deg,#7f1d1d,#ef4444)" },
];

const ease = [0.22, 1, 0.36, 1];
const fadeUp = { hidden: { opacity: 0, y: 20 }, show: { opacity: 1, y: 0, transition: { duration: 0.44, ease } } };
const stagger = { show: { transition: { staggerChildren: 0.07 } } };

/* ─────────────────────────────────────── */
export default function AdminDashboard() {
  const { user } = useAuth();
  const { regWindow } = useRegistration();
  const navigate = useNavigate();
  const [time, setTime] = useState(new Date());
  const [stats, setStats] = useState(null);
  const [emailCounts, setEmailCounts] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  /* clock */
  useEffect(() => {
    const id = setInterval(() => setTime(new Date()), 1000);
    return () => clearInterval(id);
  }, []);

  /* fetch real data */
  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        const [statsData, emailsData] = await Promise.all([
          getAdminStats(),
          getEmails(),
        ]);
        if (!alive) return;
        setStats(statsData);
        setEmailCounts(emailsData?.counts ?? null);
      } catch (err) {
        if (alive) setError(err.message || "Failed to load dashboard data");
      } finally {
        if (alive) setLoading(false);
      }
    })();
    return () => { alive = false; };
  }, []);

  const greeting = () => {
    const h = time.getHours();
    if (h < 12) return "Good Morning";
    if (h < 17) return "Good Afternoon";
    return "Good Evening";
  };

  const daysLeft = regWindow.deadline
    ? Math.max(0, Math.ceil((new Date(regWindow.deadline) - time) / 86400000))
    : null;

  const firstName = (user?.name || "Admin").split(" ")[0];
  const regPct    = stats?.registrationRate ?? 0;

  const STAT_CARDS = useMemo(() => [
    { id: "students",    label: "Total Students",  value: stats?.totalStudents    ?? 0, icon: "👥", bg: "linear-gradient(135deg,#4338ca,#818cf8)", sub: "Enrolled accounts",   trend: stats?.trends?.students,    up: stats?.trends?.students?.startsWith("+")    },
    { id: "registered",  label: "Registered",       value: stats?.totalRegistered  ?? 0, icon: "✅", bg: "linear-gradient(135deg,#15803d,#22c55e)", sub: "Course registrations", trend: stats?.trends?.registered,  up: stats?.trends?.registered?.startsWith("+")  },
    { id: "instructors", label: "Instructors",       value: stats?.totalInstructors ?? 0, icon: "🎓", bg: "linear-gradient(135deg,#1d4ed8,#3b82f6)", sub: "Active faculty",       trend: stats?.trends?.instructors, up: stats?.trends?.instructors?.startsWith("+") },
    { id: "courses",     label: "Active Courses",    value: stats?.activeCourses    ?? 0, icon: "📚", bg: "linear-gradient(135deg,#b45309,#f59e0b)", sub: "In curriculum",        trend: null,                       up: null                                        },
  ], [stats]);

  return (
    <div className={styles.page}>

      {/* ── error banner ── */}
      <AnimatePresence>
        {error && (
          <motion.div className={styles.errorBanner}
            initial={{ opacity: 0, y: -16 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -12 }}>
            ⚠️ {error}
            <button className={styles.errorClose} onClick={() => setError(null)}>✕</button>
          </motion.div>
        )}
      </AnimatePresence>

      {/* ═══════════════════════════════════
          HERO
      ═══════════════════════════════════ */}
      <motion.div className={styles.hero}
        initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.52, ease }}>

        <div className={styles.heroLeft}>
          <div className={styles.heroBadge}>
            <span className={styles.heroPulse} />
            Admin Control Panel
          </div>
          <h1 className={styles.heroTitle}>
            {greeting()}, <span className={styles.heroAccent}>{firstName}</span> 👋
          </h1>
          <p className={styles.heroSub}>
            <span className={styles.heroDate}>
              {time.toLocaleDateString("en-US", { weekday: "long", year: "numeric", month: "long", day: "numeric" })}
            </span>
            <span className={styles.heroDot} />
            <span className={styles.heroClock}>
              {time.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit", second: "2-digit" })}
            </span>
          </p>
        </div>

        <motion.div
          className={`${styles.regCard} ${regWindow.isOpen ? styles.regOpen : styles.regClosed}`}
          animate={regWindow.isOpen
            ? { boxShadow: ["0 0 0 0 rgba(34,197,94,.3)", "0 0 0 18px rgba(34,197,94,0)", "0 0 0 0 rgba(34,197,94,0)"] }
            : {}}
          transition={{ duration: 2.8, repeat: Infinity }}>
          <div className={styles.regCardIcon}>
            {regWindow.isOpen
              ? <motion.span animate={{ scale: [1, 1.2, 1] }} transition={{ duration: 2, repeat: Infinity }}>🟢</motion.span>
              : "🔴"}
          </div>
          <div className={styles.regCardBody}>
            <span className={styles.regCardTitle}>
              Registration {regWindow.isOpen ? "Open" : "Closed"}
            </span>
            <span className={styles.regCardSub}>
              {regWindow.isOpen && daysLeft !== null
                ? daysLeft === 0 ? "⚡ Closes today!" : `${daysLeft} day${daysLeft !== 1 ? "s" : ""} left · ${regWindow.semester ?? ""}`
                : regWindow.isOpen ? "Active now" : "Students cannot register courses"}
            </span>
          </div>
          <motion.button className={styles.regCardBtn}
            onClick={() => navigate("/admin/registration")}
            whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
            Manage →
          </motion.button>
        </motion.div>
      </motion.div>

      {/* ═══════════════════════════════════
          STAT CARDS
      ═══════════════════════════════════ */}
      <motion.div className={styles.statsRow} variants={stagger} initial="hidden" animate="show">
        {STAT_CARDS.map((s, i) => (
          <motion.div key={s.id} className={styles.statCard} variants={fadeUp}
            style={{ background: s.bg }}
            whileHover={{ y: -6, boxShadow: "0 22px 52px rgba(0,0,0,.28)" }}>
            <div className={styles.statGlow} />
            <div className={styles.statTop}>
              <div className={styles.statIconBox}>{s.icon}</div>
              {s.trend && (
                <span className={`${styles.statTrend} ${s.up ? styles.trendUp : styles.trendDown}`}>
                  {s.up ? "↑" : "↓"} {s.trend}
                </span>
              )}
            </div>
            <div className={styles.statValue}>
              {loading ? <Sk h={38} w={80} r={8} /> : <Counter to={s.value} />}
            </div>
            <div className={styles.statLabel}>{s.label}</div>
            <div className={styles.statSub}>{s.sub}</div>
            <motion.div className={styles.statBar}
              initial={{ scaleX: 0 }} animate={{ scaleX: 1 }}
              transition={{ delay: 0.28 + i * 0.08, duration: 0.9, ease: "easeOut" }} />
          </motion.div>
        ))}
      </motion.div>

      {/* ═══════════════════════════════════
          MIDDLE GRID
      ═══════════════════════════════════ */}
      <div className={styles.midGrid}>

        {/* ── Quick Actions ── */}
        <motion.div className={styles.card}
          initial={{ opacity: 0, x: -22 }} animate={{ opacity: 1, x: 0 }}
          transition={{ delay: 0.26, duration: 0.46, ease }}>
          <div className={styles.cardHead}>
            <div>
              <h2 className={styles.cardTitle}>Quick Actions</h2>
              <p className={styles.cardSub}>Navigate to any admin section</p>
            </div>
            <span className={styles.cardBadge}>{ACTIONS.length}</span>
          </div>
          <div className={styles.actGrid}>
            {ACTIONS.map((a, i) => (
              <motion.button key={a.label} className={styles.actBtn}
                style={{ "--ac": a.color }}
                onClick={() => navigate(a.path)}
                initial={{ opacity: 0, scale: 0.94 }} animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.32 + i * 0.05, ease }}
                whileHover={{ y: -3, scale: 1.016 }} whileTap={{ scale: 0.97 }}>
                <div className={styles.actIcon} style={{ background: a.color }}>{a.icon}</div>
                <div className={styles.actText}>
                  <span className={styles.actLabel}>{a.label}</span>
                  <span className={styles.actDesc}>{a.desc}</span>
                </div>
                <span className={styles.actArrow}>›</span>
              </motion.button>
            ))}
          </div>
        </motion.div>

        {/* ── Right stack ── */}
        <div className={styles.rightStack}>

          {/* System Overview */}
          <motion.div className={styles.card}
            initial={{ opacity: 0, x: 22 }} animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.32, duration: 0.46, ease }}>
            <div className={styles.cardHead}>
              <h2 className={styles.cardTitle}>System Overview</h2>
              <span className={styles.liveTag}>
                <motion.span className={styles.liveDot}
                  animate={{ scale: [1, 1.8, 1], opacity: [1, 0.3, 1] }}
                  transition={{ duration: 2, repeat: Infinity }} />
                Live
              </span>
            </div>

            {loading ? (
              <div className={styles.skStack}>
                <Sk h={104} r={14} />
                <Sk h={14} r={6} />
                <Sk h={14} r={6} />
                <Sk h={14} r={6} />
              </div>
            ) : (
              <div className={styles.ovLayout}>
                <div className={styles.ovArcWrap}>
                  <ArcMeter pct={regPct} color="#22c55e" size={108} />
                  <div className={styles.ovArcCenter}>
                    <span className={styles.ovPct} style={{ color: "#22c55e" }}>{regPct}%</span>
                    <span className={styles.ovPctLbl}>Reg Rate</span>
                  </div>
                </div>
                <div className={styles.ovList}>
                  {[
                    { label: "Students",    val: stats?.totalStudents    ?? 0, color: "#818cf8" },
                    { label: "Registered",  val: stats?.totalRegistered  ?? 0, color: "#22c55e" },
                    { label: "Instructors", val: stats?.totalInstructors ?? 0, color: "#3b82f6" },
                    { label: "Courses",     val: stats?.activeCourses    ?? 0, color: "#f59e0b" },
                  ].map(item => (
                    <div key={item.label} className={styles.ovRow}>
                      <div className={styles.ovRowLeft}>
                        <span className={styles.ovDot} style={{ background: item.color }} />
                        <span className={styles.ovLabel}>{item.label}</span>
                      </div>
                      <span className={styles.ovVal} style={{ color: item.color }}>
                        {item.val.toLocaleString()}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </motion.div>

          {/* Account Distribution */}
          <motion.div className={styles.card}
            initial={{ opacity: 0, x: 22 }} animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.4, duration: 0.46, ease }}>
            <div className={styles.cardHead}>
              <h2 className={styles.cardTitle}>Account Distribution</h2>
              {emailCounts && (
                <span className={styles.totalTag}>{emailCounts.total ?? 0} total</span>
              )}
            </div>

            {loading ? (
              <div className={styles.skStack}>
                {[1,2,3,4].map(k => <Sk key={k} h={44} r={10} />)}
              </div>
            ) : emailCounts ? (
              <div className={styles.distList}>
                {[
                  { label: "Students",    val: emailCounts.student    ?? 0, color: "#818cf8" },
                  { label: "Instructors", val: emailCounts.instructor ?? 0, color: "#22c55e" },
                  { label: "Admins",      val: emailCounts.admin      ?? 0, color: "#f59e0b" },
                  { label: "Suspended",   val: emailCounts.suspended  ?? 0, color: "#ef4444" },
                ].map(item => {
                  const total = emailCounts.total || 1;
                  const pct   = Math.round((item.val / total) * 100);
                  return (
                    <div key={item.label} className={styles.distRow}>
                      <div className={styles.distTop}>
                        <div className={styles.distLeft}>
                          <span className={styles.distDot} style={{ background: item.color }} />
                          <span className={styles.distLabel}>{item.label}</span>
                        </div>
                        <div className={styles.distRight}>
                          <span className={styles.distVal} style={{ color: item.color }}>{item.val}</span>
                          <span className={styles.distPct}>{pct}%</span>
                        </div>
                      </div>
                      <div className={styles.distBarBg}>
                        <motion.div className={styles.distBarFill}
                          style={{ background: item.color }}
                          initial={{ scaleX: 0 }} animate={{ scaleX: pct / 100 }}
                          transition={{ delay: 0.55, duration: 0.85, ease: "easeOut" }} />
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className={styles.emptyState}>No account data available</p>
            )}
          </motion.div>

        </div>
      </div>

      {/* ═══════════════════════════════════
          GPA CREDIT RULES
      ═══════════════════════════════════ */}
      <motion.div className={styles.gpaSection}
        initial={{ opacity: 0, y: 22 }} animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.5, ease }}>
        <div className={styles.card}>
          <div className={styles.cardHead}>
            <div>
              <h2 className={styles.cardTitle}>Credit Hour Rules</h2>
              <p className={styles.cardSub}>Applied automatically based on GPA — 133 credit hour program</p>
            </div>
            <span className={styles.gpaTag}>GPA System</span>
          </div>
          <div className={styles.gpaGrid}>
            {GPA_RULES.map((r, i) => (
              <motion.div key={r.label} className={styles.gpaCard}
                style={{ background: r.bg }}
                initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.56 + i * 0.06, ease }}
                whileHover={{ y: -5, boxShadow: "0 16px 38px rgba(0,0,0,.24)" }}>
                <div className={styles.gpaHrsRow}>
                  <span className={styles.gpaHrs}>{r.hrs}</span>
                  <span className={styles.gpaHrsSub}>hrs/sem</span>
                </div>
                <div className={styles.gpaLabel}>{r.label}</div>
                <div className={styles.gpaRange}>{r.range}</div>
                <div className={styles.gpaBarWrap}>
                  <motion.div className={styles.gpaBar}
                    initial={{ scaleX: 0 }} animate={{ scaleX: r.hrs / 21 }}
                    transition={{ delay: 0.72 + i * 0.06, duration: 0.75, ease: "easeOut" }} />
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </motion.div>

    </div>
  );
}
