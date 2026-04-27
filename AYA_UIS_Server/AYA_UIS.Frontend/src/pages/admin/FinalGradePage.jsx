// src/pages/admin/FinalGradePage.jsx
import { useState, useEffect, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./FinalGradePage.module.css";
import {
  adminGetFinalGrades,
  adminPublishFinalGrades,
  adminNotifyInstructor,
  adminGetFinalGradeReviewList,
  adminClassifyStudent,
  adminPublishAllFinalGrades,
} from "../../services/api/adminApi";

const TAB_CFG = {
  progress: {
    key: "progress",
    label: "Progress",
    short: "In review",
    icon: "⏳",
    color: "#f59e0b",
    soft: "rgba(245,158,11,.12)",
    border: "rgba(245,158,11,.30)",
    grad: "linear-gradient(135deg,#f59e0b,#f97316)",
  },
  not_completed: {
    key: "not_completed",
    label: "Not Completed",
    short: "Needs action",
    icon: "⚠️",
    color: "#ef4444",
    soft: "rgba(239,68,68,.12)",
    border: "rgba(239,68,68,.30)",
    grad: "linear-gradient(135deg,#fb7185,#ef4444)",
  },
  completed: {
    key: "completed",
    label: "Completed",
    short: "Ready",
    icon: "✅",
    color: "#22c55e",
    soft: "rgba(34,197,94,.12)",
    border: "rgba(34,197,94,.30)",
    grad: "linear-gradient(135deg,#22c55e,#14b8a6)",
  },
};

const sp = { type: "spring", stiffness: 400, damping: 28 };

const LETTER_COLOR = {
  A: "#22c55e",
  B: "#818cf8",
  C: "#3b82f6",
  D: "#f59e0b",
  F: "#ef4444",
};

const percent = (value, total) => {
  if (!total) return 0;
  return Math.max(0, Math.min(100, Math.round((value / total) * 100)));
};

function Av({ name, size = 44, bg = "#7c3aed" }) {
  const ini = (name || "?")
    .split(" ")
    .slice(0, 2)
    .map(w => w[0] || "")
    .join("")
    .toUpperCase();

  return (
    <div
      className={styles.avatar}
      style={{
        "--av-size": `${size}px`,
        "--av-radius": size > 46 ? "18px" : "14px",
        "--av-bg": `${bg}20`,
        "--av-color": bg,
        "--av-border": `${bg}44`,
      }}
    >
      {ini}
    </div>
  );
}

function CompletedWarningModal({ student, unassignedCount, onCancel, onMarkAnyway, loading }) {
  return (
    <motion.div className={styles.overlay} onClick={onCancel}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div className={styles.modal} onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, scale: .88, y: 28 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: .95 }}
        transition={sp}>
        <div className={styles.modalBand} style={{ background: "linear-gradient(90deg,#f59e0b,#ef4444)" }} />
        <div className={styles.modalBody}>
          <div className={styles.modalWarnIcon}>⚠️</div>
          <div className={styles.modalTitle}>Missing Final Grades</div>

          <div className={styles.modalInfoBlock}>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Student</span>
              <span className={styles.modalInfoVal}>{student.studentName}</span>
            </div>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Student Code</span>
              <span className={styles.modalInfoVal}>{student.studentCode}</span>
            </div>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Ungraded</span>
              <span className={styles.modalInfoVal} style={{ color: "#ef4444", fontWeight: 900 }}>
                {unassignedCount} course{unassignedCount === 1 ? "" : "s"}
              </span>
            </div>
          </div>

          <p className={styles.modalDesc}>
            This student still has registered courses without final grades. You can cancel and notify instructors,
            or mark the student as completed manually.
          </p>

          <div className={styles.modalActions}>
            <button className={styles.modalCancel} onClick={onCancel} disabled={loading}>Cancel</button>
            <motion.button
              className={styles.modalNotifyBtn}
              style={{ background: "linear-gradient(135deg,#f59e0b,#ef4444)" }}
              onClick={onMarkAnyway}
              disabled={loading}
              whileHover={{ scale: 1.03, filter: "brightness(1.1)" }}
              whileTap={{ scale: .96 }}>
              {loading ? "Saving…" : "Mark Anyway"}
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

function NotifyModal({ course, student, onClose, onSent }) {
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);

  const send = async () => {
    setLoading(true);
    try {
      await adminNotifyInstructor(student.studentId, course.courseId);
      setDone(true);
      setTimeout(onSent, 1400);
    } catch (e) {
      alert(e?.message || "Notification failed");
      setLoading(false);
    }
  };

  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div className={styles.modal} onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, scale: .88, y: 28 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: .95 }}
        transition={sp}>
        <div className={styles.modalBand} />
        <div className={styles.modalBody}>
          <div className={styles.modalWarnIcon}>🔔</div>
          <div className={styles.modalTitle}>Notify Responsible Instructor</div>

          <div className={styles.modalInfoBlock}>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Course</span>
              <span className={styles.modalInfoVal}>{course.courseCode} · {course.courseName}</span>
            </div>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Student</span>
              <span className={styles.modalInfoVal}>{student.studentName}</span>
            </div>
            <div className={styles.modalInfoRow}>
              <span className={styles.modalInfoLabel}>Code</span>
              <span className={styles.modalInfoVal}>{student.studentCode}</span>
            </div>
          </div>

          <p className={styles.modalDesc}>
            A warning notification will be sent to the instructor team responsible for this course.
          </p>

          {done ? (
            <div className={styles.modalSuccess}>Notification sent successfully.</div>
          ) : (
            <div className={styles.modalActions}>
              <button className={styles.modalCancel} onClick={onClose} disabled={loading}>Cancel</button>
              <motion.button
                className={styles.modalNotifyBtn}
                onClick={send}
                disabled={loading}
                whileHover={{ scale: 1.03, filter: "brightness(1.1)" }}
                whileTap={{ scale: .96 }}>
                {loading ? "Sending…" : "Send Notification"}
              </motion.button>
            </div>
          )}
        </div>
      </motion.div>
    </motion.div>
  );
}

function ClassificationTabs({ counts, total, onOpen, loading }) {
  return (
    <div className={styles.tabsGrid}>
      {["progress", "not_completed", "completed"].map((k, i) => {
        const cfg = TAB_CFG[k];
        const n = counts?.[k] ?? 0;
        return (
          <motion.button
            key={k}
            type="button"
            className={styles.tabCard}
            style={{ "--tab-color": cfg.color, "--tab-bg": cfg.soft, "--tab-border": cfg.border }}
            onClick={() => onOpen(k)}
            disabled={loading}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: .04 * i, ...sp }}
            whileHover={{ y: -4, scale: 1.01 }}
            whileTap={{ scale: .98 }}>
            <span className={styles.tabIcon}>{cfg.icon}</span>
            <span className={styles.tabText}>
              <span className={styles.tabLabel}>{cfg.label}</span>
              <span className={styles.tabSub}>{cfg.short}</span>
            </span>
            <span className={styles.tabCountWrap}>
              <motion.span className={styles.tabCount} key={n}
                initial={{ scale: .6, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ type: "spring", stiffness: 500, damping: 22 }}>
                {n}
              </motion.span>
              <span className={styles.tabPercent}>{percent(n, total)}%</span>
            </span>
            <span className={styles.tabBar}><span style={{ width: `${percent(n, total)}%`, background: cfg.grad }} /></span>
          </motion.button>
        );
      })}
    </div>
  );
}

function ClassificationPopup({ tab, students, onClose, onOpenStudent }) {
  const cfg = TAB_CFG[tab];
  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div className={styles.popup} onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, y: 28, scale: .96 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, scale: .96 }}
        transition={{ type: "spring", stiffness: 340, damping: 28 }}>
        <div className={styles.popupHead} style={{ "--popup-color": cfg.color, "--popup-soft": cfg.soft, "--popup-border": cfg.border }}>
          <div className={styles.popupTitleWrap}>
            <span className={styles.popupIcon}>{cfg.icon}</span>
            <div>
              <div className={styles.popupTitle}>{cfg.label}</div>
              <div className={styles.popupSub}>
                {students.length === 0 ? "No students in this status" : `${students.length} student${students.length === 1 ? "" : "s"} in this list`}
              </div>
            </div>
          </div>
          <button type="button" className={styles.popupClose} onClick={onClose}>✕</button>
        </div>

        <div className={styles.popupBody}>
          {students.length === 0 ? (
            <div className={styles.popupEmpty}>
              <span>📭</span>
              <p>Nothing to review here.</p>
            </div>
          ) : (
            <div className={styles.popupGrid}>
              {students.map(s => (
                <motion.button
                  type="button"
                  key={s.studentId}
                  className={styles.studentBlock}
                  onClick={() => onOpenStudent(s)}
                  whileHover={{ y: -3 }}
                  whileTap={{ scale: .98 }}
                  layout>
                  <Av name={s.studentName} size={44} bg={cfg.color} />
                  <div className={styles.studentBlockInfo}>
                    <div className={styles.studentBlockName}>{s.studentName}</div>
                    <div className={styles.studentBlockMeta}>
                      <span className={styles.studentBlockChip}>ID {s.studentCode}</span>
                      <span className={styles.studentBlockChip}>{s.academicYear}</span>
                      <span className={styles.studentBlockChip} style={{ color: cfg.color, borderColor: cfg.border }}>
                        {s.registeredCourses} course{s.registeredCourses === 1 ? "" : "s"}
                      </span>
                    </div>
                  </div>
                  <span className={styles.studentBlockArrow}>›</span>
                </motion.button>
              ))}
            </div>
          )}
        </div>
      </motion.div>
    </motion.div>
  );
}

function CourseCard({ course, onOpenNotify }) {
  const assigned = course.assigned;
  const lc = course.letterGrade ? (LETTER_COLOR[course.letterGrade] || "#818cf8") : "#94a3b8";
  const totalPct = course.total != null ? Math.min(100, Math.round(course.total)) : 0;
  const dashArr = 2 * Math.PI * 32;

  return (
    <motion.div
      className={`${styles.courseCard} ${!assigned ? styles.courseCardWarn : ""}`}
      style={{ "--course-color": assigned ? lc : "#f97316" }}
      initial={{ opacity: 0, y: 16, scale: .97 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      transition={sp}
      onClick={!assigned ? () => onOpenNotify(course) : undefined}
      whileHover={{ scale: 1.01, y: -4 }}
      whileTap={!assigned ? { scale: .99 } : {}}>
      <div className={styles.cardHead}>
        <div className={styles.cardHeadL}>
          <div className={styles.cardCode}>{course.courseCode}</div>
          <div className={styles.cardName}>{course.courseName}</div>
        </div>

        {assigned ? (
          <div className={styles.gradeCircle}>
            <svg viewBox="0 0 72 72" className={styles.circSvg}>
              <circle cx="36" cy="36" r="32" fill="none" stroke="var(--ring-track)" strokeWidth="6" />
              <motion.circle cx="36" cy="36" r="32" fill="none"
                stroke={lc} strokeWidth="6" strokeLinecap="round"
                strokeDasharray={dashArr}
                initial={{ strokeDashoffset: dashArr }}
                animate={{ strokeDashoffset: dashArr - dashArr * (totalPct / 100) }}
                transition={{ duration: .9, ease: "easeOut", delay: .15 }}
                style={{ transform: "rotate(-90deg)", transformOrigin: "36px 36px" }} />
            </svg>
            <div className={styles.circCenter}>
              <span className={styles.circLetter}>{course.letterGrade}</span>
            </div>
          </div>
        ) : (
          <div className={styles.warnBadge}>
            <span>Pending</span>
            <strong>Notify</strong>
          </div>
        )}
      </div>

      <div className={styles.cardBody}>
        <div className={styles.scoreSplit}>
          <div>
            <span>Coursework</span>
            <strong>{course.courseworkTotal}<em>/40</em></strong>
          </div>
          <div>
            <span>Final</span>
            <strong>{assigned ? course.finalScore : "—"}<em>/60</em></strong>
          </div>
          <div>
            <span>Total</span>
            <strong>{assigned ? course.total : "—"}<em>/100</em></strong>
          </div>
        </div>

        <div className={styles.gradeBreakdown}>
          <div className={styles.gradeRow}>
            <span className={styles.gradeLabel}>Midterm</span>
            <span className={styles.gradeVal}>{course.midtermMax > 0 ? <><strong>{course.midtermGrade}</strong> / {course.midtermMax}</> : <span className={styles.gradeNone}>—</span>}</span>
          </div>
          <div className={styles.gradeRow}>
            <span className={styles.gradeLabel}>Quizzes</span>
            <span className={styles.gradeVal}><strong>{course.quizScore}</strong> pts</span>
          </div>
          <div className={styles.gradeRow}>
            <span className={styles.gradeLabel}>Assignments</span>
            <span className={styles.gradeVal}><strong>{course.assignmentScore}</strong> pts</span>
          </div>
          {course.bonus > 0 && (
            <div className={styles.gradeRow}>
              <span className={styles.gradeLabel}>Bonus</span>
              <span className={styles.gradeVal}><strong>+{course.bonus}</strong></span>
            </div>
          )}
        </div>

        <div className={styles.statusRow}>
          <span className={`${styles.statusChip} ${assigned ? styles.statusAssigned : styles.statusPending}`}>
            {assigned ? "Assigned" : "Pending"}
          </span>
          {assigned && (
            <span className={`${styles.statusChip} ${course.published ? styles.statusPublished : styles.statusUnpublished}`}>
              {course.published ? "Published" : "Not Published"}
            </span>
          )}
          {!assigned && <span className={styles.notifyHint}>Click to notify instructor</span>}
        </div>
      </div>
    </motion.div>
  );
}

export default function FinalGradePage() {
  const [query, setQuery] = useState("");
  const [loading, setLoading] = useState(false);
  const [student, setStudent] = useState(null);
  const [error, setError] = useState(null);
  const [notifyFor, setNotifyFor] = useState(null);
  const [publishing, setPublishing] = useState(false);
  const [toast, setToast] = useState(null);

  const [reviewList, setReviewList] = useState({ progress: [], not_completed: [], completed: [], total: 0, canPublishAll: false });
  const [reviewLoading, setReviewLoading] = useState(false);
  const [popupTab, setPopupTab] = useState(null);
  const [classifying, setClassifying] = useState(false);
  const [publishingAll, setPublishingAll] = useState(false);
  const [completedWarningPending, setCompletedWarningPending] = useState(false);

  const toast$ = (msg, t = "ok") => {
    setToast({ msg, t });
    setTimeout(() => setToast(null), 2800);
  };

  const fetchReviewList = useCallback(async () => {
    setReviewLoading(true);
    try {
      const data = await adminGetFinalGradeReviewList();
      setReviewList({
        progress: data?.progress ?? [],
        not_completed: data?.notCompleted ?? [],
        completed: data?.completed ?? [],
        total: data?.total ?? 0,
        canPublishAll: data?.canPublishAll ?? false,
      });
    } catch (e) {
      console.error("Failed to load classification list", e);
    } finally {
      setReviewLoading(false);
    }
  }, []);

  useEffect(() => { fetchReviewList(); }, [fetchReviewList]);

  const tabCounts = {
    progress: reviewList.progress.length,
    not_completed: reviewList.not_completed.length,
    completed: reviewList.completed.length,
  };

  const courses = student?.courses ?? [];
  const assignedCount = courses.filter(c => c.assigned).length;
  const unassignedCount = courses.filter(c => !c.assigned).length;
  const publishedCount = courses.filter(c => c.published).length;
  const hasUnpublished = courses.some(c => c.assigned && !c.published);
  const unpublishedReady = courses.filter(c => c.assigned && !c.published).length;

  const currentReviewStatus = student
    ? (reviewList.completed.some(s => s.studentId === student.studentId)
      ? "completed"
      : reviewList.not_completed.some(s => s.studentId === student.studentId)
        ? "not_completed"
        : reviewList.progress.some(s => s.studentId === student.studentId)
          ? "progress"
          : null)
    : null;

  const openStudentFromBlock = async (s) => {
    setPopupTab(null);
    setQuery(s.studentCode || s.studentId);
    setLoading(true);
    setError(null);
    setStudent(null);
    try {
      const data = await adminGetFinalGrades(s.studentCode || s.studentId);
      setStudent(data);
    } catch (err) {
      setError(err?.message || "Student not found");
    } finally {
      setLoading(false);
    }
  };

  const doClassify = async (status) => {
    setClassifying(true);
    try {
      await adminClassifyStudent(student.studentId, status);
      toast$(`Marked as ${TAB_CFG[status].label}`);
      await fetchReviewList();
    } catch (err) {
      toast$(err?.response?.data?.error?.message || "Classification failed", "err");
    } finally {
      setClassifying(false);
    }
  };

  const classifyCurrent = (status) => {
    if (!student) return;
    if (status === "completed" && unassignedCount > 0) {
      setCompletedWarningPending(true);
      return;
    }
    doClassify(status);
  };

  const publishAll = async () => {
    if (!reviewList.canPublishAll) return;
    setPublishingAll(true);
    try {
      const res = await adminPublishAllFinalGrades();
      toast$(res?.message || "All grades published");
      await fetchReviewList();
    } catch (err) {
      toast$(err?.response?.data?.error?.message || "Publish all failed", "err");
    } finally {
      setPublishingAll(false);
    }
  };

  const search = async (e) => {
    e?.preventDefault();
    const q = query.trim();
    if (!q) return;
    setLoading(true);
    setError(null);
    setStudent(null);
    try {
      const data = await adminGetFinalGrades(q);
      setStudent(data);
    } catch (err) {
      setError(err?.message || "Student not found");
    } finally {
      setLoading(false);
    }
  };

  const publish = async () => {
    if (!student) return;
    const ready = student.courses.filter(c => c.assigned && !c.published).length;
    if (ready === 0) {
      toast$("All assigned grades are already published.", "warn");
      return;
    }
    setPublishing(true);
    try {
      const res = await adminPublishFinalGrades(student.studentId, null);
      toast$(`${res.publishedCount} grade(s) published to student`);
      const updated = await adminGetFinalGrades(student.studentCode || student.studentId);
      setStudent(updated);
    } catch (err) {
      toast$(err?.message || "Publish failed", "err");
    } finally {
      setPublishing(false);
    }
  };

  return (
    <div className={styles.page}>
      <AnimatePresence>
        {toast && (
          <motion.div
            className={`${styles.toast} ${toast.t === "err" ? styles.toastErr : toast.t === "warn" ? styles.toastWarn : ""}`}
            initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            transition={sp}>
            {toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      <motion.section className={styles.hero}
        initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: .45, ease: [.22, 1, .36, 1] }}>
        <div className={styles.heroBg} />
        <div className={styles.heroContent}>
          <div className={styles.heroCopy}>
            <span className={styles.heroBadge}><span /> Academic Control Center</span>
            <h1 className={styles.heroTitle}>Final Grade Audit</h1>
            <p className={styles.heroSub}>Review every student, classify their account, and publish final grades in one controlled flow.</p>
          </div>

          <div className={styles.heroActions}>
            <div className={styles.publishState}>
              <span className={reviewList.canPublishAll ? styles.readyDot : styles.lockedDot} />
              <div>
                <strong>{reviewList.canPublishAll ? "Ready to publish" : "Publish locked"}</strong>
                <small>{reviewList.canPublishAll ? `${reviewList.total} students completed` : `${tabCounts.progress + tabCounts.not_completed} students still pending`}</small>
              </div>
            </div>
            <motion.button
              type="button"
              className={`${styles.publishAllBtn} ${!reviewList.canPublishAll ? styles.publishAllBtnDim : ""}`}
              onClick={publishAll}
              disabled={publishingAll || !reviewList.canPublishAll}
              whileHover={reviewList.canPublishAll ? { scale: 1.03, filter: "brightness(1.08)" } : {}}
              whileTap={reviewList.canPublishAll ? { scale: .97 } : {}}
              title={reviewList.canPublishAll ? "Publish final grades for all completed students" : "Every student must be marked Completed first"}>
              {publishingAll ? "Publishing…" : "Publish All Grades"}
            </motion.button>
          </div>
        </div>
      </motion.section>

      <section className={styles.controlGrid}>
        <motion.div className={styles.panel}
          initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: .05 }}>
          <div className={styles.panelHead}>
            <div>
              <span className={styles.kicker}>Review lists</span>
              <h2>Student classification</h2>
            </div>
            <span className={styles.totalPill}>{reviewList.total} total</span>
          </div>

          <ClassificationTabs counts={tabCounts} total={reviewList.total} loading={reviewLoading} onOpen={setPopupTab} />

          {!reviewList.canPublishAll && reviewList.total > 0 && (
            <p className={styles.panelHint}>{tabCounts.progress + tabCounts.not_completed} student(s) still need review before publishing.</p>
          )}
          {reviewList.canPublishAll && reviewList.total > 0 && (
            <p className={styles.panelHintOk}>All students are completed. Publish All is now available.</p>
          )}
        </motion.div>

        <motion.div className={`${styles.panel} ${styles.searchPanel}`}
          initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: .09 }}>
          <div className={styles.panelHead}>
            <div>
              <span className={styles.kicker}>Direct lookup</span>
              <h2>Search student</h2>
            </div>
          </div>

          <form className={styles.searchForm} onSubmit={search}>
            <div className={styles.searchBox}>
              <svg className={styles.searchIco} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round">
                <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
              </svg>
              <input
                className={styles.searchIn}
                placeholder="Enter student code…"
                value={query}
                onChange={e => setQuery(e.target.value)}
                autoFocus />
              {query && (
                <button type="button" className={styles.searchX}
                  onClick={() => { setQuery(""); setStudent(null); setError(null); }}>✕</button>
              )}
            </div>
            <motion.button type="submit" className={styles.searchBtn}
              disabled={!query.trim() || loading}
              whileHover={{ scale: 1.02 }} whileTap={{ scale: .97 }}>
              {loading ? "Searching…" : "Search"}
            </motion.button>
          </form>

          {error && (
            <motion.div className={styles.errorMsg} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
              {error}
            </motion.div>
          )}
        </motion.div>
      </section>

      {!student && !loading && !error && (
        <motion.section className={styles.idleState}
          initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }}>
          <div className={styles.idleCard}>
            <div className={styles.idleIcon}>🎓</div>
            <h3 className={styles.idleTitle}>Start with a student code or a review list</h3>
            <p className={styles.idleSub}>Open Progress, Not Completed, or Completed to pick an account, or search directly using the student's public academic code.</p>
            <div className={styles.idleSteps}>
              <span>1. Pick student</span>
              <span>2. Review courses</span>
              <span>3. Classify account</span>
            </div>
          </div>
        </motion.section>
      )}

      <AnimatePresence>
        {student && (
          <motion.section
            className={styles.reviewWorkspace}
            key={student.studentId}
            initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}>

            <aside className={styles.studentPanel}>
              <div className={styles.studentHero}>
                <Av name={student.studentName} size={58} bg="#7c3aed" />
                <div>
                  <div className={styles.studentName}>{student.studentName}</div>
                  <div className={styles.studentCode}>{student.studentCode}</div>
                </div>
              </div>

              <div className={styles.studentStats}>
                <div><strong>{courses.length}</strong><span>Courses</span></div>
                <div><strong>{assignedCount}</strong><span>Assigned</span></div>
                <div><strong>{publishedCount}</strong><span>Published</span></div>
              </div>

              <div className={styles.reviewStatusBox} style={currentReviewStatus ? { "--status-color": TAB_CFG[currentReviewStatus].color, "--status-soft": TAB_CFG[currentReviewStatus].soft } : undefined}>
                <span>{currentReviewStatus ? TAB_CFG[currentReviewStatus].icon : "•"}</span>
                <div>
                  <small>Current decision</small>
                  <strong>{currentReviewStatus ? TAB_CFG[currentReviewStatus].label : "Not listed"}</strong>
                </div>
              </div>

              <div className={styles.classifyBox}>
                <span className={styles.kicker}>Review decision</span>
                <div className={styles.classifyBtns}>
                  {["progress", "not_completed", "completed"].map(k => {
                    const cfg = TAB_CFG[k];
                    return (
                      <motion.button
                        key={k}
                        type="button"
                        className={styles.classifyBtn}
                        style={{ "--btn-color": cfg.color, "--btn-bg": cfg.soft, "--btn-border": cfg.border }}
                        onClick={() => classifyCurrent(k)}
                        disabled={classifying}
                        whileHover={{ scale: 1.02 }}
                        whileTap={{ scale: .97 }}>
                        {cfg.icon} {cfg.label}
                      </motion.button>
                    );
                  })}
                </div>
              </div>

              <div className={styles.publishMiniCard}>
                <div>
                  <strong>Publish this student</strong>
                  <span>{hasUnpublished ? `${unpublishedReady} grade(s) ready` : "No pending published grades"}</span>
                </div>
                <motion.button
                  className={`${styles.publishBtn} ${!hasUnpublished ? styles.publishBtnDim : ""}`}
                  onClick={publish}
                  disabled={publishing || !hasUnpublished}
                  whileHover={hasUnpublished ? { scale: 1.03, filter: "brightness(1.08)" } : {}}
                  whileTap={hasUnpublished ? { scale: .97 } : {}}>
                  {publishing ? "Publishing…" : "Publish"}
                </motion.button>
              </div>
            </aside>

            <main className={styles.coursePanel}>
              <div className={styles.coursePanelHead}>
                <div>
                  <span className={styles.kicker}>Registered courses</span>
                  <h2>Final grade status</h2>
                </div>
                <div className={styles.courseProgress}>
                  <span>{assignedCount}/{courses.length || 0}</span>
                  <div><i style={{ width: `${percent(assignedCount, courses.length)}%` }} /></div>
                </div>
              </div>

              {unassignedCount > 0 && (
                <motion.div className={styles.warnBanner}
                  initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: .1 }}>
                  <span className={styles.warnBannerIcon}>⚠️</span>
                  <span>
                    <strong>{unassignedCount} course{unassignedCount > 1 ? "s" : ""}</strong> still need final grades. Click any pending card to notify the instructor.
                  </span>
                </motion.div>
              )}

              {courses.length === 0 ? (
                <div className={styles.empty}>
                  <span>📭</span>
                  <p>No active course registrations found for this student.</p>
                </div>
              ) : (
                <div className={styles.cardsGrid}>
                  {courses.map(c => (
                    <CourseCard key={c.courseId} course={c} onOpenNotify={setNotifyFor} />
                  ))}
                </div>
              )}
            </main>
          </motion.section>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {notifyFor && (
          <NotifyModal
            course={notifyFor}
            student={student}
            onClose={() => setNotifyFor(null)}
            onSent={() => setNotifyFor(null)}
          />
        )}
      </AnimatePresence>

      <AnimatePresence>
        {completedWarningPending && student && (
          <CompletedWarningModal
            student={student}
            unassignedCount={unassignedCount}
            loading={classifying}
            onCancel={() => setCompletedWarningPending(false)}
            onMarkAnyway={() => { setCompletedWarningPending(false); doClassify("completed"); }}
          />
        )}
      </AnimatePresence>

      <AnimatePresence>
        {popupTab && (
          <ClassificationPopup
            tab={popupTab}
            students={reviewList[popupTab] || []}
            onClose={() => setPopupTab(null)}
            onOpenStudent={openStudentFromBlock}
          />
        )}
      </AnimatePresence>
    </div>
  );
}
