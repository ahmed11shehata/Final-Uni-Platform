// src/pages/student/CourseDetailPage.jsx
import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { getStudentCourseDetail } from "../../services/api/studentApi";
import styles from "./CourseDetailPage.module.css";

/* ── Icons ── */
const IC = {
  back:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 12H5M12 5l-7 7 7 7"/></svg>,
  video:  <svg viewBox="0 0 24 24" fill="currentColor"><polygon points="5,3 19,12 5,21"/></svg>,
  pdf:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>,
  dl:     <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>,
  check:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.8" strokeLinecap="round"><polyline points="20 6 9 17 4 12"/></svg>,
  upload: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><polyline points="16 16 12 12 8 16"/><line x1="12" y1="12" x2="12" y2="21"/><path d="M20.39 18.39A5 5 0 0018 9h-1.26A8 8 0 103 16.3"/></svg>,
  lock:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>,
  quiz:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11"/></svg>,
  file:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><path d="M13 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V9z"/><polyline points="13 2 13 9 20 9"/></svg>,
};

/* ── Status config ── */
const ST = {
  graded:    { label:"Graded",    bg:"rgba(16,185,129,0.14)",  color:"#10b981", dot:"#10b981" },
  pending:   { label:"Pending",   bg:"rgba(245,158,11,0.14)",  color:"#f59e0b", dot:"#f59e0b" },
  upcoming:  { label:"Upcoming",  bg:"rgba(148,163,184,0.14)", color:"#94a3b8", dot:"#94a3b8" },
  completed: { label:"Done ✓",    bg:"rgba(16,185,129,0.14)",  color:"#10b981", dot:"#10b981" },
  available: { label:"Open Now",  bg:"rgba(139,92,246,0.14)",  color:"#a78bfa", dot:"#8b5cf6" },
  submitted: { label:"Submitted", bg:"rgba(59,130,246,0.14)",  color:"#60a5fa", dot:"#3b82f6" },
};
function Chip({ status }) {
  const s = ST[status] || ST.upcoming;
  return (
    <span className={styles.chip} style={{ background: s.bg, color: s.color }}>
      <span className={styles.chipDot} style={{ background: s.dot }}/>
      {s.label}
    </span>
  );
}

/* ── Typewriter / word reveal title ── */
function TitleReveal({ text }) {
  const words = text.split(" ");
  return (
    <h1 className={styles.heroTitle}>
      {words.map((word, i) => (
        <motion.span key={i} className={styles.revealWord}
          initial={{ opacity: 0, y: 14, filter: "blur(8px)" }}
          animate={{ opacity: 1, y: 0, filter: "blur(0px)" }}
          transition={{ delay: 0.18 + i * 0.08, duration: 0.38, ease: [0.22, 1, 0.36, 1] }}>
          {word}{" "}
        </motion.span>
      ))}
    </h1>
  );
}

/* ── Progress ring ── */
function Ring({ pct, size = 96, color }) {
  const r = (size - 10) / 2, circ = 2 * Math.PI * r;
  return (
    <div className={styles.ringWrap} style={{ width: size, height: size }}>
      <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`}>
        <circle cx={size/2} cy={size/2} r={r} fill="none" stroke="rgba(255,255,255,0.18)" strokeWidth="5"/>
        <motion.circle cx={size/2} cy={size/2} r={r} fill="none"
          stroke="rgba(255,255,255,0.9)" strokeWidth="5" strokeLinecap="round"
          strokeDasharray={circ}
          initial={{ strokeDashoffset: circ }}
          animate={{ strokeDashoffset: circ * (1 - pct / 100) }}
          transition={{ delay: 0.5, duration: 1.4, ease: [0.22, 1, 0.36, 1] }}
          style={{ transform: "rotate(-90deg)", transformOrigin: "center" }}/>
      </svg>
      <div className={styles.ringLabel}>
        <span className={styles.ringNum}>{pct}%</span>
        <span className={styles.ringText}>done</span>
      </div>
    </div>
  );
}

/* ── File upload ── */
function FileUpload({ id, types, color, onDone }) {
  const [drag, setDrag] = useState(false);
  const [file, setFile] = useState(null);
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState(false);
  const ref = useRef();

  const submit = async () => {
    if (!file) return; setBusy(true);
    // TODO: real upload
    await new Promise(r => setTimeout(r, 1300));
    setBusy(false); setDone(true); onDone?.();
  };

  if (done) return (
    <motion.div className={styles.uploadOk}
      initial={{ opacity: 0, scale: 0.9 }} animate={{ opacity: 1, scale: 1 }}>
      <div className={styles.uploadOkCircle} style={{ background: color }}>
        <span style={{ width: 18, height: 18, display: "flex", color: "#fff" }}>{IC.check}</span>
      </div>
      <div>
        <div className={styles.uploadOkTitle}>Submitted successfully!</div>
        <div className={styles.uploadOkSub}>Your instructor will review it shortly.</div>
      </div>
    </motion.div>
  );

  return (
    <div className={styles.uploadArea}>
      <motion.div
        className={`${styles.dropZone} ${drag ? styles.dropActive : ""}`}
        style={drag ? { borderColor: color, background: `${color}06` } : {}}
        onDragOver={e => { e.preventDefault(); setDrag(true); }}
        onDragLeave={() => setDrag(false)}
        onDrop={e => { e.preventDefault(); setDrag(false); setFile(e.dataTransfer.files[0]); }}
        onClick={() => ref.current?.click()}
        whileHover={{ scale: 1.01 }} whileTap={{ scale: 0.99 }}>
        <input ref={ref} type="file" style={{ display: "none" }}
          accept={types.map(t => `.${t}`).join(",")}
          onChange={e => setFile(e.target.files[0])}/>
        <div className={styles.dropIco} style={{ color }}>
          {file ? IC.file : IC.upload}
        </div>
        {file ? (
          <div className={styles.fileInfo}>
            <span className={styles.fileName}>{file.name}</span>
            <span className={styles.fileSize}>{(file.size / 1048576).toFixed(1)} MB</span>
          </div>
        ) : (
          <>
            <p className={styles.dropLine}>Drop or <span style={{ color, fontWeight: 700 }}>browse</span></p>
            <p className={styles.dropHint}>{types.map(t => t.toUpperCase()).join(" · ")}</p>
          </>
        )}
      </motion.div>
      {file && (
        <motion.button className={styles.submitBtn}
          style={{ background: color }}
          onClick={submit} disabled={busy}
          initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }}
          whileHover={{ filter: "brightness(1.1)", scale: 1.02 }} whileTap={{ scale: 0.97 }}>
          {busy
            ? <motion.span animate={{ rotate: 360 }} transition={{ duration: 0.7, repeat: Infinity, ease: "linear" }} style={{ display: "inline-block", fontSize: 20 }}>⟳</motion.span>
            : <><span style={{ width: 15, height: 15, display: "flex" }}>{IC.upload}</span> Submit</>
          }
        </motion.button>
      )}
    </div>
  );
}

/* ════════════════════════════════════════
   SHARED EMPTY STATE
════════════════════════════════════════ */
function EmptyTab({ message }) {
  return (
    <div className={styles.tabBody}>
      <div style={{ textAlign: "center", padding: "56px 0", color: "var(--text-muted)", fontSize: 15 }}>
        {message}
      </div>
    </div>
  );
}

/* ════════════════════════════════════════
   LECTURES TAB
════════════════════════════════════════ */
function LecturesTab({ lectures, color, shade }) {
  if (!lectures.length) return <EmptyTab message="📭 No lectures uploaded yet." />;
  const watchedCount = lectures.filter(l => l.watched).length;

  return (
    <div className={styles.tabBody}>
      {/* compact progress strip */}
      <div className={styles.progStrip}>
        <div className={styles.progInfo}>
          <span style={{ color, fontWeight: 700 }}>{watchedCount}/{lectures.length}</span>
          <span className={styles.progLabel}> lectures watched</span>
        </div>
        <div className={styles.progTrack}>
          <motion.div className={styles.progFill}
            style={{ background: color }}
            initial={{ width: 0 }}
            animate={{ width: `${Math.round(watchedCount / lectures.length * 100)}%` }}
            transition={{ delay: 0.2, duration: 1, ease: "easeOut" }}/>
        </div>
      </div>

      {/* Lecture CARD GRID */}
      <div className={styles.lecGrid}>
        {lectures.map((lec, i) => (
          <motion.div key={lec.id}
            className={`${styles.lecCard} ${lec.watched ? styles.lecWatched : ""}`}
            initial={{ opacity: 0, y: 20, scale: 0.94 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            transition={{ delay: i * 0.07, duration: 0.38, ease: [0.22, 1, 0.36, 1] }}
            whileHover={{ y: -5, boxShadow: `0 14px 36px rgba(0,0,0,0.13)` }}>

            {/* Card background with color tint */}
            <div className={styles.lecCardBg}
              style={{ background: lec.watched
                ? `linear-gradient(145deg, ${color}22 0%, ${color}0a 100%)`
                : "linear-gradient(145deg, #f8fafc 0%, #f1f5f9 100%)" }}/>

            {/* Top row: week badge + type pill + watched check */}
            <div className={styles.lecCardTop}>
              <span className={styles.weekBadge}
                style={{ background: lec.watched ? color : "#e2e8f0",
                         color: lec.watched ? "#fff" : "#94a3b8" }}>
                W{lec.week}
              </span>
              <span className={styles.typePill}
                style={{ background: lec.type === "video" ? "#dbeafe" : "#fef3c7",
                         color: lec.type === "video" ? "#1d4ed8" : "#92400e" }}>
                {lec.type === "video" ? "▶ VIDEO" : "📄 PDF"}
              </span>
              {lec.watched && (
                <span className={styles.checkBadge} style={{ color: "#10b981" }}>
                  {IC.check}
                </span>
              )}
            </div>

            {/* Title with gradient overlay effect */}
            <div className={styles.lecTitleWrap}>
              <div className={styles.lecTitleOverlay}
                style={{ background: lec.watched
                  ? `linear-gradient(to top, ${color}18 0%, transparent 100%)`
                  : "linear-gradient(to top, rgba(0,0,0,0.04) 0%, transparent 100%)" }}/>
              <h3 className={styles.lecTitle}>{lec.title}</h3>
            </div>

            {/* Meta row */}
            <div className={styles.lecMeta}>
              <span>⏱ {lec.duration}</span>
              <span className={styles.metaDot}>·</span>
              <span>{lec.date}</span>
              <span className={styles.metaDot}>·</span>
              <span>{lec.size}</span>
            </div>

            {/* Actions */}
            <div className={styles.lecCardActions}>
              <motion.button className={styles.lecDlBtn}
                whileHover={{ scale: 1.15 }} whileTap={{ scale: 0.9 }}>
                {IC.dl}
              </motion.button>
              <motion.button className={styles.lecMainBtn}
                style={{ background: color }}
                whileHover={{ scale: 1.04, filter: "brightness(1.1)" }}
                whileTap={{ scale: 0.95 }}>
                <span style={{ width: 11, height: 11, display: "flex" }}>
                  {lec.type === "video" ? IC.video : IC.pdf}
                </span>
                {lec.type === "video" ? "Watch" : "Open"}
              </motion.button>
            </div>
          </motion.div>
        ))}
      </div>
    </div>
  );
}

/* ════════════════════════════════════════
   ASSIGNMENTS TAB
════════════════════════════════════════ */
function AssignmentsTab({ initialAssignments, color, meta }) {
  const [list, setList]       = useState(initialAssignments);
  const [uploadAsn, setUploadAsn] = useState(null);

  if (!list.length) return <EmptyTab message="📭 No assignments posted yet." />;

  const submitted = list.filter(a => ["graded","submitted"].includes(a.status)).length;

  return (
    <div className={styles.tabBody}>
      <div className={styles.progStrip}>
        <div className={styles.progInfo}>
          <span style={{ color, fontWeight:700 }}>{submitted}/{list.length}</span>
          <span className={styles.progLabel}> submitted</span>
        </div>
        <div className={styles.progTrack}>
          <motion.div className={styles.progFill} style={{ background:color }}
            initial={{ width:0 }}
            animate={{ width:`${Math.round(submitted/list.length*100)}%` }}
            transition={{ delay:0.2, duration:1, ease:"easeOut" }}/>
        </div>
      </div>

      <div className={styles.asnGrid}>
        {list.map((a, i) => {
          const pct = a.grade !== null ? Math.round((a.grade/a.max)*100) : null;
          return (
            <motion.div key={a.id} className={styles.asnCard2}
              style={{ borderColor: a.status==="pending" ? `${color}40` : "#e4eaf2" }}
              initial={{ opacity:0, y:18, scale:0.94 }} animate={{ opacity:1, y:0, scale:1 }}
              transition={{ delay:i*0.07, duration:0.38, ease:[0.22,1,0.36,1] }}
              whileHover={{ y:-4, boxShadow:"0 14px 36px rgba(0,0,0,0.11)" }}>

              {a.status==="pending" && (
                <div className={styles.asnCardBg}
                  style={{ background:`linear-gradient(145deg,${color}10 0%,${color}04 100%)` }}/>
              )}

              <div className={styles.asnCard2Top}>
                <span className={styles.asnCard2Idx} style={{ background:`${color}15`, color }}>
                  {String(i+1).padStart(2,"0")}
                </span>
                <Chip status={a.status}/>
              </div>

              <div className={styles.asnCard2Title}>{a.title}</div>

              <div className={styles.asnCard2Meta}>
                <span>📅 {a.deadline}</span>
                <span>·</span>
                <span>{a.max} pts</span>
                {a.file && <><span>·</span><span>📎 {a.file}</span></>}
              </div>

              <div className={styles.asnCard2Footer}>
                {pct !== null ? (
                  <div className={styles.asnGradeDial}
                    style={{ background:`conic-gradient(${color} ${pct*3.6}deg, #eef2f7 0deg)` }}>
                    <div className={styles.asnGradeDialIn}>
                      <span style={{ color, fontSize:15, fontWeight:900 }}>{a.grade}</span>
                      <span style={{ color:"#94a3b8", fontSize:10 }}>/{a.max}</span>
                    </div>
                  </div>
                ) : a.status==="pending" ? (
                  <motion.button className={styles.asnSubmitBtn}
                    style={{ background:color }}
                    onClick={() => setUploadAsn(a)}
                    whileHover={{ scale:1.04, filter:"brightness(1.08)" }}
                    whileTap={{ scale:0.96 }}>
                    <span style={{ width:13,height:13,display:"flex" }}>{IC.upload}</span>
                    Submit Assignment
                  </motion.button>
                ) : (
                  <span className={styles.asnCard2Upcoming}>Opens {a.deadline}</span>
                )}
              </div>
            </motion.div>
          );
        })}
      </div>

      {/* ── Total Score Bar ── */}
      {(() => {
        const graded = list.filter(a => a.grade !== null);
        const total = graded.reduce((s,a) => s + a.grade, 0);
        const max   = graded.reduce((s,a) => s + a.max, 0);
        const pct   = max > 0 ? Math.round(total/max*100) : 0;
        const barC  = pct >= 80 ? "#22c55e" : pct >= 60 ? "#f59e0b" : "#ef4444";
        return max > 0 ? (
          <div className={styles.tabTotalBar} style={{borderColor:`${color}20`,background:`${color}06`}}>
            <span className={styles.tabTotalLabel}>📊 Total Assignment Score</span>
            <div className={styles.tabTotalRight}>
              <span className={styles.tabTotalNum} style={{color}}>{total}</span>
              <span className={styles.tabTotalMax}> / {max} pts</span>
              <div className={styles.tabTotalTrack}>
                <motion.div className={styles.tabTotalFill} style={{background:barC}}
                  initial={{width:0}} animate={{width:`${pct}%`}}
                  transition={{duration:0.9,ease:"easeOut",delay:0.3}}/>
              </div>
              <span className={styles.tabTotalPct} style={{color:barC}}>{pct}%</span>
            </div>
          </div>
        ) : null;
      })()}

      <AnimatePresence>
        {uploadAsn && (
          <motion.div className={styles.uploadModal}
            initial={{ opacity:0 }} animate={{ opacity:1 }} exit={{ opacity:0 }}>
            <motion.div className={styles.uploadModalBox}
              initial={{ y:80, opacity:0, scale:0.96 }}
              animate={{ y:0, opacity:1, scale:1 }}
              exit={{ y:40, opacity:0, scale:0.97 }}
              transition={{ duration:0.38, ease:[0.22,1,0.36,1] }}>

              <div className={styles.umHeader} style={{ background:color }}>
                <div className={styles.umHeaderBg}/>
                <div className={styles.umHeaderContent}>
                  <button className={styles.umClose} onClick={() => setUploadAsn(null)}>✕</button>
                  <div className={styles.umCourseName}>{meta.code} · {meta.name}</div>
                  <h2 className={styles.umTitle}>{uploadAsn.title}</h2>
                </div>
              </div>

              <div className={styles.umDetails}>
                {[
                  { icon:"👨‍🏫", label:"Instructor", val:meta.instructor },
                  { icon:"📅", label:"Deadline",   val:uploadAsn.deadline },
                  { icon:"⭐", label:"Points",     val:`${uploadAsn.max} pts` },
                  { icon:"📁", label:"Formats",    val:uploadAsn.types.map(t=>t.toUpperCase()).join(" · ") },
                ].map((d,i)=>(
                  <div key={i} className={styles.umDetailItem}>
                    <span className={styles.umDetailIcon}>{d.icon}</span>
                    <div>
                      <div className={styles.umDetailLabel}>{d.label}</div>
                      <div className={styles.umDetailVal}>{d.val}</div>
                    </div>
                  </div>
                ))}
              </div>

              <div className={styles.umBody}>
                <FileUpload id={uploadAsn.id} types={uploadAsn.types} color={color} large
                  onDone={() => {
                    setList(l => l.map(x => x.id===uploadAsn.id ? {...x, status:"submitted"} : x));
                    setUploadAsn(null);
                  }}/>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

/* ════════════════════════════════════════
   QUIZZES TAB
════════════════════════════════════════ */
function QuizzesTab({ quizzes, color, courseId }) {
  const navigate = useNavigate();

  if (!quizzes.length) return <EmptyTab message="📭 No quizzes available yet." />;

  const done = quizzes.filter(q => q.status === "completed").length;

  return (
    <div className={styles.tabBody}>
      <div className={styles.progStrip}>
        <div className={styles.progInfo}>
          <span style={{ color, fontWeight: 700 }}>{done}/{quizzes.length}</span>
          <span className={styles.progLabel}> completed</span>
        </div>
        <div className={styles.progTrack}>
          <motion.div className={styles.progFill} style={{ background: color }}
            initial={{ width: 0 }}
            animate={{ width: `${Math.round(done / quizzes.length * 100)}%` }}
            transition={{ delay: 0.2, duration: 1, ease: "easeOut" }}/>
        </div>
      </div>

      <div className={styles.quizGrid}>
        {quizzes.map((q, i) => (
          <motion.div key={q.id}
            className={`${styles.quizCard} ${q.status === "available" ? styles.quizOpen : ""}`}
            style={q.status === "available" ? { borderColor: `${color}50` } : {}}
            initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: i * 0.08, duration: 0.38, ease: [0.22, 1, 0.36, 1] }}
            whileHover={{ y: -4, boxShadow: "0 12px 30px rgba(0,0,0,0.1)" }}>

            <div className={styles.quizScoreArea}>
              {q.score !== null ? (
                <div className={styles.scoreDial}
                  style={{ background: `conic-gradient(${color} ${q.score/q.max*360}deg, #eef2f7 0deg)` }}>
                  <div className={styles.scoreInner}>
                    <span className={styles.scoreNum} style={{ color }}>{q.score}</span>
                    <span className={styles.scoreMax}>/{q.max}</span>
                  </div>
                </div>
              ) : (
                <div className={styles.scorePlaceholder}
                  style={{ background: q.status === "available" ? `${color}18` : "#f1f5f9",
                           color: q.status === "available" ? color : "#94a3b8" }}>
                  <span style={{ width: 22, height: 22, display: "flex" }}>
                    {q.status === "upcoming" ? IC.lock : IC.quiz}
                  </span>
                </div>
              )}
              <Chip status={q.status}/>
            </div>

            <div className={styles.quizTitle}>{q.title}</div>
            <div className={styles.quizMeta}>
              <span>📅 {q.date}</span>
              <span>⏱ {q.duration}</span>
              <span>❓ {q.questions} questions</span>
            </div>

            {q.status === "available" && (
              <motion.button className={styles.startBtn} style={{ background: color }}
                onClick={() => navigate(`/student/quiz/${courseId}/${q.id}`)}
                whileHover={{ scale: 1.04, filter: "brightness(1.08)" }}
                whileTap={{ scale: 0.96 }}>
                Start Quiz →
              </motion.button>
            )}
          </motion.div>
        ))}
      </div>

      {/* ── Total Score Bar ── */}
      {(() => {
        const done  = quizzes.filter(q => q.score !== null);
        const total = done.reduce((s,q) => s + q.score, 0);
        const max   = done.reduce((s,q) => s + q.max, 0);
        const pct   = max > 0 ? Math.round(total/max*100) : 0;
        const barC  = pct >= 80 ? "#22c55e" : pct >= 60 ? "#f59e0b" : "#ef4444";
        return max > 0 ? (
          <div className={styles.tabTotalBar} style={{borderColor:`${color}20`,background:`${color}06`}}>
            <span className={styles.tabTotalLabel}>📊 Total Quiz Score</span>
            <div className={styles.tabTotalRight}>
              <span className={styles.tabTotalNum} style={{color}}>{total}</span>
              <span className={styles.tabTotalMax}> / {max} pts</span>
              <div className={styles.tabTotalTrack}>
                <motion.div className={styles.tabTotalFill} style={{background:barC}}
                  initial={{width:0}} animate={{width:`${pct}%`}}
                  transition={{duration:0.9,ease:"easeOut",delay:0.3}}/>
              </div>
              <span className={styles.tabTotalPct} style={{color:barC}}>{pct}%</span>
            </div>
          </div>
        ) : null;
      })()}
    </div>
  );
}


/* ════════════════════════════════════════
   GRADES TAB — Side nav + detail panel
════════════════════════════════════════ */

/* Mock midterm data */
const MIDTERM_DATA = {
  date: "March 20, 2026",
  time: "10:00 AM – 12:00 PM",
  room: "Hall 144 · Building B",
  published: true,
  grade: 17,
  max:   20,
};

function GradesTab({ quizzes, assignments, color }) {
  const [active, setActive] = useState("midterm");

  const gradedAsn  = assignments.filter(a => a.grade !== null);
  const asnTotal   = gradedAsn.reduce((s,a) => s + a.grade, 0);
  const asnMax     = gradedAsn.reduce((s,a) => s + a.max, 0);

  const gradedQuiz = quizzes.filter(q => q.score !== null);
  const quizTotal  = gradedQuiz.reduce((s,q) => s + q.score, 0);
  const quizMax    = gradedQuiz.reduce((s,q) => s + q.max, 0);

  const midPct = MIDTERM_DATA.published
    ? Math.round(MIDTERM_DATA.grade / MIDTERM_DATA.max * 100)
    : null;
  const midC = midPct >= 80 ? "#22c55e" : midPct >= 60 ? "#f59e0b" : "#ef4444";

  const tabs = [
    {
      key: "midterm",
      icon: "📝",
      label: "Midterm",
      val: MIDTERM_DATA.published ? `${MIDTERM_DATA.grade}/${MIDTERM_DATA.max}` : "—",
      sub: MIDTERM_DATA.published ? `${midPct}%` : "Not released",
    },
    {
      key: "assignments",
      icon: "📋",
      label: "Assignments",
      val: `${asnTotal}/${asnMax}`,
      sub: `${gradedAsn.length}/${assignments.length} graded`,
    },
    {
      key: "quizzes",
      icon: "✏️",
      label: "Quizzes",
      val: `${quizTotal}/${quizMax}`,
      sub: `${gradedQuiz.length}/${quizzes.length} done`,
    },
  ];

  return (
    <div className={styles.gradesWrap}>

      {/* ── Top tab selector ── */}
      <div className={styles.gradesTabs}>
        {tabs.map(t => (
          <motion.button key={t.key}
            className={`${styles.gradesTab} ${active===t.key ? styles.gradesTabOn : ""}`}
            style={active===t.key ? {"--gt": color} : {}}
            onClick={() => setActive(t.key)}
            whileHover={{ y: -2 }} whileTap={{ scale: 0.97 }}>
            <motion.div className={styles.gradesTabBar}
              style={{ background: color }}
              animate={{ scaleX: active===t.key ? 1 : 0 }}
              transition={{ duration: 0.24, ease: [0.22,1,0.36,1] }}/>
            <span className={styles.gradesTabIcon}>{t.icon}</span>
            <div className={styles.gradesTabCenter}>
              <span className={styles.gradesTabLabel}>{t.label}</span>
            </div>
            <div className={styles.gradesTabRight}>
              <span className={styles.gradesTabVal} style={active===t.key?{color}:{}}>{t.val}</span>
              <span className={styles.gradesTabSub}>{t.sub}</span>
            </div>
          </motion.button>
        ))}
      </div>

      {/* ── Content area ── */}
      <div className={styles.gradesContent}>
        <AnimatePresence mode="wait">

          {/* MIDTERM */}
          {active==="midterm" && (
            <motion.div key="mid"
              initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -16 }} transition={{ duration: 0.22 }}>

              {!MIDTERM_DATA.published ? (
                <div className={styles.gradesNotPublished}>
                  <span>🔒</span>
                  <h3>Grade Not Published Yet</h3>
                  <p>Check back after the exam results are released</p>
                </div>
              ) : (
                <div className={styles.midtermFull}>

                  {/* Hero grade row */}
                  <div className={styles.midHero} style={{ borderColor: `${midC}30` }}>
                    {/* Big circle */}
                    <div className={styles.midHeroCircleWrap}>
                      <svg viewBox="0 0 120 120" className={styles.midHeroSvg}>
                        <circle cx="60" cy="60" r="52" fill="none"
                          stroke="var(--prog-track)" strokeWidth="8"/>
                        <motion.circle cx="60" cy="60" r="52" fill="none"
                          stroke={midC} strokeWidth="8"
                          strokeLinecap="round"
                          strokeDasharray={`${327}`}
                          initial={{ strokeDashoffset: 327 }}
                          animate={{ strokeDashoffset: 327 - 327 * (midPct/100) }}
                          transition={{ duration: 1.1, ease: "easeOut", delay: 0.15 }}
                          style={{ transform: "rotate(-90deg)", transformOrigin: "60px 60px" }}/>
                      </svg>
                      <div className={styles.midHeroCircleInner}>
                        <span className={styles.midHeroGrade} style={{ color: midC }}>
                          {MIDTERM_DATA.grade}
                        </span>
                        <span className={styles.midHeroMax}>/{MIDTERM_DATA.max}</span>
                      </div>
                    </div>

                    {/* Grade info */}
                    <div className={styles.midHeroInfo}>
                      <div className={styles.midHeroPct} style={{ color: midC }}>{midPct}%</div>
                      <div className={styles.midHeroLabel}>Midterm Score</div>
                      <div className={styles.midHeroBar}>
                        <div className={styles.midHeroBarTrack}>
                          <motion.div className={styles.midHeroBarFill}
                            style={{ background: midC }}
                            initial={{ width: 0 }}
                            animate={{ width: `${midPct}%` }}
                            transition={{ duration: 1, ease: "easeOut", delay: 0.2 }}/>
                        </div>
                      </div>
                      <div className={styles.midHeroRemark} style={{
                        color: midPct>=80?"#15803d":midPct>=60?"#92400e":"#b91c1c",
                        background: midPct>=80?"#f0fdf4":midPct>=60?"#fef9c3":"#fef2f2",
                        border: `1px solid ${midPct>=80?"#bbf7d0":midPct>=60?"#fde68a":"#fecaca"}`,
                      }}>
                        {midPct>=80?"Excellent ⭐":midPct>=60?"Good 👍":"Needs Improvement ⚠"}
                      </div>
                    </div>
                  </div>

                  {/* Exam details grid */}
                  <div className={styles.midDetails}>
                    {[
                      { ico: "📅", label: "Date",  val: MIDTERM_DATA.date },
                      { ico: "⏰", label: "Time",  val: MIDTERM_DATA.time },
                      { ico: "🏛",  label: "Room",  val: MIDTERM_DATA.room },
                      { ico: "⭐", label: "Score", val: `${MIDTERM_DATA.grade} out of ${MIDTERM_DATA.max} points` },
                    ].map((d,i) => (
                      <motion.div key={d.label} className={styles.midDetailCard}
                        initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.1 + i*0.06 }}>
                        <span className={styles.midDetailIco}>{d.ico}</span>
                        <div>
                          <div className={styles.midDetailLabel}>{d.label}</div>
                          <div className={styles.midDetailVal}>{d.val}</div>
                        </div>
                      </motion.div>
                    ))}
                  </div>
                </div>
              )}
            </motion.div>
          )}

          {/* ASSIGNMENTS */}
          {active==="assignments" && (
            <motion.div key="asn"
              initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -16 }} transition={{ duration: 0.22 }}>

              {/* Total banner */}
              <div className={styles.gradesBanner} style={{ borderColor:`${color}25`, background:`${color}07` }}>
                <div className={styles.gradesBannerLeft}>
                  <span className={styles.gradesBannerIcon}>📋</span>
                  <div>
                    <div className={styles.gradesBannerTitle}>Assignment Grades</div>
                    <div className={styles.gradesBannerSub}>{gradedAsn.length} of {assignments.length} graded</div>
                  </div>
                </div>
                <div className={styles.gradesBannerScore}>
                  <span className={styles.gradesBannerNum} style={{ color }}>{asnTotal}</span>
                  <span className={styles.gradesBannerMax}>/ {asnMax} pts</span>
                </div>
              </div>

              <div className={styles.gradesList}>
                {assignments.map((a, i) => {
                  const pct  = a.grade !== null ? Math.round(a.grade/a.max*100) : null;
                  const barC = pct === null ? "var(--border)" : pct>=80?"#22c55e":pct>=60?"#f59e0b":"#ef4444";
                  return (
                    <motion.div key={a.id} className={styles.gradesItem}
                      initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: i * 0.055 }}>
                      <div className={styles.gradesItemInfo}>
                        <div className={styles.gradesItemName}>{a.title}</div>
                        <div className={styles.gradesItemSub}>📅 Due {a.deadline}</div>
                      </div>
                      {pct !== null ? (
                        <div className={styles.gradesItemRight}>
                          <div className={styles.gradesItemBarWrap}>
                            <div className={styles.gradesItemBar}>
                              <motion.div style={{ background: barC, height: "100%", borderRadius: 99 }}
                                initial={{ width: 0 }}
                                animate={{ width: `${pct}%` }}
                                transition={{ delay: 0.12 + i*0.05, duration: 0.75 }}/>
                            </div>
                            <span className={styles.gradesItemPct} style={{ color: barC }}>{pct}%</span>
                          </div>
                          <span className={styles.gradesItemScore} style={{ color: barC }}>
                            {a.grade}<span className={styles.gradesItemScoreMax}>/{a.max}</span>
                          </span>
                        </div>
                      ) : (
                        <span className={styles.gradesItemNA}
                          style={{ color:a.status==="pending"?"#d97706":"#94a3b8",
                                   background:a.status==="pending"?"#fef9c3":"var(--hover-bg)" }}>
                          {a.status==="pending"?"⏰ Pending":"🔒 Upcoming"}
                        </span>
                      )}
                    </motion.div>
                  );
                })}
              </div>
            </motion.div>
          )}

          {/* QUIZZES */}
          {active==="quizzes" && (
            <motion.div key="quiz"
              initial={{ opacity: 0, x: 20 }} animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -16 }} transition={{ duration: 0.22 }}>

              {/* Total banner */}
              <div className={styles.gradesBanner} style={{ borderColor:`${color}25`, background:`${color}07` }}>
                <div className={styles.gradesBannerLeft}>
                  <span className={styles.gradesBannerIcon}>✏️</span>
                  <div>
                    <div className={styles.gradesBannerTitle}>Quiz Grades</div>
                    <div className={styles.gradesBannerSub}>{gradedQuiz.length} of {quizzes.length} completed</div>
                  </div>
                </div>
                <div className={styles.gradesBannerScore}>
                  <span className={styles.gradesBannerNum} style={{ color }}>{quizTotal}</span>
                  <span className={styles.gradesBannerMax}>/ {quizMax} pts</span>
                </div>
              </div>

              <div className={styles.gradesList}>
                {quizzes.map((q, i) => {
                  const pct  = q.score !== null ? Math.round(q.score/q.max*100) : null;
                  const barC = pct === null ? "var(--border)" : pct>=80?"#22c55e":pct>=60?"#f59e0b":"#ef4444";
                  return (
                    <motion.div key={q.id} className={styles.gradesItem}
                      initial={{ opacity: 0, y: 8 }} animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: i * 0.055 }}>
                      <div className={styles.gradesItemInfo}>
                        <div className={styles.gradesItemName}>{q.title}</div>
                        <div className={styles.gradesItemSub}>📅 {q.date} · ⏱ {q.duration}</div>
                      </div>
                      {pct !== null ? (
                        <div className={styles.gradesItemRight}>
                          <div className={styles.gradesItemBarWrap}>
                            <div className={styles.gradesItemBar}>
                              <motion.div style={{ background: barC, height: "100%", borderRadius: 99 }}
                                initial={{ width: 0 }}
                                animate={{ width: `${pct}%` }}
                                transition={{ delay: 0.12 + i*0.05, duration: 0.75 }}/>
                            </div>
                            <span className={styles.gradesItemPct} style={{ color: barC }}>{pct}%</span>
                          </div>
                          <span className={styles.gradesItemScore} style={{ color: barC }}>
                            {q.score}<span className={styles.gradesItemScoreMax}>/{q.max}</span>
                          </span>
                        </div>
                      ) : (
                        <span className={styles.gradesItemNA}>
                          {q.status==="upcoming"?"⏳ Upcoming":"🔓 Available"}
                        </span>
                      )}
                    </motion.div>
                  );
                })}
              </div>
            </motion.div>
          )}

        </AnimatePresence>
      </div>
    </div>
  );
}


/* ════════════════════════════════════════
   MAIN PAGE
════════════════════════════════════════ */
const TABS = [
  { key: "lectures",    label: "Lectures",    icon: "🎬" },
  { key: "assignments", label: "Assignments", icon: "📋" },
  { key: "quizzes",     label: "Quizzes",     icon: "✏️" },
  { key: "grades",      label: "Grades",      icon: "📊" },
];

export default function CourseDetailPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const [data, setData] = useState(null);
  const [tab, setTab] = useState("lectures");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let alive = true;
    setLoading(true);
    setTab("lectures");
    setError(null);
    setData(null);
    getStudentCourseDetail(courseId)
      .then(d => { if (alive) setData(d); })
      .catch(err => {
        if (alive) setError(
          err?.response?.data?.error?.message || "Course not found or not enrolled."
        );
      })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
  }, [courseId]);

  if (loading) return (
    <div className={styles.page}>
      <div className={styles.heroSkeleton}/>
    </div>
  );

  if (error) return (
    <div className={styles.page}>
      <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
        style={{ margin: "48px auto", maxWidth: 480, textAlign: "center",
                 padding: "32px 24px", background: "var(--card-bg)",
                 border: "1px solid var(--card-border)", borderRadius: 16 }}>
        <div style={{ fontSize: 42, marginBottom: 12 }}>📭</div>
        <div style={{ fontSize: 16, fontWeight: 700, color: "var(--text-primary)", marginBottom: 8 }}>
          {error}
        </div>
        <button onClick={() => navigate("/student/courses")}
          style={{ marginTop: 16, padding: "10px 24px", borderRadius: 99, border: "none",
                   background: "var(--accent)", color: "#fff", fontWeight: 700,
                   cursor: "pointer", fontSize: 14 }}>
          ← Back to Courses
        </button>
      </motion.div>
    </div>
  );

  const { meta, lectures, assignments, quizzes } = data;
  const counts = { lectures: lectures.length, assignments: assignments.length, quizzes: quizzes.length, grades: quizzes.filter(q=>q.score!==null).length + assignments.filter(a=>a.grade!==null).length };
  const c = meta.color;

  return (
    <div className={styles.page}>

      {/* ══ HERO CARD ══ */}
      <div className={styles.heroWrapper}>
        <motion.div className={styles.heroCard}
          initial={{ opacity: 0, y: 18, scale: 0.97 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          transition={{ duration: 0.48, ease: [0.22, 1, 0.36, 1] }}>

          {/* Background layers */}
          <div className={styles.heroCardBg} style={{ background: c }}/>
          <div className={styles.heroCardMesh}
            style={{ background: `radial-gradient(ellipse 60% 80% at 90% 10%, ${meta.shade}dd 0%, transparent 55%),
                                   radial-gradient(ellipse 40% 50% at 5% 90%, rgba(0,0,0,0.22) 0%, transparent 50%)` }}/>
          {/* Decorative dots pattern */}
          <div className={styles.heroPattern}/>

          {/* Content */}
          <div className={styles.heroInner}>
            <div className={styles.heroTopRow}>
              <motion.button className={styles.backBtn}
                onClick={() => navigate("/student/courses")}
                whileHover={{ x: -3 }} whileTap={{ scale: 0.92 }}>
                <span style={{ width: 15, height: 15, display: "flex" }}>{IC.back}</span>
                Courses
              </motion.button>
              <Ring pct={meta.progress} size={94} color={c}/>
            </div>

            <motion.div className={styles.heroInfo}
              initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.14, duration: 0.44 }}>

              <div className={styles.codeTag}>{meta.code}</div>

              {/* Typewriter title */}
              <TitleReveal text={meta.name}/>

              <p className={styles.heroDesc}>{meta.description}</p>

              <div className={styles.heroPills}>
                {[
                  `👨‍🏫 ${meta.instructor}`,
                  `🎓 Level ${meta.level}`,
                  `⭐ ${meta.credits} Credits`,
                  `📅 ${meta.semester}`,
                ].map((t, i) => (
                  <motion.span key={i} className={styles.heroPill}
                    initial={{ opacity: 0, scale: 0.85 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ delay: 0.28 + i * 0.07 }}>
                    {t}
                  </motion.span>
                ))}
              </div>
            </motion.div>

            {/* Stats row */}
            <motion.div className={styles.statsRow}
              initial={{ opacity: 0 }} animate={{ opacity: 1 }}
              transition={{ delay: 0.36 }}>
              {[
                { n: lectures.length,                                      l: "Lectures"    },
                { n: lectures.filter(x => x.watched).length,               l: "Watched"     },
                { n: assignments.filter(x => x.grade !== null).length,     l: "Graded"      },
                { n: quizzes.filter(x => x.status === "completed").length, l: "Quizzes Done"},
              ].map((s, i) => (
                <div key={i} className={styles.statItem}>
                  <span className={styles.statNum}>{s.n}</span>
                  <span className={styles.statLbl}>{s.l}</span>
                </div>
              ))}
            </motion.div>
          </div>
        </motion.div>
      </div>

      {/* ══ TAB BAR ══ */}
      <div className={styles.tabBar}>
        {TABS.map(t => (
          <button key={t.key}
            className={`${styles.tabBtn} ${tab === t.key ? styles.tabOn : ""}`}
            onClick={() => setTab(t.key)}>
            {tab === t.key && (
              <motion.div className={styles.tabSlider} layoutId="slider"
                style={{ background: c }}
                transition={{ type: "spring", stiffness: 420, damping: 35 }}/>
            )}
            <span className={styles.tabLabel}>
              {t.icon} {t.label}
              <span className={styles.tabBadge}
                style={tab === t.key ? { background: "rgba(255,255,255,0.28)", color: "#fff" } : {}}>
                {counts[t.key]}
              </span>
            </span>
          </button>
        ))}
      </div>

      {/* ══ BODY ══ */}
      <div className={styles.body}>
        <AnimatePresence mode="wait">
          <motion.div key={tab}
            initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }} transition={{ duration: 0.2 }}>
            {tab === "lectures"    && <LecturesTab    lectures={lectures}               color={c} shade={meta.shade}/>}
            {tab === "assignments" && <AssignmentsTab initialAssignments={assignments}  color={c} meta={meta}/>}
            {tab === "quizzes"     && <QuizzesTab     quizzes={quizzes}                 color={c} courseId={courseId}/>}
            {tab === "grades"      && <GradesTab      quizzes={quizzes} assignments={assignments} color={c}/>}
          </motion.div>
        </AnimatePresence>
      </div>

    </div>
  );
}