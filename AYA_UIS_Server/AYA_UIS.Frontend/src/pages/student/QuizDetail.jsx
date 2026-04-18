// src/pages/student/QuizDetail.jsx
import { useState, useEffect, useCallback, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./QuizDetail.module.css";

/* ─── TODO: replace with API calls ───────────────────────────
   GET  /api/student/quiz/:quizId → quiz details + questions
   POST /api/student/quiz/:quizId/submit → { score, total }
─────────────────────────────────────────────────────────────── */

/* ════════════════════════════════════════════════════
   QUIZ DATABASE  (mock — per course per quiz)
════════════════════════════════════════════════════ */
export const QUIZ_DB = {

  /* ─── CS401 — Artificial Intelligence ─── */
  "1-3": {
    courseCode: "CS401", courseName: "Artificial Intelligence",
    instructor: "Dr. Mohamed Farouk", courseColor: "#e8a838", courseShade: "#b07820",
    title: "Quiz 3 — ML Fundamentals",
    date: "Mar 20, 2025", deadline: "Mar 20, 2025 — 11:59 PM",
    startTime: "9:00 AM", duration: 20, questions: 8,
    mcq: [
      { q: "Which algorithm builds a model based on sample data to make predictions?",
        opts: ["K-Means","Linear Regression","BFS","Quicksort"], ans: 1 },
      { q: "What does 'overfitting' mean in machine learning?",
        opts: ["Model is too simple","Model fits training data too well but fails on new data","Model has low accuracy","Model requires too much memory"], ans: 1 },
      { q: "Which of these is a supervised learning algorithm?",
        opts: ["K-Means Clustering","PCA","Decision Tree","DBSCAN"], ans: 2 },
      { q: "What is the purpose of a validation set?",
        opts: ["Train the model","Test the final model","Tune hyperparameters","Clean the data"], ans: 2 },
      { q: "What does 'feature' mean in an ML context?",
        opts: ["A bug in the model","An individual measurable property of the data","The model output","A type of neural network"], ans: 1 },
      { q: "Which metric measures classification model accuracy?",
        opts: ["Mean Squared Error","R-squared","F1-Score","Pearson correlation"], ans: 2 },
      { q: "What is gradient descent used for?",
        opts: ["Data preprocessing","Minimizing the loss function","Generating synthetic data","Feature selection"], ans: 1 },
      { q: "Which of these is an unsupervised learning task?",
        opts: ["Spam detection","Image classification","Customer segmentation","Sentiment analysis"], ans: 2 },
    ],
  },

  "1-4": {
    courseCode: "CS401", courseName: "Artificial Intelligence",
    instructor: "Dr. Mohamed Farouk", courseColor: "#e8a838", courseShade: "#b07820",
    title: "Quiz 4 — Neural Networks",
    date: "Apr 3, 2025", deadline: "Apr 3, 2025 — 11:59 PM",
    startTime: "9:00 AM", duration: 20, questions: 10,
    mcq: [
      { q: "What is an artificial neuron modeled after?",
        opts: ["A transistor","A biological neuron","A logic gate","A resistor"], ans: 1 },
      { q: "What does the activation function do?",
        opts: ["Initializes weights","Introduces non-linearity","Reduces overfitting","Increases training speed"], ans: 1 },
      { q: "Which activation function outputs values between 0 and 1?",
        opts: ["ReLU","Tanh","Sigmoid","Softmax"], ans: 2 },
      { q: "What is backpropagation used for?",
        opts: ["Forward pass","Updating weights using gradients","Data augmentation","Batch normalization"], ans: 1 },
      { q: "What is a 'hidden layer' in a neural network?",
        opts: ["Input layer","Output layer","A layer between input and output","Dropout layer"], ans: 2 },
      { q: "What does 'epoch' mean in training?",
        opts: ["One forward pass","One complete pass through the entire training dataset","One gradient update","One weight initialization"], ans: 1 },
      { q: "What problem does dropout help prevent?",
        opts: ["Underfitting","Data leakage","Overfitting","Vanishing gradient"], ans: 2 },
      { q: "Which optimizer adapts the learning rate during training?",
        opts: ["SGD","Adam","Batch Gradient Descent","Perceptron"], ans: 1 },
      { q: "What is a convolutional layer used for?",
        opts: ["Processing sequential data","Extracting local features from images","Fully connected layers","Attention mechanisms"], ans: 1 },
      { q: "What is the vanishing gradient problem?",
        opts: ["Gradients become too large","Gradients become too small during backpropagation","Model becomes too complex","Loss function diverges"], ans: 1 },
    ],
  },

  /* ─── CS402 — Compiler Theory ─── */
  "2-3": {
    courseCode: "CS402", courseName: "Compiler Theory & Design",
    instructor: "Dr. Heba Nasser", courseColor: "#7c6fc4", courseShade: "#4a3fa0",
    title: "Quiz 3 — Code Generation",
    date: "Mar 18, 2025", deadline: "Mar 18, 2025 — 11:59 PM",
    startTime: "10:00 AM", duration: 15, questions: 8,
    mcq: [
      { q: "What is the output of the code generation phase?",
        opts: ["Parse tree","Abstract Syntax Tree","Machine code or assembly","Tokens"], ans: 2 },
      { q: "What is a 'symbol table' used for?",
        opts: ["Storing machine instructions","Tracking variable names and types","Lexical analysis","Syntax checking"], ans: 1 },
      { q: "What does 'three-address code' represent?",
        opts: ["A form of machine code","An intermediate representation","Final binary code","Source code"], ans: 1 },
      { q: "What is 'register allocation'?",
        opts: ["Allocating heap memory","Assigning variables to CPU registers","Optimizing loops","Stack management"], ans: 1 },
      { q: "Which phase comes immediately before code generation?",
        opts: ["Lexical analysis","Syntax analysis","Semantic analysis","Optimization"], ans: 3 },
      { q: "What is 'dead code elimination'?",
        opts: ["Removing syntax errors","Removing code that cannot be executed","Removing comments","Removing imports"], ans: 1 },
      { q: "What is the purpose of a 'basic block'?",
        opts: ["A syntax construct","A maximal sequence of straight-line instructions","A loop structure","A function definition"], ans: 1 },
      { q: "What is 'loop unrolling'?",
        opts: ["A parsing technique","An optimization that reduces loop overhead","A code generation error","A type of recursion"], ans: 1 },
    ],
  },

  /* ─── CS404 — Expert Systems ─── */
  "4-2": {
    courseCode: "CS404", courseName: "Expert Systems",
    instructor: "Dr. Dina Mahmoud", courseColor: "#e05c8a", courseShade: "#a01858",
    title: "Quiz 2 — Inference Methods",
    date: "Mar 7, 2025", deadline: "Mar 7, 2025 — 11:59 PM",
    startTime: "11:00 AM", duration: 20, questions: 10,
    mcq: [
      { q: "What is forward chaining?",
        opts: ["Starting from goals and working backward","Starting from facts and deriving conclusions","A search strategy","A type of neural network"], ans: 1 },
      { q: "What is backward chaining?",
        opts: ["Starting from facts","Working backward from the goal to find supporting facts","A type of loop","A database query"], ans: 1 },
      { q: "What is the inference engine?",
        opts: ["The knowledge base","The component that applies rules to facts","The user interface","The database"], ans: 1 },
      { q: "What is a production rule?",
        opts: ["A factory process","An IF-THEN rule in expert systems","A database record","A neural layer"], ans: 1 },
      { q: "What does certainty factor represent?",
        opts: ["A Boolean value","A degree of belief or confidence in a conclusion","A count of rules","A time stamp"], ans: 1 },
      { q: "CLIPS stands for?",
        opts: ["C Language Integrated Production System","Common Lisp Interface Protocol","Compiler Logic in Production Systems","Core Logic IP System"], ans: 0 },
      { q: "What is a knowledge base?",
        opts: ["RAM storage","A collection of facts and rules about a domain","A database schema","A neural network layer"], ans: 1 },
      { q: "What type of reasoning does backward chaining use?",
        opts: ["Inductive","Deductive","Abductive","Statistical"], ans: 1 },
      { q: "What is a working memory in expert systems?",
        opts: ["RAM on the server","Dynamic storage of current facts during inference","The knowledge base","The rule set"], ans: 1 },
      { q: "Which shell is most widely used for expert systems?",
        opts: ["PROLOG","CLIPS","Python","C++"], ans: 1 },
    ],
  },

  /* ─── CS405 — Natural Language Databases ─── */
  "5-2": {
    courseCode: "CS405", courseName: "Natural Language Databases",
    instructor: "Dr. Khaled Ibrahim", courseColor: "#5b9fb5", courseShade: "#2a6a80",
    title: "Quiz 2 — POS & Parsing",
    date: "Mar 4, 2025", deadline: "Mar 4, 2025 — 11:59 PM",
    startTime: "9:00 AM", duration: 20, questions: 10,
    mcq: [
      { q: "What does POS stand for in NLP?",
        opts: ["Part of Speech","Position of Sentence","Processing of Strings","Phrase of Statement"], ans: 0 },
      { q: "What is tokenization?",
        opts: ["Translating text","Splitting text into individual words or tokens","Encrypting data","Compressing text"], ans: 1 },
      { q: "What is a parse tree?",
        opts: ["A database index","A hierarchical structure showing syntactic relationships","A binary search tree","A hash table"], ans: 1 },
      { q: "What is stemming?",
        opts: ["Adding suffixes","Reducing words to their root form","Translating sentences","Parsing grammar"], ans: 1 },
      { q: "What is a stop word?",
        opts: ["A keyword","A common word with little meaning like 'the','is'","A noun","An error in parsing"], ans: 1 },
      { q: "What does NER stand for?",
        opts: ["Natural Entity Recognition","Named Entity Recognition","Noun Entity Rank","None of the above"], ans: 1 },
      { q: "What is lemmatization?",
        opts: ["Counting words","Reducing words to their dictionary base form","Building a lexicon","Tokenizing text"], ans: 1 },
      { q: "What is TF-IDF used for?",
        opts: ["Translating text","Measuring word importance in documents","Parsing syntax","Building a chatbot"], ans: 1 },
      { q: "Which parsing strategy works top-down?",
        opts: ["LR parsing","LL parsing","Earley","Bottom-up"], ans: 1 },
      { q: "What is dependency parsing?",
        opts: ["Checking syntax errors","Identifying relationships between words","Tokenizing text","Finding named entities"], ans: 1 },
    ],
  },

  /* ─── CS406 — Operating Systems ─── */
  "6-4": {
    courseCode: "CS406", courseName: "Theory of Operating Systems",
    instructor: "Dr. Rania Hassan", courseColor: "#3d8fe0", courseShade: "#0d5ab0",
    title: "Quiz 4 — Deadlocks",
    date: "Mar 31, 2025", deadline: "Mar 31, 2025 — 11:59 PM",
    startTime: "10:00 AM", duration: 20, questions: 8,
    mcq: [
      { q: "What is a deadlock in operating systems?",
        opts: ["A CPU hang","A situation where processes wait for each other's resources indefinitely","A memory overflow","A file corruption"], ans: 1 },
      { q: "Which of the following is NOT a necessary condition for deadlock?",
        opts: ["Mutual exclusion","Hold and wait","CPU scheduling","Circular wait"], ans: 2 },
      { q: "What is the Banker's algorithm used for?",
        opts: ["Memory paging","Deadlock avoidance","Process scheduling","File allocation"], ans: 1 },
      { q: "What does 'safe state' mean?",
        opts: ["No processes running","A state from which all processes can complete","A locked resource","An idle CPU"], ans: 1 },
      { q: "What is resource preemption?",
        opts: ["Allocating resources","Forcibly taking resources from a process","Scheduling a process","Creating a process"], ans: 1 },
      { q: "What is circular wait in deadlock?",
        opts: ["A scheduling algorithm","A cycle of processes each waiting for a resource held by the next","An infinite loop","A memory leak"], ans: 1 },
      { q: "What does deadlock detection do?",
        opts: ["Prevents deadlock","Identifies deadlocked processes to recover from deadlock","Allocates resources","Schedules processes"], ans: 1 },
      { q: "Which approach breaks the 'hold and wait' condition?",
        opts: ["Resource preemption","Requiring processes to acquire all resources at once before starting","Circular wait prevention","Priority inversion"], ans: 1 },
    ],
  },
};

/* ── Letter grade ── */
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

const OPTION_LABELS = ["A", "B", "C", "D"];

/* ════════════════════════════════════
   PHASE 1 — INFO
════════════════════════════════════ */
function InfoPhase({ quiz, onStart }) {
  const c = quiz.courseColor;

  return (
    <div className={styles.infoPage}>

      {/* Big color hero */}
      <motion.div className={styles.infoHero}
        style={{ background: `linear-gradient(135deg, ${c} 0%, ${quiz.courseShade} 100%)` }}
        initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.48, ease: [0.22, 1, 0.36, 1] }}>

        {/* Decorative circles */}
        <div className={styles.heroBubble1} style={{ background: "rgba(255,255,255,0.08)" }} />
        <div className={styles.heroBubble2} style={{ background: "rgba(255,255,255,0.05)" }} />

        <div className={styles.infoHeroContent}>
          <motion.span className={styles.infoCourseTag}
            initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}>
            {quiz.courseCode} · {quiz.courseName}
          </motion.span>

          <motion.h1 className={styles.infoTitle}
            initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.18, duration: 0.42 }}>
            {quiz.title}
          </motion.h1>

          <motion.p className={styles.infoInstr}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }}
            transition={{ delay: 0.26 }}>
            👨‍🏫 {quiz.instructor}
          </motion.p>
        </div>

        {/* Ring with question count */}
        <motion.div className={styles.infoRing}
          initial={{ scale: 0.7, opacity: 0 }} animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.22, type: "spring", stiffness: 260, damping: 20 }}>
          <span className={styles.infoRingNum}>{quiz.questions}</span>
          <span className={styles.infoRingLabel}>Questions</span>
        </motion.div>
      </motion.div>

      {/* Details grid */}
      <div className={styles.infoBody}>
        <motion.div className={styles.infoGrid}
          initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3, duration: 0.4 }}>
          {[
            { icon: "📅", label: "Date",           val: quiz.date },
            { icon: "⏱",  label: "Duration",       val: `${quiz.duration} minutes` },
            { icon: "❓",  label: "Questions",      val: `${quiz.questions} MCQ` },
            { icon: "⭐",  label: "Total Points",   val: `${quiz.questions} pts` },
            { icon: "⏰",  label: "Closes At",      val: quiz.deadline },
            { icon: "🎯",  label: "Points per Q",   val: `1 pt each` },
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

        {/* Rules */}
        <motion.div className={styles.infoRules}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }}
          transition={{ delay: 0.52 }}>
          <div className={styles.infoRulesTitle}>📋 Before you start</div>
          <ul className={styles.infoRulesList}>
            <li>Timer starts immediately when you click <strong>Start Now</strong></li>
            <li>You can navigate between questions freely using Prev / Next</li>
            <li>Unanswered questions count as wrong</li>
            <li>You cannot retake this quiz after submission</li>
          </ul>
        </motion.div>

        {/* Start button */}
        <motion.button className={styles.startBtn}
          style={{ background: `linear-gradient(135deg, ${c}, ${quiz.courseShade})` }}
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
function ExamPhase({ quiz, onSubmit }) {
  const c = quiz.courseColor;
  const total = quiz.mcq.length;

  const [idx, setIdx]         = useState(0);
  const [answers, setAnswers] = useState(new Array(total).fill(null));
  const [timeLeft, setTimeLeft] = useState(quiz.duration * 60);
  const [showSubmit, setShowSubmit] = useState(false);

  // Timer
  useEffect(() => {
    const t = setInterval(() => {
      setTimeLeft(s => {
        if (s <= 1) { clearInterval(t); onSubmit(answers); return 0; }
        return s - 1;
      });
    }, 1000);
    return () => clearInterval(t);
  }, []);

  const mins = String(Math.floor(timeLeft / 60)).padStart(2, "0");
  const secs = String(timeLeft % 60).padStart(2, "0");
  const timerDanger = timeLeft <= 60;

  const answered = answers.filter(a => a !== null).length;
  const q = quiz.mcq[idx];

  const pick = (optIdx) => {
    setAnswers(prev => {
      const next = [...prev];
      next[idx] = optIdx;
      return next;
    });
  };

  return (
    <div className={styles.examPage}>

      {/* ── Top bar ── */}
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
              transition={{ duration: 0.3 }}
            />
          </div>
        </div>

        <motion.div
          className={styles.timer}
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

      {/* ── Question card ── */}
      <div className={styles.examBody}>
        <AnimatePresence mode="wait">
          <motion.div key={idx}
            className={styles.questionCard}
            initial={{ opacity: 0, x: 40 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -30 }}
            transition={{ duration: 0.26, ease: [0.22, 1, 0.36, 1] }}>

            {/* Question number tag */}
            <div className={styles.questionTag} style={{ background: `${c}15`, color: c }}>
              Q{idx + 1}
            </div>

            {/* Question text */}
            <p className={styles.questionText}>{q.q}</p>

            {/* Options */}
            <div className={styles.optionsList}>
              {q.opts.map((opt, oi) => {
                const picked = answers[idx] === oi;
                return (
                  <motion.button
                    key={oi}
                    className={`${styles.option} ${picked ? styles.optionPicked : ""}`}
                    style={picked ? {
                      background: `${c}12`,
                      borderColor: c,
                      boxShadow: `0 0 0 3px ${c}20`,
                    } : {}}
                    onClick={() => pick(oi)}
                    whileHover={{ x: 4, transition: { duration: 0.14 } }}
                    whileTap={{ scale: 0.98 }}>
                    <div className={styles.optionLabel}
                      style={picked ? { background: c, color: "white", borderColor: c } : {}}>
                      {OPTION_LABELS[oi]}
                    </div>
                    <span className={styles.optionText}>{opt}</span>
                    {picked && (
                      <motion.div className={styles.optionCheck}
                        style={{ color: c }}
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

        {/* ── Question dots ── */}
        <div className={styles.dotRow}>
          {quiz.mcq.map((_, di) => (
            <button key={di}
              className={styles.dot}
              style={{
                background: di === idx ? c :
                  answers[di] !== null ? `${c}55` : "#e2e8f0",
                transform: di === idx ? "scale(1.3)" : "scale(1)",
              }}
              onClick={() => setIdx(di)}
              title={`Q${di + 1}`}
            />
          ))}
        </div>

        {/* ── Nav buttons ── */}
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
            <motion.button className={styles.navBtnNext}
              style={{ background: c }}
              onClick={() => setIdx(i => i + 1)}
              whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
              Next →
            </motion.button>
          ) : (
            <motion.button className={styles.submitBtn}
              style={{ background: c }}
              onClick={() => setShowSubmit(true)}
              whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
              Submit ✓
            </motion.button>
          )}
        </div>
      </div>

      {/* ── Submit confirm ── */}
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
                {answered < total && <><br/><span style={{ color: "#ef4444" }}>⚠️ {total - answered} unanswered question(s) will be marked wrong.</span></>}
              </p>
              <div className={styles.confirmBtns}>
                <button className={styles.confirmCancel}
                  onClick={() => setShowSubmit(false)}>Keep going</button>
                <motion.button className={styles.confirmSubmit}
                  style={{ background: c }}
                  onClick={() => onSubmit(answers)}
                  whileHover={{ scale: 1.04 }} whileTap={{ scale: 0.96 }}>
                  Yes, submit
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
function ResultsPhase({ quiz, answers, onBack }) {
  const c = quiz.courseColor;
  const total  = quiz.mcq.length;
  const score  = quiz.mcq.reduce((s, q, i) => s + (answers[i] === q.ans ? 1 : 0), 0);
  const pct    = Math.round((score / total) * 100);
  const grade  = letterGrade(pct);
  const circ   = 2 * Math.PI * 54;

  const [showReview, setShowReview] = useState(false);

  return (
    <div className={styles.resultsPage}>

      {/* Confetti dots */}
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
              transition={{ delay: Math.random() * 0.8, duration: 2.5 + Math.random() * 1.5, ease: "linear" }}
            />
          ))}
        </div>
      )}

      <div className={styles.resultsInner}>

        {/* Score ring */}
        <motion.div className={styles.scoreRingWrap}
          initial={{ scale: 0.5, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.1, type: "spring", stiffness: 240, damping: 20 }}>

          <svg width="128" height="128" viewBox="0 0 128 128"
            style={{ transform: "rotate(-90deg)" }}>
            <circle cx="64" cy="64" r="54" fill="none"
              stroke="#f1f5f9" strokeWidth="10"/>
            <motion.circle cx="64" cy="64" r="54" fill="none"
              stroke={c} strokeWidth="10" strokeLinecap="round"
              strokeDasharray={circ}
              initial={{ strokeDashoffset: circ }}
              animate={{ strokeDashoffset: circ * (1 - pct / 100) }}
              transition={{ delay: 0.3, duration: 1.4, ease: [0.22, 1, 0.36, 1] }}
            />
          </svg>

          <div className={styles.scoreRingCenter}>
            <motion.span className={styles.scoreNum} style={{ color: c }}
              initial={{ opacity: 0, scale: 0.7 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.5 }}>
              {score}
            </motion.span>
            <span className={styles.scoreTotal}>/{total}</span>
          </div>
        </motion.div>

        {/* Grade & message */}
        <motion.div className={styles.gradeBlock}
          initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}>
          <div className={styles.gradeLetter} style={{ color: grade.color, borderColor: `${grade.color}35`, background: `${grade.color}10` }}>
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

        {/* Stats row */}
        <motion.div className={styles.statsRow}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }}
          transition={{ delay: 0.65 }}>
          {[
            { icon: "✅", label: "Correct",    val: score,         col: "#22c55e" },
            { icon: "❌", label: "Wrong",       val: total - score, col: "#ef4444" },
            { icon: "📊", label: "Score",       val: `${pct}%`,     col: c },
          ].map((s, i) => (
            <div key={i} className={styles.statBox}>
              <span className={styles.statIcon}>{s.icon}</span>
              <span className={styles.statVal} style={{ color: s.col }}>{s.val}</span>
              <span className={styles.statLabel}>{s.label}</span>
            </div>
          ))}
        </motion.div>

        {/* Buttons */}
        <motion.div className={styles.resultBtns}
          initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.75 }}>
          <button className={styles.reviewBtn}
            onClick={() => setShowReview(v => !v)}>
            {showReview ? "Hide Review" : "📋 Review Answers"}
          </button>
          <motion.button className={styles.backBtn}
            style={{ background: c }}
            onClick={onBack}
            whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.97 }}>
            ← Back to Course
          </motion.button>
        </motion.div>

        {/* Review */}
        <AnimatePresence>
          {showReview && (
            <motion.div className={styles.reviewList}
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: "auto" }}
              exit={{ opacity: 0, height: 0 }}
              transition={{ duration: 0.3 }}>
              {quiz.mcq.map((q, i) => {
                const isRight = answers[i] === q.ans;
                return (
                  <motion.div key={i} className={styles.reviewItem}
                    style={{ borderColor: isRight ? "#bbf7d0" : "#fecaca", background: isRight ? "#f0fdf4" : "#fff5f5" }}
                    initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: i * 0.04 }}>
                    <div className={styles.reviewQ}>
                      <span className={styles.reviewQNum}
                        style={{ background: isRight ? "#22c55e" : "#ef4444" }}>
                        Q{i + 1}
                      </span>
                      <p className={styles.reviewQText}>{q.q}</p>
                    </div>
                    <div className={styles.reviewAnswers}>
                      {answers[i] !== null && answers[i] !== q.ans && (
                        <div className={styles.reviewWrong}>
                          ❌ Your answer: <em>{q.opts[answers[i]]}</em>
                        </div>
                      )}
                      <div className={styles.reviewCorrect}>
                        ✅ Correct: <em>{q.opts[q.ans]}</em>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}

/* ════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════ */
export default function QuizDetail() {
  const { courseId, quizId } = useParams();
  const navigate = useNavigate();
  const key      = `${courseId}-${quizId}`;
  const quiz     = QUIZ_DB[key] || QUIZ_DB["1-3"];

  const [phase,   setPhase]   = useState("info");   // info | exam | results
  const [answers, setAnswers] = useState(null);

  const handleStart  = () => setPhase("exam");
  const handleSubmit = (ans) => { setAnswers(ans); setPhase("results"); };
  const handleBack   = () => navigate(`/student/courses/${courseId}`);

  return (
    <AnimatePresence mode="wait">
      {phase === "info" && (
        <motion.div key="info"
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
          <InfoPhase quiz={quiz} onStart={handleStart} />
        </motion.div>
      )}
      {phase === "exam" && (
        <motion.div key="exam"
          initial={{ opacity: 0, x: 30 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -30 }}>
          <ExamPhase quiz={quiz} onSubmit={handleSubmit} />
        </motion.div>
      )}
      {phase === "results" && (
        <motion.div key="results"
          initial={{ opacity: 0, scale: 0.96 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }}>
          <ResultsPhase quiz={quiz} answers={answers} onBack={handleBack} />
        </motion.div>
      )}
    </AnimatePresence>
  );
}