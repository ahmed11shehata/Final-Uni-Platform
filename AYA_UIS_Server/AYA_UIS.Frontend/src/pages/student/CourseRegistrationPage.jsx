// src/pages/student/CourseRegistrationPage.jsx
import { useState, useMemo, useEffect, useContext, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useRegistration } from "../../context/RegistrationContext";
import { AuthContext } from "../../context/AuthContext";
import {
  getStudentRegistrationStatus,
  getStudentAvailableCourses,
  registerCourse,
  dropCourse,
} from "../../services/api/studentApi";
import styles from "./CourseRegistrationPage.module.css";

/* ═══════════════════════════════════════════════════════════
   GPA → Credit Hour System (fallback — backend is authoritative)
═══════════════════════════════════════════════════════════ */
function getGpaLabel(gpa) {
  if (gpa >= 3.7) return { label: "Excellent", color: "#22c55e" };
  if (gpa >= 3.0) return { label: "Very Good", color: "#4ade80" };
  if (gpa >= 2.0) return { label: "Good",      color: "#f59e0b" };
  return                  { label: "Probation", color: "#ef4444" };
}

/* ── SVG Patterns (same style as CoursesPage) ── */
function getPatternBg(pattern, color) {
  const enc = encodeURIComponent;
  const patterns = {
    mosaic:   `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='56' height='56'%3E%3Crect x='2' y='2' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.45'/%3E%3Crect x='30' y='2' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.25'/%3E%3Crect x='2' y='30' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.25'/%3E%3Crect x='30' y='30' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.45'/%3E%3C/svg%3E")`,
    circles:  `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80'%3E%3Ccircle cx='40' cy='40' r='28' fill='none' stroke='${enc(color)}' stroke-width='18' opacity='.28'/%3E%3Ccircle cx='0' cy='0' r='18' fill='none' stroke='${enc(color)}' stroke-width='12' opacity='.18'/%3E%3Ccircle cx='80' cy='80' r='18' fill='none' stroke='${enc(color)}' stroke-width='12' opacity='.18'/%3E%3C/svg%3E")`,
    squares:  `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='72' height='72'%3E%3Crect x='8' y='8' width='56' height='56' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.22'/%3E%3Crect x='18' y='18' width='36' height='36' fill='none' stroke='${enc(color)}' stroke-width='2.5' opacity='.18'/%3E%3Crect x='28' y='28' width='16' height='16' fill='none' stroke='${enc(color)}' stroke-width='2' opacity='.15'/%3E%3C/svg%3E")`,
    diamonds: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='64' height='64'%3E%3Cpolygon points='32,4 60,32 32,60 4,32' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.26'/%3E%3Cpolygon points='32,16 48,32 32,48 16,32' fill='none' stroke='${enc(color)}' stroke-width='2.5' opacity='.2'/%3E%3C/svg%3E")`,
    waves:    `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='40'%3E%3Cpath d='M0 20 Q20 0 40 20 Q60 40 80 20' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.25'/%3E%3Cpath d='M0 30 Q20 10 40 30 Q60 50 80 30' fill='none' stroke='${enc(color)}' stroke-width='2' opacity='.15'/%3E%3C/svg%3E")`,
    dots:     `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32'%3E%3Ccircle cx='4' cy='4' r='2.5' fill='${enc(color)}' opacity='.35'/%3E%3Ccircle cx='20' cy='4' r='1.5' fill='${enc(color)}' opacity='.2'/%3E%3Ccircle cx='4' cy='20' r='1.5' fill='${enc(color)}' opacity='.2'/%3E%3Ccircle cx='20' cy='20' r='2.5' fill='${enc(color)}' opacity='.35'/%3E%3C/svg%3E")`,
  };
  return patterns[pattern] || patterns.mosaic;
}

const FILTERS = ["All","Available","Registered","Completed","Locked","Full","Prerequisite"];

const STATUS_CFG = {
  available:    { color:"#22c55e",  label:"Available"     },
  registered:   { color:"#818cf8",  label:"Registered"    },
  completed:    { color:"#0ea5e9",  label:"Completed"     },
  locked:       { color:"#f59e0b",  label:"Locked"        },
  full:         { color:"#6b7280",  label:"Full"          },
  prerequisite: { color:"#a855f7",  label:"Prerequisite"  },
};

/* ── Variants ── */
const fadeUp = { hidden:{opacity:0,y:20}, show:{opacity:1,y:0} };
const stagger = { show:{ transition:{ staggerChildren:0.055 } } };

/* ═══════════════════════════════════════════════════════════
   PAGE
═══════════════════════════════════════════════════════════ */
export default function CourseRegistrationPage() {
  const { regWindow } = useRegistration();
  const { user }      = useContext(AuthContext) ?? {};

  /* ── Real API state ── */
  const [regStatus, setRegStatus] = useState(null); // from /student/registration/status
  const [courses, setCourses]     = useState([]);
  const [loading, setLoading]     = useState(true);
  const [actionLoading, setActionLoading] = useState(null); // courseCode being acted on

  const [filter,      setFilter]      = useState("All");
  const [search,      setSearch]      = useState("");
  const [confirmDrop, setConfirmDrop] = useState(null);
  const [toasts,      setToasts]      = useState([]);

  const addToast = useCallback((msg, type = "success") => {
    const id = Date.now();
    setToasts(p => [...p, {id, msg, type}]);
    setTimeout(() => setToasts(p => p.filter(t => t.id !== id)), 3500);
  }, []);

  /* ── Fetch data from backend ── */
  const fetchData = useCallback(async () => {
    try {
      const [status, coursesData] = await Promise.all([
        getStudentRegistrationStatus(),
        getStudentAvailableCourses(),
      ]);
      setRegStatus(status);
      setCourses(coursesData?.courses || []);
    } catch (err) {
      // If not authenticated or any error, show closed state
      console.error("Failed to fetch registration data:", err);
      setRegStatus({ open: false, currentCredits: 0, maxCredits: 18 });
      setCourses([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  /* ── Derived values from real backend ── */
  const isOpen = regStatus?.open ?? false;
  const studentGpa = user?.gpa ?? 0;
  const gpaLabel   = getGpaLabel(studentGpa);
  const allowedCredits = regStatus?.maxCredits ?? 18;
  const registeredCredits = regStatus?.currentCredits ?? 0;
  const remaining = allowedCredits - registeredCredits;
  const pct = allowedCredits > 0 ? Math.min(100, Math.round((registeredCredits / allowedCredits) * 100)) : 0;

  /* ── Register a course ── */
  const handleRegister = async (courseCode) => {
    if (!isOpen) return;
    const c = courses.find(x => x.code === courseCode);
    if (c && remaining < c.credits) {
      addToast("Not enough credit hours remaining", "error");
      return;
    }
    setActionLoading(courseCode);
    try {
      await registerCourse(courseCode);
      addToast(`${courseCode} registered successfully ✓`);
      await fetchData(); // refresh all data
    } catch (err) {
      const msg = err?.response?.data?.error?.message || err.message || "Registration failed";
      addToast(msg, "error");
    } finally {
      setActionLoading(null);
    }
  };

  /* ── Drop a course ── */
  const handleDrop = async (courseCode) => {
    setActionLoading(courseCode);
    setConfirmDrop(null);
    try {
      await dropCourse(courseCode);
      addToast(`${courseCode} dropped`, "info");
      await fetchData(); // refresh all data
    } catch (err) {
      const msg = err?.response?.data?.error?.message || err.message || "Drop failed";
      addToast(msg, "error");
    } finally {
      setActionLoading(null);
    }
  };

  /* ── Filtering ── */
  const filtered = useMemo(() => courses.filter(c => {
    const matchStatus = c.status && STATUS_CFG[c.status]; // only show known statuses
    const f = filter==="All" || c.status?.toLowerCase()===filter.toLowerCase();
    const s = c.name?.toLowerCase().includes(search.toLowerCase()) ||
              c.code?.toLowerCase().includes(search.toLowerCase());
    return f && s && matchStatus;
  }), [courses, filter, search]);

  const counts = useMemo(() => ({
    all:          courses.filter(c => STATUS_CFG[c.status]).length,
    available:    courses.filter(c=>c.status==="available").length,
    registered:   courses.filter(c=>c.status==="registered").length,
    completed:    courses.filter(c=>c.status==="completed").length,
    locked:       courses.filter(c=>c.status==="locked").length,
    full:         courses.filter(c=>c.status==="full").length,
    prerequisite: courses.filter(c=>c.status==="prerequisite").length,
  }), [courses]);

  /* ── Loading state ── */
  if (loading) {
    return (
      <div className={styles.page} style={{display:"flex",alignItems:"center",justifyContent:"center",minHeight:"60vh"}}>
        <div style={{textAlign:"center",color:"var(--text-muted)"}}>
          <div style={{fontSize:28,marginBottom:12}}>⏳</div>
          Loading registration data...
        </div>
      </div>
    );
  }

  /* ── Registration CLOSED ── */
  if (!isOpen) {
    return (
      <div className={styles.page}>
        <ClosedState regWindow={regWindow} />
      </div>
    );
  }

  /* ── Deadline info (from admin context) ── */
  const deadlineDate = regWindow?.deadline ? new Date(regWindow.deadline) : null;
  const daysLeft = deadlineDate
    ? Math.max(0, Math.ceil((deadlineDate - new Date()) / 86400000))
    : null;

  return (
    <div className={styles.page}>

      {/* Toasts */}
      <div className={styles.toastStack}>
        <AnimatePresence>
          {toasts.map(t => (
            <motion.div key={t.id}
              className={`${styles.toast} ${styles[`toast_${t.type}`]}`}
              initial={{opacity:0,y:-16,scale:0.94}}
              animate={{opacity:1,y:0,scale:1}}
              exit={{opacity:0,x:60,scale:0.94}}
              transition={{type:"spring",stiffness:400,damping:28}}
            >
              <span className={styles.toastDot}/>
              {t.msg}
            </motion.div>
          ))}
        </AnimatePresence>
      </div>

      {/* Drop Modal */}
      <AnimatePresence>
        {confirmDrop && (
          <motion.div className={styles.overlay}
            initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}
            onClick={()=>setConfirmDrop(null)}
          >
            <motion.div className={styles.modal}
              initial={{scale:0.86,y:28,opacity:0}}
              animate={{scale:1,y:0,opacity:1}}
              exit={{scale:0.9,opacity:0}}
              transition={{type:"spring",stiffness:360,damping:26}}
              onClick={e=>e.stopPropagation()}
            >
              <div className={styles.modalRing}>
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#ef4444" strokeWidth="1.8">
                  <polyline points="3 6 5 6 21 6"/>
                  <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/>
                </svg>
              </div>
              <h3 className={styles.modalTitle}>Drop Course?</h3>
              <p className={styles.modalSub}>
                <strong>{confirmDrop.code}</strong> — {confirmDrop.name}
                <br/>You can re-register before the deadline.
              </p>
              <div className={styles.modalRow}>
                <button className={styles.modalCancel} onClick={()=>setConfirmDrop(null)}>Cancel</button>
                <button className={styles.modalDrop} onClick={()=>handleDrop(confirmDrop.code)}
                  disabled={actionLoading === confirmDrop.code}>
                  {actionLoading === confirmDrop.code ? "Dropping..." : "Drop Course"}
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── Header ── */}
      <motion.div className={styles.header} variants={fadeUp} initial="hidden" animate="show">
        <div className={styles.headerLeft}>
          <div className={styles.headerIcon}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round">
              <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/>
            </svg>
          </div>
          <div>
            <h1 className={styles.headerTitle}>Course Registration</h1>
            <p className={styles.headerSub}>{regWindow?.semester || ""} · {regWindow?.academicYear || ""}</p>
          </div>
        </div>

        {/* Live banner */}
        <div className={styles.liveBanner}>
          <span className={styles.liveDot}/>
          <span className={styles.liveLabel}>Registration Open</span>
          {daysLeft !== null && (
            <span className={`${styles.deadline} ${daysLeft<=3?styles.deadlineUrgent:""}`}>
              {daysLeft===0 ? "Closes Today!" : `${daysLeft}d left`}
            </span>
          )}
        </div>
      </motion.div>

      {/* ── GPA + Credits Widget ── */}
      <motion.div className={styles.creditsCard}
        variants={fadeUp} initial="hidden" animate="show" transition={{delay:0.06}}>

        {/* Left: GPA info */}
        <div className={styles.gpaBlock}>
          <div className={styles.gpaCircle} style={{"--gc": gpaLabel.color}}>
            <span className={styles.gpaNum}>{studentGpa.toFixed(1)}</span>
            <span className={styles.gpaSlash}>GPA</span>
          </div>
          <div>
            <div className={styles.gpaTag} style={{color:gpaLabel.color, background:`${gpaLabel.color}15`}}>
              {gpaLabel.label}
            </div>
            <div className={styles.gpaRule}>
              Credit limit: <strong>{allowedCredits} hrs</strong>
            </div>
          </div>
        </div>

        {/* Divider */}
        <div className={styles.creditsDivider}/>

        {/* Centre: progress */}
        <div className={styles.progressBlock}>
          <div className={styles.progressNums}>
            <span className={styles.progressUsed}
              style={{color: pct>=90?"#ef4444":pct>=70?"#f59e0b":"var(--accent)"}}>
              {registeredCredits}
            </span>
            <span className={styles.progressOf}>/ {allowedCredits}</span>
            <span className={styles.progressLabel}>credit hours used</span>
          </div>
          <div className={styles.progressTrack}>
            <motion.div className={styles.progressBar}
              initial={{width:0}}
              animate={{width:`${pct}%`}}
              transition={{duration:1.1, ease:[0.22,1,0.36,1]}}
              style={{background: pct>=90?"#ef4444":pct>=70?"#f59e0b":"var(--accent)"}}
            />
          </div>
          <div className={styles.progressRemain}>
            <span>{remaining} hrs remaining</span>
            <span>{pct}%</span>
          </div>
        </div>

        {/* Right: status pills */}
        <div className={styles.pillsBlock}>
          {[
            {key:"registered", label:"Registered"},
            {key:"available",  label:"Available"},
            {key:"locked",     label:"Locked"},
            {key:"full",       label:"Full"},
          ].map(p => (
            <div key={p.key} className={styles.pill}>
              <span className={styles.pillDot} style={{background:STATUS_CFG[p.key].color}}/>
              <span className={styles.pillCount} style={{color:STATUS_CFG[p.key].color}}>
                {counts[p.key]}
              </span>
              <span className={styles.pillLabel}>{p.label}</span>
            </div>
          ))}
        </div>

        {/* Credit legend */}
        <div className={styles.creditLegend}>
          <div className={styles.creditLegendTitle}>Credit Rules</div>
          {[
            {range:"GPA ≥ 3.0", hrs:"21 hrs", color:"#22c55e"},
            {range:"GPA ≥ 2.0", hrs:"18 hrs", color:"#f59e0b"},
            {range:"GPA < 2.0", hrs:"15 hrs", color:"#ef4444"},
          ].map(r => (
            <div key={r.range} className={styles.creditRule}
              style={{opacity: allowedCredits === parseInt(r.hrs) ? 1 : 0.4}}>
              <span className={styles.creditRuleDot} style={{background:r.color}}/>
              <span>{r.range}</span>
              <span className={styles.creditRuleHrs} style={{color:r.color}}>{r.hrs}</span>
            </div>
          ))}
        </div>
      </motion.div>

      {/* ── Controls ── */}
      <motion.div className={styles.controls}
        variants={fadeUp} initial="hidden" animate="show" transition={{delay:0.1}}>
        <div className={styles.filterRow}>
          {FILTERS.map(f => {
            const count = f==="All" ? counts.all : counts[f.toLowerCase()] ?? 0;
            return (
              <button key={f}
                className={`${styles.chip} ${filter===f?styles.chipOn:""}`}
                onClick={()=>setFilter(f)}
              >
                {f}
                <span className={styles.chipCount}>{count}</span>
              </button>
            );
          })}
        </div>
        <div className={styles.searchBox}>
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
          </svg>
          <input className={styles.searchIn} placeholder="Search by name or code…"
            value={search} onChange={e=>setSearch(e.target.value)}/>
          {search && (
            <button className={styles.clearBtn} onClick={()=>setSearch("")}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          )}
        </div>
      </motion.div>

      {/* ── Grid ── */}
      <motion.div className={styles.grid}
        variants={stagger} initial="hidden" animate="show">
        <AnimatePresence mode="popLayout">
          {filtered.map((course, i) => (
            <CourseCard key={course.code} course={course} index={i}
              remaining={remaining}
              actionLoading={actionLoading}
              onRegister={()=>handleRegister(course.code)}
              onDrop={()=>setConfirmDrop(course)}
            />
          ))}
        </AnimatePresence>
        {filtered.length===0 && (
          <motion.div className={styles.empty} variants={fadeUp}>
            <div className={styles.emptyIcon}>
              <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.3">
                <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
              </svg>
            </div>
            <p className={styles.emptyText}>No courses match your search</p>
            <button className={styles.emptyReset} onClick={()=>{setSearch("");setFilter("All");}}>
              Reset filters
            </button>
          </motion.div>
        )}
      </motion.div>
    </div>
  );
}

/* ═══════════════════════════════════════════════════════════
   COURSE CARD
═══════════════════════════════════════════════════════════ */
function CourseCard({ course, index, remaining, actionLoading, onRegister, onDrop }) {
  const cfg    = STATUS_CFG[course.status] || STATUS_CFG.available;
  const capPct = course.capacity > 0 ? Math.round((course.enrolled / course.capacity) * 100) : 0;
  const canReg = course.credits <= remaining;
  const pat    = getPatternBg(course.pattern, course.color);
  const busy   = actionLoading === course.code;

  return (
    <motion.article
      className={`${styles.card} ${styles[`card_${course.status}`]}`}
      variants={fadeUp}
      layout
      whileHover={course.status!=="full"&&course.status!=="locked"
        ? {y:-6, transition:{duration:0.2}}
        : {scale:1.01}
      }
    >
      {/* ── Cover ── */}
      <div className={styles.cover}
        style={{
          background: course.color,
          backgroundImage: pat,
          backgroundSize: "64px 64px",
        }}
      >
        <div className={styles.coverDark}/>

        {/* Top row */}
        <div className={styles.coverTop}>
          <span className={styles.codeTag}>{course.code}</span>
          <span className={styles.crTag}>{course.credits} cr</span>
        </div>

        {/* Status badge */}
        <div className={styles.coverBot}>
          <span className={styles.statusBadge}>
            <span className={styles.statusDot} style={{background:cfg.color}}/>
            {cfg.label}
          </span>
          {course.status==="registered" && (
            <span className={styles.checkmark}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
            </span>
          )}
          {course.status==="completed" && (
            <span className={styles.checkmark} style={{background:"#0ea5e9"}}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
            </span>
          )}
          {course.status==="locked" && (
            <span className={styles.lockIcon}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.2">
                <rect x="3" y="11" width="18" height="11" rx="2"/>
                <path d="M7 11V7a5 5 0 0110 0v4"/>
              </svg>
            </span>
          )}
        </div>
      </div>

      {/* ── Body ── */}
      <div className={styles.cardBody}>
        <h3 className={styles.courseName}>{course.name}</h3>

        <div className={styles.metaList}>
          {course.instructor && (
            <div className={styles.metaRow}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/>
                <circle cx="12" cy="7" r="4"/>
              </svg>
              <span>{course.instructor}</span>
            </div>
          )}
          {course.schedule && (
            <div className={styles.metaRow}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
              </svg>
              <span>{course.schedule}</span>
            </div>
          )}
        </div>

        {/* Capacity bar */}
        {course.capacity > 0 && (
          <div className={styles.capRow}>
            <div className={styles.capTrack}>
              <motion.div className={styles.capFill}
                initial={{width:0}}
                animate={{width:`${capPct}%`}}
                transition={{duration:0.9,ease:"easeOut",delay:index*0.04+0.3}}
                style={{background:capPct>=95?"#ef4444":capPct>=75?"#f59e0b":"#22c55e"}}
              />
            </div>
            <span className={styles.capLabel}>{course.enrolled}/{course.capacity}</span>
          </div>
        )}

        {/* Prereq / lock warning */}
        {course.status==="locked" && (course.prereqs?.length>0 || course.lockReason) && (
          <div className={styles.prereq}>
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="#f59e0b" strokeWidth="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="12" y1="8" x2="12" y2="12"/>
              <line x1="12" y1="16" x2="12.01" y2="16"/>
            </svg>
            {course.lockReason || `Requires: ${course.prereqs.join(", ")}`}
          </div>
        )}

        {/* Prerequisite breakdown — shown for any course with prereq metadata.
            Highlights passed/missing per prerequisite. */}
        {Array.isArray(course.prerequisiteDetails) && course.prerequisiteDetails.length>0 && (
          <div style={{
            marginTop:8,
            padding:"8px 10px",
            border:"1px solid var(--card-border)",
            borderRadius:8,
            background:"var(--hover-bg)",
            fontSize:12.5,
            lineHeight:1.5,
          }}>
            <div style={{
              fontWeight:700,
              color:"var(--text-primary)",
              marginBottom:4,
              fontSize:12,
            }}>
              Prerequisites
            </div>
            {course.prerequisiteDetails.map(p => (
              <div key={p.code} style={{
                display:"flex",
                justifyContent:"space-between",
                alignItems:"center",
                gap:8,
                padding:"3px 0",
              }}>
                <span style={{color:"var(--text-secondary)"}}>
                  <strong>{p.code}</strong> {p.name}
                </span>
                <span style={{
                  fontWeight:700,
                  fontSize:11,
                  padding:"2px 8px",
                  borderRadius:999,
                  background: p.passed ? "rgba(34,197,94,.12)" : "rgba(239,68,68,.12)",
                  color:      p.passed ? "#22c55e" : "#ef4444",
                }}>
                  {p.passed ? "Passed" : "Missing"}
                </span>
              </div>
            ))}
          </div>
        )}

        {/* Action button */}
        <div className={styles.cardFoot}>
          {course.status==="registered" && (
            <motion.button className={styles.dropBtn} onClick={onDrop}
              disabled={busy}
              whileHover={{scale:1.02}} whileTap={{scale:0.97}}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <polyline points="3 6 5 6 21 6"/>
                <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/>
              </svg>
              {busy ? "Dropping..." : "Drop Course"}
            </motion.button>
          )}
          {course.status==="available" && (
            <motion.button
              className={`${styles.regBtn} ${!canReg?styles.regBtnDis:""}`}
              onClick={canReg?onRegister:undefined}
              disabled={!canReg || busy}
              whileHover={canReg&&!busy?{scale:1.02}:{}}
              whileTap={canReg&&!busy?{scale:0.97}:{}}
            >
              {busy ? "Registering..." : canReg ? (
                <>
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                    <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                  </svg>
                  Register
                </>
              ) : "Not Enough Credits"}
            </motion.button>
          )}
          {course.status==="full" && (
            <div className={styles.fullBtn}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"/>
              </svg>
              Section Full
            </div>
          )}
          {course.status==="completed" && (
            <div className={styles.fullBtn} style={{
              background:"rgba(14,165,233,.12)",
              color:"#0ea5e9",
              borderColor:"rgba(14,165,233,.35)",
            }}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4">
                <polyline points="20 6 9 17 4 12"/>
              </svg>
              Course Completed
            </div>
          )}
          {course.status==="locked" && (
            <div className={styles.lockedBtn}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <rect x="3" y="11" width="18" height="11" rx="2"/>
                <path d="M7 11V7a5 5 0 0110 0v4"/>
              </svg>
              Prerequisite Required
            </div>
          )}
          {course.status==="prerequisite" && (
            <div className={styles.lockedBtn} style={{
              background: "rgba(168,85,247,.12)",
              color: "#a855f7",
              borderColor: "rgba(168,85,247,.35)",
            }}>
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M3 3v18h18"/>
                <path d="M7 14l4-4 4 4 5-5"/>
              </svg>
              Prerequisite Not Met
            </div>
          )}
        </div>
      </div>
    </motion.article>
  );
}

/* ═══════════════════════════════════════════════════════════
   CLOSED STATE
═══════════════════════════════════════════════════════════ */
function ClosedState({ regWindow }) {
  return (
    <motion.div className={styles.closed}
      initial={{opacity:0,y:32}} animate={{opacity:1,y:0}}
      transition={{duration:0.55,ease:[0.22,1,0.36,1]}}>

      <div className={styles.closedOrb}>
        <div className={styles.closedLock}>
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
            <rect x="3" y="11" width="18" height="11" rx="2"/>
            <path d="M7 11V7a5 5 0 0110 0v4"/>
          </svg>
        </div>
      </div>

      <div className={styles.closedText}>
        <h2 className={styles.closedTitle}>Registration Closed</h2>
        <p className={styles.closedSub}>
          Course registration is currently unavailable.<br/>
          The administration will open it soon.
        </p>
      </div>

      <div className={styles.closedInfo}>
        <div className={styles.closedInfoItem}>
          <span className={styles.closedInfoLabel}>Academic Year</span>
          <span className={styles.closedInfoVal}>{regWindow?.academicYear || "2025/2026"}</span>
        </div>
        <div className={styles.closedInfoDiv}/>
        <div className={styles.closedInfoItem}>
          <span className={styles.closedInfoLabel}>Semester</span>
          <span className={styles.closedInfoVal}>{regWindow?.semester || "—"}</span>
        </div>
        {regWindow?.startDate && (
          <>
            <div className={styles.closedInfoDiv}/>
            <div className={styles.closedInfoItem}>
              <span className={styles.closedInfoLabel}>Opens On</span>
              <span className={styles.closedInfoVal}>
                {new Date(regWindow.startDate).toLocaleDateString("en-US",{month:"long",day:"numeric",year:"numeric"})}
              </span>
            </div>
          </>
        )}
      </div>
    </motion.div>
  );
}
