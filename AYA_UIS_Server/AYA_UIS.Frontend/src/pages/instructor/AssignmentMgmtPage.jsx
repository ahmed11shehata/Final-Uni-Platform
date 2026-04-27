// src/pages/instructor/AssignmentMgmtPage.jsx
import { useState, useEffect, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./AssignmentMgmtPage.module.css";
import {
  getInstructorCourses,
  getInstructorAssignments,
  createAssignment,
  updateAssignment,
  deleteAssignment,
} from "../../services/api/instructorApi";

const sp = { type: "spring", stiffness: 400, damping: 28 };
const FMTS = ["pdf", "zip", "docx", "py", "cpp", "java", "mp4"];

function fmtDate(iso) {
  if (!iso) return "—";
  try {
    return new Date(iso).toLocaleDateString("en-US", {
      month: "short", day: "numeric", year: "numeric",
    });
  } catch { return iso; }
}

function isDeadlinePassed(deadline) {
  if (!deadline) return false;
  return new Date(deadline) < new Date();
}

/** Backend returns `submissionsCount` (plural). Older shapes are accepted for safety. */
function getSubCount(a) {
  return Number(
    a?.submissionsCount ?? a?.submissionCount ?? a?.totalSubmissions ?? a?.pendingCount ?? 0
  ) || 0;
}

function getStatusInfo(a) {
  if (isDeadlinePassed(a.deadline)) {
    return { label: "Closed", color: "#ef4444", bg: "rgba(239,68,68,.12)" };
  }
  const count = getSubCount(a);
  if (count > 0) {
    return { label: "Active", color: "#22c55e", bg: "rgba(34,197,94,.12)" };
  }
  return { label: "Open", color: "#818cf8", bg: "rgba(129,140,248,.12)" };
}

/* ── Assignment Form (create + edit) ── */
function AssignmentForm({ courseCode, color, onDone, onCancel, editMode = false, initial = null }) {
  const [title,      setTitle]      = useState(initial?.title ?? "");
  const [desc,       setDesc]       = useState(initial?.description ?? "");
  const [deadline,   setDeadline]   = useState(
    initial?.deadline ? initial.deadline.split("T")[0] : ""
  );
  const [maxPts,     setMaxPts]     = useState(String(initial?.maxGrade ?? "20"));
  const [allowFmt,   setAllowFmt]   = useState(initial?.allowedFormats ?? ["pdf"]);
  const [attachFile, setAttachFile] = useState(null);
  const [removeAttachment, setRemoveAttachment] = useState(false);
  const [loading,    setLoading]    = useState(false);
  const fileRef = useRef();

  // Backend blocks point edits once any submission has been graded; we mirror that
  // here so the field is disabled with a clear explanation.
  const subCount    = editMode ? getSubCount(initial) : 0;
  const pointsLocked = editMode && subCount > 0;
  const existingAttachUrl = initial?.attachmentUrl || null;

  const toggleFmt = (f) =>
    setAllowFmt((p) => (p.includes(f) ? p.filter((x) => x !== f) : [...p, f]));

  const valid = title.trim() && deadline && allowFmt.length > 0;

  const submit = async () => {
    if (!valid) return;
    setLoading(true);
    try {
      if (editMode && initial?.id) {
        const dto = {
          title,
          description: desc,
          deadline: new Date(deadline + "T23:59:00").toISOString(),
          // Only send maxGrade when not locked → backend won't reject.
          maxGrade: pointsLocked ? undefined : Number(maxPts),
          removeAttachment: removeAttachment && !attachFile,
        };
        await updateAssignment(initial.id, dto, attachFile || null);
      } else {
        const dto = {
          title,
          description: desc,
          courseCode,
          deadline: new Date(deadline + "T23:59:00").toISOString(),
          maxGrade: Number(maxPts),
          allowedFormats: allowFmt,
        };
        await createAssignment(dto, attachFile || null);
      }
      onDone?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Operation failed. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.form}>
      {/* Title */}
      <div className={styles.field}>
        <label className={styles.label}>
          Title <span className={styles.req}>*</span>
        </label>
        <input
          className={styles.inp}
          style={title ? { borderColor: `${color}60` } : {}}
          placeholder="e.g. Chapter 3 Exercise"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
      </div>

      {/* Description */}
      <div className={styles.field}>
        <label className={styles.label}>
          Instructions <span className={styles.opt}>(optional)</span>
        </label>
        <textarea
          className={styles.textarea}
          rows={3}
          placeholder="Describe what students need to submit…"
          value={desc}
          onChange={(e) => setDesc(e.target.value)}
        />
      </div>

      {/* Deadline + Max pts */}
      <div className={styles.twoCol}>
        <div className={styles.field}>
          <label className={styles.label}>
            Deadline <span className={styles.req}>*</span>
          </label>
          <input
            type="date"
            className={styles.inp}
            style={deadline ? { borderColor: `${color}60` } : {}}
            value={deadline}
            onChange={(e) => setDeadline(e.target.value)}
            min={new Date().toISOString().split("T")[0]}
          />
        </div>
        <div className={styles.field}>
          <label className={styles.label}>
            Max Grade (pts){pointsLocked && <span className={styles.opt}> — locked</span>}
          </label>
          <div className={styles.starRow}>
            {[5, 10, 20, 50, 100].map((n) => (
              <button
                key={n}
                className={`${styles.starBtn} ${Number(maxPts) === n ? styles.starBtnOn : ""}`}
                style={
                  Number(maxPts) === n
                    ? { background: color, borderColor: color, color: "#fff", opacity: pointsLocked ? 0.7 : 1 }
                    : pointsLocked
                    ? { opacity: 0.5, cursor: "not-allowed" }
                    : {}
                }
                disabled={pointsLocked}
                onClick={() => !pointsLocked && setMaxPts(String(n))}>
                {n}
              </button>
            ))}
          </div>
          {pointsLocked && (
            <span style={{ fontSize: 11.5, color: "#f59e0b", marginTop: 6, display: "block" }}>
              ⚠ {subCount} submission{subCount !== 1 ? "s" : ""} already received. Maximum points are
              locked to keep existing grades fair. Other fields can still be updated.
            </span>
          )}
        </div>
      </div>

      {/* Formats */}
      <div className={styles.field}>
        <label className={styles.label}>
          Allowed Formats <span className={styles.req}>*</span>
        </label>
        <div className={styles.fmtRow}>
          {FMTS.map((f) => (
            <button
              key={f}
              className={`${styles.fmtBtn} ${allowFmt.includes(f) ? styles.fmtBtnOn : ""}`}
              style={allowFmt.includes(f) ? { background: color, borderColor: color } : {}}
              onClick={() => toggleFmt(f)}>
              .{f}
            </button>
          ))}
        </div>
      </div>

      {/* Attachment file — available in both create and edit modes */}
      <div className={styles.field}>
        <label className={styles.label}>
          {editMode ? "Replace Attachment" : "Starter File"}{" "}
          <span className={styles.opt}>(optional)</span>
        </label>

        {/* In edit mode: show existing attachment if not slated for removal and no replacement chosen */}
        {editMode && existingAttachUrl && !attachFile && !removeAttachment && (
          <div style={{
            display: "flex", alignItems: "center", gap: 10, padding: "8px 12px",
            border: `1.5px solid ${color}40`, borderRadius: 10, background: `${color}08`,
            marginBottom: 8,
          }}>
            <span style={{ flex: 1, fontSize: 13, color: "var(--text-secondary)" }}>
              📎 Current file: <a href={existingAttachUrl} target="_blank" rel="noreferrer" style={{ color }}>open</a>
            </span>
            <button
              type="button"
              onClick={() => setRemoveAttachment(true)}
              style={{
                padding: "5px 10px", borderRadius: 8, cursor: "pointer",
                border: "1.5px solid rgba(239,68,68,0.3)",
                background: "rgba(239,68,68,0.07)",
                color: "#ef4444", fontFamily: "inherit", fontSize: 11.5, fontWeight: 700,
              }}>
              Remove
            </button>
          </div>
        )}

        {editMode && removeAttachment && !attachFile && (
          <div style={{
            display: "flex", alignItems: "center", gap: 10, padding: "8px 12px",
            border: "1.5px solid rgba(239,68,68,0.25)", borderRadius: 10,
            background: "rgba(239,68,68,0.06)", marginBottom: 8,
          }}>
            <span style={{ flex: 1, fontSize: 12.5, color: "#ef4444" }}>
              The current attachment will be deleted on save.
            </span>
            <button
              type="button"
              onClick={() => setRemoveAttachment(false)}
              style={{
                padding: "4px 10px", borderRadius: 8, cursor: "pointer",
                border: "1px solid var(--border)", background: "transparent",
                color: "var(--text-secondary)", fontFamily: "inherit", fontSize: 11, fontWeight: 700,
              }}>
              Undo
            </button>
          </div>
        )}

        <input
          ref={fileRef}
          type="file"
          style={{ display: "none" }}
          onChange={(e) => {
            setAttachFile(e.target.files[0] || null);
            setRemoveAttachment(false);
          }}
        />
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <button
            type="button"
            className={styles.inp}
            style={{
              cursor: "pointer", textAlign: "left", flex: 1,
              color: attachFile ? "var(--text-primary)" : "var(--text-muted)",
              borderColor: attachFile ? `${color}60` : undefined,
            }}
            onClick={() => fileRef.current?.click()}>
            {attachFile
              ? `📎 ${attachFile.name}`
              : editMode
              ? (existingAttachUrl && !removeAttachment ? "📎 Click to replace the current file…" : "📎 Click to attach a file…")
              : "📎 Click to attach a file…"}
          </button>
          {attachFile && (
            <button
              type="button"
              onClick={() => { setAttachFile(null); if (fileRef.current) fileRef.current.value = ""; }}
              style={{
                padding: "6px 10px", borderRadius: 8, cursor: "pointer",
                border: "1.5px solid rgba(239,68,68,0.3)",
                background: "rgba(239,68,68,0.07)",
                color: "#ef4444", fontFamily: "inherit", fontSize: 12, fontWeight: 700,
              }}>
              ✕
            </button>
          )}
        </div>
        {attachFile && (
          <span style={{ fontSize: 11.5, color: "var(--text-muted)", marginTop: 2 }}>
            {(attachFile.size / 1048576).toFixed(1)} MB — will be downloadable by students
          </span>
        )}
      </div>

      {/* Actions */}
      <div className={styles.formActions}>
        {onCancel && (
          <button className={styles.cancelBtn} onClick={onCancel} disabled={loading}>
            Cancel
          </button>
        )}
        <motion.button
          className={styles.submitBtn}
          style={{
            background: `linear-gradient(135deg, ${color}cc, ${color})`,
            opacity: valid ? 1 : 0.45,
          }}
          disabled={!valid || loading}
          onClick={submit}
          whileHover={valid ? { scale: 1.02, filter: "brightness(1.08)" } : {}}
          whileTap={valid ? { scale: 0.97 } : {}}>
          {loading ? (
            <motion.span
              animate={{ rotate: 360 }}
              transition={{ duration: 0.8, repeat: Infinity, ease: "linear" }}>
              ⟳
            </motion.span>
          ) : editMode ? "✎ Save Changes" : "📋 Create Assignment"}
        </motion.button>
      </div>
    </div>
  );
}

/* ── Assignment Card ── */
function AsnCard({ a, color, index, onEdit, onDelete, onRepublish }) {
  const statusInfo = getStatusInfo(a);
  const deadlinePassed = isDeadlinePassed(a.deadline);
  const subCount = getSubCount(a);
  const hasSubmissions = subCount > 0;
  // Editing remains possible while there are submissions — backend locks point changes only.
  const canEdit = !deadlinePassed;
  const editDisabledReason = deadlinePassed ? "Deadline has passed" : null;
  const editHint = !deadlinePassed && hasSubmissions
    ? `${subCount} submission${subCount !== 1 ? "s" : ""} — points locked`
    : null;

  return (
    <motion.div
      className={styles.asnCard}
      initial={{ opacity: 0, y: 18, scale: 0.95 }}
      animate={{ opacity: 1, y: 0, scale: 1 }}
      exit={{ opacity: 0, scale: 0.95 }}
      transition={{ delay: index * 0.06, ...sp }}
      whileHover={{ y: -4, boxShadow: "0 14px 40px rgba(0,0,0,0.1)" }}>

      <div className={styles.asnCardStripe} style={{ background: color }} />

      <div className={styles.asnCardBody}>
        <div className={styles.asnCardHead}>
          <span className={styles.asnCardIdx} style={{ background: `${color}15`, color }}>
            {String(index + 1).padStart(2, "0")}
          </span>
          <span
            className={styles.asnStatus}
            style={{ background: statusInfo.bg, color: statusInfo.color }}>
            {statusInfo.label}
          </span>
        </div>

        <div className={styles.asnCardTitle}>{a.title}</div>

        <div className={styles.asnCardMeta}>
          <span>📅 {fmtDate(a.deadline)}</span>
          <span className={styles.metaDot}>·</span>
          <span>⭐ {a.maxGrade ?? "—"} pts</span>
          {hasSubmissions && (
            <>
              <span className={styles.metaDot}>·</span>
              <span>📬 {subCount} submission{subCount !== 1 ? "s" : ""}</span>
            </>
          )}
        </div>

        {a.allowedFormats?.length > 0 && (
          <div className={styles.asnFmtRow}>
            {a.allowedFormats.map((f) => (
              <span key={f} className={styles.asnFmt}>.{f}</span>
            ))}
            {a.attachmentUrl && (
              <span className={styles.asnFmt} style={{ color, borderColor: `${color}40`, background: `${color}08` }}>
                📎 file
              </span>
            )}
          </div>
        )}
      </div>

      <div className={styles.asnCardActions}>
        {/* Edit button — disabled if deadline passed or submissions exist */}
        <div className={styles.editWrap}>
          <motion.button
            className={`${styles.asnBtn} ${canEdit ? styles.asnBtnEdit : styles.asnBtnDisabled}`}
            disabled={!canEdit}
            onClick={canEdit ? () => onEdit(a) : undefined}
            title={editDisabledReason ?? "Edit this assignment"}
            whileHover={canEdit ? { scale: 1.05 } : {}}
            whileTap={canEdit ? { scale: 0.95 } : {}}>
            ✎ Edit
          </motion.button>
          {!canEdit && editDisabledReason && (
            <span className={styles.editHint}>{editDisabledReason}</span>
          )}
          {canEdit && editHint && (
            <span className={styles.editHint} style={{ color: "#f59e0b" }}>{editHint}</span>
          )}
        </div>

        <motion.button
          className={`${styles.asnBtn} ${styles.asnBtnRepublish}`}
          onClick={() => onRepublish(a)}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}>
          🔄 Republish
        </motion.button>

        <motion.button
          className={`${styles.asnBtn} ${styles.asnBtnDelete}`}
          onClick={() => onDelete(a)}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}>
          🗑 Delete
        </motion.button>
      </div>
    </motion.div>
  );
}

/* ── Delete Confirm Modal ── */
function DeleteModal({ assignment, onConfirm, onClose, loading }) {
  return (
    <motion.div
      className={styles.overlay}
      onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className={styles.modal}
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, scale: 0.88, y: 28 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95 }}
        transition={sp}>
        <div className={styles.modalBand} style={{ background: "linear-gradient(135deg,#b91c1c,#ef4444)" }} />
        <div className={styles.modalBody}>
          <div className={styles.modalTitle}>Delete Assignment?</div>
          <div className={styles.modalSub}>
            "<strong>{assignment?.title}</strong>" will be permanently removed and cannot be recovered.
          </div>
          <div className={styles.mActions}>
            <button className={styles.mCancel} onClick={onClose} disabled={loading}>
              Cancel
            </button>
            <motion.button
              className={styles.mDelete}
              onClick={onConfirm}
              disabled={loading}
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.97 }}>
              {loading ? "Deleting…" : "🗑 Delete"}
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ════════════════════════════════
   MAIN PAGE
════════════════════════════════ */
export default function AssignmentMgmtPage() {
  const [courses,      setCourses]      = useState([]);
  const [courseId,     setCourseId]     = useState(null);
  const [loadingC,     setLoadingC]     = useState(true);
  const [assignments,  setAssignments]  = useState([]);
  const [loadingA,     setLoadingA]     = useState(false);
  const [showForm,     setShowForm]     = useState(false);
  const [editTarget,   setEditTarget]   = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting,     setDeleting]     = useState(false);
  const [toast,        setToast]        = useState(null);

  const toast$ = (msg, t = "ok") => {
    setToast({ msg, t });
    setTimeout(() => setToast(null), 2500);
  };

  useEffect(() => {
    setLoadingC(true);
    getInstructorCourses()
      .then((data) => setCourses(Array.isArray(data) ? data : []))
      .catch(() => setCourses([]))
      .finally(() => setLoadingC(false));
  }, []);

  const loadAssignments = (cid) => {
    setLoadingA(true);
    setAssignments([]);
    getInstructorAssignments(cid)
      .then((data) => setAssignments(Array.isArray(data) ? data : []))
      .catch(() => setAssignments([]))
      .finally(() => setLoadingA(false));
  };

  const selectCourse = (id) => {
    setCourseId(id);
    setShowForm(false);
    setEditTarget(null);
    loadAssignments(id);
  };

  const handleCreated = () => {
    setShowForm(false);
    setEditTarget(null);
    toast$("✓ Assignment saved successfully");
    if (courseId) loadAssignments(courseId);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteAssignment(deleteTarget.id);
      setAssignments((p) => p.filter((a) => a.id !== deleteTarget.id));
      setDeleteTarget(null);
      toast$("🗑 Assignment deleted");
    } catch (e) {
      toast$(e?.response?.data?.error?.message || "Delete failed", "err");
    } finally {
      setDeleting(false);
    }
  };

  const handleRepublish = () => {
    // TODO: backend does not yet expose a republish/extend endpoint.
    // When available: call PUT /instructor/assignments/{id}/republish or similar.
    toast$("🔄 Republish — backend endpoint not yet available", "err");
  };

  const c = courseId ? (courses.find((x) => x.id === courseId) || null) : null;
  const color = c?.color || "#818cf8";

  if (loadingC) return (
    <div style={{
      display: "flex", justifyContent: "center", padding: "80px 0",
      color: "var(--text-muted)", fontFamily: "Sora, sans-serif",
    }}>
      Loading courses…
    </div>
  );

  return (
    <div className={styles.page}>

      {/* ── Toast ── */}
      <AnimatePresence>
        {toast && (
          <motion.div
            className={`${styles.toast} ${toast.t === "err" ? styles.toastErr : ""}`}
            initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            transition={sp}>
            {toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── Hero Header ── */}
      <motion.div
        className={styles.hero}
        initial={{ opacity: 0, y: -18 }} animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.45, ease: [0.22, 1, 0.36, 1] }}>
        <div className={styles.heroContent}>
          <div>
            <h1 className={styles.heroTitle}>Assignment Management</h1>
            <p className={styles.heroSub}>Create and manage assignments for your students</p>
          </div>
        </div>
      </motion.div>

      {/* ── Course Picker (inside content, not in hero) ── */}
      <motion.div
        className={styles.coursePicker}
        initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.1 }}>
        <div className={styles.coursePickerLabel}>Select a course to manage assignments</div>

        {courses.length === 0 ? (
          <div className={styles.noCoursesMsg}>
            This account is not responsible for any course yet.
          </div>
        ) : (
          <div className={styles.courseRow}>
            {courses.map((cr, i) => (
              <motion.button
                key={cr.id}
                className={`${styles.coursePill} ${courseId === cr.id ? styles.coursePillOn : ""}`}
                style={{ "--cp": cr.color }}
                onClick={() => selectCourse(cr.id)}
                initial={{ opacity: 0, x: 16 }} animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * 0.06, ...sp }}
                whileHover={{ y: -2 }} whileTap={{ scale: 0.96 }}>
                <span className={styles.cpIcon}>{cr.icon || "📚"}</span>
                <div className={styles.cpText}>
                  <span className={styles.cpCode} style={{ color: cr.color }}>{cr.code}</span>
                  <span className={styles.cpName}>{cr.name}</span>
                </div>
              </motion.button>
            ))}
          </div>
        )}
      </motion.div>

      {/* ── Main Content ── */}
      {!courseId ? (
        <motion.div className={styles.idle} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <div className={styles.idleIcon} style={{ background: "rgba(129,140,248,.12)", color: "#818cf8" }}>
            📋
          </div>
          <p className={styles.idleTitle}>Select a course above</p>
          <p className={styles.idleSub}>Choose a course to view and manage its assignments</p>
        </motion.div>
      ) : (
        <AnimatePresence mode="wait">
          <motion.div
            key={courseId}
            className={styles.content}
            initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }} transition={{ duration: 0.24 }}>

            {/* ── Create / Edit Form Panel ── */}
            <div className={styles.formSection}>
              <div className={styles.formSectionHead}>
                <div className={styles.formSectionTitle}>
                  {editTarget ? "✎ Edit Assignment" : "➕ Create New Assignment"}
                  {c && (
                    <span className={styles.courseTag} style={{ color, background: `${color}15` }}>
                      {c.code}
                    </span>
                  )}
                </div>
                {!showForm && !editTarget && (
                  <motion.button
                    className={styles.openFormBtn}
                    style={{ background: `linear-gradient(135deg, ${color}cc, ${color})` }}
                    onClick={() => setShowForm(true)}
                    whileHover={{ scale: 1.03 }} whileTap={{ scale: 0.97 }}>
                    + Create Assignment
                  </motion.button>
                )}
              </div>

              <AnimatePresence>
                {(showForm || editTarget) && (
                  <motion.div
                    initial={{ opacity: 0, height: 0 }}
                    animate={{ opacity: 1, height: "auto" }}
                    exit={{ opacity: 0, height: 0 }}
                    transition={{ duration: 0.28 }}>
                    <AssignmentForm
                      courseCode={c?.code}
                      color={color}
                      editMode={!!editTarget}
                      initial={editTarget}
                      onDone={handleCreated}
                      onCancel={() => { setShowForm(false); setEditTarget(null); }}
                    />
                  </motion.div>
                )}
              </AnimatePresence>
            </div>

            {/* ── Assignment Cards ── */}
            <div className={styles.cardsSection}>
              <div className={styles.cardsSectionHead}>
                <div className={styles.cardsSectionTitle}>
                  All Assignments
                  {assignments.length > 0 && (
                    <span className={styles.countBadge} style={{ background: `${color}18`, color }}>
                      {assignments.length}
                    </span>
                  )}
                </div>
              </div>

              {loadingA ? (
                <div className={styles.empty}><span>⏳</span><p>Loading assignments…</p></div>
              ) : assignments.length === 0 ? (
                <div className={styles.empty}><span>📭</span><p>No assignments for this course yet.</p></div>
              ) : (
                <div className={styles.cardsGrid}>
                  <AnimatePresence>
                    {assignments.map((a, i) => (
                      <AsnCard
                        key={a.id}
                        a={a}
                        color={color}
                        index={i}
                        onEdit={(asn) => {
                          setEditTarget(asn);
                          setShowForm(false);
                          window.scrollTo({ top: 0, behavior: "smooth" });
                        }}
                        onDelete={setDeleteTarget}
                        onRepublish={handleRepublish}
                      />
                    ))}
                  </AnimatePresence>
                </div>
              )}
            </div>
          </motion.div>
        </AnimatePresence>
      )}

      {/* ── Delete Confirm Modal ── */}
      <AnimatePresence>
        {deleteTarget && (
          <DeleteModal
            assignment={deleteTarget}
            loading={deleting}
            onConfirm={handleDelete}
            onClose={() => setDeleteTarget(null)}
          />
        )}
      </AnimatePresence>
    </div>
  );
}
