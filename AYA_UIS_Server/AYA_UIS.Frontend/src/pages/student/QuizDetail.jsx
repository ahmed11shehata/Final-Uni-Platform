// src/pages/student/QuizDetail.jsx
import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./QuizDetail.module.css";
import { getQuizDetail, submitQuiz } from "../../services/api/studentApi";

/* ── helpers ── */
function letterGrade(pct) {
  if (pct >= 90) return { letter: "A+", color: "#16a34a" };
  if (pct >= 85) return { letter: "A",  color: "#16a34a" };
  if (pct >= 80) return { letter: "A-", color: "#22c55e" };
  if (pct >= 75) return { letter: "B+", color: "#0ea5e9" };
  if (pct >= 70) return { letter: "B",  color: "#3b82f6" };
  if (pct >= 65) return { letter: "B-", color: "#6366f1" };
  if (pct >= 60) return { letter: "C+", color: "#f59e0b" };
  if (pct >= 55) return { letter: "C",  color: "#f59e0b" };
  return                { letter: "F",  color: "#ef4444" };
}

function fmtDate(iso) {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
}

function fmtDeadline(iso) {
  if (!iso) return "—";
  return new Date(iso).toLocaleString("en-US", { month: "short", day: "numeric", year: "numeric", hour: "2-digit", minute: "2-digit" });
}

const OPTION_LABELS = ["A", "B", "C", "D", "E"];

/* ════════════════════════════════════
   PHASE 1 — INFO
════════════════════════════════════ */
function InfoPhase({ quiz, color, courseName, onStart }) {
  const c = color;
  const shade = `color-mix(in srgb, ${c} 70%, #000)`;

  return (
    <div className={styles.infoPage}>
      <motion.div className={styles.infoHero}
        style={{ background: `linear-gradient(135deg, ${c} 0%, ${shade} 100%)` }}
        initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.48, ease: [0.22, 1, 0.36, 1] }}>

        <div className={styles.heroBubble1} style={{ background: "rgba(255,255,255,0.08)" }} />
        <div className={styles.heroBubble2} style={{ background: "rgba(255,255,255,0.05)" }} />

        <div className={styles.infoHeroContent}>
          <motion.span className={styles.infoCourseTag}
            initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}>
            {quiz.courseCode} · {courseName || quiz.courseCode}
          </motion.span>

          <motion.h1 className={styles.infoTitle}
            initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.18, duration: 0.42 }}>
            {quiz.title}
          </motion.h1>
        </div>

        <motion.div className={styles.infoRing}
          initial={{ scale: 0.7, opacity: 0 }} animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.22, type: "spring", stiffness: 260, damping: 20 }}>
          <span className={styles.infoRingNum}>{quiz.questionCount}</span>
          <span className={styles.infoRingLabel}>Questions</span>
        </motion.div>
      </motion.div>

      <div className={styles.infoBody}>
        <motion.div className={styles.infoGrid}
          initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3, duration: 0.4 }}>
          {[
            { icon: "📅", label: "Date",         val: fmtDate(quiz.startTime) },
            { icon: "⏱",  label: "Duration",     val: `${quiz.duration} minutes` },
            { icon: "❓",  label: "Questions",    val: `${quiz.questionCount} MCQ` },
            { icon: "⭐",  label: "Total Points", val: `${quiz.totalPoints ?? quiz.questionCount} pts` },
            { icon: "⏰",  label: "Closes At",    val: fmtDeadline(quiz.endTime) },
            { icon: "🎯",  label: "Per Question", val: `${(quiz.totalPoints && quiz.questionCount) ? Math.round((quiz.totalPoints / quiz.questionCount) * 10) / 10 : 1} pt each` },
          ].map((d, i) => (
            <motion.div key={i} className={styles.infoCard}
              style={{ borderColor: `${c}22` }}
              initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.34 + i * 0.05 }}>
              <div className={styles.infoCardIcon}
                style={{ background: `${c}14`, color: c }}>{d.icon}</div>
              <div className={styles.infoCardLabel}>{d.label}</div>
              <div className={styles.infoCardVal}>{d.val}</div>
            </motion.div>
          ))}
        </motion.div>

        <motion.div className={styles.infoRules}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }}
          transition={{ delay: 0.52 }}>
          <div className={styles.infoRulesTitle}>📋 Before you start</div>
          <ul className={styles.infoRulesList}>
            <li>Timer starts immediately when you click <strong>Start Now</strong></li>
            <li>Navigate freely between questions using Prev / Next</li>
            <li>Unanswered questions count as wrong</li>
            <li>You cannot retake this quiz after submission</li>
          </ul>
        </motion.div>

        <motion.button className={styles.startBtn}
          style={{ background: `linear-gradient(135deg, ${c}, ${shade})` }}
          onClick={onStart}
          initial={{ opacity: 0, y: 16, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          transition={{ delay: 0.58, type: "spring", stiffness: 360, damping: 26 }}
          whileHover={{ scale: 1.04, filter: "brightness(1.06)" }}
          whileTap={{ scale: 0.97 }}>
          🚀 Start Now
        </motion.button>
      </div>
    </div>
  );
}

/* ════════════════════════════════════
   PHASE 2 — EXAM
════════════════════════════════════ */
function ExamPhase({ quiz, color, onSubmit }) {
  const c = color;
  const questions = quiz.questions || [];
  const total = questions.length;

  const [idx, setIdx]           = useState(0);
  const [answers, setAnswers]   = useState(() => new Array(total).fill(null)); // null | optionId
  const [timeLeft, setTimeLeft] = useState(quiz.duration * 60);
  const [showSubmit, setShowSubmit] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr]           = useState("");

  // Keep a ref so timer callback always sees latest answers
  const answersRef = useRef(answers);
  useEffect(() => { answersRef.current = answers; }, [answers]);

  useEffect(() => {
    const t = setInterval(() => {
      setTimeLeft(s => {
        if (s <= 1) {
          clearInterval(t);
          const cur = answersRef.current;
          const payload = questions
            .map((q, i) => cur[i] !== null ? { questionId: q.id, selectedOptionId: cur[i] } : null)
            .filter(Boolean);
          submitQuiz(quiz.id, payload)
            .then(res => onSubmit(res?.data ?? 0, total))
            .catch(() => onSubmit(0, total));
          return 0;
        }
        return s - 1;
      });
    }, 1000);
    return () => clearInterval(t);
  }, []);

  const mins = String(Math.floor(timeLeft / 60)).padStart(2, "0");
  const secs = String(timeLeft % 60).padStart(2, "0");
  const timerDanger = timeLeft <= 60;
  const answered = answers.filter(a => a !== null).length;
  const q = questions[idx];

  const pick = (optionId) => {
    setAnswers(prev => { const n = [...prev]; n[idx] = optionId; return n; });
  };

  const handleSubmit = async () => {
    setSubmitting(true);
    setErr("");
    try {
      const payload = questions
        .map((q, i) => answers[i] !== null ? { questionId: q.id, selectedOptionId: answers[i] } : null)
        .filter(Boolean);

      const res = await submitQuiz(quiz.id, payload);
      const score = res?.data ?? 0;
      onSubmit(score, total);
    } catch (e) {
      setErr(e?.response?.data?.error?.message || "Submission failed. Try again.");
      setSubmitting(false);
      setShowSubmit(false);
    }
  };

  if (!q) return null;

  return (
    <div className={styles.examPage}>
      <div className={styles.examTopBar}>
        <div className={styles.examCourseInfo}>
          <div className={styles.examDot} style={{ background: c }} />
          <div>
            <div className={styles.examCourseCode} style={{ color: c }}>{quiz.courseCode}</div>
            <div className={styles.examQuizTitle}>{quiz.title}</div>
          </div>
        </div>

        <div className={styles.examCenter}>
          <span className={styles.examQCount}>
            Question <strong style={{ color: c }}>{idx + 1}</strong> / {total}
          </span>
          <div className={styles.examProgressTrack}>
            <motion.div className={styles.examProgressFill}
              style={{ background: c }}
              animate={{ width: `${((idx + 1) / total) * 100}%` }}
              transition={{ duration: 0.3 }}/>
          </div>
        </div>

        <motion.div className={styles.timer}
          style={{
            background: timerDanger ? "#fef2f2" : "#f8fafc",
            borderColor: timerDanger ? "#fca5a5" : "#e2e8f0",
            color: timerDanger ? "#dc2626" : "#334155",
          }}
          animate={timerDanger ? { scale: [1, 1.04, 1] } : {}}
          transition={{ duration: 0.6, repeat: timerDanger ? Infinity : 0 }}>
          {timerDanger ? "🔴" : "⏱"} {mins}:{secs}
        </motion.div>
      </div>

      <div className={styles.examBody}>
        <AnimatePresence mode="wait">
          <motion.div key={idx} className={styles.questionCard}
            initial={{ opacity: 0, x: 40 }} animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -30 }}
            transition={{ duration: 0.26, ease: [0.22, 1, 0.36, 1] }}>

            <div className={styles.questionTag} style={{ background: `${c}15`, color: c }}>
              Q{idx + 1}
            </div>

            <p className={styles.questionText}>{q.text}</p>

            <div className={styles.optionsList}>
              {q.options.map((opt, oi) => {
                const picked = answers[idx] === opt.id;
                return (
                  <motion.button key={opt.id}
                    className={`${styles.option} ${picked ? styles.optionPicked : ""}`}
                    style={picked ? { background: `${c}12`, borderColor: c, boxShadow: `0 0 0 3px ${c}20` } : {}}
                    onClick={() => pick(opt.id)}
                    whileHover={{ x: 4, transition: { duration: 0.14 } }}
                    whileTap={{ scale: 0.98 }}>
                    <div className={styles.optionLabel}
                      style={picked ? { background: c, color: "white", borderColor: c } : {}}>
                      {OPTION_LABELS[oi]}
                    </div>
                    <span className={styles.optionText}>{opt.text}</span>
                    {picked && (
                      <motion.div className={styles.optionCheck} style={{ color: c }}
                        initial={{ scale: 0 }} animate={{ scale: 1 }}
                        transition={{ type: "spring", stiffness: 500, damping: 24 }}>
                        ✓
                      </motion.div>
                    )}
                  </motion.button>
                );
              })}
            </div>
          </motion.div>
        </AnimatePresence>

        <div className={styles.dotRow}>
          {questions.map((_, di) => (
            <button key={di} className={styles.dot}
              style={{
                background: di === idx ? c : answers[di] !== null ? `${c}55` : "#e2e8f0",
                transform: di === idx ? "scale(1.3)" : "scale(1)",
              }}
              onClick={() => setIdx(di)}
              title={`Q${di + 1}`}/>
          ))}
        </div>

        {err && <p style={{ color: "#ef4444", textAlign: "center", fontSize: 14 }}>{err}</p>}

        <div className={styles.navRow}>
          <motion.button className={styles.navBtn}
            disabled={idx === 0}
            onClick={() => setIdx(i => i - 1)}
            whileHover={idx > 0 ? { scale: 1.04 } : {}}
            whileTap={idx > 0 ? { scale: 0.96 } : {}}>
            ← Prev
          </motion.button>

          <div className={styles.answeredBadge}>
            <span style={{ color: c, fontWeight: 800 }}>{answered}</span>
            <span> / {total} answered</span>
          </div>

          {idx < total - 1 ? (
            <motion.button className={styles.navBtnNext} style={{ background: c }}
              onClick={() => setIdx(i => i + 1)}
              whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
              Next →
            </motion.button>
          ) : (
            <motion.button className={styles.submitBtn} style={{ background: c }}
              onClick={() => setShowSubmit(true)}
              disabled={submitting}
              whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
              Submit ✓
            </motion.button>
          )}
        </div>
      </div>

      <AnimatePresence>
        {showSubmit && (
          <motion.div className={styles.confirmOverlay}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
            <motion.div className={styles.confirmBox}
              initial={{ scale: 0.88, y: 30 }} animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.9, y: 20 }}
              transition={{ type: "spring", stiffness: 400, damping: 28 }}>
              <div className={styles.confirmEmoji}>🚨</div>
              <h3 className={styles.confirmTitle}>Submit Quiz?</h3>
              <p className={styles.confirmText}>
                You answered <strong style={{ color: c }}>{answered}</strong> out of <strong>{total}</strong> questions.
                {answered < total && <><br/><span style={{ color: "#ef4444" }}>⚠️ {total - answered} unanswered will be marked wrong.</span></>}
              </p>
              <div className={styles.confirmBtns}>
                <button className={styles.confirmCancel}
                  onClick={() => setShowSubmit(false)}>Keep going</button>
                <motion.button className={styles.confirmSubmit}
                  style={{ background: c }}
                  onClick={handleSubmit}
                  disabled={submitting}
                  whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
                  {submitting ? "Submitting…" : "Yes, submit"}
                </motion.button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

/* ════════════════════════════════════
   PHASE 3 — RESULTS
════════════════════════════════════ */
function ResultsPhase({ quiz, score, total, reviewAvailable, color, onBack }) {
  const c = color;
  const pct   = total > 0 ? Math.round((score / total) * 100) : 0;
  const grade = letterGrade(pct);
  const circ  = 2 * Math.PI * 54;

  const [showReview, setShowReview] = useState(false);

  return (
    <div className={styles.resultsPage}>
      {score / total >= 0.7 && (
        <div className={styles.confettiWrap}>
          {Array.from({ length: 18 }).map((_, i) => (
            <motion.div key={i} className={styles.confetti}
              style={{
                left: `${Math.random() * 100}%`,
                background: [c, "#22c55e", "#f59e0b", "#ef4444", "#6366f1"][i % 5],
              }}
              initial={{ y: -20, opacity: 0, rotate: 0 }}
              animate={{ y: "110vh", opacity: [0, 1, 1, 0], rotate: Math.random() * 360 }}
              transition={{ delay: Math.random() * 0.8, duration: 2.5 + Math.random() * 1.5, ease: "linear" }}/>
          ))}
        </div>
      )}

      <div className={styles.resultsInner}>
        <motion.div className={styles.scoreRingWrap}
          initial={{ scale: 0.5, opacity: 0 }} animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.1, type: "spring", stiffness: 240, damping: 20 }}>
          <svg width="128" height="128" viewBox="0 0 128 128"
            style={{ transform: "rotate(-90deg)" }}>
            <circle cx="64" cy="64" r="54" fill="none" stroke="#f1f5f9" strokeWidth="10"/>
            <motion.circle cx="64" cy="64" r="54" fill="none"
              stroke={c} strokeWidth="10" strokeLinecap="round"
              strokeDasharray={circ}
              initial={{ strokeDashoffset: circ }}
              animate={{ strokeDashoffset: circ * (1 - pct / 100) }}
              transition={{ delay: 0.3, duration: 1.4, ease: [0.22, 1, 0.36, 1] }}/>
          </svg>
          <div className={styles.scoreRingCenter}>
            <motion.span className={styles.scoreNum} style={{ color: c }}
              initial={{ opacity: 0, scale: 0.7 }} animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.5 }}>
              {score}
            </motion.span>
            <span className={styles.scoreTotal}>/{total}</span>
          </div>
        </motion.div>

        <motion.div className={styles.gradeBlock}
          initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}>
          <div className={styles.gradeLetter}
            style={{ color: grade.color, borderColor: `${grade.color}35`, background: `${grade.color}10` }}>
            {grade.letter}
          </div>
          <div className={styles.pctText} style={{ color: c }}>{pct}%</div>
          <p className={styles.resultMsg}>
            {pct >= 90 ? "🏆 Outstanding! Perfect performance!" :
             pct >= 75 ? "🌟 Great work! Keep it up!" :
             pct >= 60 ? "👍 Good effort! You passed." :
             "📚 Don't give up. Review and try again!"}
          </p>
        </motion.div>

        <motion.div className={styles.statsRow}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }}
          transition={{ delay: 0.65 }}>
          {[
            { icon: "✅", label: "Correct", val: score,         col: "#22c55e" },
            { icon: "❌", label: "Wrong",   val: total - score, col: "#ef4444" },
            { icon: "📊", label: "Score",   val: `${pct}%`,     col: c },
          ].map((s, i) => (
            <div key={i} className={styles.statBox}>
              <span className={styles.statIcon}>{s.icon}</span>
              <span className={styles.statVal} style={{ color: s.col }}>{s.val}</span>
              <span className={styles.statLabel}>{s.label}</span>
            </div>
          ))}
        </motion.div>

        <motion.div className={styles.resultBtns}
          initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.75 }}>
          {reviewAvailable ? (
            <button className={styles.reviewBtn}
              onClick={() => setShowReview(v => !v)}>
              {showReview ? "Hide Review" : "📋 View My Answers"}
            </button>
          ) : (
            <div className={styles.reviewLocked}>
              🔒 Review will be available once the instructor releases results.
            </div>
          )}
          <motion.button className={styles.backBtn} style={{ background: c }}
            onClick={onBack}
            whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.97 }}>
            ← Back to Course
          </motion.button>
        </motion.div>

        {reviewAvailable && (
          <AnimatePresence>
            {showReview && (
              <motion.div className={styles.reviewList}
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: "auto" }}
                exit={{ opacity: 0, height: 0 }}
                transition={{ duration: 0.3 }}>
                {(quiz.questions || []).map((q, i) => (
                  <motion.div key={q.id} className={styles.reviewItem}
                    style={{ borderColor: "#e2e8f0", background: "var(--card-bg)" }}
                    initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: i * 0.04 }}>
                    <div className={styles.reviewQ}>
                      <span className={styles.reviewQNum}
                        style={{ background: c }}>Q{i + 1}</span>
                      <p className={styles.reviewQText}>{q.text}</p>
                    </div>
                    <div className={styles.reviewAnswers}>
                      {(q.options || []).map((o) => {
                        // TODO: confirm field names — backend may use mySelectedOptionId / isCorrect
                        const wasSelected = o.id === (q.mySelectedOptionId ?? null);
                        const isCorrect   = o.isCorrect ?? false; // only populated if review is allowed by backend
                        const highlight   = wasSelected || isCorrect;
                        return (
                          <div key={o.id} style={{
                            display: "flex", alignItems: "center", gap: 8,
                            padding: "7px 10px", borderRadius: 8, marginBottom: 4,
                            background: isCorrect
                              ? "rgba(34,197,94,.1)"
                              : wasSelected
                              ? "rgba(239,68,68,.08)"
                              : "transparent",
                            border: `1px solid ${isCorrect
                              ? "rgba(34,197,94,.3)"
                              : wasSelected
                              ? "rgba(239,68,68,.28)"
                              : "transparent"}`,
                            color: "var(--text-primary)",
                            fontSize: 13,
                            fontWeight: highlight ? 700 : 400,
                          }}>
                            <span style={{
                              width: 18, flexShrink: 0, fontSize: 14,
                              color: isCorrect ? "#22c55e" : wasSelected ? "#ef4444" : "transparent",
                            }}>
                              {isCorrect ? "✓" : wasSelected ? "✗" : ""}
                            </span>
                            <span style={{ flex: 1 }}>{o.text}</span>
                            {wasSelected && (
                              <span style={{ fontSize: 11, color: "#94a3b8", flexShrink: 0 }}>
                                Your answer
                              </span>
                            )}
                            {isCorrect && !wasSelected && (
                              <span style={{ fontSize: 11, color: "#22c55e", flexShrink: 0 }}>
                                Correct
                              </span>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  </motion.div>
                ))}
              </motion.div>
            )}
          </AnimatePresence>
        )}
      </div>
    </div>
  );
}

/* ════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════ */
export default function QuizDetail() {
  const { courseId, quizId } = useParams();
  const navigate  = useNavigate();
  const location  = useLocation();

  const stateColor      = location.state?.color;
  const stateCourseName = location.state?.courseName;
  const color           = stateColor || "#818cf8";

  const [quiz,    setQuiz]    = useState(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState("");
  const [phase,   setPhase]   = useState("info");   // info | exam | results
  const [score,   setScore]   = useState(0);

  useEffect(() => {
    (async () => {
      try {
        const res = await getQuizDetail(quizId);
        const q = res?.data ?? res;
        setQuiz(q);
        // already submitted → go straight to results
        if (q.myScore !== null && q.myScore !== undefined) {
          setScore(q.myScore);
          setPhase("results");
        }
      } catch (e) {
        setError(e?.response?.data?.error?.message || "Could not load quiz.");
      } finally {
        setLoading(false);
      }
    })();
  }, [quizId]);

  const handleBack = () => navigate(`/student/courses/${courseId}`);

  if (loading) return (
    <div style={{ display: "flex", justifyContent: "center", padding: "80px 0", color: "var(--text-muted)" }}>
      Loading quiz…
    </div>
  );

  if (error || !quiz) return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", padding: "80px 0", gap: 16 }}>
      <p style={{ color: "#ef4444" }}>{error || "Quiz not found."}</p>
      <button onClick={handleBack}
        style={{ padding: "8px 20px", background: color, color: "#fff", border: "none", borderRadius: 8, cursor: "pointer" }}>
        Back to Course
      </button>
    </div>
  );

  return (
    <AnimatePresence mode="wait">
      {phase === "info" && (
        <motion.div key="info"
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
          <InfoPhase quiz={quiz} color={color} courseName={stateCourseName}
            onStart={() => setPhase("exam")}/>
        </motion.div>
      )}
      {phase === "exam" && (
        <motion.div key="exam"
          initial={{ opacity: 0, x: 30 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -30 }}>
          <ExamPhase quiz={quiz} color={color}
            onSubmit={(s, t) => { setScore(s); setPhase("results"); }}/>
        </motion.div>
      )}
      {phase === "results" && (
        <motion.div key="results"
          initial={{ opacity: 0, scale: 0.96 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }}>
          <ResultsPhase quiz={quiz} score={score} total={quiz.totalPoints ?? quiz.questionCount}
            reviewAvailable={quiz.reviewAvailable} color={color} onBack={handleBack}/>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
