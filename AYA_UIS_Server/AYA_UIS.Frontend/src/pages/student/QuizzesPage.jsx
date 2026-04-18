// src/pages/student/QuizzesPage.jsx
import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { QUIZ_DB } from "./QuizDetail";
import styles from "./QuizzesPage.module.css";

/* ─── TODO: GET /api/student/quizzes/available → today's quizzes ─── */

const COURSE_META = {
  1: { code: "CS401", color: "#e8a838", shade: "#b07820", icon: "🤖" },
  2: { code: "CS402", color: "#7c6fc4", shade: "#4a3fa0", icon: "⚙️" },
  3: { code: "CS403", color: "#2e9e8a", shade: "#0a6a5a", icon: "🖼️" },
  4: { code: "CS404", color: "#e05c8a", shade: "#a01858", icon: "🧠" },
  5: { code: "CS405", color: "#5b9fb5", shade: "#2a6a80", icon: "💬" },
  6: { code: "CS406", color: "#3d8fe0", shade: "#0d5ab0", icon: "💻" },
};

// Pull available quizzes from QUIZ_DB
const TODAY_QUIZZES = Object.entries(QUIZ_DB).map(([key, quiz]) => {
  const [courseId, quizId] = key.split("-").map(Number);
  const meta = COURSE_META[courseId] || COURSE_META[1];
  return { ...quiz, courseId, quizId, ...meta };
});

export default function QuizzesPage() {
  const navigate  = useNavigate();
  const [active, setActive] = useState(null);

  return (
    <div className={styles.page}>

      {/* ── Page header ── */}
      <motion.div className={styles.header}
        initial={{ opacity: 0, y: -16 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4, ease: [0.22, 1, 0.36, 1] }}>

        <div className={styles.headerLeft}>
          <div className={styles.headerIcon}>✏️</div>
          <div>
            <h1 className={styles.title}>Today's Quizzes</h1>
            <p className={styles.subtitle}>
              {TODAY_QUIZZES.length} quiz{TODAY_QUIZZES.length !== 1 ? "zes" : ""} available right now
            </p>
          </div>
        </div>

        <div className={styles.dateBadge}>
          <span className={styles.dateIcon}>📅</span>
          <span>{new Date().toLocaleDateString("en-US", { weekday: "long", month: "long", day: "numeric" })}</span>
        </div>
      </motion.div>

      {/* ── Quiz grid ── */}
      {TODAY_QUIZZES.length === 0 ? (
        <div className={styles.empty}>
          <motion.div className={styles.emptyIcon}
            animate={{ y: [0, -10, 0] }}
            transition={{ duration: 3, repeat: Infinity, ease: "easeInOut" }}>
            🎉
          </motion.div>
          <p className={styles.emptyTitle}>No quizzes today!</p>
          <p className={styles.emptySub}>You're all caught up. Enjoy your free time.</p>
        </div>
      ) : (
        <div className={styles.grid}>
          {TODAY_QUIZZES.map((q, i) => (
            <motion.button
              key={`${q.courseId}-${q.quizId}`}
              className={styles.card}
              onClick={() => setActive(q)}
              initial={{ opacity: 0, y: 24, scale: 0.93 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              transition={{ delay: i * 0.07, duration: 0.42, ease: [0.22, 1, 0.36, 1] }}
              whileHover={{ y: -6, transition: { duration: 0.2 } }}
              whileTap={{ scale: 0.97 }}
            >
              {/* Color top bar */}
              <div className={styles.cardBar} style={{ background: q.color }} />

              {/* Course header */}
              <div className={styles.cardHead}>
                <div className={styles.cardIcon}
                  style={{ background: `${q.color}18`, color: q.color }}>
                  {q.icon}
                </div>
                <div>
                  <span className={styles.cardCode} style={{ color: q.color }}>{q.code}</span>
                  <p className={styles.cardCourse}>{q.courseName}</p>
                </div>
                <div className={styles.availBadge}>
                  <span className={styles.availDot} />
                  Live
                </div>
              </div>

              {/* Quiz name */}
              <p className={styles.cardTitle}>{q.title}</p>

              {/* Pills */}
              <div className={styles.cardPills}>
                <span className={styles.pill}>⏱ {q.duration} min</span>
                <span className={styles.pill}>❓ {q.questions} Qs</span>
              </div>

              {/* Deadline */}
              <div className={styles.cardDeadline}>
                <span>🕐</span> Closes: <strong>{q.deadline.split(" – ")[1] || q.deadline}</strong>
              </div>

              {/* CTA hint */}
              <div className={styles.cardCta} style={{ color: q.color }}>
                View details →
              </div>
            </motion.button>
          ))}
        </div>
      )}

      {/* ── Detail popup ── */}
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

              {/* Popup header */}
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
                  <p className={styles.popInstr}>👨‍🏫 {active.instructor}</p>
                </div>
              </div>

              {/* Details grid */}
              <div className={styles.popBody}>
                <div className={styles.popGrid}>
                  {[
                    { icon: "📅", label: "Date",       val: active.date },
                    { icon: "⏱",  label: "Duration",   val: `${active.duration} minutes` },
                    { icon: "❓",  label: "Questions",  val: `${active.questions} MCQ` },
                    { icon: "⭐",  label: "Max Score",  val: `${active.questions} pts` },
                    { icon: "⏰",  label: "Deadline",   val: active.deadline },
                    { icon: "👨‍🏫", label: "Instructor", val: active.instructor },
                  ].map((d, i) => (
                    <motion.div key={i} className={styles.popItem}
                      initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: 0.05 + i * 0.04 }}>
                      <div className={styles.popItemIcon}
                        style={{ background: `${active.color}15`, color: active.color }}>
                        {d.icon}
                      </div>
                      <div>
                        <div className={styles.popItemLabel}>{d.label}</div>
                        <div className={styles.popItemVal}>{d.val}</div>
                      </div>
                    </motion.div>
                  ))}
                </div>

                {/* Tip */}
                <div className={styles.popTip}>
                  💡 Make sure you have a stable connection before starting. The timer begins immediately after pressing Start.
                </div>
              </div>

              {/* Footer CTA */}
              <div className={styles.popFooter}>
                <button className={styles.popCancel} onClick={() => setActive(null)}>
                  Cancel
                </button>
                <motion.button className={styles.popGo}
                  style={{ background: active.color }}
                  onClick={() => navigate(`/student/quiz/${active.courseId}/${active.quizId}`)}
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