// src/pages/instructor/GradesMgmtPage.jsx
import { useState, useEffect, useCallback } from "react";
import { useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./GradesMgmtPage.module.css";
import {
  getInstructorCourses,
  getInstructorAssignments,
  getSubmissions,
  acceptSubmission,
  rejectSubmission,
  getMidtermGrades,
  setMidtermGrade,
  publishMidterm,
  getFinalGradeStatus,
  getFinalGradeStudents,
  setFinalGrade,
} from "../../services/api/instructorApi";

const REJECT_REASONS = [
  { icon: "🤖", label: "AI-generated",     value: "Suspected AI-generated content" },
  { icon: "📋", label: "Wrong format",     value: "Incorrect submission format" },
  { icon: "📄", label: "Plagiarized",      value: "Plagiarism detected" },
  { icon: "📦", label: "Incomplete",       value: "Submission is incomplete" },
  { icon: "🔗", label: "Wrong assignment", value: "Wrong assignment submitted" },
  { icon: "⚠️", label: "Corrupted file",  value: "File is corrupted or unreadable" },
];

const sp = { type: "spring", stiffness: 400, damping: 28 };

/* ── Avatar ── */
function Av({ name, color, size = 44 }) {
  const ini = (name || "?").split(" ").slice(0, 2).map(w => w[0]).join("");
  return (
    <div style={{
      width: size, height: size, borderRadius: size > 40 ? 14 : 10, flexShrink: 0,
      display: "flex", alignItems: "center", justifyContent: "center",
      background: `${color}22`, color, border: `1.5px solid ${color}40`,
      fontSize: size > 48 ? "1rem" : ".78rem", fontWeight: 800, letterSpacing: "-.02em",
    }}>{ini}</div>
  );
}

/* ════════ REJECT MODAL ════════ */
function RejectModal({ sub, onConfirm, onClose, loading }) {
  const [preset, setPreset] = useState("");
  const [custom, setCustom] = useState("");
  const reason = custom.trim() || preset;

  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div className={styles.modal} onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, scale: .88, y: 28 }} animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: .95 }} transition={sp}>
        <div className={styles.modalBand} style={{ background: "linear-gradient(135deg,#b91c1c,#ef4444)" }} />
        <div className={styles.modalBody}>
          <div className={styles.modalTitle}>Reject Submission</div>
          <div className={styles.modalSub}>{sub.studentName} · {sub.assignmentTitle}</div>
          <div className={styles.reasonGrid}>
            {REJECT_REASONS.map(r => (
              <button key={r.value}
                className={`${styles.reasonBtn} ${preset === r.value && !custom.trim() ? styles.reasonOn : ""}`}
                onClick={() => { setPreset(r.value); setCustom(""); }}>
                <span>{r.icon}</span>{r.label}
              </button>
            ))}
          </div>
          <div className={styles.customWrap}>
            <label className={styles.customLabel}>Or write a custom reason:</label>
            <textarea className={styles.customInput} rows={3}
              placeholder="Describe the specific issue…"
              value={custom}
              onChange={e => { setCustom(e.target.value); if (e.target.value.trim()) setPreset(""); }} />
          </div>
          <div className={styles.mActions}>
            <button className={styles.mCancel} onClick={onClose} disabled={loading}>Cancel</button>
            <motion.button className={styles.mReject}
              disabled={!reason || loading} style={{ opacity: reason ? 1 : .4 }}
              onClick={() => reason && onConfirm(reason)}
              whileHover={reason ? { scale: 1.02 } : {}} whileTap={reason ? { scale: .97 } : {}}>
              {loading ? "Rejecting…" : "Reject Submission"}
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ════════ APPROVE MODAL ════════ */
function ApproveModal({ sub, onConfirm, onClose, loading }) {
  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div className={styles.modal} onClick={e => e.stopPropagation()}
        initial={{ opacity: 0, scale: .88, y: 28 }} animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: .95 }} transition={sp}>
        <div className={styles.modalBand} style={{ background: "linear-gradient(135deg,#15803d,#22c55e)" }} />
        <div className={styles.modalBody}>
          <div className={styles.modalTitle}>✓ Accept Submission</div>
          <div className={styles.modalSub}>{sub.studentName} · {sub.assignmentTitle}</div>
          <div style={{ margin: "18px 0 6px", fontSize: ".84rem", color: "var(--text-secondary)" }}>
            Student receives <strong style={{ color: "#22c55e" }}>{sub.maxGrade} / {sub.maxGrade} pts</strong> (full mark).
          </div>
          <div className={styles.mActions} style={{ marginTop: 20 }}>
            <button className={styles.mCancel} onClick={onClose} disabled={loading}>Cancel</button>
            <motion.button className={styles.mApprove}
              onClick={onConfirm} disabled={loading}
              whileHover={{ scale: 1.02 }} whileTap={{ scale: .97 }}>
              {loading ? "Accepting…" : "✓ Accept Submission"}
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ════════ ASSIGNMENTS SECTION ════════ */
function AssignmentsSection({ courseId, color }) {
  const [assignments, setAssignments] = useState([]);
  const [selected,    setSelected]    = useState(null); // selected assignment id
  const [subs,        setSubs]        = useState([]);
  const [filter,      setFilter]      = useState("all");
  const [rejectT,     setRejectT]     = useState(null);
  const [approveT,    setApproveT]    = useState(null);
  const [toast,       setToast]       = useState(null);
  const [loading,     setLoading]     = useState(true);
  const [subLoading,  setSubLoading]  = useState(false);
  const [actionLoading, setActionLoading] = useState(false);

  const toast$ = (msg, t = "ok") => { setToast({ msg, t }); setTimeout(() => setToast(null), 2400); };

  // Load assignments
  useEffect(() => {
    setLoading(true);
    getInstructorAssignments(courseId)
      .then(data => { setAssignments(Array.isArray(data) ? data : []); setSelected(null); setSubs([]); })
      .catch(() => setAssignments([]))
      .finally(() => setLoading(false));
  }, [courseId]);

  // Load submissions when assignment selected
  useEffect(() => {
    if (!selected) { setSubs([]); return; }
    setSubLoading(true);
    getSubmissions(selected)
      .then(data => setSubs(Array.isArray(data) ? data : []))
      .catch(() => setSubs([]))
      .finally(() => setSubLoading(false));
  }, [selected]);

  const counts = {
    all: subs.length,
    pending:  subs.filter(s => s.status === "pending").length,
    accepted: subs.filter(s => s.status === "accepted").length,
    rejected: subs.filter(s => s.status === "rejected").length,
  };
  const show = subs.filter(s => filter === "all" ? true : s.status === filter);

  const doAccept = async (sub) => {
    setActionLoading(true);
    try {
      await acceptSubmission(sub.assignmentId, sub.id);
      setSubs(p => p.map(s => s.id === sub.id ? { ...s, status: "accepted", grade: sub.maxGrade } : s));
      setApproveT(null);
      toast$(`✓ Accepted · ${sub.maxGrade}/${sub.maxGrade} pts`);
    } catch (e) {
      toast$(e?.response?.data?.error?.message || "Accept failed", "err");
    } finally {
      setActionLoading(false);
    }
  };

  const doReject = async (sub, reason) => {
    setActionLoading(true);
    try {
      await rejectSubmission(sub.assignmentId, sub.id, reason);
      setSubs(p => p.map(s => s.id === sub.id ? { ...s, status: "rejected", grade: 0, rejectionReason: reason } : s));
      setRejectT(null);
      toast$("✗ Submission rejected", "err");
    } catch (e) {
      toast$(e?.response?.data?.error?.message || "Reject failed", "err");
    } finally {
      setActionLoading(false);
    }
  };

  const selAssignment = assignments.find(a => a.id === selected);

  if (loading) return <div className={styles.section}><div className={styles.empty}><span>⏳</span><p>Loading assignments…</p></div></div>;

  return (
    <div className={styles.section}>
      <AnimatePresence>
        {toast && (
          <motion.div className={`${styles.toast} ${toast.t === "err" ? styles.toastErr : ""}`}
            initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            transition={sp}>{toast.msg}</motion.div>
        )}
      </AnimatePresence>

      {/* Assignment selector */}
      {assignments.length === 0 ? (
        <div className={styles.empty}><span>📭</span><p>No assignments for this course yet.</p></div>
      ) : (
        <div className={styles.assignmentChooser}>
          <div className={styles.sectionKicker}>Select assignment</div>
          <div className={styles.assignmentChipRow}>
            {assignments.map(a => (
              <button key={a.id}
                className={`${styles.assignmentChip} ${selected === a.id ? styles.assignmentChipOn : ""}`}
                onClick={() => { setSelected(a.id); setFilter("all"); }}
                style={selected === a.id ? { borderColor: color, background: `${color}16`, color } : {}}>
                <span className={styles.assignmentChipText}>{a.title}</span>
                {a.pendingCount > 0 && (
                  <span className={styles.assignmentChipBadge} style={{ background: color }}>
                    {a.pendingCount}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {!selected && assignments.length > 0 && (
        <div className={styles.empty}><span>👆</span><p>Select an assignment above to review submissions</p></div>
      )}

      {selected && (
        <>
          {/* Filter bar */}
          <div className={styles.filterBar}>
            <div className={styles.filters}>
              {[["all", "All"], ["pending", "Pending"], ["accepted", "Accepted"], ["rejected", "Rejected"]].map(([k, l]) => (
                <button key={k}
                  className={`${styles.pill} ${filter === k ? styles.pillOn : ""}`}
                  style={filter === k ? { color, borderColor: `${color}60`, background: `${color}12` } : {}}
                  onClick={() => setFilter(k)}>
                  {l} <span className={styles.pillCount}>{counts[k] ?? 0}</span>
                </button>
              ))}
            </div>
          </div>

          {/* Submissions list */}
          {subLoading ? (
            <div className={styles.empty}><span>⏳</span><p>Loading submissions…</p></div>
          ) : (
            <div className={styles.subList}>
              <AnimatePresence initial={false}>
                {show.map((s, i) => (
                  <motion.div key={s.id}
                    className={styles.subCard}
                    initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }}
                    exit={{ opacity: 0, x: -20 }} transition={{ delay: i * .04, ...sp }}>

                    <Av name={s.studentName} color={color} />

                    <div className={styles.subInfo}>
                      <div className={styles.subHead}>
                        <span className={styles.subName}>{s.studentName}</span>
                        <span className={styles.subStudId}>{s.studentId}</span>
                      </div>
                      <div className={styles.subTitle}>{s.assignmentTitle}</div>
                      <div className={styles.subMeta}>
                        <span>🕐 {s.submittedAt}</span>
                        {s.fileUrl ? (
                          <a href={s.fileUrl} target="_blank" rel="noopener noreferrer"
                            className={styles.fileLink} style={{ color }}>
                            📎 {s.fileName} ↗
                          </a>
                        ) : (
                          <span>📎 {s.fileName}</span>
                        )}
                      </div>
                      {s.status === "rejected" && s.rejectionReason && (
                        <span className={styles.rejectTag}>⛔ {s.rejectionReason}</span>
                      )}
                    </div>

                    <div className={styles.subActions}>
                      {s.status === "pending" && (
                        <>
                          <span className={styles.sPending}>⏳ Pending</span>
                          <div className={styles.subBtns}>
                            <motion.button className={styles.btnApp}
                              onClick={() => setApproveT(s)}
                              whileHover={{ scale: 1.04 }} whileTap={{ scale: .95 }}>✓ Accept</motion.button>
                            <motion.button className={styles.btnRej}
                              onClick={() => setRejectT(s)}
                              whileHover={{ scale: 1.04 }} whileTap={{ scale: .95 }}>✗ Reject</motion.button>
                          </div>
                        </>
                      )}
                      {s.status === "accepted" && (
                        <>
                          <div className={styles.scoreDisplay}>
                            <span className={styles.scoreNum} style={{ color }}>{s.grade}</span>
                            <span className={styles.scoreOf}>/{s.maxGrade}</span>
                          </div>
                          <span className={styles.sApproved}>✅ Accepted</span>
                        </>
                      )}
                      {s.status === "rejected" && (
                        <span className={styles.sRejected}>✗ Rejected</span>
                      )}
                    </div>
                  </motion.div>
                ))}
              </AnimatePresence>
              {show.length === 0 && (
                <div className={styles.empty}><span>📭</span><p>No submissions here</p></div>
              )}
            </div>
          )}
        </>
      )}

      <AnimatePresence>
        {approveT && (
          <ApproveModal sub={approveT} loading={actionLoading}
            onConfirm={() => doAccept(approveT)}
            onClose={() => setApproveT(null)} />
        )}
        {rejectT && (
          <RejectModal sub={rejectT} loading={actionLoading}
            onConfirm={(r) => doReject(rejectT, r)}
            onClose={() => setRejectT(null)} />
        )}
      </AnimatePresence>
    </div>
  );
}

/* ════════ MIDTERM SECTION ════════ */
function MidtermSection({ courseId, color }) {
  const [students,  setStudents]  = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [query,     setQuery]     = useState("");
  const [draft,     setDraft]     = useState("");
  const [editing,   setEditing]   = useState(false); // edit mode for a saved grade
  const [maxPts,    setMaxPts]    = useState("20");
  const [maxSet,    setMaxSet]    = useState(false);
  const [toast,     setToast]     = useState(null);
  const [saving,    setSaving]    = useState(false);
  const [publishing, setPublishing] = useState(false);

  const toast$ = (msg, t = "ok") => { setToast({ msg, t }); setTimeout(() => setToast(null), 2400); };

  const loadGrades = useCallback(() => {
    setLoading(true);
    getMidtermGrades(courseId)
      .then(data => {
        setStudents(Array.isArray(data) ? data : []);
        // If any student already has a grade, detect existing max
        const withGrade = (data || []).find(s => s.submitted);
        if (withGrade) { setMaxPts(String(withGrade.maxGrade)); setMaxSet(true); }
      })
      .catch(() => setStudents([]))
      .finally(() => setLoading(false));
  }, [courseId]);

  useEffect(() => { loadGrades(); }, [loadGrades]);

  const max   = Number(maxPts) || 20;
  // Match by academic code (primary, case-insensitive) — fall back to GUID so
  // legacy/deep-link GUIDs still resolve. Trimmed lowercase comparison so
  // typing "s2024001" matches stored "S2024001".
  const q = query.trim();
  const qLow = q.toLowerCase();
  const found = q
    ? students.find(s =>
        (s.studentCode && String(s.studentCode).trim().toLowerCase() === qLow) ||
        (s.studentId   && String(s.studentId).toLowerCase()           === qLow))
    : null;

  // If deep-link or hint passed a GUID, promote it to the public code so the UI never exposes GUID
  useEffect(() => {
    if (!q || !found) return;
    if (found.studentCode && found.studentCode !== q) setQuery(found.studentCode);
  }, [q, found]);

  // Reset dirty state whenever the selected student changes (includes leaving)
  useEffect(() => {
    setDraft("");
    setEditing(false);
  }, [found?.studentId]);

  const startEdit = () => {
    if (!found) return;
    setDraft(found.submitted ? String(found.grade) : "");
    setEditing(true);
  };

  const cancelEdit = () => {
    setDraft("");
    setEditing(false);
  };

  const save = async () => {
    if (!found || draft === "") return;
    const v = Number(draft);
    if (isNaN(v) || v < 0 || v > max) return;
    setSaving(true);
    try {
      await setMidtermGrade(courseId, found.studentId, { grade: v, max, published: false });
      setStudents(p => p.map(s => s.studentId === found.studentId
        ? { ...s, grade: v, maxGrade: max, submitted: true }
        : s));
      setDraft("");
      setEditing(false);
      toast$(`✓ Grade saved for ${found.studentName}`);
    } catch (e) {
      toast$(e?.response?.data?.error?.message || "Save failed", "err");
    } finally {
      setSaving(false);
    }
  };

  const publishAll = async (pub) => {
    setPublishing(true);
    try {
      await publishMidterm(courseId, pub);
      toast$(pub ? "✅ Grades published to students" : "🔒 Grades hidden from students");
      loadGrades();
    } catch {
      toast$("Publish failed", "err");
    } finally {
      setPublishing(false);
    }
  };

  if (loading) return <div className={styles.section}><div className={styles.empty}><span>⏳</span><p>Loading grades…</p></div></div>;

  return (
    <div className={styles.section}>
      <AnimatePresence>
        {toast && (
          <motion.div className={`${styles.toast} ${toast.t === "err" ? styles.toastErr : ""}`}
            initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            transition={sp}>{toast.msg}</motion.div>
        )}
      </AnimatePresence>

      {/* Max pts config */}
      <div className={styles.examBanner} style={{ borderColor: `${color}30`, background: `${color}07` }}>
        <div className={styles.examBannerL}>
          <div className={styles.examDate}>📝 Midterm — out of 40 max</div>
        </div>
        {!maxSet ? (
          <div className={styles.setMax}>
            <span>Total points:</span>
            <input type="number" min={1} max={40} value={maxPts}
              onChange={e => setMaxPts(e.target.value)}
              className={styles.maxIn} />
            <span>/ 40</span>
            <button className={styles.maxBtn} style={{ background: color }}
              onClick={() => setMaxSet(true)}>Confirm</button>
          </div>
        ) : (
          <div className={styles.setMax}>
            <span className={styles.examDate}>📊 Exam out of <strong>{maxPts} pts</strong></span>
            <button className={styles.maxEdit} onClick={() => setMaxSet(false)}>Edit</button>
            <motion.button className={styles.maxBtn} style={{ background: "#22c55e", marginLeft: 8 }}
              disabled={publishing}
              onClick={() => publishAll(true)}
              whileHover={{ scale: 1.03 }} whileTap={{ scale: .97 }}>
              {publishing ? "…" : "📢 Publish All"}
            </motion.button>
          </div>
        )}
      </div>

      {/* Search */}
      <div className={styles.searchWrap}>
        <div className={styles.searchBox}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" className={styles.searchIco}>
            <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
          </svg>
          <input className={styles.searchIn}
            placeholder="Type student code…"
            value={query} onChange={e => { setQuery(e.target.value); setDraft(""); setEditing(false); }} />
          {query && <button className={styles.searchX} onClick={() => { setQuery(""); setDraft(""); setEditing(false); }}>✕</button>}
        </div>
      </div>

      <AnimatePresence mode="wait">
        {!query.trim() && (
          <motion.div key="idle" className={styles.examIdle}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
            <div className={styles.examIdleIcon} style={{ background: `${color}14`, color }}>📝</div>
            <p className={styles.examIdleT}>Enter a student code to record their grade</p>
            <div className={styles.examHints}>
              {students.map(s => (
                <button key={s.studentId} className={styles.hint} onClick={() => setQuery(s.studentCode || "")}>
                  <Av name={s.studentName} color={color} size={28} />
                  <span>{s.studentName}</span>
                  <span className={styles.hintId}>{s.studentCode}</span>
                  {s.submitted && (
                    <span className={styles.hintGrade} style={{ color }}>{s.grade}/{s.maxGrade}</span>
                  )}
                </button>
              ))}
            </div>
          </motion.div>
        )}

        {query.trim() && !found && (
          <motion.div key="nf" className={styles.notFound}
            initial={{ opacity: 0, scale: .96 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }}>
            <span>🔍</span>
            <p>No student found with code <strong>{query.trim()}</strong></p>
          </motion.div>
        )}

        {found && (
          <motion.div key={found.studentId} className={styles.stuCard}
            style={{ borderColor: `${color}35` }}
            initial={{ opacity: 0, y: 20, scale: .97 }} animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -10 }} transition={sp}>

            <div className={styles.stuHead} style={{ background: `${color}0d`, borderBottom: `1px solid ${color}25` }}>
              <Av name={found.studentName} color={color} size={58} />
              <div className={styles.stuInfo}>
                <div className={styles.stuName}>{found.studentName}</div>
                <div className={styles.stuChips}>
                  <span className={styles.chip}>🆔 {found.studentCode || "—"}</span>
                </div>
              </div>

              {found.submitted && !editing && (() => {
                const p = Math.round(found.grade / max * 100);
                const gc = p >= 80 ? "#22c55e" : p >= 60 ? "#f59e0b" : "#ef4444";
                const dashArr = 2 * Math.PI * 40;
                return (
                  <div className={styles.existCircle}>
                    <svg viewBox="0 0 100 100" className={styles.circSvg}>
                      <circle cx="50" cy="50" r="40" fill="none" stroke="var(--prog-track)" strokeWidth="8" />
                      <motion.circle cx="50" cy="50" r="40" fill="none"
                        stroke={gc} strokeWidth="8" strokeLinecap="round"
                        strokeDasharray={dashArr}
                        initial={{ strokeDashoffset: dashArr }}
                        animate={{ strokeDashoffset: dashArr - dashArr * (p / 100) }}
                        transition={{ duration: 1, ease: "easeOut", delay: .1 }}
                        style={{ transform: "rotate(-90deg)", transformOrigin: "50px 50px" }} />
                    </svg>
                    <div className={styles.circInner}>
                      <span className={styles.circGrade} style={{ color: gc }}>{found.grade}</span>
                      <span className={styles.circMax}>/{max}</span>
                    </div>
                    <div className={styles.circPct} style={{ color: gc }}>{p}%</div>
                  </div>
                );
              })()}
            </div>

            <div className={styles.stuBody}>
              <div className={styles.entryLabel}>
                Midterm Grade
                <span className={styles.entryOf}>/ {max} pts</span>
              </div>

              {/* Read-only view — saved grade with Edit button */}
              {found.submitted && !editing && (
                <div className={styles.entryRow}>
                  <div className={styles.readOnlyGrade} style={{ borderColor: `${color}35`, color }}>
                    <strong>{found.grade}</strong>
                    <span className={styles.readOnlyOf}>/ {max}</span>
                  </div>
                  <motion.button className={styles.saveBtn}
                    style={{ background: color }}
                    onClick={startEdit}
                    whileHover={{ scale: 1.03, filter: "brightness(1.08)" }}
                    whileTap={{ scale: .97 }}>
                    ✎ Edit
                  </motion.button>
                </div>
              )}

              {/* Edit / first-time input — draft state */}
              {(!found.submitted || editing) && (
                <>
                  <div className={styles.entryRow}>
                    <input type="number" min={0} max={max}
                      className={styles.gradeIn}
                      style={draft ? { borderColor: `${color}70`, boxShadow: `0 0 0 4px ${color}18` } : {}}
                      placeholder={found.submitted ? String(found.grade) : "0"}
                      value={draft}
                      onChange={e => setDraft(e.target.value)}
                      onKeyDown={e => e.key === "Enter" && save()} />
                    <span className={styles.entryOf} style={{ fontSize: "1rem" }}>/ {max}</span>
                    <motion.button className={styles.saveBtn}
                      disabled={draft === "" || Number(draft) < 0 || Number(draft) > max || saving}
                      style={{ background: color, opacity: draft !== "" && Number(draft) >= 0 && Number(draft) <= max ? 1 : .4 }}
                      onClick={save}
                      whileHover={{ scale: 1.03, filter: "brightness(1.08)" }}
                      whileTap={{ scale: .97 }}>
                      {saving ? "Saving…" : found.submitted ? "✓ Save" : "+ Save Grade"}
                    </motion.button>
                    {editing && (
                      <button className={styles.cancelBtn} onClick={cancelEdit} disabled={saving}>
                        Cancel
                      </button>
                    )}
                  </div>
                  {draft !== "" && (Number(draft) < 0 || Number(draft) > max) && (
                    <p className={styles.err}>Grade must be 0 – {max}</p>
                  )}
                </>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

/* ════════ FINAL GRADE SECTION ════════ */
const LETTER_COLOR = { A: "#22c55e", B: "#818cf8", C: "#3b82f6", D: "#f59e0b", F: "#ef4444" };
const safeNum = (value, fallback = 0) => {
  const n = Number(value);
  return Number.isFinite(n) ? n : fallback;
};
const calcLetter = (total) => {
  const t = safeNum(total);
  return t >= 90 ? "A" : t >= 80 ? "B" : t >= 70 ? "C" : t >= 60 ? "D" : "F";
};

function FinalGradeSection({ courseId, color, initialQuery = null }) {
  const [status,    setStatus]    = useState(null);   // { locked, examDate, examTime, unlockAt }
  const [students,  setStudents]  = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [loadError, setLoadError] = useState(null);
  const [query,     setQuery]     = useState(initialQuery || "");
  const [draft,     setDraft]     = useState("");      // finalScore input
  const [bonus,     setBonus]     = useState("0");
  const [showBonus, setShowBonus] = useState(false);
  const [editing,   setEditing]   = useState(false);   // edit mode for a saved final grade
  const [bonusMsg,  setBonusMsg]  = useState("");
  const [toast,     setToast]     = useState(null);
  const [saving,    setSaving]    = useState(false);

  const toast$ = (msg, t = "ok") => { setToast({ msg, t }); setTimeout(() => setToast(null), 2400); };

  const load = useCallback(() => {
    setLoading(true);
    setLoadError(null);
    Promise.all([
      getFinalGradeStatus(courseId),
      getFinalGradeStudents(courseId),
    ])
      .then(([st, studs]) => {
        setStatus(st);
        setStudents(Array.isArray(studs) ? studs : []);
      })
      .catch(e => {
        setLoadError(e?.response?.data?.error?.message || e?.message || "Failed to load student data.");
      })
      .finally(() => setLoading(false));
  }, [courseId]);

  useEffect(() => { load(); }, [load]);

  // Match by:
  //   1. academic code — exact, case-insensitive, trimmed (primary)
  //   2. student GUID  — exact, case-insensitive (deep-link / legacy)
  //   3. student name  — contains, case-insensitive (fallback for students without a code)
  const q = query.trim();
  const qLow = q.toLowerCase();
  const found = q
    ? students.find(s =>
        (s.studentCode && String(s.studentCode).trim().toLowerCase() === qLow) ||
        (s.studentId   && String(s.studentId).toLowerCase()           === qLow) ||
        (s.studentName && String(s.studentName).toLowerCase().includes(qLow)))
    : null;

  // If deep-link or hint passed a GUID, promote it to the public code so the UI never exposes GUID
  useEffect(() => {
    if (!q || !found) return;
    if (found.studentCode && found.studentCode !== q) setQuery(found.studentCode);
  }, [q, found]);

  // Reset dirty state whenever the selected student changes (includes leaving)
  useEffect(() => {
    setDraft("");
    setBonus("0");
    setShowBonus(false);
    setEditing(false);
    setBonusMsg("");
  }, [found?.studentId]);

  // ── Bonus rule: remaining room up to 40 after core coursework ──
  // coreCw = midterm + quizzes + assignments (before bonus)
  const coreCw      = found ? (safeNum(found.midtermGrade, 0) + safeNum(found.quizScore, 0) + safeNum(found.assignmentScore, 0)) : 0;
  const bonusCap    = Math.max(0, Math.min(10, 40 - coreCw));
  const bonusLocked = bonusCap === 0;

  const startEdit = () => {
    if (!found) return;
    setDraft(found.submitted ? String(found.finalScore ?? "") : "");
    setBonus(String(found.bonus ?? 0));
    setShowBonus(false);
    setEditing(true);
    setBonusMsg("");
  };

  const cancelEdit = () => {
    setDraft("");
    setBonus(String(found?.bonus ?? 0));
    setShowBonus(false);
    setEditing(false);
    setBonusMsg("");
  };

  const onBonusChange = (raw) => {
    setBonusMsg("");
    if (bonusLocked) {
      setBonus("0");
      setBonusMsg("Coursework already reached 40 — bonus is locked.");
      return;
    }
    if (raw === "" || raw === "-") { setBonus(raw); return; }
    const n = Number(raw);
    if (isNaN(n) || n < 0) { setBonus("0"); return; }
    if (n > bonusCap) {
      setBonus(String(bonusCap));
      setBonusMsg(`Bonus can add at most ${bonusCap} pt${bonusCap === 1 ? "" : "s"} (coursework capped at 40).`);
      return;
    }
    setBonus(String(n));
  };

  const save = async () => {
    if (!found || draft === "") return;
    const fs = Number(draft);
    // Effective bonus is whatever is currently in the input if the instructor toggled it,
    // otherwise the already-stored bonus. Either way, clamp to the remaining room.
    const rawBonus = showBonus ? Number(bonus) || 0 : (found.bonus ?? 0);
    const bn = Math.max(0, Math.min(bonusCap, rawBonus));
    if (isNaN(fs) || fs < 0 || fs > 60) return;
    setSaving(true);
    try {
      const res = await setFinalGrade(courseId, found.studentId, { finalScore: fs, bonus: bn });
      const savedFinalScore = safeNum(res?.finalScore, fs);
      const savedBonus = safeNum(res?.bonus, bn);
      const savedCoursework = safeNum(res?.courseworkTotal, Math.min(40, coreCw + savedBonus));
      const savedTotal = safeNum(res?.total, Math.min(100, savedCoursework + savedFinalScore));
      const savedLetter = res?.letterGrade || calcLetter(savedTotal);

      setStudents(p => p.map(s => s.studentId === found.studentId
        ? { ...s,
            finalScore: savedFinalScore,
            bonus: savedBonus,
            courseworkTotal: savedCoursework,
            total: savedTotal,
            letterGrade: savedLetter,
            submitted: true }
        : s));
      setDraft("");
      setBonus(String(savedBonus));
      setShowBonus(false);
      setEditing(false);
      setBonusMsg("");
      toast$(`✓ Final grade saved · ${savedLetter} (${savedTotal}/100)`);
    } catch (e) {
      const msg = e?.response?.data?.error?.message || e?.message || "Save failed";
      toast$(msg, "err");
      // Reload student list so UI reflects latest server state after a conflict
      if (e?.response?.status === 409) load();
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className={styles.section}><div className={styles.empty}><span>⏳</span><p>Loading…</p></div></div>;

  if (loadError) return (
    <div className={styles.section}>
      <div className={styles.notFound} style={{ borderColor: "#ef444430", background: "#ef44440a" }}>
        <span>⚠</span>
        <p>{loadError}</p>
        <button className={styles.searchX} style={{ marginTop: 8, padding: "6px 14px" }} onClick={load}>Retry</button>
      </div>
    </div>
  );

  // ── Lock banner ──
  if (status?.locked) {
    const unlock = status.unlockAt ? new Date(status.unlockAt).toLocaleString() : "—";
    return (
      <div className={styles.section}>
        <div className={styles.examBanner} style={{ borderColor: "#ef444430", background: "#ef44440a" }}>
          <div className={styles.examBannerL}>
            <div className={styles.examDate}>🔒 Final Grading Locked</div>
            <div style={{ fontSize: ".8rem", color: "var(--text-muted)", marginTop: 4 }}>
              {status.examDate && <>Exam: <strong>{status.examDate}</strong> · {status.examTime}<br /></>}
              Grading opens 24 h after exam ends · <strong>{unlock}</strong>
            </div>
          </div>
        </div>
      </div>
    );
  }

  const isEditingOrNew = editing || !found?.submitted;
  const numericDraft = Number(draft);
  const draftFinalScore = Number.isFinite(numericDraft) ? numericDraft : 0;
  const savedFinalScore = safeNum(found?.finalScore, 0);
  const finalPts = isEditingOrNew ? draftFinalScore : savedFinalScore;

  // Coursework total preview: core (midterm+quiz+assignment) + effective bonus, capped at 40.
  // When the grade is already saved and not being edited, always display the latest saved
  // total/letter from the API instead of falling back to an empty draft, which made it look like F.
  const effectiveBonusPreview = bonusLocked ? 0 : (showBonus ? safeNum(bonus, 0) : safeNum(found?.bonus, 0));
  const savedCoursework = safeNum(found?.courseworkTotal, Math.min(40, coreCw + safeNum(found?.bonus, 0)));
  const cwPreview = isEditingOrNew ? Math.min(40, coreCw + effectiveBonusPreview) : savedCoursework;
  const savedTotal = safeNum(found?.total, Math.min(100, savedCoursework + savedFinalScore));
  const totalPreview = isEditingOrNew ? Math.min(100, cwPreview + finalPts) : savedTotal;
  const letterPreview = isEditingOrNew ? calcLetter(totalPreview) : (found?.letterGrade || calcLetter(totalPreview));
  const lcColor = LETTER_COLOR[letterPreview] || "#818cf8";
  const showCircleTotal = found?.submitted || draft !== "";
  const dashArr = 2 * Math.PI * 40;

  return (
    <div className={styles.section}>
      <AnimatePresence>
        {toast && (
          <motion.div className={`${styles.toast} ${toast.t === "err" ? styles.toastErr : ""}`}
            initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            transition={sp}>{toast.msg}</motion.div>
        )}
      </AnimatePresence>

      {/* Info banner */}
      <div className={styles.examBanner} style={{ borderColor: `${color}30`, background: `${color}07` }}>
        <div className={styles.examBannerL}>
          <div className={styles.examDate}>🎓 Final Grades — out of 100</div>
          <div style={{ fontSize: ".8rem", color: "var(--text-muted)", marginTop: 4 }}>
            Formula: <strong>Coursework (max 40)</strong> + <strong>Final Exam (max 60)</strong>
            {status?.examDate && <> · Exam: {status.examDate} {status.examTime}</>}
          </div>
        </div>
      </div>

      {/* Search */}
      <div className={styles.searchWrap}>
        <div className={styles.searchBox}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" className={styles.searchIco}>
            <circle cx="11" cy="11" r="8" /><path d="m21 21-4.35-4.35" />
          </svg>
          <input className={styles.searchIn}
            placeholder="Type student code or name…"
            value={query} onChange={e => { setQuery(e.target.value); setDraft(""); setBonus("0"); setShowBonus(false); setEditing(false); setBonusMsg(""); }} />
          {query && <button className={styles.searchX} onClick={() => { setQuery(""); setDraft(""); setBonus("0"); setShowBonus(false); setEditing(false); setBonusMsg(""); }}>✕</button>}
        </div>
      </div>

      <AnimatePresence mode="wait">
        {!query.trim() && (
          <motion.div key="idle" className={styles.examIdle}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
            <div className={styles.examIdleIcon} style={{ background: `${color}14`, color }}>🎓</div>
            <p className={styles.examIdleT}>Enter a student code or name to record their final grade</p>
            <div className={styles.examHints}>
              {students.map(s => (
                <button key={s.studentId} className={styles.hint}
                  onClick={() => setQuery(s.studentCode || s.studentName || "")}>
                  <Av name={s.studentName} color={color} size={28} />
                  <span>{s.studentName}</span>
                  <span className={styles.hintId}>{s.studentCode || "—"}</span>
                  {s.submitted && (
                    <span className={styles.hintGrade} style={{ color: LETTER_COLOR[s.letterGrade] || color }}>
                      {s.letterGrade} · {s.total}/100
                    </span>
                  )}
                </button>
              ))}
            </div>
          </motion.div>
        )}

        {query.trim() && !found && (
          <motion.div key="nf" className={styles.notFound}
            initial={{ opacity: 0, scale: .96 }} animate={{ opacity: 1, scale: 1 }} exit={{ opacity: 0 }}>
            <span>🔍</span>
            <p>No student found matching <strong>{query.trim()}</strong></p>
            <p style={{ fontSize: ".8rem", color: "var(--text-muted)", marginTop: 4 }}>
              Search by student code or name, or click a hint below
            </p>
          </motion.div>
        )}

        {found && (
          <motion.div key={found.studentId} className={styles.stuCard}
            style={{ borderColor: `${color}35` }}
            initial={{ opacity: 0, y: 20, scale: .97 }} animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: -10 }} transition={sp}>

            {/* Header */}
            <div className={styles.stuHead} style={{ background: `${color}0d`, borderBottom: `1px solid ${color}25` }}>
              <Av name={found.studentName} color={color} size={58} />
              <div className={styles.stuInfo}>
                <div className={styles.stuName}>{found.studentName}</div>
                <div className={styles.stuChips}>
                  <span className={styles.chip}>🆔 {found.studentCode || "—"}</span>
                </div>
              </div>
            </div>

            {/* Two-column body */}
            <div className={styles.finalBody}>
              {/* LEFT — coursework breakdown */}
              <div className={styles.finalLeft}>
                <div className={styles.finalSectionTitle}>Coursework Breakdown</div>

                <div className={styles.finalRow}>
                  <span className={styles.finalRowLabel}>📝 Midterm</span>
                  <span className={styles.finalRowVal}>
                    {found.midtermGrade !== undefined && found.midtermGrade !== null
                      ? <><strong>{found.midtermGrade}</strong>/{found.midtermMax || "—"}</>
                      : <span style={{ color: "#ef4444" }}>Not set</span>}
                  </span>
                </div>

                <div className={styles.finalRow}>
                  <span className={styles.finalRowLabel}>🧩 Quizzes</span>
                  <span className={styles.finalRowVal}><strong>{safeNum(found.quizScore)}</strong> pts</span>
                </div>

                <div className={styles.finalRow}>
                  <span className={styles.finalRowLabel}>📋 Assignments</span>
                  <span className={styles.finalRowVal}><strong>{safeNum(found.assignmentScore)}</strong> pts</span>
                </div>

                <div className={styles.finalRow} style={{ alignItems: "center" }}>
                  <span className={styles.finalRowLabel}>
                    🎁 Bonus
                    {(editing || !found.submitted) && (
                      <button className={styles.bonusToggle}
                        onClick={() => {
                          if (bonusLocked) {
                            setBonusMsg("Coursework already reached 40 — bonus is locked.");
                            return;
                          }
                          setShowBonus(v => !v);
                          if (!showBonus) setBonus(String(found.bonus || 0));
                          setBonusMsg("");
                        }}>
                        {showBonus ? "▲ hide" : "▼ edit"}
                      </button>
                    )}
                  </span>
                  {showBonus ? (
                    <input type="number" min={1} max={bonusCap || 0}
                      className={styles.bonusIn}
                      value={bonusLocked ? "0" : bonus}
                      disabled={bonusLocked}
                      onChange={e => onBonusChange(e.target.value)}
                      onBlur={() => {
                        if (bonusLocked) return;
                        const n = Number(bonus);
                        if (!isNaN(n) && n > 0 && n < 1) setBonus("1");
                      }} />
                  ) : (
                    <span className={styles.finalRowVal}>
                      <strong>{bonusLocked ? 0 : (found.bonus ?? 0)}</strong> pts
                    </span>
                  )}
                </div>
                {(editing || showBonus) && (bonusMsg || bonusLocked) && (
                  <p className={styles.bonusHint}>
                    {bonusMsg || "Coursework already reached 40 — bonus locked at 0."}
                  </p>
                )}

                <div className={styles.finalDivider} />

                <div className={styles.finalRow} style={{ fontWeight: 700 }}>
                  <span className={styles.finalRowLabel} style={{ color }}>Total Coursework</span>
                  <span className={styles.finalRowVal} style={{ color, fontSize: "1rem" }}>
                    <strong>{Math.round(cwPreview * 10) / 10}</strong>/40
                  </span>
                </div>
              </div>

              {/* RIGHT — grade circle + final input */}
              <div className={styles.finalRight}>
                {/* Live grade circle */}
                <div className={styles.finalCircleWrap}>
                  <svg viewBox="0 0 100 100" className={styles.circSvg}>
                    <circle cx="50" cy="50" r="40" fill="none" stroke="var(--prog-track)" strokeWidth="8" />
                    <motion.circle cx="50" cy="50" r="40" fill="none"
                      stroke={lcColor} strokeWidth="8" strokeLinecap="round"
                      strokeDasharray={dashArr}
                      animate={{ strokeDashoffset: dashArr - dashArr * (Math.min(100, totalPreview) / 100) }}
                      transition={{ duration: .6, ease: "easeOut" }}
                      style={{ transform: "rotate(-90deg)", transformOrigin: "50px 50px" }} />
                  </svg>
                  <div className={styles.circInner}>
                    <span className={styles.circGrade} style={{ color: lcColor, fontSize: "1.4rem" }}>
                      {letterPreview}
                    </span>
                    <span className={styles.circMax} style={{ fontSize: ".75rem" }}>
                      {showCircleTotal ? `${Math.round(totalPreview)}/100` : "—"}
                    </span>
                  </div>
                </div>
                <div className={styles.finalLetterLabel} style={{ color: lcColor }}>
                  Course Grade · {letterPreview}
                </div>

                <div className={styles.entryLabel} style={{ marginTop: 12 }}>
                  Final Exam Score
                  <span className={styles.entryOf}>/ 60 pts</span>
                </div>

                {/* Read-only view — saved final score with Edit button */}
                {found.submitted && !editing && (
                  <>
                    <div className={styles.readOnlyGrade} style={{ borderColor: `${lcColor}45`, color: lcColor, marginBottom: 6 }}>
                      <strong>{savedFinalScore}</strong>
                      <span className={styles.readOnlyOf}>/ 60</span>
                    </div>
                    <div className={styles.finalSavedMeta}>
                      <span>Saved final: {savedFinalScore}/60</span>
                      <span>Total: {Math.round(totalPreview)}/100</span>
                    </div>
                    <motion.button className={styles.saveBtn}
                      style={{ background: color, marginTop: 10, width: "100%" }}
                      onClick={startEdit}
                      whileHover={{ scale: 1.03, filter: "brightness(1.08)" }}
                      whileTap={{ scale: .97 }}>
                      ✎ Edit Grade
                    </motion.button>
                  </>
                )}

                {/* Edit / first-time entry */}
                {(!found.submitted || editing) && (
                  <>
                    <div className={styles.entryRow}>
                      <input type="number" min={0} max={60}
                        className={styles.gradeIn}
                        style={draft ? { borderColor: `${color}70`, boxShadow: `0 0 0 4px ${color}18` } : {}}
                        placeholder={found.submitted ? String(found.finalScore ?? 0) : "0"}
                        value={draft}
                        onChange={e => setDraft(e.target.value)}
                        onKeyDown={e => e.key === "Enter" && save()} />
                      <span className={styles.entryOf} style={{ fontSize: "1rem" }}>/ 60</span>
                    </div>

                    {draft !== "" && (Number(draft) < 0 || Number(draft) > 60) && (
                      <p className={styles.err}>Score must be 0 – 60</p>
                    )}

                    <motion.button className={styles.saveBtn}
                      disabled={draft === "" || Number(draft) < 0 || Number(draft) > 60 || saving}
                      style={{ background: color, marginTop: 10, width: "100%",
                        opacity: draft !== "" && Number(draft) >= 0 && Number(draft) <= 60 ? 1 : .4 }}
                      onClick={save}
                      whileHover={{ scale: 1.03, filter: "brightness(1.08)" }}
                      whileTap={{ scale: .97 }}>
                      {saving ? "Saving…" : found.submitted ? "✓ Save" : "✓ Save Final Grade"}
                    </motion.button>

                    {editing && (
                      <button className={styles.cancelBtn}
                        style={{ marginTop: 8, width: "100%" }}
                        onClick={cancelEdit}
                        disabled={saving}>
                        Cancel
                      </button>
                    )}
                  </>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

/* ════════ MAIN PAGE ════════ */
export default function GradesMgmtPage() {
  const location = useLocation();

  const [courses,   setCourses]   = useState([]);
  const [course,    setCourse]    = useState(null);
  const [activeTab, setActiveTab] = useState(null);
  const [loadingC,  setLoadingC]  = useState(true);

  // Deep-link state from notification action button
  // Shape: { tab: "final", courseId: "42", studentQuery: "guid" } | null
  //
  // NOTE: useState lazy initializer only runs on mount.  If the instructor is
  // already on this page when they click the notification, React Router updates
  // location.state WITHOUT re-mounting the component.  The effect below watches
  // location.key (unique per navigate() call) to catch that case.
  const [deepLink,           setDeepLink]           = useState(null);
  const [pendingStudentQuery, setPendingStudentQuery] = useState(null);

  // Re-apply deep-link whenever we navigate to this page — even if already here.
  useEffect(() => {
    if (location.state?.tab) {
      setDeepLink(location.state);
    }
  }, [location.key]); // location.key changes with every navigate() call

  useEffect(() => {
    setLoadingC(true);
    getInstructorCourses()
      .then(data => {
        const list = Array.isArray(data) ? data : [];
        setCourses(list);
      })
      .catch(() => setCourses([]))
      .finally(() => setLoadingC(false));
  }, []);

  // Apply deep-link once courses have loaded
  useEffect(() => {
    if (!deepLink || courses.length === 0) return;
    const { tab, courseId, studentQuery } = deepLink;
    if (courseId) {
      const found = courses.find(c => c.id === courseId || c.id === String(courseId));
      if (found) {
        setCourse(found.id);
        if (tab) setActiveTab(tab);
        if (studentQuery) setPendingStudentQuery(studentQuery);
      }
    }
    setDeepLink(null);
    // Clear router state so back-navigation doesn't re-apply the deep link
    window.history.replaceState({}, document.title);
  }, [deepLink, courses]);

  const c = course ? (courses.find(x => x.id === course) || null) : null;

  const TABS = [
    {
      key: "assignments", icon: "📋",
      title: "Assignments", sub: "Review & grade submissions",
      color: c?.color || "#818cf8",
      stats: [{ val: "—", label: "Submissions", c: "#f59e0b" }],
      available: true,
    },
    {
      key: "midterm", icon: "📝",
      title: "Midterm Grades", sub: "Record exam grades · max 40 pts",
      color: "#0ea5e9",
      stats: [{ val: "max 40", label: "Points", c: "#0ea5e9" }],
      available: true,
    },
    {
      key: "final", icon: "🎓",
      title: "Final Grades", sub: "Enter final exam scores · max 60 pts",
      color: "#a855f7",
      stats: [{ val: "max 100", label: "Total", c: "#a855f7" }],
      available: true,
    },
  ];

  if (loadingC) return (
    <div className={styles.screenState}>
      <div className={styles.screenStateIcon}>⏳</div>
      <p className={styles.screenStateText}>Loading courses…</p>
    </div>
  );

  if (courses.length === 0) return (
    <div className={styles.screenState}>
      <div className={styles.screenStateIcon}>📭</div>
      <p className={styles.screenStateText}>No courses assigned yet.</p>
    </div>
  );

  return (
    <div className={styles.page} style={{ "--accent": c?.color || "#7c3aed" }}>

      {/* ════ HERO HEADER ════ */}
      <motion.div className={styles.hero}
        initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: .45, ease: [.22, 1, .36, 1] }}>
        <div className={styles.heroBg}
          style={{ background: `radial-gradient(ellipse 60% 100% at 10% 50%, ${c?.color || "#818cf8"}18 0%, transparent 70%)` }} />

        <div className={styles.heroContent}>
          <div>
            <h1 className={styles.heroTitle}>Grade Management</h1>
            <p className={styles.heroSub}>Manage assignments, midterm and final grades for your courses</p>
          </div>
        </div>
      </motion.div>

      {/* ════ COURSE PICKER (below hero) ════ */}
      <motion.div className={styles.coursePickerWrap}
        initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.08 }}>
        <div className={styles.sectionKicker}>Select a course</div>
        {courses.length === 0 ? (
          <div style={{ padding: "16px 20px", borderRadius: 14, background: "rgba(245,158,11,.07)", border: "1.5px solid rgba(245,158,11,.22)", color: "#92400e", fontSize: ".84rem", fontWeight: 600 }}>
            This account is not responsible for any course yet.
          </div>
        ) : (
          <div className={styles.courseRow}>
            {courses.map((cr, i) => (
              <motion.button key={cr.id}
                className={`${styles.coursePill} ${course === cr.id ? styles.coursePillOn : ""}`}
                style={{ "--cp": cr.color }}
                onClick={() => { setCourse(cr.id); setActiveTab(null); setPendingStudentQuery(null); }}
                initial={{ opacity: 0, x: 16 }} animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * .06, type: "spring", stiffness: 400, damping: 26 }}
                whileHover={{ y: -2 }} whileTap={{ scale: .96 }}>
                <span className={styles.cpIcon}>{cr.icon}</span>
                <div className={styles.cpText}>
                  <span className={styles.cpCode} style={{ color: cr.color }}>{cr.code}</span>
                  <span className={styles.cpName}>{cr.name}</span>
                </div>
              </motion.button>
            ))}
          </div>
        )}
      </motion.div>

      {/* ════ NO COURSE SELECTED ════ */}
      {!course && courses.length > 0 && (
        <motion.div className={styles.idleWrap} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <div className={styles.idleIcon} style={{ background: "rgba(129,140,248,.12)", color: "#818cf8" }}>📊</div>
          <h3 className={styles.idleTitle}>Select a course above</h3>
          <p className={styles.idleSub}>Choose a course to manage its assignments, midterm and final grades</p>
        </motion.div>
      )}

      {/* ════ SECTION TABS ════ */}
      {course && <div className={styles.tabRow}>
        {TABS.map((t, i) => (
          <motion.button key={t.key}
            className={`${styles.tab} ${activeTab === t.key ? styles.tabOn : ""}`}
            style={{ "--tc": t.color }}
            onClick={() => setActiveTab(p => p === t.key ? null : t.key)}
            initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}
            transition={{ delay: .1 + i * .06, type: "spring", stiffness: 380, damping: 28 }}
            whileHover={{ y: -4 }} whileTap={{ scale: .98 }}>

            <motion.div className={styles.tabGlow}
              style={{ background: `linear-gradient(135deg,${t.color}40,${t.color}00)` }}
              animate={{ opacity: activeTab === t.key ? 1 : 0 }} transition={{ duration: .22 }} />
            <motion.div className={styles.tabAccent} style={{ background: t.color }}
              animate={{ scaleX: activeTab === t.key ? 1 : 0 }}
              transition={{ duration: .28, ease: [.22, 1, .36, 1] }} />

            <div className={styles.tabInner}>
              <div className={styles.tabIcon}
                style={{ background: `${t.color}18`, border: `2px solid ${t.color}28`, color: t.color }}>
                {t.icon}
              </div>
              <div className={styles.tabMid}>
                <div className={styles.tabTitle} style={activeTab === t.key ? { color: t.color } : {}}>{t.title}</div>
                <div className={styles.tabSub}>{t.sub}</div>
              </div>
              <div className={styles.tabStats}>
                {t.stats.map((s, si) => (
                  <div key={si} className={styles.tabStat}>
                    <span className={styles.tabStatVal} style={{ color: s.c }}>{s.val}</span>
                    <span className={styles.tabStatLabel}>{s.label}</span>
                  </div>
                ))}
              </div>
            </div>
          </motion.button>
        ))}
      </div>}

      {/* ════ CONTENT PANEL ════ */}
      {course && <AnimatePresence mode="wait">
        {!activeTab && (
          <motion.div key="idle" className={styles.idleWrap}
            initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
            transition={{ duration: .18 }}>
            <div className={styles.idleIcon}
              style={{ background: `${c?.color || "#818cf8"}14`, color: c?.color || "#818cf8" }}>
              {c?.icon || "📚"}
            </div>
            <h3 className={styles.idleTitle}>Select a section above</h3>
            <p className={styles.idleSub}>Click Assignments, Midterm, or Final to start grading</p>
          </motion.div>
        )}

        {activeTab && (
          <motion.div key={`${course}-${activeTab}`} className={styles.panel}
            initial={{ opacity: 0, y: 18 }} animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -12 }} transition={{ duration: .24, ease: [.22, 1, .36, 1] }}>

            <div className={styles.panelHead}
              style={{ borderTop: `3px solid ${TABS.find(t => t.key === activeTab)?.color || c?.color}` }}>
              <div className={styles.panelHeadL}>
                <span className={styles.panelIco}>{TABS.find(t => t.key === activeTab)?.icon}</span>
                <div>
                  <div className={styles.panelTitle}>
                    {activeTab === "assignments" ? "Manage Assignments"
                      : activeTab === "midterm" ? "Manage Midterm Grades"
                      : "Manage Final Grades"}
                  </div>
                  <div className={styles.panelSub}>{c?.code} · {c?.name}</div>
                </div>
              </div>
              <button className={styles.panelClose} onClick={() => { setActiveTab(null); setPendingStudentQuery(null); }}>✕ Close</button>
            </div>

            {activeTab === "assignments" && (
              <AssignmentsSection key={course} courseId={course} color={c?.color || "#818cf8"} />
            )}
            {activeTab === "midterm" && (
              <MidtermSection key={course} courseId={course} color="#0ea5e9" />
            )}
            {activeTab === "final" && (
              <FinalGradeSection
                key={course}
                courseId={course}
                color="#a855f7"
                initialQuery={pendingStudentQuery}
              />
            )}
          </motion.div>
        )}
      </AnimatePresence>}
    </div>
  );
}
