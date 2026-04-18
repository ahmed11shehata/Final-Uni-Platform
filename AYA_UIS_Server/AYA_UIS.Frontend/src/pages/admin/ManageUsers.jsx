// src/pages/admin/ManageUsers.jsx
import { useState, useMemo, useRef, useCallback, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  getAdminStudent,
  adminAddCourse,
  adminRemoveCourse,
  adminLockCourse,
  adminUnlockCourse,
  adminSetMaxCredits,
  adminGetAcademicSetup,
  adminSaveAcademicSetup,
  getAdminCourses,
} from "../../services/api/adminApi";
import styles from "./ManageUsers.module.css";

// ── Constants ────────────────────────────────────────────────
const YEAR_LABELS = ["", "1st Year", "2nd Year", "3rd Year", "4th Year"];
const YEAR_COLORS = ["", "#818cf8", "#22c55e", "#f59e0b", "#ef4444"];

const STANDING_META = {
  excellent: { label: "Excellent", color: "#22c55e", bg: "rgba(34,197,94,0.08)", border: "rgba(34,197,94,0.25)", maxCredits: 21 },
  vgood:     { label: "Very Good", color: "#818cf8", bg: "rgba(129,140,248,0.08)", border: "rgba(129,140,248,0.25)", maxCredits: 18 },
  good:      { label: "Good",      color: "#3b82f6", bg: "rgba(59,130,246,0.08)", border: "rgba(59,130,246,0.25)", maxCredits: 18 },
  pass:      { label: "Pass",      color: "#f59e0b", bg: "rgba(245,158,11,0.08)", border: "rgba(245,158,11,0.25)", maxCredits: 15 },
  warning:   { label: "Warning",   color: "#f97316", bg: "rgba(249,115,22,0.08)", border: "rgba(249,115,22,0.25)", maxCredits: 12 },
  probation: { label: "Probation", color: "#ef4444", bg: "rgba(239,68,68,0.08)", border: "rgba(239,68,68,0.25)", maxCredits: 9  },
};

const GRADE_MAP = {
  A_Plus:"A+", A:"A", A_Minus:"A-", B_Plus:"B+", B:"B", B_Minus:"B-",
  C_Plus:"C+", C:"C", C_Minus:"C-", D_Plus:"D+", D:"D", F:"F",
};

function fmtGrade(g) { return GRADE_MAP[g] ?? g ?? "—"; }
function gradeColor(g) {
  const l = fmtGrade(g);
  if (l.startsWith("A")) return "#22c55e";
  if (l.startsWith("B")) return "#84cc16";
  if (l.startsWith("C")) return "#f59e0b";
  if (l.startsWith("D")) return "#fb923c";
  return "#ef4444";
}
function standingMeta(id) { return STANDING_META[id] ?? STANDING_META.pass; }

function computeGradeFromTotal(total) {
  const t = Number(total);
  if (isNaN(t) || t < 60 || t > 100) return null;
  if (t >= 97) return "A+";
  if (t >= 93) return "A";
  if (t >= 90) return "A-";
  if (t >= 87) return "B+";
  if (t >= 83) return "B";
  if (t >= 80) return "B-";
  if (t >= 77) return "C+";
  if (t >= 73) return "C";
  if (t >= 70) return "C-";
  if (t >= 67) return "D+";
  if (t >= 60) return "D";
  return null;
}

function yearNum(yearStr) {
  const m = { "First Year": 1, "Second Year": 2, "Third Year": 3, "Fourth Year": 4 };
  return m[yearStr] ?? 1;
}

// ── SVG Icons ────────────────────────────────────────────────
const SVG = {
  search:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>,
  user:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>,
  book:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M4 19.5A2.5 2.5 0 016.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z"/></svg>,
  grades:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>,
  gear:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 010 2.83 2 2 0 01-2.83 0l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z"/></svg>,
  plus:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>,
  trash:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/></svg>,
  lock:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>,
  unlock:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 019.9-1"/></svg>,
  close:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>,
  warn:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>,
  info:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>,
  check:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>,
  credits: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 8v4l3 3"/></svg>,
};

const fadeUp = { hidden: { opacity: 0, y: 18 }, show: { opacity: 1, y: 0, transition: { duration: 0.38, ease: [0.22, 1, 0.36, 1] } } };

// ── Root Component ───────────────────────────────────────────
export default function ManageUsers() {
  const [searchCode, setSearchCode] = useState("");
  const [studentData, setStudentData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [notFound, setNotFound] = useState(false);
  const [view, setView] = useState(null);
  const [toast, setToast] = useState(null);
  const toastTimer = useRef(null);

  const showToast = useCallback((msg, type = "success") => {
    clearTimeout(toastTimer.current);
    setToast({ msg, type });
    toastTimer.current = setTimeout(() => setToast(null), 3200);
  }, []);

  // refresh by GUID (post-search operations always have the ID)
  const refreshStudent = useCallback(async (id) => {
    try {
      const data = await getAdminStudent(id);
      setStudentData(data);
    } catch (e) {
      showToast(e.message || "Failed to refresh", "error");
    }
  }, [showToast]);

  // search accepts academic code entered by the admin
  const handleSearch = async () => {
    const code = searchCode.trim();
    if (!code) return;
    setLoading(true);
    setNotFound(false);
    setStudentData(null);
    setView(null);
    try {
      const data = await getAdminStudent(code); // backend accepts academic code or GUID
      setStudentData(data);
    } catch (e) {
      if (e.response?.status === 404 || e.message?.toLowerCase().includes("not found")) {
        setNotFound(true);
      } else {
        showToast(e.message || "Search failed", "error");
      }
    } finally {
      setLoading(false);
    }
  };

  const studentId = studentData?.student?.id;

  return (
    <div className={styles.page}>

      {/* Toast */}
      <AnimatePresence>
        {toast && (
          <motion.div
            className={`${styles.toast} ${styles[`toast_${toast.type}`]}`}
            initial={{ opacity: 0, y: -20, x: "-50%", scale: 0.92 }}
            animate={{ opacity: 1, y: 0, x: "-50%", scale: 1 }}
            exit={{ opacity: 0, x: "-50%" }}
            transition={{ type: "spring", stiffness: 440, damping: 28 }}
          >
            <span className={styles.toastDot} />{toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      {/* Search — clearly by academic code */}
      <motion.div className={styles.searchWrap} initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}>
        <div className={styles.searchBox}>
          <span className={styles.searchIcon}>{SVG.search}</span>
          <input
            className={styles.searchIn}
            placeholder="Academic Code  (e.g. CS2024001)"
            value={searchCode}
            onChange={e => setSearchCode(e.target.value)}
            onKeyDown={e => e.key === "Enter" && handleSearch()}
          />
          {searchCode && (
            <button className={styles.clearBtn} onClick={() => { setSearchCode(""); setStudentData(null); setNotFound(false); setView(null); }}>
              {SVG.close}
            </button>
          )}
        </div>
        <motion.button
          className={styles.searchBtn}
          onClick={handleSearch}
          disabled={loading}
          whileHover={{ scale: 1.02 }}
          whileTap={{ scale: 0.97 }}
        >
          {loading ? "…" : "Search"}
        </motion.button>
      </motion.div>
      <p className={styles.searchHint}>Enter the student's <strong>academic code</strong> to manage their profile and courses</p>

      {/* Not found */}
      <AnimatePresence>
        {notFound && !studentData && (
          <motion.div className={styles.notFound} initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}>
            {SVG.warn}
            <span>No student found with academic code <strong>"{searchCode}"</strong></span>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Empty state */}
      {!studentData && !notFound && (
        <motion.div className={styles.emptyState} initial={{ opacity: 0 }} animate={{ opacity: 1, transition: { delay: 0.2 } }}>
          <motion.div className={styles.emptyOrb} animate={{ y: [0, -10, 0] }} transition={{ duration: 4, repeat: Infinity, ease: "easeInOut" }}>
            {SVG.user}
          </motion.div>
          <p className={styles.emptyTitle}>Search by Academic Code</p>
          <p className={styles.emptySub}>Enter the student's academic code (e.g. CS2024001) to view their profile, manage enrolled courses, and adjust academic settings.</p>
        </motion.div>
      )}

      {/* Student loaded */}
      <AnimatePresence>
        {studentData && (
          <motion.div key={studentData.student.id} className={styles.studentWrap}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>

            {/* Profile card */}
            <StudentProfile data={studentData} />

            {/* Navigation cards */}
            {view === null && (
              <motion.div className={styles.navCards}
                initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.1 }}>
                <NavCard
                  icon={SVG.book} color="#818cf8"
                  title="Registered Courses"
                  desc="Add, drop, lock and unlock course access"
                  badge={studentData.registeredCourses?.length ?? 0}
                  badgeLabel="enrolled"
                  onClick={() => setView("courses")}
                />
                <NavCard
                  icon={SVG.grades} color="#22c55e"
                  title="Academic Transcript"
                  desc="Complete grade history by year and semester"
                  badge={studentData.student.gpa?.toFixed(2) ?? "N/A"}
                  badgeLabel="GPA"
                  onClick={() => setView("grades")}
                />
                <NavCard
                  icon={SVG.gear} color="#f59e0b"
                  title="Academic Controls"
                  desc="Standing, credit override and grade history setup"
                  badge={studentData.standing?.maxCredits ?? "—"}
                  badgeLabel="max credits"
                  onClick={() => setView("controls")}
                />
              </motion.div>
            )}

            {/* Back button */}
            {view && (
              <motion.button className={styles.backBtn} onClick={() => setView(null)}
                initial={{ opacity: 0, x: -10 }} animate={{ opacity: 1, x: 0 }}>
                ← Back
              </motion.button>
            )}

            {/* Panel views */}
            <AnimatePresence mode="wait">
              {view === "courses" && (
                <motion.div key="courses"
                  initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -10 }}>
                  <CoursesPanel data={studentData} studentId={studentId} refresh={() => refreshStudent(studentId)} toast={showToast} />
                </motion.div>
              )}
              {view === "grades" && (
                <motion.div key="grades"
                  initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -10 }}>
                  <GradesPanel data={studentData} studentId={studentId} />
                </motion.div>
              )}
              {view === "controls" && (
                <motion.div key="controls"
                  initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -10 }}>
                  <AcademicControlsPanel data={studentData} studentId={studentId} refresh={() => refreshStudent(studentId)} toast={showToast} />
                </motion.div>
              )}
            </AnimatePresence>

          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

// ── Student Profile ──────────────────────────────────────────
function StudentProfile({ data }) {
  const { student, standing, registeredCourses, completedCourses } = data;
  const st = standingMeta(standing?.standingId);
  const yr = yearNum(student.year);
  const init = (student.name || "??").split(" ").map(n => n[0]).join("").slice(0, 2).toUpperCase();
  const passed = completedCourses?.filter(c => c.grade !== "F")?.length ?? 0;
  const reg = registeredCourses?.length ?? 0;

  return (
    <motion.div className={styles.profile} variants={fadeUp} initial="hidden" animate="show">
      <div className={styles.profileAccentBar}
        style={{ background: `linear-gradient(90deg,${st.color}50,${YEAR_COLORS[yr] || "#818cf8"}30,transparent)` }} />
      <div className={styles.profileMain}>
        {/* Avatar */}
        <motion.div className={styles.avatar}
          style={{ background: `linear-gradient(135deg,${st.color}cc,${st.color}66)` }}
          initial={{ scale: 0.6, opacity: 0 }} animate={{ scale: 1, opacity: 1 }}
          transition={{ type: "spring", stiffness: 300, damping: 20, delay: 0.08 }}>
          <span className={styles.avatarLetters}>{init}</span>
          <motion.div className={styles.avatarRing} style={{ borderColor: `${st.color}50` }}
            animate={{ scale: [1, 1.18, 1], opacity: [0.5, 0.1, 0.5] }}
            transition={{ duration: 2.6, repeat: Infinity, ease: "easeInOut" }} />
        </motion.div>

        {/* Info */}
        <div className={styles.profileInfo}>
          <h2 className={styles.profileName}>{student.name}</h2>
          <div className={styles.profileMeta}>
            <span className={styles.profileId}>{student.academicCode}</span>
            <span className={styles.sep}>·</span>
            <span style={{ color: YEAR_COLORS[yr] || "#818cf8", fontWeight: 700 }}>{student.year}</span>
            {student.email && <><span className={styles.sep}>·</span><span>{student.email}</span></>}
          </div>
          <div className={styles.standingPill}
            style={{ color: st.color, background: st.bg, borderColor: st.border }}>
            <span className={styles.standingDot} style={{ background: st.color }} />
            {st.label} — {standing?.maxCredits ?? st.maxCredits} hrs/semester
            {standing?.mustRetakeFirst && (
              <span className={styles.standingWarnTag}> ⚠ Must retake failed first</span>
            )}
          </div>
        </div>

        {/* Stats */}
        <div className={styles.profileStats}>
          {[
            { v: student.gpa?.toFixed(2) ?? "—", l: "Cumulative GPA",   c: st.color     },
            { v: student.totalCreditsEarned ?? 0,  l: "Credits Earned",   c: "#818cf8"    },
            { v: reg,                               l: "This Semester",    c: "#22c55e"    },
            { v: passed,                            l: "Passed",           c: "#14b8a6"    },
          ].map((s, i) => (
            <motion.div key={s.l} className={styles.statBox}
              initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.12 + i * 0.06 }}>
              <span className={styles.statVal} style={{ color: s.c }}>{s.v}</span>
              <span className={styles.statLbl}>{s.l}</span>
            </motion.div>
          ))}
        </div>
      </div>
    </motion.div>
  );
}

// ── Nav Card ─────────────────────────────────────────────────
function NavCard({ icon, color, title, desc, badge, badgeLabel, onClick }) {
  return (
    <motion.button className={styles.navCard} style={{ "--nc": color }} onClick={onClick}
      whileHover={{ y: -6, boxShadow: "0 20px 48px rgba(0,0,0,0.14)" }} whileTap={{ scale: 0.98 }}>
      <div className={styles.navCardIcon} style={{ background: `${color}14`, color }}>{icon}</div>
      <div className={styles.navCardBody}>
        <div className={styles.navCardTitle}>{title}</div>
        <div className={styles.navCardDesc}>{desc}</div>
      </div>
      <div className={styles.navCardBadge} style={{ color, background: `${color}12`, borderColor: `${color}25` }}>
        <span className={styles.navBadgeNum}>{badge}</span>
        <span className={styles.navBadgeLbl}>{badgeLabel}</span>
      </div>
      <span className={styles.navCardArrow} style={{ color }}>→</span>
    </motion.button>
  );
}

// ── Courses Panel ────────────────────────────────────────────
function CoursesPanel({ data, studentId, refresh, toast }) {
  const { student, standing, registeredCourses } = data;
  const st = standingMeta(standing?.standingId);
  const max = standing?.maxCredits ?? st.maxCredits;
  const used = (registeredCourses ?? []).reduce((s, c) => s + (c.credits ?? 3), 0);
  const pct = Math.min(100, Math.round((used / max) * 100));

  const [addOpen,       setAddOpen]       = useState(false);
  const [addSearch,     setAddSearch]     = useState("");
  const [allCourses,    setAllCourses]    = useState([]);
  const [loadingCrs,    setLoadingCrs]    = useState(false);
  const [lockTarget,    setLockTarget]    = useState(null);
  const [lockReason,    setLockReason]    = useState("");
  const [busy,          setBusy]          = useState(false);

  const openAddModal = async () => {
    setAddOpen(true);
    if (allCourses.length === 0) {
      setLoadingCrs(true);
      try {
        const courses = await getAdminCourses();
        setAllCourses(courses ?? []);
      } catch {
        toast("Failed to load courses", "error");
      } finally {
        setLoadingCrs(false);
      }
    }
  };

  const registeredCodes = useMemo(
    () => new Set((registeredCourses ?? []).map(c => c.code)),
    [registeredCourses]
  );

  const addable = useMemo(() => {
    let list = allCourses.filter(c => !registeredCodes.has(c.code));
    if (addSearch.trim()) {
      const q = addSearch.toLowerCase();
      list = list.filter(c => c.name?.toLowerCase().includes(q) || c.code?.toLowerCase().includes(q));
    }
    return list.slice(0, 30);
  }, [allCourses, registeredCodes, addSearch]);

  const doAdd = async (courseCode) => {
    setBusy(true);
    try {
      await adminAddCourse(studentId, courseCode);
      await refresh();
      toast(`${courseCode} added`, "success");
      setAddOpen(false);
    } catch (e) {
      toast(e.message || "Failed to add", "error");
    } finally { setBusy(false); }
  };

  const doDrop = async (code) => {
    setBusy(true);
    try {
      await adminRemoveCourse(studentId, code);
      await refresh();
      toast(`${code} dropped`, "info");
    } catch (e) {
      toast(e.message || "Failed to drop", "error");
    } finally { setBusy(false); }
  };

  const doLock = async () => {
    if (!lockTarget) return;
    setBusy(true);
    try {
      await adminLockCourse(studentId, lockTarget.code, lockReason);
      await refresh();
      toast(`${lockTarget.code} locked`, "info");
      setLockTarget(null);
      setLockReason("");
    } catch (e) {
      toast(e.message || "Failed to lock", "error");
    } finally { setBusy(false); }
  };

  const doUnlock = async (code) => {
    setBusy(true);
    try {
      await adminUnlockCourse(studentId, code);
      await refresh();
      toast(`${code} unlocked`, "success");
    } catch (e) {
      toast(e.message || "Failed to unlock", "error");
    } finally { setBusy(false); }
  };

  return (
    <div className={styles.coursesPanel}>

      {/* Lock-reason mini modal */}
      <AnimatePresence>
        {lockTarget && (
          <motion.div className={styles.overlay}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={() => setLockTarget(null)}>
            <motion.div className={styles.miniModal}
              initial={{ scale: 0.88, y: 24, opacity: 0 }} animate={{ scale: 1, y: 0, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }} transition={{ type: "spring", stiffness: 360, damping: 26 }}
              onClick={e => e.stopPropagation()}>
              <div className={styles.miniModalHead}>
                <span className={styles.miniModalTitle}>Lock {lockTarget.code}</span>
                <button className={styles.addModalClose} onClick={() => setLockTarget(null)}>{SVG.close}</button>
              </div>
              <p className={styles.miniModalSub}>Optionally provide a reason for locking this course.</p>
              <input className={styles.miniModalInput} placeholder="Reason (optional)"
                value={lockReason} onChange={e => setLockReason(e.target.value)} />
              <div className={styles.miniModalActions}>
                <button className={styles.cancelBtn} onClick={() => setLockTarget(null)}>Cancel</button>
                <button className={styles.lockBtn} onClick={doLock} disabled={busy}>{SVG.lock} Lock</button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Add-course modal */}
      <AnimatePresence>
        {addOpen && (
          <motion.div className={styles.overlay}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={() => setAddOpen(false)}>
            <motion.div className={styles.addModal}
              initial={{ scale: 0.88, y: 24, opacity: 0 }} animate={{ scale: 1, y: 0, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }} transition={{ type: "spring", stiffness: 360, damping: 26 }}
              onClick={e => e.stopPropagation()}>
              <div className={styles.addModalHead}>
                <div>
                  <h3 className={styles.addModalTitle}>Add Course</h3>
                  <p className={styles.addModalSub}>Force-enroll {student.name} in a course</p>
                </div>
                <button className={styles.addModalClose} onClick={() => setAddOpen(false)}>{SVG.close}</button>
              </div>
              <div className={styles.addModalSearch}>
                {SVG.search}
                <input className={styles.addModalIn} placeholder="Search by name or code…"
                  autoFocus value={addSearch} onChange={e => setAddSearch(e.target.value)} />
              </div>
              <div className={styles.addModalList}>
                {loadingCrs && <div className={styles.addEmpty}>{SVG.info} Loading courses…</div>}
                {!loadingCrs && addable.map(c => (
                  <motion.div key={c.code} className={styles.addModalRow}
                    whileHover={{ background: "var(--hover-bg)" }}>
                    <div className={styles.addRowLeft}>
                      <span className={styles.addRowCode} style={{ color: "var(--accent)" }}>{c.code}</span>
                      <div>
                        <div className={styles.addRowName}>{c.name}</div>
                        <div className={styles.addRowMeta}>
                          {c.credits} cr{c.year ? ` · Year ${c.year}` : ""}{c.semester ? ` · Sem ${c.semester}` : ""}
                        </div>
                      </div>
                    </div>
                    <motion.button className={styles.addRowBtn}
                      onClick={() => doAdd(c.code)} disabled={busy}
                      whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.95 }}>
                      {SVG.plus} Add
                    </motion.button>
                  </motion.div>
                ))}
                {!loadingCrs && addable.length === 0 && (
                  <div className={styles.addEmpty}>{SVG.info} No courses match</div>
                )}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── Credit bar ── */}
      <div className={styles.credBar}>
        <div className={styles.credBarRow}>
          <div>
            <span className={styles.credUsed}
              style={{ color: pct >= 90 ? "#ef4444" : pct >= 70 ? "#f59e0b" : "var(--accent)" }}>
              {used}
            </span>
            <span className={styles.credMax}>/ {max} credit hours</span>
          </div>
          <div className={styles.credBarRight}>
            <span className={styles.credStanding} style={{ color: st.color }}>{st.label}</span>
            <motion.button className={styles.addBtn} onClick={openAddModal}
              whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.97 }}>
              {SVG.plus} Add Course
            </motion.button>
          </div>
        </div>
        <div className={styles.credTrack}>
          <motion.div className={styles.credFill}
            initial={{ width: 0 }} animate={{ width: `${pct}%` }}
            transition={{ duration: 1.1, ease: [0.22, 1, 0.36, 1] }}
            style={{ background: pct >= 90 ? "#ef4444" : pct >= 70 ? "#f59e0b" : "var(--accent)" }} />
        </div>
        <div className={styles.credNote}>{max - used} hrs remaining</div>
      </div>

      {/* ── Course cards ── */}
      {!(registeredCourses?.length) ? (
        <div className={styles.noCoursesMsg}>
          {SVG.info}<span>No courses registered. Use <strong>Add Course</strong> above.</span>
        </div>
      ) : (
        <div className={styles.courseGrid}>
          {(registeredCourses ?? []).map((c, i) => {
            const isLocked = c.status?.toLowerCase() === "locked";
            const statusColor  = isLocked ? "#f59e0b" : "#22c55e";
            const headerBg     = isLocked ? "rgba(245,158,11,0.11)" : "rgba(129,140,248,0.11)";
            const codeColor    = isLocked ? "#f59e0b" : "var(--accent)";
            return (
              <motion.div
                key={c.code}
                className={`${styles.courseCard} ${isLocked ? styles.courseCardLocked : ""}`}
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.04, duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
                whileHover={{ y: -4, transition: { duration: 0.18 } }}
              >
                {/* Header */}
                <div className={styles.courseCardHeader} style={{ background: headerBg }}>
                  <span className={styles.courseCardCode} style={{ color: codeColor }}>{c.code}</span>
                  <span className={styles.courseCardCr}>{c.credits ?? 3} cr</span>
                </div>

                {/* Body */}
                <div className={styles.courseCardBody}>
                  <div className={styles.courseCardName}>{c.name || "—"}</div>
                  <span
                    className={styles.courseCardStatus}
                    style={{ color: statusColor, background: `${statusColor}18` }}
                  >
                    <span className={styles.courseCardStatusDot} style={{ background: statusColor }} />
                    {isLocked ? "Locked" : "Enrolled"}
                  </span>
                  {isLocked && c.lockReason && (
                    <div className={styles.courseCardLockReason}>{c.lockReason}</div>
                  )}
                </div>

                {/* Actions */}
                <div className={styles.courseCardFoot}>
                  {isLocked ? (
                    <button
                      className={`${styles.cardActionBtn} ${styles.cardActionUnlock}`}
                      onClick={() => doUnlock(c.code)}
                      disabled={busy}
                    >
                      {SVG.unlock} Unlock
                    </button>
                  ) : (
                    <button
                      className={`${styles.cardActionBtn} ${styles.cardActionLock}`}
                      onClick={() => { setLockTarget(c); setLockReason(""); }}
                      disabled={busy}
                    >
                      {SVG.lock} Lock
                    </button>
                  )}
                  <button
                    className={`${styles.cardActionBtn} ${styles.cardActionDrop}`}
                    onClick={() => doDrop(c.code)}
                    disabled={busy}
                  >
                    {SVG.trash} Drop
                  </button>
                </div>
              </motion.div>
            );
          })}
        </div>
      )}
    </div>
  );
}

// ── Grades Panel ─────────────────────────────────────────────
function GradesPanel({ data, studentId }) {
  const { student } = data;
  const [openYear, setOpenYear] = useState(null);
  const [setup,    setSetup]    = useState(null);
  const [loading,  setLoading]  = useState(true);

  useEffect(() => {
    let cancelled = false;
    adminGetAcademicSetup(studentId)
      .then(d => { if (!cancelled) setSetup(d); })
      .catch(() => {})
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [studentId]);

  // Build a map: { year: { semester: [completedCourses] } }
  const byYear = useMemo(() => {
    if (!setup) return {};
    const y = {};
    const years = setup.academicSetup?.years ?? {};
    for (const [yrKey, yrData] of Object.entries(years)) {
      const yr = Number(yrKey);
      const semesters = yrData?.semesters ?? {};
      for (const [smKey, smCourses] of Object.entries(semesters)) {
        const sm = Number(smKey);
        const completed = (smCourses ?? []).filter(c => c.selected);
        if (completed.length === 0) continue;
        if (!y[yr]) y[yr] = {};
        if (!y[yr][sm]) y[yr][sm] = [];
        y[yr][sm].push(...completed);
      }
    }
    return y;
  }, [setup]);

  function semGPA(cs) {
    const pts = cs.reduce((s, c) => s + (c.gpaPoints ?? 0) * (c.credits ?? 3), 0);
    const h   = cs.reduce((s, c) => s + (c.credits ?? 3), 0);
    return h > 0 ? (pts / h).toFixed(2) : "—";
  }

  if (loading) return <div className={styles.gradesEmpty}>{SVG.info} Loading transcript…</div>;

  const years = Object.keys(byYear).map(Number).sort();
  if (!years.length) return <div className={styles.gradesEmpty}>{SVG.info} No completed courses found</div>;

  const st = standingMeta(data.standing?.standingId);

  return (
    <div className={styles.gradesPanel}>

      {/* GPA summary card */}
      <div className={styles.gpaCard}>
        <div className={styles.gpaCardLeft}>
          <div className={styles.gpaCardVal} style={{ color: st.color }}>
            {student.gpa?.toFixed(2) ?? "N/A"}
          </div>
          <div className={styles.gpaCardLbl}>Cumulative GPA</div>
        </div>
        <div className={styles.gpaCardYears}>
          {years.map(yr => {
            const all = Object.values(byYear[yr]).flat();
            return (
              <div key={yr} className={styles.gpaYearItem}>
                <span className={styles.gpaYearVal} style={{ color: YEAR_COLORS[yr] }}>{semGPA(all)}</span>
                <span className={styles.gpaYearLbl}>Year {yr}</span>
              </div>
            );
          })}
        </div>
      </div>

      {/* Year accordion blocks */}
      {years.map(yr => {
        const yd   = byYear[yr];
        const sems = Object.keys(yd).map(Number).sort();
        const all  = sems.flatMap(s => yd[s]);
        const isOpen = openYear === yr;

        return (
          <motion.div key={yr} className={styles.yearBlock}
            initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: yr * 0.06 }}>

            <button
              className={`${styles.yearHead} ${isOpen ? styles.yearHeadOpen : ""}`}
              style={{ "--yc": YEAR_COLORS[yr] }}
              onClick={() => setOpenYear(isOpen ? null : yr)}>
              <div className={styles.yearHeadL}>
                <span className={styles.yearDot} style={{ background: YEAR_COLORS[yr] }} />
                <span className={styles.yearLabel}>{YEAR_LABELS[yr]}</span>
                <span className={styles.yearCount}>
                  {all.length} courses · {all.reduce((s, c) => s + (c.credits ?? 3), 0)} hrs
                </span>
              </div>
              <div className={styles.yearHeadR}>
                <span className={styles.yearGPA} style={{ color: YEAR_COLORS[yr] }}>GPA {semGPA(all)}</span>
                <span className={`${styles.yearChev} ${isOpen ? styles.yearChevOpen : ""}`}>›</span>
              </div>
            </button>

            <AnimatePresence>
              {isOpen && (
                <motion.div className={styles.semsWrap}
                  initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: "auto" }}
                  exit={{ opacity: 0, height: 0 }}>
                  {sems.map(sm => {
                    const cs = yd[sm];
                    return (
                      <div key={sm} className={styles.semBlock}>
                        <div className={styles.semHead}>
                          <span className={styles.semLabel}>Semester {sm}</span>
                          <span className={styles.semMeta}>
                            {cs.reduce((s, c) => s + (c.credits ?? 3), 0)} hrs · GPA {semGPA(cs)}
                          </span>
                        </div>
                        <div className={styles.gradeTable}>
                          <div className={styles.gradeHead}>
                            <span>Code</span><span>Course</span><span>Hrs</span><span>Grade</span>
                          </div>
                          {cs.map(c => {
                            const gl = fmtGrade(c.grade);
                            const gc = gradeColor(c.grade);
                            return (
                              <motion.div key={`${c.courseCode}-${sm}`}
                                className={`${styles.gradeRow} ${gl === "F" ? styles.gradeRowFail : ""}`}
                                initial={{ opacity: 0, x: -8 }} animate={{ opacity: 1, x: 0 }}>
                                <span className={styles.gradeCode} style={{ color: "var(--accent)" }}>{c.courseCode}</span>
                                <span className={styles.gradeName}>{c.name}</span>
                                <span className={styles.gradeHrs}>{c.credits ?? 3}</span>
                                <span className={styles.gradeGrade}
                                  style={{ color: gc, background: `${gc}14`, borderColor: `${gc}28` }}>
                                  {gl}
                                </span>
                              </motion.div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })}
                </motion.div>
              )}
            </AnimatePresence>
          </motion.div>
        );
      })}
    </div>
  );
}

// ── Academic Controls Panel ──────────────────────────────────

function AcademicControlsPanel({ data, studentId, refresh, toast }) {
  const { student, standing } = data;
  const st = standingMeta(standing?.standingId);

  const [maxCredInput, setMaxCredInput] = useState(String(standing?.maxCredits ?? st.maxCredits));
  const [maxBusy, setMaxBusy] = useState(false);

  const [setupEntries, setSetupEntries] = useState(null); // { [code]: entry }
  const [setupCurYear, setSetupCurYear] = useState(null);
  const [setupLoading, setSetupLoading] = useState(false);
  const [setupOpen, setSetupOpen] = useState(false);
  const [saveBusy, setSaveBusy] = useState(false);
  const [allCourses, setAllCourses] = useState([]);
  const [courseQuery, setCourseQuery] = useState("");

  const handleSetMaxCredits = async () => {
    const val = parseInt(maxCredInput, 10);
    if (isNaN(val) || val < 1 || val > 30) {
      toast("Enter a value between 1 and 30", "error");
      return;
    }
    setMaxBusy(true);
    try {
      await adminSetMaxCredits(studentId, val);
      await refresh();
      toast(`Max credits set to ${val}`, "success");
    } catch (e) {
      toast(e.message || "Failed to update", "error");
    } finally {
      setMaxBusy(false);
    }
  };

  const buildEntriesMap = useCallback((setupData, catalog) => {
    const nameByCode = new Map((catalog ?? []).map(c => [String(c.code).toUpperCase(), c]));
    const mapped = {};
    const yearsMap = setupData?.academicSetup?.years ?? {};

    for (const [yrKey, yrData] of Object.entries(yearsMap)) {
      const yr = Number(yrKey);
      for (const [smKey, smCourses] of Object.entries(yrData?.semesters ?? {})) {
        const sm = Number(smKey);
        for (const c of (smCourses ?? [])) {
          if (!c.selected || c.total == null) continue;
          const code = String(c.courseCode).toUpperCase();
          const fromCatalog = nameByCode.get(code);
          mapped[code] = {
            courseCode: code,
            name: c.name ?? fromCatalog?.name ?? code,
            credits: c.credits ?? fromCatalog?.credits ?? 3,
            year: yr,
            semester: sm,
            total: String(c.total ?? ""),
          };
        }
      }
    }
    return mapped;
  }, []);

  const openSetup = async () => {
    setSetupOpen(true);
    if (setupEntries !== null) return;

    setSetupLoading(true);
    try {
      const [setupData, courses] = await Promise.all([
        adminGetAcademicSetup(studentId),
        getAdminCourses(),
      ]);
      const sortedCourses = [...(courses ?? [])].sort((a, b) =>
        String(a.code).localeCompare(String(b.code))
      );
      setAllCourses(sortedCourses);
      setSetupCurYear(setupData.academicSetup?.currentYear ?? yearNum(student.year));
      setSetupEntries(buildEntriesMap(setupData, sortedCourses));
    } catch (e) {
      toast(e.message || "Failed to load academic setup", "error");
      setSetupOpen(false);
    } finally {
      setSetupLoading(false);
    }
  };

  const toggleCourse = (course) => {
    const code = String(course.code).toUpperCase();
    setSetupEntries(prev => {
      const next = { ...(prev ?? {}) };
      if (next[code]) {
        delete next[code];
      } else {
        next[code] = {
          courseCode: code,
          name: course.name ?? code,
          credits: course.credits ?? 3,
          year: setupCurYear ?? yearNum(student.year),
          semester: 1,
          total: "60",
        };
      }
      return next;
    });
  };

  const updateEntry = (code, field, value) => {
    setSetupEntries(prev => ({
      ...(prev ?? {}),
      [code]: {
        ...(prev?.[code] ?? {}),
        [field]: value,
      },
    }));
  };

  const filteredCourses = useMemo(() => {
    const q = courseQuery.trim().toLowerCase();
    const list = [...(allCourses ?? [])];
    if (!q) return list;
    return list.filter(c =>
      String(c.code ?? "").toLowerCase().includes(q) ||
      String(c.name ?? "").toLowerCase().includes(q)
    );
  }, [allCourses, courseQuery]);

  const activeCount = useMemo(() => Object.keys(setupEntries ?? {}).length, [setupEntries]);

  const handleSaveSetup = async () => {
    if (setupEntries == null) return;

    const entries = Object.values(setupEntries);
    const invalid = entries.find((e) => {
      const total = parseInt(e.total, 10);
      return !e.courseCode || isNaN(total) || total < 60 || total > 100 || !e.year || !e.semester;
    });

    if (invalid) {
      toast(`Please complete ${invalid.courseCode} with a valid total (60–100), year, and semester`, "error");
      return;
    }

    const yearsPayload = {
      "1": { completedCourses: [] },
      "2": { completedCourses: [] },
      "3": { completedCourses: [] },
      "4": { completedCourses: [] },
    };

    for (const entry of entries) {
      const yr = String(Math.min(4, Math.max(1, Number(entry.year))));
      yearsPayload[yr].completedCourses.push({
        courseCode: entry.courseCode,
        total: parseInt(entry.total, 10),
        semester: Number(entry.semester),
      });
    }

    const payload = {
      currentYear: setupCurYear ?? yearNum(student.year),
      years: yearsPayload,
    };

    setSaveBusy(true);
    try {
      await adminSaveAcademicSetup(studentId, payload);
      await refresh();
      const fresh = await adminGetAcademicSetup(studentId);
      setSetupEntries(buildEntriesMap(fresh, allCourses));
      toast("Academic history saved successfully", "success");
    } catch (e) {
      toast(e.message || "Failed to save", "error");
    } finally {
      setSaveBusy(false);
    }
  };

  return (
    <div className={styles.controlsPanel}>
      <div className={styles.controlsTopGrid}>
        <div className={`${styles.controlCard} ${styles.standingCard}`}>
          <div className={styles.controlCardHead}>
            <span className={styles.controlCardTitle}>Academic Standing</span>
            <span className={styles.controlCardSub}>Current status, GPA and registration constraints</span>
          </div>

          <div className={styles.standingHero}>
            <div
              className={styles.standingHeroBadge}
              style={{ color: st.color, background: st.bg, borderColor: st.border }}
            >
              <span className={styles.standingHeroDot} style={{ background: st.color }} />
              {st.label}
            </div>

            <div className={styles.standingHeroStats}>
              <div className={styles.standingHeroStat}>
                <span className={styles.standingHeroVal} style={{ color: st.color }}>
                  {student.gpa?.toFixed(2) ?? "—"}
                </span>
                <span className={styles.standingHeroLbl}>GPA</span>
              </div>
              <div className={styles.standingHeroStat}>
                <span className={styles.standingHeroVal}>{standing?.maxCredits ?? st.maxCredits}</span>
                <span className={styles.standingHeroLbl}>Current Max Credits</span>
              </div>
            </div>
          </div>

          <div className={styles.standingGrid}>
            <div className={styles.standingMiniCard}>
              <span className={styles.standingMiniLabel}>Default Max Credits</span>
              <span className={styles.standingMiniValue}>{st.maxCredits}</span>
            </div>
            <div className={styles.standingMiniCard}>
              <span className={styles.standingMiniLabel}>Current Policy</span>
              <span className={styles.standingMiniValue}>{st.label}</span>
            </div>
          </div>

          <div className={styles.standingAlerts}>
            {standing?.mustRetakeFirst && (
              <div className={styles.standingAlertWarn}>Must retake failed courses first</div>
            )}
            {standing?.canOnlyRetake && (
              <div className={styles.standingAlertDanger}>Can only register retake courses</div>
            )}
            {!standing?.mustRetakeFirst && !standing?.canOnlyRetake && (
              <div className={styles.standingAlertInfo}>Registration rules look normal for this student.</div>
            )}
          </div>
        </div>

        <div className={`${styles.controlCard} ${styles.overrideCard}`}>
          <div className={styles.controlCardHead}>
            <span className={styles.controlCardTitle}>Override Max Credits</span>
            <span className={styles.controlCardSub}>Adjust this semester’s registration ceiling without changing the core standing logic</span>
          </div>

          <div className={styles.overrideHero}>
            <div className={styles.overrideInputWrap}>
              <input
                className={styles.overrideInput}
                type="number"
                min={1}
                max={30}
                value={maxCredInput}
                onChange={(e) => setMaxCredInput(e.target.value)}
              />
              <div className={styles.overrideMeta}>
                <span className={styles.overrideMetaTop}>Credit hours / semester</span>
                <span className={styles.overrideMetaSub}>Recommended default for {st.label}: {st.maxCredits}</span>
              </div>
            </div>

            <motion.button
              className={styles.overrideApplyBtn}
              onClick={handleSetMaxCredits}
              disabled={maxBusy}
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.97 }}
            >
              {maxBusy ? "Applying…" : "Apply"}
            </motion.button>
          </div>

          <div className={styles.overrideFootNote}>
            This override persists until the student’s academic standing is recalculated again.
          </div>
        </div>
      </div>

      <div className={`${styles.controlCard} ${styles.historyCard}`}>
        <div className={styles.historyHeader}>
          <div className={styles.historyHeaderText}>
            <span className={styles.controlCardTitle}>Academic History Setup</span>
            <span className={styles.controlCardSub}>
              Activate the completed courses, assign their academic year and semester, then enter the final numeric total.
            </span>
          </div>

          <div className={styles.historyHeaderActions}>
            {!setupOpen ? (
              <motion.button
                className={styles.openSetupBtn}
                onClick={openSetup}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.97 }}
              >
                Open History Builder
              </motion.button>
            ) : (
              <motion.button
                className={styles.saveSetupBtnTop}
                onClick={handleSaveSetup}
                disabled={saveBusy || setupLoading}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.97 }}
              >
                {SVG.check} {saveBusy ? "Saving…" : "Save Changes"}
              </motion.button>
            )}
          </div>
        </div>

        <AnimatePresence>
          {setupOpen && (
            <motion.div
              className={styles.setupWorkspace}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -8 }}
            >
              {setupLoading && (
                <div className={styles.setupLoading}>{SVG.info} Loading academic history…</div>
              )}

              {!setupLoading && setupEntries !== null && (
                <>
                  <div className={styles.setupTopBar}>
                    <div className={styles.setupSearchBox}>
                      <span className={styles.setupSearchIcon}>{SVG.search}</span>
                      <input
                        className={styles.setupSearchInput}
                        placeholder="Search courses by code or name…"
                        value={courseQuery}
                        onChange={(e) => setCourseQuery(e.target.value)}
                      />
                    </div>

                    <div className={styles.currentYearChooser}>
                      <span className={styles.currentYearLabel}>Student current year</span>
                      <div className={styles.currentYearPills}>
                        {[1, 2, 3, 4].map((year) => (
                          <button
                            key={year}
                            type="button"
                            className={`${styles.currentYearPill} ${(setupCurYear ?? yearNum(student.year)) === year ? styles.currentYearPillActive : ""}`}
                            onClick={() => setSetupCurYear(year)}
                          >
                            Year {year}
                          </button>
                        ))}
                      </div>
                    </div>
                  </div>

                  <div className={styles.setupStatusBar}>
                    <div className={styles.setupStatusItem}>
                      <span className={styles.setupStatusValue}>{activeCount}</span>
                      <span className={styles.setupStatusLabel}>Activated courses</span>
                    </div>
                    <div className={styles.setupStatusItem}>
                      <span className={styles.setupStatusValue}>{filteredCourses.length}</span>
                      <span className={styles.setupStatusLabel}>Visible after filter</span>
                    </div>
                  </div>

                  <div className={styles.catalogGrid}>
                    {filteredCourses.map((course, idx) => {
                      const code = String(course.code).toUpperCase();
                      const entry = setupEntries[code];
                      const active = !!entry;
                      const grade = active ? computeGradeFromTotal(entry.total) : null;
                      const gradeClr = grade ? gradeColor(grade) : "var(--text-muted)";

                      return (
                        <motion.div
                          key={code}
                          className={`${styles.historyCourseCard} ${active ? styles.historyCourseCardActive : styles.historyCourseCardMuted}`}
                          initial={{ opacity: 0, y: 12 }}
                          animate={{ opacity: 1, y: 0 }}
                          transition={{ delay: idx * 0.015, duration: 0.24 }}
                          whileHover={{ y: -4 }}
                        >
                          <div className={styles.historyCourseHead}>
                            <div className={styles.historyCourseHeadMain}>
                              <span className={styles.historyCourseCode}>{code}</span>
                              <span className={styles.historyCourseCredits}>{course.credits ?? 3} credit hours</span>
                            </div>
                            <button
                              type="button"
                              className={`${styles.activateCourseBtn} ${active ? styles.activateCourseBtnActive : ""}`}
                              onClick={() => toggleCourse(course)}
                            >
                              {active ? "Activated" : "Activate"}
                            </button>
                          </div>

                          <div className={styles.historyCourseName}>{course.name}</div>

                          <div className={styles.historyCardBody}>
                            <div className={styles.choiceBlock}>
                              <div className={styles.choiceLabel}>Study Year</div>
                              <div className={styles.choicePills}>
                                {[1, 2, 3, 4].map((year) => (
                                  <button
                                    key={year}
                                    type="button"
                                    disabled={!active}
                                    className={`${styles.choicePill} ${active && Number(entry?.year) === year ? styles.choicePillActive : ""}`}
                                    onClick={() => updateEntry(code, "year", year)}
                                  >
                                    Year {year}
                                  </button>
                                ))}
                              </div>
                            </div>

                            <div className={styles.choiceBlock}>
                              <div className={styles.choiceLabel}>Semester</div>
                              <div className={styles.choicePills}>
                                {[1, 2].map((sem) => (
                                  <button
                                    key={sem}
                                    type="button"
                                    disabled={!active}
                                    className={`${styles.choicePill} ${active && Number(entry?.semester) === sem ? styles.choicePillActive : ""}`}
                                    onClick={() => updateEntry(code, "semester", sem)}
                                  >
                                    Semester {sem}
                                  </button>
                                ))}
                              </div>
                            </div>

                            <div className={styles.totalGradeRow}>
                              <div className={styles.totalInputGroup}>
                                <label className={styles.choiceLabel}>Final Total</label>
                                <input
                                  className={styles.historyTotalInput}
                                  type="number"
                                  min={60}
                                  max={100}
                                  disabled={!active}
                                  value={entry?.total ?? ""}
                                  onChange={(e) => updateEntry(code, "total", e.target.value)}
                                  placeholder="60–100"
                                />
                              </div>

                              <div className={styles.gradePreviewBox}>
                                <span className={styles.choiceLabel}>Grade Preview</span>
                                <span
                                  className={styles.historyGradeBadge}
                                  style={{ color: gradeClr, borderColor: `${gradeClr}30`, background: `${gradeClr}14` }}
                                >
                                  {grade ?? "—"}
                                </span>
                              </div>
                            </div>
                          </div>
                        </motion.div>
                      );
                    })}
                  </div>

                  <div className={styles.setupFooter}>
                    <p className={styles.setupFooterNote}>
                      Activate only the courses that belong to the student’s completed academic history. Inactive courses remain visible for reference only.
                    </p>
                    <motion.button
                      className={styles.saveSetupBtn}
                      onClick={handleSaveSetup}
                      disabled={saveBusy}
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.97 }}
                    >
                      {SVG.check} {saveBusy ? "Saving…" : "Save Academic History"}
                    </motion.button>
                  </div>
                </>
              )}
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}

