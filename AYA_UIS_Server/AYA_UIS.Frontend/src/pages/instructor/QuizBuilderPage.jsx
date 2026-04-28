// src/pages/instructor/QuizBuilderPage.jsx
import { useCallback, useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./QuizBuilderPage.module.css";
import {
  createQuiz,
  deleteQuiz,
  getCourseworkBudget,
  getInstructorCourses,
  getInstructorQuizzes,
  updateQuiz,
} from "../../services/api/instructorApi";

const LETTERS = ["A", "B", "C", "D", "E", "F"];
const spring = { type: "spring", stiffness: 320, damping: 28 };

const makeQ = () => ({
  id: Date.now() + Math.random(),
  text: "",
  answers: [{ text: "" }, { text: "" }, { text: "" }, { text: "" }],
  correct: 0,
});

function CourseSelector({ courses, courseId, onSelect }) {
  if (courses.length === 0) {
    return (
      <div className={styles.warningCard}>
        <span className={styles.warningIcon}>⚠️</span>
        <div>
          <p className={styles.warningTitle}>No assigned courses</p>
          <p className={styles.warningText}>This account is not responsible for any course yet.</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.courseGrid}>
      {courses.map((course, index) => {
        const active = courseId === course.id;
        return (
          <motion.button
            key={course.id}
            type="button"
            className={`${styles.courseCard} ${active ? styles.courseCardActive : ""}`}
            style={{ "--accent": course.color || "#7c3aed" }}
            onClick={() => onSelect(course.id)}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ ...spring, delay: index * 0.04 }}
            whileHover={{ y: -3 }}
            whileTap={{ scale: 0.985 }}
          >
            <div className={styles.courseBadge}>{course.icon}</div>
            <div className={styles.courseCopy}>
              <span className={styles.courseCode}>{course.code}</span>
              <span className={styles.courseName}>{course.name}</span>
            </div>
            <span className={styles.courseFlag}>{active ? "Selected" : "Pick"}</span>
          </motion.button>
        );
      })}
    </div>
  );
}

function SettingCard({ icon, title, subtitle, color, children, wide }) {
  return (
    <div
      className={`${styles.settingCard} ${wide ? styles.settingCardWide : ""}`}
      style={{ "--accent": color }}
    >
      <div className={styles.settingHead}>
        <div className={styles.settingIcon}>{icon}</div>
        <div>
          <h3 className={styles.settingTitle}>{title}</h3>
          <p className={styles.settingSubtitle}>{subtitle}</p>
        </div>
      </div>
      <div className={styles.settingBody}>{children}</div>
    </div>
  );
}

function StepIndicator({ step, setStep, color, questions }) {
  const tabs = [
    {
      n: 1,
      icon: "📋",
      title: "Quiz settings",
      desc: "Course, title, time, grading",
    },
    {
      n: 2,
      icon: "❓",
      title: "Questions",
      desc: `${questions.length} question${questions.length !== 1 ? "s" : ""}`,
    },
  ];

  return (
    <div className={styles.stepRail}>
      {tabs.map((tab) => {
        const active = step === tab.n;
        const complete = step > tab.n;
        return (
          <button
            key={tab.n}
            type="button"
            className={`${styles.stepCard} ${active ? styles.stepCardActive : ""}`}
            style={{ "--accent": color }}
            onClick={() => complete && setStep(tab.n)}
          >
            <div className={`${styles.stepNumber} ${complete ? styles.stepNumberDone : ""}`}>
              {complete ? "✓" : tab.n}
            </div>
            <div className={styles.stepCopy}>
              <strong>
                {tab.icon} {tab.title}
              </strong>
              <span>{tab.desc}</span>
            </div>
          </button>
        );
      })}
    </div>
  );
}

function AnswerRow({ answer, idx, correct, onText, onCorrect, onRemove, canRemove, color }) {
  return (
    <div className={`${styles.answerRow} ${correct ? styles.answerRowCorrect : ""}`} style={{ "--accent": color }}>
      <div className={`${styles.answerLetter} ${correct ? styles.answerLetterCorrect : ""}`}>{LETTERS[idx]}</div>
      <input
        className={styles.answerInput}
        placeholder={`Answer ${LETTERS[idx]}…`}
        value={answer.text}
        onChange={(e) => onText(e.target.value)}
      />
      <button
        type="button"
        className={`${styles.correctButton} ${correct ? styles.correctButtonActive : ""}`}
        onClick={onCorrect}
      >
        {correct ? "Correct" : "Mark correct"}
      </button>
      {canRemove && (
        <button type="button" className={styles.removeButton} onClick={onRemove}>
          ✕
        </button>
      )}
    </div>
  );
}

function QuestionCard({ q, idx, color, onChange, onDelete, canDelete }) {
  return (
    <motion.div
      className={styles.questionCard}
      style={{ "--accent": color }}
      layout
      initial={{ opacity: 0, y: 18 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, x: -20, scale: 0.97 }}
      transition={spring}
    >
      <div className={styles.questionHead}>
        <div className={styles.questionBadge}>Q{idx + 1}</div>
        <div className={styles.questionMain}>
          <label className={styles.fieldLabel}>Question {idx + 1}</label>
          <textarea
            className={styles.textarea}
            placeholder="Type your question here…"
            value={q.text}
            rows={3}
            onChange={(e) => onChange({ ...q, text: e.target.value })}
          />
        </div>
        {canDelete && (
          <button type="button" className={styles.deleteQuestion} onClick={onDelete}>
            Delete
          </button>
        )}
      </div>

      <div className={styles.answerSection}>
        <div className={styles.answerSectionTop}>
          <span>Answer options</span>
          <small>{q.answers.length}/6 options</small>
        </div>

        {q.answers.map((answer, ai) => (
          <AnswerRow
            key={ai}
            answer={answer}
            idx={ai}
            correct={q.correct === ai}
            color={color}
            onText={(value) =>
              onChange({
                ...q,
                answers: q.answers.map((item, i) => (i === ai ? { ...item, text: value } : item)),
              })
            }
            onCorrect={() => onChange({ ...q, correct: ai })}
            onRemove={() => {
              const nextAnswers = q.answers.filter((_, i) => i !== ai);
              onChange({
                ...q,
                answers: nextAnswers,
                correct: q.correct >= nextAnswers.length ? nextAnswers.length - 1 : q.correct,
              });
            }}
            canRemove={q.answers.length > 2}
          />
        ))}

        {q.answers.length < 6 && (
          <button
            type="button"
            className={styles.addOption}
            onClick={() => onChange({ ...q, answers: [...q.answers, { text: "" }] })}
          >
            + Add option
          </button>
        )}
      </div>
    </motion.div>
  );
}

function StepSettings({ data, onChange, color }) {
  const isDurCustom = data.duration && ![10, 15, 20, 30, 45, 60, 90].includes(data.duration);
  const showPreview = data.title && data.startDate && data.startTime && data.duration;

  return (
    <div className={styles.settingsGrid}>
      <SettingCard icon="📋" title="Quiz title" subtitle="Name your assessment clearly" color={color}>
        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>
            Title <span className={styles.required}>*</span>
          </label>
          <input
            className={styles.input}
            placeholder="e.g. Quiz 3 — ML Fundamentals"
            value={data.title}
            onChange={(e) => onChange({ ...data, title: e.target.value })}
          />
        </div>
      </SettingCard>

      <SettingCard icon="⏱️" title="Duration" subtitle="Students race the timer after they start" color={color}>
        <div className={styles.durationGrid}>
          {[10, 15, 20, 30, 45, 60, 90].map((minutes) => (
            <button
              key={minutes}
              type="button"
              className={`${styles.durationPill} ${data.duration === minutes && !isDurCustom ? styles.durationPillActive : ""}`}
              onClick={() => onChange({ ...data, duration: minutes })}
            >
              {minutes}m
            </button>
          ))}
          <div className={styles.customDurationWrap}>
            <input
              type="number"
              min={1}
              max={180}
              className={styles.customDurationInput}
              placeholder="Custom"
              value={isDurCustom ? data.duration : ""}
              onChange={(e) => e.target.value && onChange({ ...data, duration: Number(e.target.value) })}
            />
            <span className={styles.unitLabel}>min</span>
          </div>
        </div>
      </SettingCard>

      <SettingCard icon="🗓️" title="Schedule" subtitle="Define when the quiz opens and closes" color={color}>
        <div className={styles.twoCol}>
          <div className={styles.fieldBlock}>
            <label className={styles.fieldLabel}>
              Start date <span className={styles.required}>*</span>
            </label>
            <input
              type="date"
              className={styles.input}
              value={data.startDate}
              onChange={(e) => onChange({ ...data, startDate: e.target.value })}
              min={new Date().toISOString().split("T")[0]}
            />
          </div>
          <div className={styles.fieldBlock}>
            <label className={styles.fieldLabel}>
              Start time <span className={styles.required}>*</span>
            </label>
            <input
              type="time"
              className={styles.input}
              value={data.startTime}
              onChange={(e) => onChange({ ...data, startTime: e.target.value })}
            />
          </div>
          <div className={styles.fieldBlock}>
            <label className={styles.fieldLabel}>
              Deadline date <span className={styles.required}>*</span>
            </label>
            <p className={styles.hint}>No new starts after this date.</p>
            <input
              type="date"
              className={styles.input}
              value={data.deadlineDate}
              onChange={(e) => onChange({ ...data, deadlineDate: e.target.value })}
              min={data.startDate || new Date().toISOString().split("T")[0]}
            />
          </div>
          <div className={styles.fieldBlock}>
            <label className={styles.fieldLabel}>
              Deadline time <span className={styles.required}>*</span>
            </label>
            <p className={styles.hint}>Quiz locks at this exact time.</p>
            <input
              type="time"
              className={styles.input}
              value={data.deadlineTime}
              onChange={(e) => onChange({ ...data, deadlineTime: e.target.value })}
            />
          </div>
        </div>
      </SettingCard>

      <SettingCard icon="⚙️" title="Options" subtitle="Grading and randomization behavior" color={color}>
        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>Points per question</label>
          <input
            type="number"
            min={1}
            max={10}
            className={styles.input}
            value={data.gradePerQ || ""}
            placeholder="1"
            onChange={(e) => onChange({ ...data, gradePerQ: Number(e.target.value) })}
          />
        </div>

        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>Shuffle</label>
          <div className={styles.toggleRow}>
            <button
              type="button"
              className={`${styles.toggleChip} ${data.shuffleQ ? styles.toggleChipActive : ""}`}
              onClick={() => onChange({ ...data, shuffleQ: !data.shuffleQ })}
            >
              <span className={styles.toggleDot} />
              Shuffle questions
            </button>
            <button
              type="button"
              className={`${styles.toggleChip} ${data.shuffleA ? styles.toggleChipActive : ""}`}
              onClick={() => onChange({ ...data, shuffleA: !data.shuffleA })}
            >
              <span className={styles.toggleDot} />
              Shuffle answers
            </button>
          </div>
          <div className={styles.infoBox}>
            <strong>Shuffle Questions</strong> changes the question order per student. <br />
            <strong>Shuffle Answers</strong> randomizes A/B/C/D options to reduce copying.
          </div>
        </div>
      </SettingCard>

      <AnimatePresence>
        {showPreview && (
          <motion.div
            className={styles.previewCard}
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
          >
            <div className={styles.previewHead}>
              <span className={styles.previewKicker}>Ready to continue</span>
              <h3 className={styles.previewTitle}>Quiz summary</h3>
            </div>
            <div className={styles.previewGrid}>
              {[
                { label: "Title", value: data.title },
                { label: "Opens", value: `${data.startDate} · ${data.startTime}` },
                { label: "Closes", value: `${data.deadlineDate || "—"} · ${data.deadlineTime || "—"}` },
                { label: "Duration", value: `${data.duration} minutes` },
                { label: "Pts / question", value: data.gradePerQ || 1 },
                {
                  label: "Shuffle",
                  value:
                    [data.shuffleQ ? "Questions" : "", data.shuffleA ? "Answers" : ""]
                      .filter(Boolean)
                      .join(", ") || "Off",
                },
              ].map((item) => (
                <div key={item.label} className={styles.previewItem}>
                  <span>{item.label}</span>
                  <strong>{item.value}</strong>
                </div>
              ))}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

function StepQuestions({ questions, setQuestions, color }) {
  const addQuestion = () => setQuestions((prev) => [...prev, makeQ()]);
  const filled = questions.filter((q) => q.text.trim() && q.answers.some((a) => a.text.trim())).length;
  const correctMarked = questions.filter((q) => q.answers[q.correct]?.text.trim()).length;
  const progress = questions.length > 0 ? Math.round((filled / questions.length) * 100) : 0;

  const bulkAdd = (target) => {
    const current = questions.length;
    if (target <= current) return;
    const toAdd = Array.from({ length: target - current }, makeQ);
    setQuestions((prev) => [...prev, ...toAdd]);
  };

  return (
    <div className={styles.questionsLayout}>
      <div className={styles.questionsMain}>
        <div className={styles.listHeader}>
          <div>
            <h3 className={styles.listTitle}>{questions.length} question{questions.length !== 1 ? "s" : ""}</h3>
            <p className={styles.listText}>{filled} filled · choose exactly one correct answer per question</p>
          </div>
          <motion.button
            type="button"
            className={styles.primaryButton}
            style={{ "--accent": color }}
            onClick={addQuestion}
            whileHover={{ y: -2 }}
            whileTap={{ scale: 0.985 }}
          >
            + Add question
          </motion.button>
        </div>

        {questions.length === 0 && (
          <div className={styles.emptyQuestions}>
            <div className={styles.emptyQuestionsIcon}>❓</div>
            <h3 className={styles.emptyQuestionsTitle}>No questions yet</h3>
            <p className={styles.emptyQuestionsText}>Set the target count from the sidebar or create your first question now.</p>
            <motion.button
              type="button"
              className={styles.primaryButton}
              style={{ "--accent": color }}
              onClick={addQuestion}
              whileHover={{ y: -2 }}
              whileTap={{ scale: 0.985 }}
            >
              + Add first question
            </motion.button>
          </div>
        )}

        <AnimatePresence>
          {questions.map((question, index) => (
            <QuestionCard
              key={question.id}
              q={question}
              idx={index}
              color={color}
              onChange={(nextQuestion) =>
                setQuestions((prev) => prev.map((item) => (item.id === question.id ? nextQuestion : item)))
              }
              onDelete={() => setQuestions((prev) => prev.filter((item) => item.id !== question.id))}
              canDelete={questions.length > 1}
            />
          ))}
        </AnimatePresence>

        {questions.length > 0 && (
          <button type="button" className={styles.secondaryButton} onClick={addQuestion}>
            + Add another question
          </button>
        )}
      </div>

      <div className={styles.sidebar}>
        <div className={styles.sideCard}>
          <div className={styles.sideHead}>🔢 Number of questions</div>
          <div className={styles.sideBody}>
            <div className={styles.counterWrap}>
              <button
                type="button"
                className={styles.counterButton}
                onClick={() => questions.length > 1 && setQuestions((prev) => prev.slice(0, -1))}
              >
                −
              </button>
              <input
                type="number"
                min={1}
                max={50}
                className={styles.counterInput}
                value={questions.length}
                onChange={(e) => {
                  const next = Number(e.target.value);
                  if (next < 1 || next > 50) return;
                  if (next > questions.length) bulkAdd(next);
                  else if (next < questions.length) setQuestions((prev) => prev.slice(0, next));
                }}
              />
              <button
                type="button"
                className={styles.counterButton}
                onClick={() => questions.length < 50 && addQuestion()}
              >
                +
              </button>
            </div>
            <p className={styles.sideHint}>Increase or reduce the full list in bulk. Maximum 50 questions.</p>
          </div>
        </div>

        <div className={styles.sideCard}>
          <div className={styles.sideHead}>📊 Progress</div>
          <div className={styles.sideBody}>
            <div className={styles.metricRow}>
              <span>Questions added</span>
              <strong style={{ color }}>{questions.length}</strong>
            </div>
            <div className={styles.metricRow}>
              <span>Filled</span>
              <strong style={{ color: "#22c55e" }}>{filled}</strong>
            </div>
            <div className={styles.metricRow}>
              <span>Correct marked</span>
              <strong style={{ color: "#38bdf8" }}>{correctMarked}</strong>
            </div>
            <div className={styles.progressTrack}>
              <div className={styles.progressFill} style={{ width: `${progress}%`, background: progress === 100 ? "#22c55e" : color }} />
            </div>
            <p className={styles.sideHint}>{progress}% complete</p>
          </div>
        </div>

        <div className={styles.sideCard}>
          <div className={styles.sideHead}>💡 Tips</div>
          <div className={styles.sideBody}>
            {[
              "Each question needs at least 2 options.",
              "Mark one option as correct for every question.",
              "You can add up to 6 answer choices.",
              "Keep question wording short and unambiguous.",
            ].map((tip) => (
              <div key={tip} className={styles.tipRow}>
                <span className={styles.tipDot} style={{ background: color }} />
                <span>{tip}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ── Quiz cards / edit / delete ─────────────────────────────── */
function QuizCard({ q, color, onEdit, onDelete }) {
  const statusColor = q.status === "active" ? "#22c55e" : q.status === "upcoming" ? "#818cf8" : "#94a3b8";
  return (
    <motion.div
      layout
      initial={{ opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.97 }}
      style={{
        position: "relative",
        background: "var(--card-bg)",
        border: "1px solid var(--border)",
        borderRadius: 18,
        padding: 18,
        display: "flex",
        flexDirection: "column",
        gap: 10,
        boxShadow: "0 8px 24px rgba(0,0,0,0.06)",
      }}
    >
      <div style={{
        position: "absolute", top: 0, left: 0, right: 0, height: 3,
        background: color, borderRadius: "18px 18px 0 0",
      }} />

      <div style={{ display: "flex", alignItems: "center", gap: 8, flexWrap: "wrap" }}>
        <span style={{
          padding: "3px 9px", borderRadius: 99, background: `${statusColor}18`, color: statusColor,
          fontSize: 11, fontWeight: 800, textTransform: "uppercase", letterSpacing: ".06em",
        }}>{q.status}</span>
        {q.hasAttempts && (
          <span style={{
            padding: "3px 9px", borderRadius: 99, background: "rgba(245,158,11,0.12)", color: "#f59e0b",
            fontSize: 11, fontWeight: 800,
          }}>🔒 questions locked</span>
        )}
      </div>

      <h4 style={{ margin: 0, fontSize: "1.05rem", fontWeight: 800, color: "var(--text-primary)" }}>
        {q.title}
      </h4>
      {q.courseCode && (
        <span style={{ fontSize: 12, fontWeight: 700, color }}>
          {q.courseCode}{q.courseName ? ` · ${q.courseName}` : ""}
        </span>
      )}

      <div style={{
        display: "grid", gap: 6, gridTemplateColumns: "1fr 1fr",
        background: "var(--hover-bg)", padding: "10px 12px", borderRadius: 12,
      }}>
        <div>
          <span style={{ fontSize: 10.5, fontWeight: 800, textTransform: "uppercase",
            letterSpacing: ".08em", color: "var(--text-muted)" }}>Start</span>
          <div style={{ fontSize: 12.5, fontWeight: 700 }}>{q.startTime || "—"}</div>
        </div>
        <div>
          <span style={{ fontSize: 10.5, fontWeight: 800, textTransform: "uppercase",
            letterSpacing: ".08em", color: "var(--text-muted)" }}>End</span>
          <div style={{ fontSize: 12.5, fontWeight: 700 }}>{q.endTime || "—"}</div>
        </div>
        <div>
          <span style={{ fontSize: 10.5, fontWeight: 800, textTransform: "uppercase",
            letterSpacing: ".08em", color: "var(--text-muted)" }}>Questions</span>
          <div style={{ fontSize: 12.5, fontWeight: 700 }}>{q.questions ?? 0}</div>
        </div>
        <div>
          <span style={{ fontSize: 10.5, fontWeight: 800, textTransform: "uppercase",
            letterSpacing: ".08em", color: "var(--text-muted)" }}>Total pts</span>
          <div style={{ fontSize: 12.5, fontWeight: 700 }}>{q.totalPoints ?? q.questions ?? "—"}</div>
        </div>
      </div>

      <div style={{ display: "flex", gap: 8, marginTop: "auto", paddingTop: 8 }}>
        <button type="button" onClick={() => onEdit(q)}
          style={{
            padding: "8px 14px", borderRadius: 10, cursor: "pointer",
            border: "1px solid var(--border)", background: "var(--hover-bg)",
            color: "var(--text-primary)", fontFamily: "inherit", fontSize: 12.5, fontWeight: 700,
          }}>
          ✎ Update
        </button>
        <button type="button" onClick={() => onDelete(q)}
          style={{
            padding: "8px 14px", borderRadius: 10, cursor: "pointer",
            border: "1.5px solid rgba(239,68,68,0.3)", background: "rgba(239,68,68,0.07)",
            color: "#ef4444", fontFamily: "inherit", fontSize: 12.5, fontWeight: 700,
          }}>
          🗑 Delete
        </button>
      </div>
    </motion.div>
  );
}

function ConfirmModal({ title, body, onConfirm, onClose, busy }) {
  return (
    <motion.div
      onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
      style={{
        position: "fixed", inset: 0, background: "rgba(0,0,0,0.55)", backdropFilter: "blur(8px)",
        display: "flex", alignItems: "center", justifyContent: "center", zIndex: 999, padding: 16,
      }}
    >
      <motion.div
        onClick={(e) => e.stopPropagation()}
        initial={{ scale: 0.92, y: 16 }} animate={{ scale: 1, y: 0 }} exit={{ scale: 0.95 }}
        transition={spring}
        style={{
          width: "min(440px, 100%)", background: "var(--card-bg)",
          borderRadius: 18, overflow: "hidden",
          border: "1px solid var(--border)", boxShadow: "0 24px 60px rgba(0,0,0,0.32)",
        }}>
        <div style={{ height: 4, background: "linear-gradient(135deg,#b91c1c,#ef4444)" }} />
        <div style={{ padding: 22 }}>
          <h3 style={{ margin: "0 0 8px", fontSize: "1.1rem", fontWeight: 800 }}>{title}</h3>
          <p style={{ margin: 0, fontSize: 13.5, color: "var(--text-secondary)", lineHeight: 1.55 }}>
            {body}
          </p>
          <div style={{ display: "flex", gap: 10, marginTop: 18, justifyContent: "flex-end" }}>
            <button type="button" onClick={onClose} disabled={busy}
              style={{
                padding: "10px 16px", borderRadius: 11, cursor: "pointer",
                border: "1px solid var(--border)", background: "var(--hover-bg)",
                color: "var(--text-primary)", fontFamily: "inherit", fontSize: 13, fontWeight: 700,
              }}>Cancel</button>
            <button type="button" onClick={onConfirm} disabled={busy}
              style={{
                padding: "10px 16px", borderRadius: 11, cursor: "pointer", border: "none",
                background: "linear-gradient(135deg,#b91c1c,#ef4444)", color: "#fff",
                fontFamily: "inherit", fontSize: 13, fontWeight: 800,
              }}>{busy ? "Deleting…" : "🗑 Delete"}</button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

function EditQuizModal({ quiz, courseId, color, onClose, onSaved }) {
  const [title, setTitle]   = useState(quiz?.title ?? "");
  const [startStr, setStart] = useState(() => (quiz?.startTime ? quiz.startTime.replace(" ", "T") : ""));
  const [endStr, setEnd]   = useState(() => (quiz?.endTime ? quiz.endTime.replace(" ", "T") : ""));
  const [busy, setBusy]    = useState(false);

  const locked = !!quiz?.hasAttempts;

  const submit = async () => {
    if (!title.trim() || !startStr || !endStr) return;
    setBusy(true);
    try {
      await updateQuiz(courseId, quiz.id, {
        title,
        startTime: new Date(startStr).toISOString(),
        endTime: new Date(endStr).toISOString(),
        // questions intentionally omitted — keep existing structure
      });
      onSaved?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Update failed");
    } finally { setBusy(false); }
  };

  return (
    <motion.div
      onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
      style={{
        position: "fixed", inset: 0, background: "rgba(0,0,0,0.55)", backdropFilter: "blur(8px)",
        display: "flex", alignItems: "center", justifyContent: "center", zIndex: 999, padding: 16, overflowY: "auto",
      }}>
      <motion.div
        onClick={(e) => e.stopPropagation()}
        initial={{ scale: 0.94, y: 18 }} animate={{ scale: 1, y: 0 }} exit={{ scale: 0.96 }}
        transition={spring}
        style={{
          width: "min(560px, 100%)", background: "var(--card-bg)",
          borderRadius: 20, overflow: "hidden",
          border: "1px solid var(--border)", boxShadow: "0 24px 60px rgba(0,0,0,0.32)",
        }}>
        <div style={{ height: 4, background: color }} />
        <div style={{ padding: 22, display: "flex", flexDirection: "column", gap: 14 }}>
          <h3 style={{ margin: 0, fontSize: "1.15rem", fontWeight: 800 }}>Edit quiz</h3>

          {locked && (
            <div style={{
              padding: "10px 13px", borderRadius: 12,
              background: "rgba(245,158,11,0.10)",
              border: "1px solid rgba(245,158,11,0.32)",
              color: "#b45309", fontSize: 12.5, fontWeight: 700, lineHeight: 1.5,
            }}>
              Students have already attempted this quiz, so the question structure is frozen. You can still update the
              title and start/end times.
            </div>
          )}

          <div>
            <label className={styles.fieldLabel}>Title</label>
            <input className={styles.input} value={title} onChange={(e) => setTitle(e.target.value)} />
          </div>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div>
              <label className={styles.fieldLabel}>Start (ISO)</label>
              <input type="datetime-local" className={styles.input}
                     value={startStr} onChange={(e) => setStart(e.target.value)} />
            </div>
            <div>
              <label className={styles.fieldLabel}>End (ISO)</label>
              <input type="datetime-local" className={styles.input}
                     value={endStr} onChange={(e) => setEnd(e.target.value)} />
            </div>
          </div>

          <div style={{ display: "flex", gap: 10, justifyContent: "flex-end", paddingTop: 4 }}>
            <button type="button" onClick={onClose} disabled={busy}
              style={{
                padding: "10px 16px", borderRadius: 11, cursor: "pointer",
                border: "1px solid var(--border)", background: "var(--hover-bg)",
                color: "var(--text-primary)", fontFamily: "inherit", fontSize: 13, fontWeight: 700,
              }}>Cancel</button>
            <button type="button" onClick={submit} disabled={busy || !title.trim() || !startStr || !endStr}
              style={{
                padding: "10px 18px", borderRadius: 11, cursor: "pointer", border: "none",
                background: color, color: "#fff", fontFamily: "inherit", fontSize: 13, fontWeight: 800,
                opacity: title.trim() && startStr && endStr ? 1 : 0.5,
              }}>{busy ? "Saving…" : "Save changes"}</button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

function QuizList({ courseId, color, refreshKey, onChanged }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [deleting, setDeleting] = useState(null);
  const [busy, setBusy] = useState(false);

  const reload = useCallback(async () => {
    if (!courseId) return;
    setLoading(true);
    try {
      const list = await getInstructorQuizzes(courseId);
      setItems(Array.isArray(list) ? list : []);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [courseId]);

  useEffect(() => { reload(); }, [reload, refreshKey]);

  const handleDelete = async () => {
    if (!deleting) return;
    setBusy(true);
    try {
      await deleteQuiz(courseId, deleting.id);
      setItems((prev) => prev.filter((q) => q.id !== deleting.id));
      setDeleting(null);
      onChanged?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Delete failed");
    } finally { setBusy(false); }
  };

  return (
    <section className={styles.panel}>
      <div className={styles.sectionHead}>
        <div>
          <span className={styles.sectionKicker}>Library</span>
          <h2 className={styles.sectionTitle}>Existing quizzes</h2>
          <p className={styles.sectionText}>
            All quizzes you have published in this course. Update title/time, or delete the quiz entirely.
          </p>
        </div>
      </div>

      {loading ? (
        <div style={{ padding: 32, textAlign: "center", color: "var(--text-muted)" }}>Loading…</div>
      ) : items.length === 0 ? (
        <div style={{ padding: 24, textAlign: "center", color: "var(--text-muted)", fontSize: 13.5 }}>
          No quizzes yet. Use the builder above to publish your first one.
        </div>
      ) : (
        <div style={{
          display: "grid", gap: 14,
          gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))",
        }}>
          <AnimatePresence>
            {items.map((q) => (
              <QuizCard key={q.id} q={q} color={color}
                        onEdit={setEditing} onDelete={setDeleting} />
            ))}
          </AnimatePresence>
        </div>
      )}

      <AnimatePresence>
        {editing && (
          <EditQuizModal
            quiz={editing}
            courseId={courseId}
            color={color}
            onClose={() => setEditing(null)}
            onSaved={() => { setEditing(null); reload(); onChanged?.(); }}
          />
        )}
        {deleting && (
          <ConfirmModal
            title="Delete quiz?"
            body={`"${deleting.title}" will be permanently removed. All questions, options, and student attempts will be deleted and coursework totals recalculated.`}
            onClose={() => setDeleting(null)}
            onConfirm={handleDelete}
            busy={busy}
          />
        )}
      </AnimatePresence>
    </section>
  );
}

export default function QuizBuilderPage() {
  const [courses, setCourses] = useState([]);
  const [courseId, setCourseId] = useState(null);
  const [step, setStep] = useState(1);
  const [saving, setSaving] = useState(false);
  const [done, setDone] = useState(false);
  const [info, setInfo] = useState({
    title: "",
    startDate: "",
    startTime: "",
    deadlineDate: "",
    deadlineTime: "",
    duration: 20,
    gradePerQ: 1,
    shuffleQ: false,
    shuffleA: false,
  });
  const [questions, setQuestions] = useState([makeQ()]);
  const [refreshKey, setRefreshKey] = useState(0);
  const [budget, setBudget] = useState(null);

  // Reload coursework budget whenever the course changes or the user publishes/deletes.
  useEffect(() => {
    if (!courseId) { setBudget(null); return; }
    getCourseworkBudget(courseId).then(setBudget).catch(() => setBudget(null));
  }, [courseId, refreshKey]);

  // Backend treats quiz total = (non-empty questions) × points-per-question. Match that
  // here so the banner reflects what the budget validator will actually see.
  const computedQuizTotal =
    questions.filter(q => q.text.trim()).length * Math.max(1, Number(info.gradePerQ) || 1);
  const budgetOk = !budget || computedQuizTotal <= (budget?.remaining ?? 0);

  useEffect(() => {
    getInstructorCourses()
      .then((data) => {
        const list = Array.isArray(data) ? data : [];
        setCourses(list);
      })
      .catch(() => {});
  }, []);

  const currentCourse = (courseId ? courses.find((course) => course.id === courseId) : null) || {
    color: "#8b5cf6",
    code: "Select a course",
    name: "",
    icon: "📚",
  };

  const infoOk = !!courseId && info.title && info.startDate && info.startTime && info.deadlineDate && info.deadlineTime && info.duration;
  const questionsOk = questions.length > 0 && questions.every((q) => q.text.trim() && q.answers.some((a) => a.text.trim()));
  const allOk = infoOk && questionsOk && budgetOk;

  const publish = async () => {
    if (!infoOk || !questionsOk) return;
    setSaving(true);
    try {
      const dto = {
        title: info.title,
        duration: info.duration,
        startTime: new Date(`${info.startDate}T${info.startTime}:00`).toISOString(),
        endTime: new Date(`${info.deadlineDate}T${info.deadlineTime}:00`).toISOString(),
        gradePerQ: info.gradePerQ || 1,
        questions: questions.map((q) => ({
          text: q.text,
          answers: q.answers.map((a) => ({ text: a.text })),
          correct: q.correct,
        })),
      };
      await createQuiz(courseId, dto);
      setDone(true);
      setRefreshKey((k) => k + 1);
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Failed to publish quiz");
    } finally {
      setSaving(false);
    }
  };

  const reset = () => {
    setDone(false);
    setStep(1);
    setInfo({
      title: "",
      startDate: "",
      startTime: "",
      deadlineDate: "",
      deadlineTime: "",
      duration: 20,
      gradePerQ: 1,
      shuffleQ: false,
      shuffleA: false,
    });
    setQuestions([makeQ()]);
  };

  if (done) {
    return (
      <div className={styles.page}>
        <div className={styles.successPage} style={{ "--accent": currentCourse.color }}>
          <div className={styles.successIcon}>🎉</div>
          <span className={styles.heroKicker}>Published</span>
          <h2 className={styles.successTitle}>Quiz published successfully</h2>
          <p className={styles.successText}>
            <strong>{info.title}</strong> is live for <strong>{currentCourse.code}</strong>. It opens on {info.startDate} at {info.startTime} and runs for {info.duration} minutes.
          </p>
          <div className={styles.successStats}>
            {[
              { value: questions.length, label: "Questions" },
              { value: `${info.duration}m`, label: "Duration" },
              { value: questions.length * (info.gradePerQ || 1), label: "Total pts" },
            ].map((item) => (
              <div key={item.label} className={styles.successStat}>
                <strong>{item.value}</strong>
                <span>{item.label}</span>
              </div>
            ))}
          </div>
          <button type="button" className={styles.primaryButton} style={{ "--accent": currentCourse.color }} onClick={reset}>
            Create another quiz
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.heroGlowLeft} />
        <div className={styles.heroGlowRight} />
        <div className={styles.heroContent}>
          <div>
            <span className={styles.heroKicker}>Assessment workspace</span>
            <h1 className={styles.heroTitle}>Quiz Builder</h1>
            <p className={styles.heroText}>
              Create structured quizzes with a cleaner setup flow, refined question management, and a much stronger visual hierarchy.
            </p>
          </div>
          <div className={styles.heroStats}>
            <div className={styles.heroStatCard}>
              <strong>{courses.length}</strong>
              <span>Available courses</span>
            </div>
            <div className={styles.heroStatCard}>
              <strong>{questions.length}</strong>
              <span>Current questions</span>
            </div>
            <div className={styles.heroStatCard}>
              <strong>{step}/2</strong>
              <span>Current step</span>
            </div>
          </div>
        </div>
      </section>

      <div className={styles.content}>
        <section className={styles.panel}>
          <div className={styles.sectionHead}>
            <div>
              <span className={styles.sectionKicker}>Step 1</span>
              <h2 className={styles.sectionTitle}>Choose the course</h2>
              <p className={styles.sectionText}>The selected course controls where the quiz will be published.</p>
            </div>
            {!courseId && courses.length > 0 && <div className={styles.inlineWarning}>Choose a course to continue</div>}
          </div>
          <CourseSelector courses={courses} courseId={courseId} onSelect={setCourseId} />
        </section>

        {!courseId && courses.length > 0 && (
          <div className={styles.emptyCard}>
            <div className={styles.emptyCardIcon}>🧭</div>
            <h3>Select a course first</h3>
            <p>After that, you can configure the quiz settings and start writing questions.</p>
          </div>
        )}

        {courseId && (
          <>
            <section className={styles.panel}>
              <div className={styles.sectionHead}>
                <div>
                  <span className={styles.sectionKicker}>Build flow</span>
                  <h2 className={styles.sectionTitle}>Configure then publish</h2>
                  <p className={styles.sectionText}>
                    {currentCourse.icon} {currentCourse.code} · {currentCourse.name}
                  </p>
                </div>
                <div className={styles.courseMiniBadge} style={{ "--accent": currentCourse.color }}>
                  {currentCourse.code}
                </div>
              </div>
              <StepIndicator step={step} setStep={setStep} color={currentCourse.color} questions={questions} />
            </section>

            <div className={styles.bodyPanel}>
              <AnimatePresence mode="wait">
                {step === 1 && (
                  <motion.div
                    key="settings"
                    initial={{ opacity: 0, x: 16 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -12 }}
                    transition={{ duration: 0.2 }}
                  >
                    <StepSettings data={info} onChange={setInfo} color={currentCourse.color} />
                  </motion.div>
                )}
                {step === 2 && (
                  <motion.div
                    key="questions"
                    initial={{ opacity: 0, x: 16 }}
                    animate={{ opacity: 1, x: 0 }}
                    exit={{ opacity: 0, x: -12 }}
                    transition={{ duration: 0.2 }}
                  >
                    <StepQuestions questions={questions} setQuestions={setQuestions} color={currentCourse.color} />
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            <QuizList
              courseId={courseId}
              color={currentCourse.color}
              refreshKey={refreshKey}
              onChanged={() => setRefreshKey((k) => k + 1)}
            />

            {budget && (
              <div style={{
                margin: "0 0 12px",
                padding: "10px 14px",
                borderRadius: 12,
                border: budgetOk ? "1px solid var(--border)" : "1px solid rgba(239,68,68,.35)",
                background: budgetOk ? "var(--hover-bg)" : "rgba(239,68,68,.06)",
                color: "var(--text-secondary)", fontSize: 12.5, lineHeight: 1.55,
              }}>
                <strong style={{ color: "var(--text-primary)" }}>Coursework budget</strong> —
                Used: <strong>{budget.used}</strong> / {budget.budget} · Remaining: <strong>{budget.remaining}</strong>
                · Quiz total (questions × {Math.max(1, Number(info.gradePerQ) || 1)} pt): <strong>{computedQuizTotal}</strong>
                {!budgetOk && (
                  <div style={{ color: "#ef4444", fontWeight: 700, marginTop: 4 }}>
                    ⚠ This quiz total exceeds the remaining 40-point coursework budget. Reduce the
                    number of questions or archive other coursework first.
                  </div>
                )}
              </div>
            )}

            <div className={styles.footerBar}>
              <div className={styles.footerInfo}>
                {step === 1
                  ? "Step 1 of 2 — finish the quiz settings, then move to the questions."
                  : `Step 2 of 2 — ${questions.length} questions · ${questions.filter((q) => q.text.trim()).length} drafted.`}
              </div>
              <div className={styles.footerActions}>
                {step === 2 && (
                  <button type="button" className={styles.backButton} onClick={() => setStep(1)}>
                    ← Back
                  </button>
                )}
                {step === 1 && (
                  <motion.button
                    type="button"
                    className={styles.primaryButton}
                    style={{ "--accent": currentCourse.color }}
                    disabled={!infoOk}
                    onClick={() => setStep(2)}
                    whileHover={infoOk ? { y: -2 } : {}}
                    whileTap={infoOk ? { scale: 0.985 } : {}}
                  >
                    Next: add questions →
                  </motion.button>
                )}
                {step === 2 && (
                  <motion.button
                    type="button"
                    className={styles.primaryButton}
                    style={{ "--accent": currentCourse.color }}
                    disabled={!allOk || saving}
                    onClick={publish}
                    whileHover={allOk ? { y: -2 } : {}}
                    whileTap={allOk ? { scale: 0.985 } : {}}
                  >
                    {saving ? (
                      <>
                        <motion.span animate={{ rotate: 360 }} transition={{ duration: 0.7, repeat: Infinity, ease: "linear" }}>
                          ⟳
                        </motion.span>
                        Publishing…
                      </>
                    ) : (
                      <>✅ Publish quiz ({questions.length}Q)</>
                    )}
                  </motion.button>
                )}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
