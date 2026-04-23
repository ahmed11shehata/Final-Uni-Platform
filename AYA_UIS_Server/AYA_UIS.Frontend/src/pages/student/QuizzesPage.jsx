// src/pages/student/QuizzesPage.jsx
import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { getStudentCourses, getStudentCourseDetail } from "../../services/api/studentApi";
import styles from "./QuizzesPage.module.css";

const todayIso = new Date().toISOString().split("T")[0]; // "yyyy-MM-dd"

export default function QuizzesPage() {
  const navigate  = useNavigate();
  const [active,  setActive]  = useState(null);
  const [quizzes, setQuizzes] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const courses = await getStudentCourses();
        if (!Array.isArray(courses) || courses.length === 0) return;

        const details = await Promise.all(
          courses.map(c =>
            getStudentCourseDetail(c.id)
              .then(d => ({ course: c, detail: d }))
              .catch(() => null)
          )
        );

        const today = [];
        for (const entry of details) {
          if (!entry) continue;
          const { course, detail } = entry;
          for (const q of detail.quizzes || []) {
            // Only keep quizzes whose start date is today
            if ((q.startIso || "") !== todayIso) continue;

            today.push({
              quizId:     q.id,
              title:      q.title,
              date:       q.date,
              startIso:   q.startIso  || todayIso,
              startTime:  q.startTime || q.date,
              duration:   q.duration,
              questions:  q.questions,
              max:        q.max,
              score:      q.score ?? null,
              deadline:   q.deadline,
              status:     q.status,
              courseId:   course.id,
              code:       course.code,
              courseName: course.name,
              color:      course.color || "#818cf8",
              shade:      course.shade || "#4a3fa0",
              icon:       course.icon  || "📚",
              instructor: detail.meta?.instructor || "",
            });
          }
        }

        // Sort: live first, then upcoming, then completed
        const order = { available: 0, upcoming: 1, completed: 2 };
        today.sort((a, b) => (order[a.status] ?? 3) - (order[b.status] ?? 3));

        setQuizzes(today);
      } catch {
        // stay empty
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const todayLabel = new Date().toLocaleDateString("en-US", {
    weekday: "long", month: "long", day: "numeric",
  });

  return (
    <div className={styles.page}>

      {/* ── Header ── */}
      <motion.div className={styles.header}
        initial={{ opacity: 0, y: -14 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.38, ease: [0.22, 1, 0.36, 1] }}>

        <div className={styles.headerLeft}>
          <div className={styles.headerIcon}>✏️</div>
          <div>
            <h1 className={styles.title}>Today's Quizzes</h1>
            <p className={styles.subtitle}>
              {loading
                ? "Loading…"
                : quizzes.length === 0
                  ? "No quizzes scheduled for today"
                  : `${quizzes.length} quiz${quizzes.length !== 1 ? "es" : ""} today`}
            </p>
          </div>
        </div>

        <div className={styles.dateBadge}>
          <span className={styles.dateIcon}>📅</span>
          <span>{todayLabel}</span>
        </div>
      </motion.div>

      {/* ── Loading ── */}
      {loading && (
        <div className={styles.loadingWrap}>
          <div className={styles.spinner} />
          <p>Loading today's quizzes…</p>
        </div>
      )}

      {/* ── Grid ── */}
      {!loading && quizzes.length > 0 && (
        <div className={styles.grid}>
          {quizzes.map((q, i) => {
            const isLive = q.status === "available";
            return (
              <motion.button
                key={`${q.courseId}-${q.quizId}`}
                className={`${styles.card} ${isLive ? styles.cardLive : ""}`}
                onClick={() => isLive ? setActive(q) : undefined}
                style={{ cursor: isLive ? "pointer" : "default" }}
                initial={{ opacity: 0, y: 24, scale: 0.93 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                transition={{ delay: i * 0.07, duration: 0.42, ease: [0.22, 1, 0.36, 1] }}
                whileHover={isLive ? { y: -6, transition: { duration: 0.18 } } : { y: -3, transition: { duration: 0.18 } }}
                whileTap={isLive ? { scale: 0.97 } : {}}>

                {/* Colored top */}
                <div className={styles.cardTop}
                  style={{ background: `linear-gradient(148deg, ${q.color}e8 0%, ${q.color}b0 100%)` }}>

                  <div className={styles.cardTopRow}>
                    <span className={styles.cardIcon}>{q.icon}</span>
                    {isLive && (
                      <div className={styles.liveBadge}>
                        <span className={styles.liveDot} />
                        Live
                      </div>
                    )}
                    {q.status === "upcoming" && (
                      <div className={styles.upcomingBadge}>Soon</div>
                    )}
                    {q.status === "completed" && (
                      <div className={styles.doneBadge}>Done</div>
                    )}
                  </div>

                  <span className={styles.cardCode}>{q.code}</span>
                  <p className={styles.cardCourse}>{q.courseName}</p>
                </div>

                {/* Content bottom */}
                <div className={styles.cardBottom}>
                  <p className={styles.cardTitle}>{q.title}</p>
                  {q.instructor ? (
                    <p className={styles.cardInstr}>👨‍🏫 {q.instructor}</p>
                  ) : (
                    <p className={styles.cardInstrEmpty} />
                  )}
                  <div className={styles.cardTime}>
                    <span>🕐</span>
                    <span>{q.startTime}</span>
                  </div>
                </div>
              </motion.button>
            );
          })}
        </div>
      )}

      {/* ── Empty state ── */}
      {!loading && quizzes.length === 0 && (
        <motion.div className={styles.empty}
          initial={{ opacity: 0, scale: 0.96 }} animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.38 }}>
          <motion.div className={styles.emptyIcon}
            animate={{ y: [0, -10, 0] }}
            transition={{ duration: 3, repeat: Infinity, ease: "easeInOut" }}>
            📭
          </motion.div>
          <p className={styles.emptyTitle}>No quizzes today</p>
          <p className={styles.emptySub}>
            No quizzes are scheduled for {todayLabel}.<br />Check back tomorrow!
          </p>
        </motion.div>
      )}

      {/* ── Live Quiz Popup ── */}
      <AnimatePresence>
        {active && (
          <motion.div className={styles.overlay}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={() => setActive(null)}>

            <motion.div className={styles.popup}
              initial={{ opacity: 0, y: 60, scale: 0.93 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 30, scale: 0.95 }}
              transition={{ duration: 0.38, ease: [0.22, 1, 0.36, 1] }}
              onClick={e => e.stopPropagation()}>

              <div className={styles.popHeader} style={{ background: active.color }}>
                <div className={styles.popHeaderGlow}
                  style={{ background: `radial-gradient(ellipse 70% 100% at 80% 0%, ${active.shade}99, transparent)` }} />
                <button className={styles.popClose} onClick={() => setActive(null)}>✕</button>

                <div className={styles.popHeaderBody}>
                  <div className={styles.popCourseTag}>
                    <span>{active.icon}</span>
                    {active.code} · {active.courseName}
                  </div>
                  <h2 className={styles.popTitle}>{active.title}</h2>
                  {active.instructor && (
                    <p className={styles.popInstr}>👨‍🏫 {active.instructor}</p>
                  )}
                </div>
              </div>

              <div className={styles.popBody}>
                <div className={styles.popGrid}>
                  {[
                    { icon: "🕐", label: "Start Time",  val: active.startTime },
                    { icon: "⏱",  label: "Duration",    val: active.duration },
                    { icon: "❓",  label: "Questions",   val: `${active.questions} MCQ` },
                    { icon: "⭐",  label: "Max Score",   val: `${active.max} pts` },
                    { icon: "⏰",  label: "Closes",      val: active.deadline },
                    active.instructor
                      ? { icon: "👨‍🏫", label: "Instructor", val: active.instructor }
                      : null,
                  ].filter(Boolean).map((d, idx) => (
                    <motion.div key={idx} className={styles.popItem}
                      initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: 0.05 + idx * 0.04 }}>
                      <div className={styles.popItemIcon}
                        style={{ background: `${active.color}15`, color: active.color }}>
                        {d.icon}
                      </div>
                      <div>
                        <span className={styles.popItemLabel}>{d.label}</span>
                        <span className={styles.popItemVal}>{d.val}</span>
                      </div>
                    </motion.div>
                  ))}
                </div>

                <div className={styles.popTip}>
                  💡 Make sure you have a stable connection before starting. The timer begins immediately after pressing Start.
                </div>
              </div>

              <div className={styles.popFooter}>
                <button className={styles.popCancel} onClick={() => setActive(null)}>
                  Cancel
                </button>
                <motion.button
                  className={styles.popGo}
                  style={{ background: active.color }}
                  onClick={() => navigate(
                    `/student/quiz/${active.courseId}/${active.quizId}`,
                    { state: { color: active.color, courseName: active.courseName } }
                  )}
                  whileHover={{ scale: 1.03, filter: "brightness(1.08)" }}
                  whileTap={{ scale: 0.97 }}>
                  🚀 Go to Quiz
                </motion.button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
