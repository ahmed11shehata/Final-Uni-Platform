import { useState, useEffect, useMemo } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { getStudentTranscript, getStudentRegistrationStatus } from "../../services/api/studentApi";
import styles from "./GradesPage.module.css";

const GRADE_MAP = {
  A_Plus: "A+", A: "A", A_Minus: "A-", B_Plus: "B+", B: "B", B_Minus: "B-",
  C_Plus: "C+", C: "C", C_Minus: "C-", D_Plus: "D+", D: "D", F: "F",
};

function fmtGrade(g) { return GRADE_MAP[g] ?? g ?? null; }

function gradeColor(g) {
  const l = fmtGrade(g);
  if (!l) return "#94a3b8";
  if (l.startsWith("A")) return "#22c55e";
  if (l.startsWith("B")) return "#3b82f6";
  if (l.startsWith("C")) return "#f59e0b";
  if (l.startsWith("D")) return "#fb923c";
  return "#ef4444";
}

function gpaFromCourses(courses) {
  if (!courses.length) return null;
  const pts = courses.reduce((s, c) => s + (Number(c.gpaPoints ?? 0) * Number(c.credits ?? 3)), 0);
  const hrs = courses.reduce((s, c) => s + Number(c.credits ?? 3), 0);
  return hrs > 0 ? (pts / hrs).toFixed(2) : null;
}

function Bar({ pct, color, delay = 0 }) {
  return (
    <div className={styles.miniBarTrack}>
      <motion.div
        className={styles.miniBarFill}
        style={{ background: color }}
        initial={{ width: 0 }}
        animate={{ width: `${pct}%` }}
        transition={{ delay, duration: 0.75, ease: "easeOut" }}
      />
    </div>
  );
}

const YEAR_COLORS = ["#f59e0b", "#6366f1", "#14b8a6", "#ec4899"];
const YEAR_LABELS = ["", "First Year", "Second Year", "Third Year", "Fourth Year"];
const YEAR_ORDINALS = ["1st", "2nd", "3rd", "4th"];

function CurrentCoursesView({ courses }) {
  return (
    <motion.section
      className={styles.semView}
      initial={{ opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -12 }}
      transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
    >
      <div className={`${styles.coursesTable} ${styles.currentTable}`}>
        <div className={styles.tableHead}>
          <span>Course</span>
          <span className={styles.thCenter}>Credits</span>
          <span className={styles.thCenter}>Status</span>
          <span className={styles.thCenter}>Note</span>
        </div>

        {courses.length ? (
          courses.map((course, i) => (
            <motion.div
              key={course.code ?? course.courseCode ?? `${course.name}-${i}`}
              className={`${styles.tableRow} ${styles.currentTableRow}`}
              initial={{ opacity: 0, x: -14 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.12 + i * 0.04, duration: 0.26, ease: [0.22, 1, 0.36, 1] }}
            >
              <div className={styles.tdCourse}>
                <span className={styles.courseCode}>{course.code ?? course.courseCode ?? "COURSE"}</span>
                <span className={styles.courseName}>{course.name ?? "Registered Course"}</span>
              </div>

              <span className={styles.tdCredits}>{course.credits ?? 3} hrs</span>

              <span className={styles.tdGrade}>
                <span className={`${styles.gradePill} ${styles.currentGradePill}`}>Pending</span>
              </span>

              <div className={styles.currentNoteCell}>
                <span className={styles.currentHintText}>Not Published Yet</span>
              </div>
            </motion.div>
          ))
        ) : (
          <div className={styles.currentEmptyInline}>
            <div className={styles.currentEmptyTitle}>No confirmed current courses</div>
            <div className={styles.currentEmptySub}>No courses are available in the current section right now.</div>
          </div>
        )}
      </div>

      {courses.length > 0 && (
        <div className={styles.currentBigNotice}>
          <div className={styles.currentBigNoticeTitle}>Not Published Yet</div>
          <div className={styles.currentBigNoticeSub}>
            These courses are confirmed in your current registration, but no grades are published for them yet.
          </div>
        </div>
      )}
    </motion.section>
  );
}

function SemesterView({ semCourses, allCompleted, totalEarned, yearColor }) {
  const semGpa = gpaFromCourses(semCourses);
  const cumGpa = gpaFromCourses(allCompleted);
  const semCred = semCourses.reduce((s, c) => s + Number(c.credits ?? 3), 0);

  return (
    <motion.section
      className={styles.semView}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -12 }}
      transition={{ duration: 0.32, ease: [0.22, 1, 0.36, 1] }}
    >
      <div className={styles.statCards}>
        {[
          { icon: "📈", label: "Semester GPA", val: semGpa ?? "—", color: yearColor },
          { icon: "🎓", label: "Cumulative GPA", val: cumGpa ?? "—", color: "#6366f1" },
          { icon: "📚", label: "Credit Hours", val: semCred, color: "#0ea5e9", sub: `${totalEarned} total earned` },
          { icon: "📋", label: "Courses", val: semCourses.length, color: "#22c55e" },
        ].map((s, i) => (
          <motion.div
            key={s.label}
            className={styles.statCard}
            initial={{ opacity: 0, scale: 0.93 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.04 + i * 0.05 }}
          >
            <div className={styles.statCardIcon} style={{ background: `${s.color}18`, color: s.color }}>
              {s.icon}
            </div>
            <div className={styles.statCardBody}>
              <div className={styles.statCardVal} style={{ color: s.color }}>{s.val}</div>
              <div className={styles.statCardLabel}>{s.label}</div>
              {s.sub && <div className={styles.statCardSub}>{s.sub}</div>}
            </div>
          </motion.div>
        ))}
      </div>

      <div className={styles.coursesTable}>
        <div className={styles.tableHead}>
          <span>Course</span>
          <span className={styles.thCenter}>Credits</span>
          <span className={styles.thCenter}>Grade</span>
          <span className={styles.thCenter}>Progress</span>
        </div>

        {semCourses.map((course, i) => {
          const gl = fmtGrade(course.grade);
          const gc = gradeColor(course.grade);
          const pct = course.gpaPoints != null ? (Number(course.gpaPoints) / 4) * 100 : 0;
          return (
            <motion.div
              key={course.courseCode}
              className={styles.tableRow}
              initial={{ opacity: 0, x: -14 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.18 + i * 0.05, duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
            >
              <div className={styles.tdCourse}>
                <span className={styles.courseCode} style={{ color: yearColor }}>{course.courseCode}</span>
                <span className={styles.courseName}>{course.name}</span>
              </div>
              <span className={styles.tdCredits}>{course.credits ?? 3} hrs</span>
              <span className={styles.tdGrade}>
                <span className={styles.gradePill} style={{ background: `${gc}15`, color: gc }}>
                  {gl ?? "—"}
                </span>
              </span>
              <div className={styles.tdBar}>
                <Bar pct={pct} color={gc} delay={0.22 + i * 0.05} />
              </div>
            </motion.div>
          );
        })}
      </div>
    </motion.section>
  );
}

export default function GradesPage() {
  const [transcript, setTranscript] = useState(null);
  const [regStatus, setRegStatus] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    Promise.all([
      getStudentTranscript(),
      getStudentRegistrationStatus().catch(() => null),
    ])
      .then(([t, r]) => {
        if (!cancelled) {
          setTranscript(t);
          setRegStatus(r);
        }
      })
      .catch((e) => {
        if (!cancelled) setError(e.message || "Failed to load grades");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, []);

  const { years, allCompleted, totalEarned, currentYear } = useMemo(() => {
    if (!transcript) return { years: [], allCompleted: [], totalEarned: 0, currentYear: 1 };

    const curYear = transcript.student?.currentYear ?? 1;
    const grouped = {};

    for (const c of (transcript.completedCourses ?? [])) {
      const yr = c.year ?? 1;
      const sm = c.semester ?? 1;
      if (!grouped[yr]) grouped[yr] = {};
      if (!grouped[yr][sm]) grouped[yr][sm] = [];
      grouped[yr][sm].push(c);
    }

    const all = transcript.completedCourses ?? [];
    const earned = all.reduce((s, c) => s + Number(c.credits ?? 3), 0);

    const parsedYears = Object.keys(grouped)
      .map(Number)
      .sort((a, b) => a - b)
      .map((yr) => ({
        yr,
        sems: Object.keys(grouped[yr]).map(Number).sort((a, b) => a - b).map((sm) => ({
          sm,
          courses: grouped[yr][sm],
        })),
      }));

    return { years: parsedYears, allCompleted: all, totalEarned: earned, currentYear: curYear };
  }, [transcript]);

  const studentInfo = transcript?.student;
  const overallGpa = gpaFromCourses(allCompleted) ?? "—";
  const currentCourses = regStatus?.registeredCourses ?? [];

  const [selectedYear, setSelectedYear] = useState(null);
  const [selectedSem, setSelectedSem] = useState(null);
  const [selectedView, setSelectedView] = useState("transcript");

  useEffect(() => {
    if (!years.length || selectedYear !== null) return;
    const first = years[years.length - 1];
    setSelectedYear(first.yr);
    setSelectedSem(first.sems[0]?.sm ?? 1);
  }, [years, selectedYear]);

  const activeYearData = years.find((y) => y.yr === selectedYear);
  const activeSemData = activeYearData?.sems.find((s) => s.sm === selectedSem);
  const yearColor = YEAR_COLORS[(selectedYear ?? 1) - 1] || "#6366f1";

  if (loading) {
    return (
      <div className={styles.page}>
        <div className={styles.loadingState}>
          <div className={styles.loadingSpinner} />
          <p>Loading transcript…</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>⚠ {error}</div>
      </div>
    );
  }

  if (!years.length && !currentCourses.length) {
    return (
      <div className={styles.page}>
        <div className={styles.errorState}>No academic records found.</div>
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <motion.div
        className={styles.header}
        initial={{ opacity: 0, y: -14 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.36 }}
      >
        <div className={styles.headerLeft}>
          <h1 className={styles.pageTitle}>My Grades</h1>
        </div>

        <div className={styles.headerSummary}>
          <div className={styles.summaryItem}>
            <span className={styles.summaryVal} style={{ color: "#6366f1" }}>{overallGpa}</span>
            <span className={styles.summaryLabel}>Overall GPA</span>
          </div>
          <div className={styles.summaryDivider} />
          <div className={styles.summaryItem}>
            <span className={styles.summaryVal} style={{ color: "#0ea5e9" }}>{totalEarned}</span>
            <span className={styles.summaryLabel}>Credits Earned</span>
          </div>
          <div className={styles.summaryDivider} />
          <div className={styles.summaryItem}>
            <span className={styles.summaryVal} style={{ color: "#22c55e" }}>
              {currentYear}{["st", "nd", "rd", "th"][currentYear - 1] ?? "th"}
            </span>
            <span className={styles.summaryLabel}>Current Year</span>
          </div>
        </div>
      </motion.div>

      <div className={styles.transcriptTop}>
        <div className={styles.transcriptControls}>
          <div className={styles.yearTabs}>
            {years.map(({ yr }) => {
              const isActive = selectedView === "transcript" && yr === selectedYear;
              const yc = YEAR_COLORS[yr - 1];
              return (
                <motion.button
                  key={yr}
                  className={`${styles.yearTab} ${isActive ? styles.yearTabActive : ""}`}
                  style={isActive ? { background: yc, borderColor: yc, color: "#fff" } : { "--tab-accent": yc }}
                  onClick={() => {
                    const yrData = years.find((y) => y.yr === yr);
                    setSelectedView("transcript");
                    setSelectedYear(yr);
                    setSelectedSem(yrData?.sems[0]?.sm ?? 1);
                  }}
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                >
                  <span className={styles.yearTabOrd}>{YEAR_ORDINALS[yr - 1]}</span>
                  <span className={styles.yearTabLabel}>{YEAR_LABELS[yr]}</span>
                </motion.button>
              );
            })}

            <motion.button
              className={`${styles.yearTab} ${styles.currentTabBtn} ${selectedView === "current" ? styles.currentTabActive : ""}`}
              onClick={() => setSelectedView("current")}
              whileHover={{ scale: 1.03 }}
              whileTap={{ scale: 0.97 }}
            >
              <span className={styles.yearTabOrd}>•</span>
              <span className={styles.yearTabLabel}>Current</span>
            </motion.button>
          </div>

          {selectedView === "transcript" && (
            <AnimatePresence mode="wait">
              <motion.div
                key={selectedYear}
                className={styles.semSelector}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.2 }}
              >
                {activeYearData?.sems.map(({ sm }) => (
                  <button
                    key={sm}
                    className={`${styles.semBtn} ${selectedSem === sm ? styles.semBtnActive : ""}`}
                    style={selectedSem === sm ? { borderColor: yearColor, color: yearColor } : {}}
                    onClick={() => setSelectedSem(sm)}
                  >
                    Semester {sm}
                  </button>
                ))}
              </motion.div>
            </AnimatePresence>
          )}
        </div>
      </div>

      <div className={styles.content}>
        <AnimatePresence mode="wait">
          {selectedView === "current" ? (
            <CurrentCoursesView key="current-courses" courses={currentCourses} />
          ) : (
            activeSemData && (
              <SemesterView
                key={`${selectedYear}-${selectedSem}`}
                semCourses={activeSemData.courses}
                allCompleted={allCompleted}
                totalEarned={totalEarned}
                yearColor={yearColor}
              />
            )
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
