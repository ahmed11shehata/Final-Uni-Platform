// src/pages/student/CourseDetailPage.jsx
import { useState, useEffect, useRef, useMemo } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { getStudentCourseDetail, submitAssignment, removeSubmission } from "../../services/api/studentApi";
import styles from "./CourseDetailPage.module.css";

const CARD_COLORS = [
  "#5D8FA3",
  "#6F90B7",
  "#B88943",
  "#8A78B4",
  "#B35D82",
  "#6A9AA7",
  "#7E8E98",
  "#4E88C7",
];

function stableIndex(seed, mod) {
  let h = 0;
  const s = String(seed || "course");
  for (let i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) >>> 0;
  return h % mod;
}

function formatCourseName(value) {
  if (!value) return "Untitled Course";
  return String(value)
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2")
    .replace(/\bIntroto\b/gi, "Intro to")
    .replace(/\s+/g, " ")
    .trim();
}

function resolveCourseColor(meta, navState) {
  if (navState?.courseColor) return navState.courseColor;
  const seed = meta?.code || meta?.name || meta?.id || "course";
  return CARD_COLORS[stableIndex(seed, CARD_COLORS.length)];
}

function hexToRgba(hex, alpha = 1) {
  if (!hex) return `rgba(99,102,241,${alpha})`;
  const clean = hex.replace("#", "");
  const normalized = clean.length === 3
    ? clean.split("").map((c) => c + c).join("")
    : clean;
  const num = parseInt(normalized, 16);
  const r = (num >> 16) & 255;
  const g = (num >> 8) & 255;
  const b = num & 255;
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

function buildCourseTheme(color) {
  return {
    "--course-color": color,
    "--course-shade": color,
    "--course-soft": hexToRgba(color, 0.08),
    "--course-soft-2": hexToRgba(color, 0.12),
    "--course-soft-3": hexToRgba(color, 0.16),
    "--course-border": hexToRgba(color, 0.24),
    "--course-border-strong": hexToRgba(color, 0.36),
    "--course-glow": hexToRgba(color, 0.18),
    "--course-text": color,
    "--course-track": hexToRgba(color, 0.18),
  };
}

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

const ST = {
  graded:    { label:"Graded",    bg:"rgba(16,185,129,0.14)",  color:"#10b981", dot:"#10b981" },
  pending:   { label:"Pending",   bg:"rgba(245,158,11,0.14)",  color:"#f59e0b", dot:"#f59e0b" },
  upcoming:  { label:"Upcoming",  bg:"rgba(148,163,184,0.14)", color:"#94a3b8", dot:"#94a3b8" },
  locked:    { label:"Locked",    bg:"rgba(148,163,184,0.14)", color:"#94a3b8", dot:"#94a3b8" },
  completed: { label:"Done ✓",    bg:"rgba(16,185,129,0.14)",  color:"#10b981", dot:"#10b981" },
  available: { label:"Open Now",  bg:"rgba(139,92,246,0.14)",  color:"#a78bfa", dot:"#8b5cf6" },
  submitted: { label:"Submitted", bg:"rgba(59,130,246,0.14)",  color:"#60a5fa", dot:"#3b82f6" },
  rejected:  { label:"Rejected",  bg:"rgba(239,68,68,0.14)",   color:"#ef4444", dot:"#ef4444" },
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

function TitleReveal({ text }) {
  const words = formatCourseName(text).split(" ");
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

function Ring({ pct, size = 96 }) {
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

function FileUpload({ id, types, color, onDone }) {
  const [drag, setDrag] = useState(false);
  const [file, setFile] = useState(null);
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState(false);
  const [err,  setErr]  = useState(null);
  const ref = useRef();

  const submit = async () => {
    if (!file) return;
    setBusy(true); setErr(null);
    try {
      await submitAssignment(id, file);
      setBusy(false); setDone(true); onDone?.();
    } catch (e) {
      setBusy(false);
      setErr(e?.message || "Upload failed. Try again.");
    }
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
      {err && (
        <div style={{ color: "#ef4444", fontSize: 13, marginBottom: 8, padding: "6px 10px",
                      background: "rgba(239,68,68,0.08)", borderRadius: 8, border: "1px solid rgba(239,68,68,0.2)" }}>
          ⚠️ {err}
        </div>
      )}
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

/* Open a file URL in a new tab (view) or trigger browser download */
function openFile(url) {
  window.open(url, "_blank", "noopener,noreferrer");
}
async function downloadFile(url) {
  try {
    const res  = await fetch(url);
    const blob = await res.blob();
    const bUrl = URL.createObjectURL(blob);
    const a    = document.createElement("a");
    a.href     = bUrl;
    a.download = url.split("/").pop() || "file";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(bUrl);
  } catch {
    openFile(url); // fallback: open in new tab
  }
}

function EmptyTab({ message }) {
  return (
    <div className={styles.tabBody}>
      <div className={styles.emptyTone}>
        {message}
      </div>
    </div>
  );
}

function LecturesTab({ lectures, color }) {
  if (!lectures.length) return <EmptyTab message="📭 No lectures uploaded yet." />;
  const watchedCount = lectures.filter(l => l.watched).length;

  return (
    <div className={styles.tabBody}>
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

      <div className={styles.lecGrid}>
        {lectures.map((lec, i) => (
          <motion.div key={lec.id}
            className={`${styles.lecCard} ${lec.watched ? styles.lecWatched : ""}`}
            initial={{ opacity: 0, y: 20, scale: 0.94 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            transition={{ delay: i * 0.07, duration: 0.38, ease: [0.22, 1, 0.36, 1] }}
            whileHover={{ y: -5, boxShadow: `0 14px 36px rgba(0,0,0,0.13)` }}>

            <div className={styles.lecCardBg}
              style={{ background: lec.watched
                ? `linear-gradient(145deg, ${color}22 0%, ${color}0a 100%)`
                : "linear-gradient(145deg, var(--course-soft) 0%, var(--card-bg) 100%)" }}/>

            <div className={styles.lecCardTop}>
              {lec.week != null && (
                <span className={styles.weekBadge}
                  style={{ background: lec.watched ? color : "var(--course-soft-2)",
                           color: lec.watched ? "#fff" : "var(--course-text)" }}>
                  W{lec.week}
                </span>
              )}
              <span className={styles.typePill}
                style={{ background: lec.type === "video" ? "var(--course-soft-2)" : "rgba(245,158,11,0.14)",
                         color: lec.type === "video" ? color : "#92400e",
                         border: lec.type === "video" ? `1px solid var(--course-border)` : "1px solid rgba(245,158,11,0.22)" }}>
                {lec.type === "video" ? "▶ VIDEO" : "📄 PDF"}
              </span>
              {lec.watched && (
                <span className={styles.checkBadge} style={{ color: "#10b981" }}>
                  {IC.check}
                </span>
              )}
            </div>

            <div className={styles.lecTitleWrap}>
              <div className={styles.lecTitleOverlay}
                style={{ background: lec.watched
                  ? `linear-gradient(to top, ${color}18 0%, transparent 100%)`
                  : "linear-gradient(to top, var(--course-soft) 0%, transparent 100%)" }}/>
              <h3 className={styles.lecTitle}>{lec.title}</h3>
            </div>

            <div className={styles.lecMeta}>
              <span>⏱ {lec.duration}</span>
              <span className={styles.metaDot}>·</span>
              <span>{lec.date}</span>
              <span className={styles.metaDot}>·</span>
              <span>{lec.size}</span>
            </div>

            <div className={styles.lecCardActions}>
              <motion.button className={styles.lecDlBtn}
                title="Download"
                onClick={() => lec.url ? downloadFile(lec.url) : undefined}
                style={lec.url ? {} : { opacity: 0.35, cursor: "not-allowed" }}
                whileHover={lec.url ? { scale: 1.15 } : {}}
                whileTap={lec.url ? { scale: 0.9 } : {}}>
                {IC.dl}
              </motion.button>
              <motion.button className={styles.lecMainBtn}
                style={{ background: color, opacity: lec.url ? 1 : 0.45, cursor: lec.url ? "pointer" : "not-allowed" }}
                onClick={() => lec.url ? openFile(lec.url) : undefined}
                whileHover={lec.url ? { scale: 1.04, filter: "brightness(1.1)" } : {}}
                whileTap={lec.url ? { scale: 0.95 } : {}}>
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

const MAX_ATTEMPTS = 4; // must match backend MaxSubmissionAttempts

function AssignmentsTab({ initialAssignments, color, meta }) {
  const [list, setList]               = useState(initialAssignments);
  const [uploadAsn, setUploadAsn]     = useState(null);
  // modal-internal states
  const [confirmDel, setConfirmDel]   = useState(false);
  const [deleting,   setDeleting]     = useState(false);
  const [deleteErr,  setDeleteErr]    = useState(null);
  const [showUpload, setShowUpload]   = useState(false); // true after confirmed delete

  if (!list.length) return <EmptyTab message="📭 No assignments posted yet." />;

  const submitted = list.filter(a => ["graded","submitted","rejected"].includes(a.status)).length;

  const closeModal = () => {
    setUploadAsn(null);
    setConfirmDel(false);
    setDeleting(false);
    setDeleteErr(null);
    setShowUpload(false);
  };

  const openModal = (a) => {
    setUploadAsn(a);
    setConfirmDel(false);
    setDeleting(false);
    setDeleteErr(null);
    setShowUpload(false);
  };

  const handleDelete = async () => {
    if (!uploadAsn) return;
    setDeleting(true);
    setDeleteErr(null);
    try {
      await removeSubmission(parseInt(uploadAsn.id));
      setList(l => l.map(x => x.id === uploadAsn.id
        ? { ...x, status: "available", submissionFileName: null, submissionFileUrl: null,
                  submissionDate: null, canSubmit: true }
        : x));
      setConfirmDel(false);
      setShowUpload(true);
    } catch (e) {
      setDeleteErr(e?.message || "Delete failed. Please try again.");
    }
    setDeleting(false);
  };

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

      <AnimatePresence>
        {uploadAsn && (() => {
          const asn = uploadAsn;
          const hasSubmission = !!asn.submissionFileName && !showUpload;
          const canDelete     = asn.canSubmit && (asn.attemptCount ?? 0) < MAX_ATTEMPTS;
          const atMax         = (asn.attemptCount ?? 0) >= MAX_ATTEMPTS;
          const pastDeadline  = asn.status === "locked";

          return (
          <motion.div
            className={styles.uploadModal}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            onClick={closeModal}>
            <motion.div
              className={styles.uploadModalBox}
              initial={{ opacity: 0, y: 40, scale: 0.94 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              exit={{ opacity: 0, y: 20, scale: 0.96 }}
              transition={{ duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
              onClick={e => e.stopPropagation()}>

              {/* ── Colored header ── */}
              <div className={styles.umHeader} style={{ background: color }}>
                <div className={styles.umHeaderBg}/>
                <button className={styles.umClose} onClick={closeModal}>✕</button>
                <div className={styles.umHeaderContent}>
                  {meta?.name && <div className={styles.umCourseName}>{meta.name}</div>}
                  <h2 className={styles.umTitle}>{asn.title}</h2>
                </div>
              </div>

              {/* ── Info grid ── */}
              <div className={styles.umDetails}>
                <div className={styles.umDetailItem}>
                  <span className={styles.umDetailIcon}>📅</span>
                  <div>
                    <div className={styles.umDetailLabel}>Deadline</div>
                    <div className={styles.umDetailVal}>{asn.deadline}</div>
                  </div>
                </div>
                {asn.attachmentUrl ? (
                  <button
                    className={styles.umDetailItem}
                    onClick={() => downloadFile(asn.attachmentUrl)}
                    title="Download instructor attachment"
                    style={{ cursor: "pointer", border: "none", background: "none",
                             width: "100%", textAlign: "left", padding: 0, fontFamily: "inherit" }}>
                    <span className={styles.umDetailIcon}>📎</span>
                    <div style={{ minWidth: 0 }}>
                      <div className={styles.umDetailLabel}>Assignment File</div>
                      <div className={styles.umDetailVal}
                        style={{ color, textDecoration: "underline",
                                 wordBreak: "break-all", overflow: "hidden",
                                 textOverflow: "ellipsis", whiteSpace: "nowrap", maxWidth: 130 }}>
                        {asn.attachmentUrl.split("/").pop() || "Download"}
                      </div>
                    </div>
                  </button>
                ) : (
                  <div className={styles.umDetailItem}>
                    <span className={styles.umDetailIcon}>📎</span>
                    <div>
                      <div className={styles.umDetailLabel}>Assignment File</div>
                      <div className={styles.umDetailVal}
                        style={{ color: "var(--text-muted,#94a3b8)" }}>
                        No attachment
                      </div>
                    </div>
                  </div>
                )}
                {asn.releaseDate && (
                  <div className={styles.umDetailItem}>
                    <span className={styles.umDetailIcon}>📣</span>
                    <div>
                      <div className={styles.umDetailLabel}>Releases</div>
                      <div className={styles.umDetailVal}>{asn.releaseDate}</div>
                    </div>
                  </div>
                )}
                <div className={styles.umDetailItem}>
                  <span className={styles.umDetailIcon}>🔄</span>
                  <div>
                    <div className={styles.umDetailLabel}>Attempts</div>
                    <div className={styles.umDetailVal}
                      style={{ color: atMax ? "#ef4444" : "var(--text-primary,#0f172a)" }}>
                      {asn.attemptCount ?? 0} / {MAX_ATTEMPTS}
                    </div>
                  </div>
                </div>
              </div>

              {/* ── Body ── */}
              <div className={styles.umBody}>
                {/* Description */}
                {asn.description && (
                  <div style={{ marginBottom: 14, fontSize: 13, color: "var(--text-secondary,#475569)",
                                lineHeight: 1.65, padding: "10px 14px",
                                background: "var(--card-bg-2,#f8fafc)",
                                border: "1.5px solid var(--course-border)", borderRadius: 10 }}>
                    {asn.description}
                  </div>
                )}

                {/* ── Existing submission block ── */}
                {hasSubmission && (
                  <AnimatePresence mode="wait">
                    {!confirmDel ? (
                      <motion.div key="sub-card"
                        initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -4 }} transition={{ duration: 0.2 }}
                        style={{ padding: "12px 14px", borderRadius: 12, marginBottom: 14,
                                 border: `1.5px solid ${color}30`, background: `${color}08` }}>
                        <div style={{ display: "flex", alignItems: "flex-start", gap: 10 }}>
                          <span style={{ fontSize: 22, lineHeight: 1 }}>📎</span>
                          <div style={{ flex: 1, minWidth: 0 }}>
                            <div style={{ fontSize: 13, fontWeight: 700,
                                          color: "var(--text-primary,#0f172a)",
                                          whiteSpace: "nowrap", overflow: "hidden",
                                          textOverflow: "ellipsis" }}>
                              {asn.submissionFileName}
                            </div>
                            <div style={{ fontSize: 11.5, color: "var(--text-muted,#94a3b8)",
                                          marginTop: 2 }}>
                              Submitted: {asn.submissionDate}
                            </div>
                          </div>
                          {asn.submissionFileUrl && (
                            <button
                              onClick={() => downloadFile(asn.submissionFileUrl)}
                              title="Download your submission"
                              style={{ padding: "5px 9px", borderRadius: 8, cursor: "pointer",
                                       border: `1px solid ${color}40`, background: `${color}10`,
                                       color, fontFamily: "inherit", fontSize: 11, fontWeight: 700 }}>
                              ↓ Download
                            </button>
                          )}
                        </div>

                        {/* Action row */}
                        <div style={{ marginTop: 12, display: "flex", gap: 8 }}>
                          {canDelete && !pastDeadline ? (
                            <button
                              onClick={() => setConfirmDel(true)}
                              style={{ padding: "7px 14px", borderRadius: 9, cursor: "pointer",
                                       border: "1px solid rgba(239,68,68,0.35)",
                                       background: "rgba(239,68,68,0.07)",
                                       color: "#ef4444", fontSize: 12.5, fontWeight: 700,
                                       fontFamily: "inherit" }}>
                              🗑 Delete Submission
                            </button>
                          ) : pastDeadline ? (
                            <span style={{ fontSize: 12, color: "var(--text-muted,#94a3b8)",
                                           fontWeight: 600 }}>
                              🔒 Deadline passed — read only
                            </span>
                          ) : atMax ? (
                            <span style={{ fontSize: 12, color: "#ef4444", fontWeight: 600 }}>
                              🚫 Max resubmissions reached ({MAX_ATTEMPTS}/{MAX_ATTEMPTS})
                            </span>
                          ) : null}
                        </div>
                      </motion.div>
                    ) : (
                      /* ── Confirmation ── */
                      <motion.div key="confirm"
                        initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }} transition={{ duration: 0.18 }}
                        style={{ padding: "14px 16px", borderRadius: 12, marginBottom: 14,
                                 border: "1.5px solid rgba(239,68,68,0.3)",
                                 background: "rgba(239,68,68,0.06)" }}>
                        <p style={{ margin: "0 0 10px", fontSize: 13, fontWeight: 700,
                                    color: "#b91c1c" }}>
                          🗑 Permanently delete this submission?
                        </p>
                        <p style={{ margin: "0 0 14px", fontSize: 12.5,
                                    color: "var(--text-secondary,#475569)", lineHeight: 1.5 }}>
                          This action cannot be undone. After deletion you can upload a
                          new file ({MAX_ATTEMPTS - (asn.attemptCount ?? 0)} attempt
                          {MAX_ATTEMPTS - (asn.attemptCount ?? 0) !== 1 ? "s" : ""} remaining).
                        </p>
                        {deleteErr && (
                          <div style={{ marginBottom: 10, fontSize: 12.5, color: "#ef4444",
                                        padding: "5px 8px", borderRadius: 7,
                                        background: "rgba(239,68,68,0.08)",
                                        border: "1px solid rgba(239,68,68,0.2)" }}>
                            ⚠️ {deleteErr}
                          </div>
                        )}
                        <div style={{ display: "flex", gap: 8 }}>
                          <button
                            onClick={() => { setConfirmDel(false); setDeleteErr(null); }}
                            disabled={deleting}
                            style={{ flex: 1, padding: "8px", borderRadius: 9, cursor: "pointer",
                                     border: "1.5px solid var(--course-border)",
                                     background: "var(--card-bg,#fff)",
                                     fontSize: 13, fontWeight: 700, fontFamily: "inherit",
                                     color: "var(--text-secondary,#475569)" }}>
                            Cancel
                          </button>
                          <button
                            onClick={handleDelete}
                            disabled={deleting}
                            style={{ flex: 1, padding: "8px", borderRadius: 9,
                                     cursor: deleting ? "not-allowed" : "pointer",
                                     border: "none", background: "#ef4444",
                                     color: "#fff", fontSize: 13, fontWeight: 700,
                                     fontFamily: "inherit", opacity: deleting ? 0.7 : 1 }}>
                            {deleting ? "Deleting…" : "Yes, Delete"}
                          </button>
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>
                )}

                {/* ── Upload area ── */}
                {(showUpload || (!hasSubmission && asn.canSubmit)) ? (
                  <>
                    {atMax && (
                      <div style={{ padding: "12px 14px", marginBottom: 12, borderRadius: 10,
                                    border: "1.5px solid rgba(239,68,68,0.25)",
                                    background: "rgba(239,68,68,0.06)", fontSize: 13,
                                    color: "#b91c1c", fontWeight: 600 }}>
                        🚫 Maximum resubmissions reached. No further uploads allowed.
                      </div>
                    )}
                    {!atMax && (
                      <FileUpload
                        id={parseInt(asn.id)}
                        types={asn.types || ["pdf","zip","docx"]}
                        color={color}
                        onDone={() => {
                          setList(l => l.map(x => x.id === asn.id
                            ? { ...x, status: "submitted",
                                      submissionFileName: "Your file",
                                      attemptCount: (x.attemptCount ?? 0) + 1 }
                            : x));
                          closeModal();
                        }}
                      />
                    )}
                  </>
                ) : !hasSubmission ? (
                  /* Non-submittable state messages */
                  <div style={{ textAlign: "center", padding: "20px 0 4px",
                                color: "var(--text-muted,#94a3b8)", fontSize: 13, fontWeight: 600 }}>
                    {asn.status === "upcoming"
                      ? `⏳ Submissions open on ${asn.releaseDate || "the release date"}`
                      : asn.status === "locked"
                        ? "🔒 Submission deadline has passed."
                        : asn.status === "rejected"
                          ? "🚫 Resubmission not allowed."
                          : "Submissions are currently closed."}
                  </div>
                ) : null}
              </div>
            </motion.div>
          </motion.div>
          );
        })()}
      </AnimatePresence>

      <div className={styles.asnGrid}>
        {list.map((a, i) => {
          const pct = a.grade !== null ? Math.round((a.grade/a.max)*100) : null;
          return (
            <motion.div key={a.id} className={styles.asnCard2}
              style={{ borderColor: (a.status==="pending"||a.status==="available") ? `${color}40` : "var(--course-border)", cursor: "pointer" }}
              onClick={() => setUploadAsn(a)}
              initial={{ opacity:0, y:18, scale:0.94 }} animate={{ opacity:1, y:0, scale:1 }}
              transition={{ delay:i*0.07, duration:0.38, ease:[0.22,1,0.36,1] }}
              whileHover={{ y:-4, boxShadow:"0 14px 36px rgba(0,0,0,0.11)" }}>

              {(a.status==="pending"||a.status==="available") && (
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

              {a.status === "rejected" && a.rejectionReason && (
                <div style={{ fontSize:12, color:"#ef4444", margin:"4px 0 6px",
                              padding:"5px 8px", borderRadius:6,
                              background:"rgba(239,68,68,0.07)", border:"1px solid rgba(239,68,68,0.18)" }}>
                  ❌ Reason: {a.rejectionReason}
                </div>
              )}

              <div className={styles.asnCard2Footer}>
                {pct !== null ? (
                  <div className={styles.asnGradeDial}
                    style={{ background:`conic-gradient(${color} ${pct*3.6}deg, #eef2f7 0deg)` }}>
                    <div className={styles.asnGradeDialIn}>
                      <span style={{ color, fontSize:15, fontWeight:900 }}>{a.grade}</span>
                      <span style={{ color:"#94a3b8", fontSize:10 }}>/{a.max}</span>
                    </div>
                  </div>
                ) : a.canSubmit ? (
                  <motion.button className={styles.asnSubmitBtn}
                    style={{ background:color }}
                    onClick={e => { e.stopPropagation(); setUploadAsn(a); }}
                    whileHover={{ scale:1.04, filter:"brightness(1.08)" }}
                    whileTap={{ scale:0.96 }}>
                    <span style={{ width:13,height:13,display:"flex" }}>{IC.upload}</span>
                    {a.status === "submitted" ? "Manage" : "Submit"}
                  </motion.button>
                ) : (
                  <span className={styles.asnCard2Upcoming}>
                    {a.status === "locked"    ? "🔒 Deadline passed" :
                     a.status === "submitted" ? "⏳ Awaiting review" :
                     a.status === "rejected"  ? "🚫 Cannot resubmit" :
                     a.status === "upcoming"  ? `⏳ Opens ${a.releaseDate || "soon"}` :
                     "Tap to view"}
                  </span>
                )}
              </div>
            </motion.div>
          );
        })}
      </div>
    </div>
  );
}

function QuizzesTab({ quizzes, color, courseId, courseName }) {
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
                  style={{ background: q.status === "available" ? `${color}18` : "var(--course-soft)",
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
                onClick={() => navigate(`/student/quiz/${courseId}/${q.id}`, { state: { color, courseName } })}
                whileHover={{ scale: 1.04, filter: "brightness(1.08)" }}
                whileTap={{ scale: 0.96 }}>
                Start Quiz →
              </motion.button>
            )}
          </motion.div>
        ))}
      </div>
    </div>
  );
}

function GradesTab({ midterm }) {
  if (!midterm) return (
    <div className={styles.tabBody}>
      <div className={styles.gradesNotPublished}>
        <span>📊</span>
        <h3>No Midterm Data</h3>
        <p>Midterm grade has not been entered yet</p>
      </div>
    </div>
  );

  const midPct = midterm.published && midterm.grade != null
    ? Math.round(midterm.grade / midterm.max * 100)
    : null;
  const midC = midPct == null ? "#94a3b8" : midPct >= 80 ? "#22c55e" : midPct >= 60 ? "#f59e0b" : "#ef4444";

  if (!midterm.published) return (
    <div className={styles.tabBody}>
      <div className={styles.gradesNotPublished}>
        <span>🔒</span>
        <h3>Grade Not Published Yet</h3>
        <p>Check back after the exam results are released</p>
      </div>
    </div>
  );

  return (
    <div className={styles.tabBody}>
      <div className={styles.midtermFull}>
        <div className={styles.midHero}>
          <div className={styles.midHeroCircleWrap}>
            <svg viewBox="0 0 120 120" className={styles.midHeroSvg}>
              <circle cx="60" cy="60" r="52" fill="none"
                stroke="var(--prog-track)" strokeWidth="8"/>
              <motion.circle cx="60" cy="60" r="52" fill="none"
                stroke={midC} strokeWidth="8"
                strokeLinecap="round"
                strokeDasharray={`${327}`}
                initial={{ strokeDashoffset: 327 }}
                animate={{ strokeDashoffset: 327 - 327 * ((midPct ?? 0)/100) }}
                transition={{ duration: 1.1, ease: "easeOut", delay: 0.15 }}
                style={{ transform: "rotate(-90deg)", transformOrigin: "60px 60px" }}/>
            </svg>
            <div className={styles.midHeroCircleInner}>
              <span className={styles.midHeroGrade} style={{ color: midC }}>
                {midterm.grade}
              </span>
              <span className={styles.midHeroMax}>/{midterm.max}</span>
            </div>
          </div>

          <div className={styles.midHeroInfo}>
            <div className={styles.midHeroPct} style={{ color: midC }}>{midPct}%</div>
            <div className={styles.midHeroLabel}>Midterm Score</div>
            <div className={styles.midHeroBar}>
              <div className={styles.midHeroBarTrack}>
                <motion.div className={styles.midHeroBarFill}
                  style={{ background: midC }}
                  initial={{ width: 0 }}
                  animate={{ width: `${midPct ?? 0}%` }}
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

        <div className={styles.midDetails}>
          {[
            midterm.date && { ico: "📅", label: "Date",  val: midterm.date },
            midterm.time && { ico: "⏰", label: "Time",  val: midterm.time },
            midterm.room && { ico: "🏛",  label: "Room",  val: midterm.room },
            { ico: "⭐", label: "Score", val: `${midterm.grade} out of ${midterm.max} points` },
          ].filter(Boolean).map((d,i) => (
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
    </div>
  );
}

const TABS = [
  { key: "lectures",    label: "Lectures",    icon: "🎬" },
  { key: "assignments", label: "Assignments", icon: "📋" },
  { key: "quizzes",     label: "Quizzes",     icon: "✏️" },
  { key: "grades",      label: "Grades",      icon: "📊" },
];

export default function CourseDetailPage() {
  const { courseId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const navState = location.state || {};
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

  const derivedColor = useMemo(() => resolveCourseColor(data?.meta, navState), [data, navState]);
  const courseTheme = useMemo(() => buildCourseTheme(derivedColor), [derivedColor]);

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
  const counts = { lectures: lectures.length, assignments: assignments.length, quizzes: quizzes.length, grades: data.midterm ? 1 : 0 };
  const displayName = navState.courseDisplayName || formatCourseName(meta.name);
  const c = derivedColor;

  return (
    <div className={styles.page} style={courseTheme}>
      <div className={styles.heroWrapper}>
        <motion.div className={styles.heroCard}
          initial={{ opacity: 0, y: 18, scale: 0.97 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          transition={{ duration: 0.48, ease: [0.22, 1, 0.36, 1] }}>

          <div className={styles.heroCardBg} style={{ background: c }}/>
          <div className={styles.heroCardMesh}
            style={{ background: `radial-gradient(ellipse 60% 80% at 90% 10%, ${hexToRgba(c, 0.55)} 0%, transparent 55%),
                                   radial-gradient(ellipse 40% 50% at 5% 90%, rgba(0,0,0,0.22) 0%, transparent 50%)` }}/>
          <div className={styles.heroPattern}/>

          <div className={styles.heroInner}>
            <div className={styles.heroTopRow}>
              <motion.button className={styles.backBtn}
                onClick={() => navigate("/student/courses")}
                whileHover={{ x: -3 }} whileTap={{ scale: 0.92 }}>
                <span style={{ width: 15, height: 15, display: "flex" }}>{IC.back}</span>
                Courses
              </motion.button>
              <Ring pct={meta.progress} size={94}/>
            </div>

            <motion.div className={styles.heroInfo}
              initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.14, duration: 0.44 }}>

              <div className={styles.codeTag}>{meta.code}</div>
              <TitleReveal text={displayName}/>
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

      <div className={styles.body}>
        <AnimatePresence mode="wait">
          <motion.div key={tab}
            initial={{ opacity: 0, y: 14 }} animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }} transition={{ duration: 0.2 }}>
            {tab === "lectures"    && <LecturesTab    lectures={lectures}              color={c} />}
            {tab === "assignments" && <AssignmentsTab initialAssignments={assignments} color={c} meta={{ ...meta, name: displayName }} />}
            {tab === "quizzes"     && <QuizzesTab     quizzes={quizzes}                color={c} courseId={courseId} courseName={displayName}/>}
            {tab === "grades"      && <GradesTab      midterm={data.midterm} />}
          </motion.div>
        </AnimatePresence>
      </div>
    </div>
  );
}
