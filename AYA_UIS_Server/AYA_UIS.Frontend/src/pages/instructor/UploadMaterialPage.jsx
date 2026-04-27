// src/pages/instructor/UploadMaterialPage.jsx
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import styles from "./UploadMaterialPage.module.css";
import {
  createAssignment,
  getCourseMaterials,
  getInstructorCourses,
  uploadLecture,
  updateLecture,
  deleteLecture,
} from "../../services/api/instructorApi";

const WEEKS = Array.from({ length: 16 }, (_, i) => `Week ${i + 1}`);
const spring = { type: "spring", stiffness: 320, damping: 28 };

function FileZone({ accept, onFile, file, color, label, helper }) {
  const ref = useRef(null);
  const [dragging, setDragging] = useState(false);

  const handleFile = (incoming) => {
    if (incoming) onFile(incoming);
  };

  return (
    <button
      type="button"
      className={`${styles.fileZone} ${dragging ? styles.fileZoneDrag : ""} ${file ? styles.fileZoneFilled : ""}`}
      style={{ "--accent": color }}
      onClick={() => ref.current?.click()}
      onDragOver={(e) => {
        e.preventDefault();
        setDragging(true);
      }}
      onDragLeave={() => setDragging(false)}
      onDrop={(e) => {
        e.preventDefault();
        setDragging(false);
        handleFile(e.dataTransfer.files[0]);
      }}
    >
      <input
        ref={ref}
        type="file"
        accept={accept}
        className={styles.hiddenInput}
        onChange={(e) => handleFile(e.target.files?.[0])}
      />

      <div className={styles.fileZoneGlow} />

      {!file ? (
        <div className={styles.fileEmpty}>
          <div className={styles.fileEmptyIcon}>⤴</div>
          <div className={styles.fileEmptyCopy}>
            <strong>{label}</strong>
            <span>Drop file here or click to browse</span>
            <small>{helper}</small>
          </div>
          <span className={styles.fileZoneFormats}>{accept}</span>
        </div>
      ) : (
        <div className={styles.fileCard}>
          <div className={styles.fileBadge}>📎</div>
          <div className={styles.fileMeta}>
            <span className={styles.fileName}>{file.name}</span>
            <span className={styles.fileSize}>{(file.size / 1024 / 1024).toFixed(2)} MB</span>
          </div>
          <button
            type="button"
            className={styles.fileRemove}
            onClick={(e) => {
              e.stopPropagation();
              onFile(null);
            }}
          >
            Remove
          </button>
        </div>
      )}
    </button>
  );
}

function CoursePicker({ courses, course, setCourse }) {
  if (courses.length === 0) {
    return (
      <div className={styles.noticeCard}>
        <span className={styles.noticeIcon}>⚠️</span>
        <div>
          <p className={styles.noticeTitle}>No assigned courses</p>
          <p className={styles.noticeText}>This account is not responsible for any course yet.</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.courseGrid}>
      {courses.map((item, index) => {
        const active = course === item.id;
        return (
          <motion.button
            key={item.id}
            type="button"
            className={`${styles.courseCard} ${active ? styles.courseCardActive : ""}`}
            style={{ "--accent": item.color || "#7c3aed" }}
            onClick={() => setCourse(item.id)}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ ...spring, delay: index * 0.04 }}
            whileHover={{ y: -2 }}
            whileTap={{ scale: 0.99 }}
          >
            <span className={styles.courseBadge}>{item.icon}</span>
            <div className={styles.courseCopy}>
              <span className={styles.courseCode}>{item.code}</span>
              <span className={styles.courseName}>{item.name}</span>
            </div>
            <span className={styles.courseState}>{active ? "Selected" : "Choose"}</span>
          </motion.button>
        );
      })}
    </div>
  );
}

function TypeSelector({ matType, setMatType, accent }) {
  const cards = [
    {
      key: "lecture",
      icon: "🎬",
      title: "Upload Lecture",
      subtitle: "Video, PDF slides, labs, and supporting material.",
      points: ["MP4, PDF, PPTX, ZIP", "Optional week mapping", "Immediate or scheduled release"],
      color: "#5a67d8",
    },
    {
      key: "assignment",
      icon: "📝",
      title: "Create Assignment",
      subtitle: "Homework, project briefs, and student submissions.",
      points: ["Deadline is required", "Starter attachment is optional", "Release timing is controlled"],
      color: accent,
    },
  ];

  return (
    <div className={styles.typeGrid}>
      {cards.map((card, index) => {
        const active = matType === card.key;
        return (
          <motion.button
            key={card.key}
            type="button"
            className={`${styles.typeCard} ${active ? styles.typeCardActive : ""}`}
            style={{ "--accent": card.color }}
            onClick={() => setMatType((prev) => (prev === card.key ? null : card.key))}
            initial={{ opacity: 0, y: 14 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ ...spring, delay: 0.05 + index * 0.05 }}
            whileHover={{ y: -3 }}
            whileTap={{ scale: 0.99 }}
          >
            <div className={styles.typeTop}>
              <span className={styles.typeBadge}>{card.icon}</span>
              <span className={styles.typePill}>{active ? "Opened" : "Open"}</span>
            </div>
            <h3 className={styles.typeTitle}>{card.title}</h3>
            <p className={styles.typeSubtitle}>{card.subtitle}</p>
            <div className={styles.typeList}>
              {card.points.map((point) => (
                <span key={point} className={styles.typeChip}>
                  {point}
                </span>
              ))}
            </div>
          </motion.button>
        );
      })}
    </div>
  );
}

function ReleaseControl({ relNow, setRelNow, relDate, setRelDate, color, title, text }) {
  return (
    <div className={styles.releasePanel} style={{ "--accent": color }}>
      <div className={styles.releaseIntro}>
        <span className={styles.sectionEyebrow}>Visibility</span>
        <h4 className={styles.releaseTitle}>{title}</h4>
        <p className={styles.releaseText}>{text}</p>
      </div>

      <div className={styles.releaseGrid}>
        <button
          type="button"
          className={`${styles.releaseCard} ${relNow ? styles.releaseCardActive : ""}`}
          onClick={() => setRelNow(true)}
        >
          <span className={styles.releaseIcon}>⚡</span>
          <span className={styles.releaseCardCopy}>
            <strong>Publish now</strong>
            <small>Students can access it immediately.</small>
          </span>
        </button>

        <button
          type="button"
          className={`${styles.releaseCard} ${!relNow ? styles.releaseCardActive : ""}`}
          onClick={() => setRelNow(false)}
        >
          <span className={styles.releaseIcon}>🗓️</span>
          <span className={styles.releaseCardCopy}>
            <strong>Schedule for later</strong>
            <small>Choose the day students can see it.</small>
          </span>
        </button>
      </div>

      <AnimatePresence>
        {!relNow && (
          <motion.div
            className={styles.releaseDateWrap}
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: "auto", opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.22 }}
          >
            <label className={styles.fieldLabel}>Release date</label>
            <input
              type="date"
              className={styles.input}
              value={relDate}
              onChange={(e) => setRelDate(e.target.value)}
              min={new Date().toISOString().split("T")[0]}
            />
            <p className={styles.fieldHint}>Students will not see this item before the selected date.</p>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

function SuccessState({ icon, title, text, buttonText, color, onReset }) {
  return (
    <motion.div
      className={styles.successShell}
      style={{ "--accent": color }}
      initial={{ opacity: 0, scale: 0.96, y: 14 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      transition={spring}
    >
      <div className={styles.successOrb}>{icon}</div>
      <span className={styles.sectionEyebrow}>Completed</span>
      <h3 className={styles.successTitle}>{title}</h3>
      <p className={styles.successText}>{text}</p>
      <button type="button" className={styles.primaryAction} onClick={onReset}>
        {buttonText}
      </button>
    </motion.div>
  );
}

function LectureForm({ courseId, color, onUploaded }) {
  const [title, setTitle] = useState("");
  const [week, setWeek] = useState("");
  const [desc, setDesc] = useState("");
  const [file, setFile] = useState(null);
  const [relDate, setRelDate] = useState("");
  const [relNow, setRelNow] = useState(true);
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);
  const [createdItem, setCreatedItem] = useState(null);

  const valid = title.trim() && file && (relNow || relDate);

  const reset = () => {
    setDone(false);
    setTitle("");
    setWeek("");
    setDesc("");
    setFile(null);
    setRelDate("");
    setRelNow(true);
    setCreatedItem(null);
  };

  const submit = async () => {
    if (!valid) return;
    setLoading(true);
    try {
      const weekNum = week ? parseInt(week.replace("Week ", ""), 10) : undefined;
      const created = await uploadLecture(
        courseId,
        {
          title,
          description: desc || undefined,
          week: weekNum,
          releaseDate: relNow ? null : relDate || null,
        },
        file,
      );
      setCreatedItem(created);
      setDone(true);
      onUploaded?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Upload failed. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  if (done) {
    return (
      <SuccessState
        icon="🎬"
        title="Lecture uploaded successfully"
        text={`${title} is ${relNow ? "visible to students now" : `scheduled for ${relDate}`}.`}
        buttonText="Upload another lecture"
        color={color}
        onReset={reset}
      />
    );
  }

  return (
    <div className={styles.formShell} style={{ "--accent": color }}>
      <div className={styles.formIntro}>
        <div>
          <span className={styles.sectionEyebrow}>Lecture setup</span>
          <h3 className={styles.formTitle}>Create a focused lecture post</h3>
          <p className={styles.formText}>
            Add the lecture details, upload the final file, and choose how students receive it.
          </p>
        </div>
        <div className={styles.metaRail}>
          <div className={styles.metaCard}>
            <strong>{file ? "1 file attached" : "File required"}</strong>
            <span>{relNow ? "Instant release" : "Scheduled release"}</span>
          </div>
          <div className={styles.metaCard}>
            <strong>{week || "No week selected"}</strong>
            <span>Week mapping</span>
          </div>
        </div>
      </div>

      <div className={styles.formGrid}>
        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>
            Lecture title <span className={styles.required}>*</span>
          </label>
          <input
            className={styles.input}
            placeholder="e.g. Deep Learning & CNNs"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
        </div>

        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>Week</label>
          <select className={styles.select} value={week} onChange={(e) => setWeek(e.target.value)}>
            <option value="">Select week…</option>
            {WEEKS.map((item) => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
        </div>

        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>Type</label>
          <select className={styles.select} defaultValue="Video Lecture">
            <option>Video Lecture</option>
            <option>PDF Slides</option>
            <option>Lab Session</option>
          </select>
        </div>

        <div className={`${styles.fieldBlock} ${styles.fieldBlockWide}`}>
          <label className={styles.fieldLabel}>Description</label>
          <textarea
            className={styles.textarea}
            rows={4}
            placeholder="Brief overview of what students will learn…"
            value={desc}
            onChange={(e) => setDesc(e.target.value)}
          />
        </div>

        <div className={`${styles.fieldBlock} ${styles.fieldBlockWide}`}>
          <FileZone
            accept=".mp4,.pdf,.pptx,.zip"
            onFile={setFile}
            file={file}
            color={color}
            label="Lecture file"
            helper="Use the final version students should access."
          />
        </div>
      </div>

      <ReleaseControl
        relNow={relNow}
        setRelNow={setRelNow}
        relDate={relDate}
        setRelDate={setRelDate}
        color={color}
        title="Choose when students can access this lecture"
        text="You can publish immediately or keep it hidden until the selected date."
      />

      <motion.button
        type="button"
        className={styles.primaryAction}
        disabled={!valid || loading}
        onClick={submit}
        whileHover={valid ? { y: -2 } : {}}
        whileTap={valid ? { scale: 0.99 } : {}}
      >
        {loading ? (
          <motion.span animate={{ rotate: 360 }} transition={{ duration: 0.8, repeat: Infinity, ease: "linear" }}>
            ⟳
          </motion.span>
        ) : (
          "Upload lecture"
        )}
      </motion.button>
    </div>
  );
}

function AssignmentForm({ courseCode, color }) {
  const [title, setTitle] = useState("");
  const [desc, setDesc] = useState("");
  const [deadline, setDeadline] = useState("");
  const [releaseDate, setReleaseDate] = useState("");
  const [relNow, setRelNow] = useState(true);
  const [maxPts, setMaxPts] = useState("20");
  const [file, setFile] = useState(null);
  const [allowFmt, setAllowFmt] = useState(["pdf"]);
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  const formats = ["pdf", "zip", "docx", "py", "cpp", "java", "mp4"];
  const toggleFmt = (format) => {
    setAllowFmt((prev) =>
      prev.includes(format) ? prev.filter((item) => item !== format) : [...prev, format],
    );
  };

  const valid = title.trim() && deadline && allowFmt.length > 0 && (relNow || releaseDate);

  const reset = () => {
    setDone(false);
    setTitle("");
    setDesc("");
    setDeadline("");
    setReleaseDate("");
    setRelNow(true);
    setMaxPts("20");
    setFile(null);
    setAllowFmt(["pdf"]);
  };

  const submit = async () => {
    if (!valid) return;
    setLoading(true);
    try {
      await createAssignment(
        {
          title,
          description: desc,
          courseCode,
          deadline: new Date(`${deadline}T23:59:00`).toISOString(),
          releaseDate: relNow
            ? null
            : releaseDate
              ? new Date(`${releaseDate}T00:00:00`).toISOString()
              : null,
          maxGrade: Number(maxPts),
          allowedFormats: allowFmt,
        },
        file,
      );
      setDone(true);
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Failed to create assignment");
    } finally {
      setLoading(false);
    }
  };

  if (done) {
    return (
      <SuccessState
        icon="📝"
        title="Assignment created successfully"
        text={`${title} will be ${relNow ? "available immediately" : `released on ${releaseDate}`}. Deadline: ${deadline}.`}
        buttonText="Create another assignment"
        color={color}
        onReset={reset}
      />
    );
  }

  return (
    <div className={styles.formShell} style={{ "--accent": color }}>
      <div className={styles.formIntro}>
        <div>
          <span className={styles.sectionEyebrow}>Assignment setup</span>
          <h3 className={styles.formTitle}>Prepare a clean student submission brief</h3>
          <p className={styles.formText}>
            Set the instructions, choose accepted formats, attach a starter file if needed, and control release timing.
          </p>
        </div>
        <div className={styles.metaRail}>
          <div className={styles.metaCard}>
            <strong>{deadline || "Deadline required"}</strong>
            <span>Final due date</span>
          </div>
          <div className={styles.metaCard}>
            <strong>{allowFmt.length} format(s)</strong>
            <span>Submission types</span>
          </div>
        </div>
      </div>

      <div className={styles.formGrid}>
        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>
            Assignment title <span className={styles.required}>*</span>
          </label>
          <input
            className={styles.input}
            placeholder="e.g. Neural Network from Scratch"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
        </div>

        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>
            Deadline <span className={styles.required}>*</span>
          </label>
          <input
            type="date"
            className={styles.input}
            value={deadline}
            onChange={(e) => setDeadline(e.target.value)}
            min={new Date().toISOString().split("T")[0]}
          />
        </div>

        <div className={styles.fieldBlock}>
          <label className={styles.fieldLabel}>Max grade (1–5)</label>
          <div className={styles.scoreRow}>
            {[1, 2, 3, 4, 5].map((item) => (
              <button
                key={item}
                type="button"
                className={`${styles.scoreButton} ${Number(maxPts) === item ? styles.scoreButtonActive : ""}`}
                onClick={() => setMaxPts(String(item))}
              >
                {item}
              </button>
            ))}
          </div>
        </div>

        <div className={`${styles.fieldBlock} ${styles.fieldBlockWide}`}>
          <label className={styles.fieldLabel}>Instructions</label>
          <textarea
            className={styles.textarea}
            rows={4}
            placeholder="Describe what students need to do…"
            value={desc}
            onChange={(e) => setDesc(e.target.value)}
          />
        </div>

        <div className={`${styles.fieldBlock} ${styles.fieldBlockWide}`}>
          <label className={styles.fieldLabel}>
            Allowed file formats <span className={styles.required}>*</span>
          </label>
          <div className={styles.formatRow}>
            {formats.map((format) => (
              <button
                key={format}
                type="button"
                className={`${styles.formatPill} ${allowFmt.includes(format) ? styles.formatPillActive : ""}`}
                onClick={() => toggleFmt(format)}
              >
                .{format}
              </button>
            ))}
          </div>
        </div>

        <div className={`${styles.fieldBlock} ${styles.fieldBlockWide}`}>
          <FileZone
            accept=".pdf,.zip,.docx"
            onFile={setFile}
            file={file}
            color={color}
            label="Starter attachment"
            helper="Optional: upload a template or reference file for students."
          />
        </div>
      </div>

      <ReleaseControl
        relNow={relNow}
        setRelNow={setRelNow}
        relDate={releaseDate}
        setRelDate={setReleaseDate}
        color={color}
        title="Choose when students can see this assignment"
        text="Assignments can appear instantly or stay pending until the selected release date."
      />

      <motion.button
        type="button"
        className={styles.primaryAction}
        disabled={!valid || loading}
        onClick={submit}
        whileHover={valid ? { y: -2 } : {}}
        whileTap={valid ? { scale: 0.99 } : {}}
      >
        {loading ? (
          <motion.span animate={{ rotate: 360 }} transition={{ duration: 0.8, repeat: Infinity, ease: "linear" }}>
            ⟳
          </motion.span>
        ) : (
          "Create assignment"
        )}
      </motion.button>
    </div>
  );
}

/* ── Lecture cards list ─────────────────────────────────────────── */
function LectureCard({ item, color, onEdit, onDelete }) {
  return (
    <motion.div
      layout
      initial={{ opacity: 0, y: 14 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, scale: 0.97 }}
      transition={{ ...spring }}
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
          padding: "3px 9px", borderRadius: 99, background: `${color}15`, color,
          fontSize: 11, fontWeight: 800, textTransform: "uppercase", letterSpacing: ".06em",
        }}>
          {item.type || "lecture"}
        </span>
        {item.week != null && (
          <span style={{
            padding: "3px 9px", borderRadius: 99, background: "var(--hover-bg)",
            fontSize: 11, fontWeight: 700, color: "var(--text-secondary)",
          }}>
            Week {item.week}
          </span>
        )}
        {item.releaseDate && (
          <span style={{
            padding: "3px 9px", borderRadius: 99, background: "var(--hover-bg)",
            fontSize: 11, fontWeight: 700, color: "var(--text-secondary)",
          }}>
            🗓 {item.releaseDate}
          </span>
        )}
      </div>

      <h4 style={{ margin: 0, fontSize: "1.05rem", fontWeight: 800, color: "var(--text-primary)" }}>
        {item.title}
      </h4>
      {item.courseCode && (
        <span style={{ fontSize: 12, fontWeight: 700, color }}>
          {item.courseCode}{item.courseName ? ` · ${item.courseName}` : ""}
        </span>
      )}
      {item.description && (
        <p style={{ margin: 0, fontSize: 13, lineHeight: 1.5, color: "var(--text-secondary)" }}>
          {item.description}
        </p>
      )}
      {item.url && (
        <a href={item.url} target="_blank" rel="noreferrer"
           style={{
             fontSize: 12, fontWeight: 700, color, textDecoration: "none",
             padding: "6px 10px", borderRadius: 8, background: `${color}10`,
             border: `1px solid ${color}30`, alignSelf: "flex-start",
           }}>
          📎 {item.fileName || "Open file"}
        </a>
      )}
      <div style={{ display: "flex", gap: 8, marginTop: "auto", paddingTop: 8 }}>
        <button
          type="button"
          onClick={() => onEdit(item)}
          style={{
            padding: "8px 14px", borderRadius: 10, cursor: "pointer",
            border: "1px solid var(--border)", background: "var(--hover-bg)",
            color: "var(--text-primary)", fontFamily: "inherit", fontSize: 12.5, fontWeight: 700,
          }}
        >
          ✎ Update
        </button>
        <button
          type="button"
          onClick={() => onDelete(item)}
          style={{
            padding: "8px 14px", borderRadius: 10, cursor: "pointer",
            border: "1.5px solid rgba(239,68,68,0.3)", background: "rgba(239,68,68,0.07)",
            color: "#ef4444", fontFamily: "inherit", fontSize: 12.5, fontWeight: 700,
          }}
        >
          🗑 Delete
        </button>
      </div>
    </motion.div>
  );
}

function ConfirmDeleteModal({ title, body, onConfirm, onClose, busy }) {
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
        }}
      >
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
              }}>
              Cancel
            </button>
            <button type="button" onClick={onConfirm} disabled={busy}
              style={{
                padding: "10px 16px", borderRadius: 11, cursor: "pointer", border: "none",
                background: "linear-gradient(135deg,#b91c1c,#ef4444)", color: "#fff",
                fontFamily: "inherit", fontSize: 13, fontWeight: 800,
              }}>
              {busy ? "Deleting…" : "🗑 Delete"}
            </button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

function EditLectureModal({ item, courseId, color, onSaved, onClose }) {
  const [title, setTitle] = useState(item?.title ?? "");
  const [desc, setDesc] = useState(item?.description ?? "");
  const [week, setWeek] = useState(item?.week != null ? `Week ${item.week}` : "");
  const [relDate, setRelDate] = useState(item?.releaseDate ?? "");
  const [file, setFile] = useState(null);
  const [busy, setBusy] = useState(false);
  const fileRef = useRef(null);

  const submit = async () => {
    if (!title.trim()) return;
    setBusy(true);
    try {
      const weekNum = week ? parseInt(week.replace("Week ", ""), 10) : undefined;
      await updateLecture(courseId, item.id, {
        title,
        description: desc,
        week: weekNum,
        releaseDate: relDate || undefined,
      }, file || null);
      onSaved?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Update failed");
    } finally {
      setBusy(false);
    }
  };

  return (
    <motion.div
      onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
      style={{
        position: "fixed", inset: 0, background: "rgba(0,0,0,0.55)", backdropFilter: "blur(8px)",
        display: "flex", alignItems: "center", justifyContent: "center", zIndex: 999, padding: 16, overflowY: "auto",
      }}
    >
      <motion.div
        onClick={(e) => e.stopPropagation()}
        initial={{ scale: 0.94, y: 18 }} animate={{ scale: 1, y: 0 }} exit={{ scale: 0.96 }}
        transition={spring}
        style={{
          width: "min(560px, 100%)", background: "var(--card-bg)",
          borderRadius: 20, overflow: "hidden",
          border: "1px solid var(--border)", boxShadow: "0 24px 60px rgba(0,0,0,0.32)",
        }}
      >
        <div style={{ height: 4, background: color }} />
        <div style={{ padding: 22, display: "flex", flexDirection: "column", gap: 14 }}>
          <h3 style={{ margin: 0, fontSize: "1.15rem", fontWeight: 800 }}>Edit lecture</h3>

          <div>
            <label className={styles.fieldLabel}>Title</label>
            <input className={styles.input} value={title} onChange={(e) => setTitle(e.target.value)} />
          </div>
          <div>
            <label className={styles.fieldLabel}>Description</label>
            <textarea className={styles.textarea} rows={3} value={desc} onChange={(e) => setDesc(e.target.value)} />
          </div>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div>
              <label className={styles.fieldLabel}>Week</label>
              <select className={styles.select} value={week} onChange={(e) => setWeek(e.target.value)}>
                <option value="">No week</option>
                {WEEKS.map((w) => <option key={w} value={w}>{w}</option>)}
              </select>
            </div>
            <div>
              <label className={styles.fieldLabel}>Release date</label>
              <input type="date" className={styles.input} value={relDate} onChange={(e) => setRelDate(e.target.value)} />
            </div>
          </div>

          <div>
            <label className={styles.fieldLabel}>Replace file (optional)</label>
            <input ref={fileRef} type="file" style={{ display: "none" }}
                   onChange={(e) => setFile(e.target.files[0] || null)} />
            <button
              type="button"
              onClick={() => fileRef.current?.click()}
              style={{
                width: "100%", textAlign: "left", padding: "11px 14px", borderRadius: 12,
                border: `1.5px solid ${file ? `${color}60` : "var(--border)"}`,
                background: "var(--hover-bg)", cursor: "pointer", fontFamily: "inherit",
                fontSize: 13, color: file ? "var(--text-primary)" : "var(--text-muted)",
              }}>
              {file ? `📎 ${file.name}` : item?.fileName ? `Keep current: ${item.fileName}` : "📎 Choose a new file…"}
            </button>
          </div>

          <div style={{ display: "flex", gap: 10, justifyContent: "flex-end", paddingTop: 4 }}>
            <button type="button" onClick={onClose} disabled={busy}
              style={{
                padding: "10px 16px", borderRadius: 11, cursor: "pointer",
                border: "1px solid var(--border)", background: "var(--hover-bg)",
                color: "var(--text-primary)", fontFamily: "inherit", fontSize: 13, fontWeight: 700,
              }}>
              Cancel
            </button>
            <button type="button" onClick={submit} disabled={busy || !title.trim()}
              style={{
                padding: "10px 18px", borderRadius: 11, cursor: "pointer", border: "none",
                background: color, color: "#fff", fontFamily: "inherit", fontSize: 13, fontWeight: 800,
                opacity: title.trim() ? 1 : 0.5,
              }}>
              {busy ? "Saving…" : "Save changes"}
            </button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

function LectureList({ courseId, color, refreshKey, onChanged }) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [deleting, setDeleting] = useState(null);
  const [busy, setBusy] = useState(false);

  const reload = useCallback(async () => {
    if (!courseId) return;
    setLoading(true);
    try {
      const list = await getCourseMaterials(courseId);
      setItems(Array.isArray(list) ? list : []);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [courseId]);

  useEffect(() => { reload(); }, [reload, refreshKey]);

  const handleDelete = async () => {
    if (!deleting) return;
    setBusy(true);
    try {
      await deleteLecture(courseId, deleting.id);
      setItems((prev) => prev.filter((m) => m.id !== deleting.id));
      setDeleting(null);
      onChanged?.();
    } catch (e) {
      alert(e?.response?.data?.error?.message || "Delete failed");
    } finally { setBusy(false); }
  };

  return (
    <section className={styles.surface} style={{ "--accent": color }}>
      <div className={styles.surfaceHead}>
        <div>
          <span className={styles.sectionEyebrow}>Library</span>
          <h2 className={styles.sectionTitle}>Existing lectures</h2>
          <p className={styles.sectionText}>
            All lectures you have published in this course. Update or delete each item individually.
          </p>
        </div>
      </div>

      {loading ? (
        <div style={{ padding: 32, textAlign: "center", color: "var(--text-muted)" }}>Loading…</div>
      ) : items.length === 0 ? (
        <div style={{ padding: 24, textAlign: "center", color: "var(--text-muted)", fontSize: 13.5 }}>
          No lectures yet. Use the form above to publish your first one.
        </div>
      ) : (
        <div style={{
          display: "grid", gap: 14,
          gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))",
        }}>
          <AnimatePresence>
            {items.map((m) => (
              <LectureCard key={m.id} item={m} color={color}
                onEdit={setEditing} onDelete={setDeleting} />
            ))}
          </AnimatePresence>
        </div>
      )}

      <AnimatePresence>
        {editing && (
          <EditLectureModal
            item={editing}
            courseId={courseId}
            color={color}
            onClose={() => setEditing(null)}
            onSaved={() => { setEditing(null); reload(); onChanged?.(); }}
          />
        )}
        {deleting && (
          <ConfirmDeleteModal
            title="Delete lecture?"
            body={`"${deleting.title}" and its uploaded file will be permanently removed and cannot be recovered.`}
            onClose={() => setDeleting(null)}
            onConfirm={handleDelete}
            busy={busy}
          />
        )}
      </AnimatePresence>
    </section>
  );
}

export default function UploadMaterialPage() {
  const [courses, setCourses] = useState([]);
  const [course, setCourse] = useState(null);
  const [matType, setMatType] = useState(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    getInstructorCourses()
      .then((data) => {
        const list = Array.isArray(data) ? data : [];
        setCourses(list);
      })
      .catch(() => {});
  }, []);

  const currentCourse = useMemo(
    () =>
      (course ? courses.find((item) => item.id === course) : null) || {
        color: "#7c3aed",
        code: "",
        name: "",
        icon: "📚",
      },
    [course, courses],
  );

  const activeAccent = matType === "lecture" ? "#5a67d8" : currentCourse.color || "#7c3aed";

  return (
    <div className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.heroAuraLeft} />
        <div className={styles.heroAuraRight} />
        <div className={styles.heroGrid}>
          <div className={styles.heroCopy}>
            <span className={styles.sectionEyebrow}>Instructor workspace</span>
            <h1 className={styles.heroTitle}>Material Publishing Studio</h1>
            <p className={styles.heroText}>
              A calmer, academic workflow for organizing lectures and assignments with better rhythm,
              clearer structure, and polished release control.
            </p>
          </div>

          <div className={styles.heroSummary}>
            <div className={styles.heroChip}>
              <strong>{courses.length}</strong>
              <span>Courses available</span>
            </div>
            <div className={styles.heroChip}>
              <strong>{matType || "idle"}</strong>
              <span>Current mode</span>
            </div>
            <div className={styles.heroChip}>
              <strong>{course ? currentCourse.code : "none"}</strong>
              <span>Selected course</span>
            </div>
          </div>
        </div>
      </section>

      <main className={styles.shell}>
        <section className={styles.surface}>
          <div className={styles.surfaceHead}>
            <div>
              <span className={styles.sectionEyebrow}>Step 1</span>
              <h2 className={styles.sectionTitle}>Choose your course</h2>
              <p className={styles.sectionText}>
                Start by selecting the course, then move to lecture upload or assignment creation.
              </p>
            </div>
            {course && (
              <div className={styles.currentCourseTag} style={{ "--accent": currentCourse.color || "#7c3aed" }}>
                <span>{currentCourse.icon}</span>
                <strong>{currentCourse.code}</strong>
                <small>{currentCourse.name}</small>
              </div>
            )}
          </div>

          <CoursePicker
            courses={courses}
            course={course}
            setCourse={(value) => {
              setCourse(value);
              setMatType(null);
            }}
          />
        </section>

        {!course && courses.length > 0 && (
          <motion.section
            className={styles.emptyState}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
          >
            <div className={styles.emptyIcon}>📚</div>
            <h3 className={styles.emptyTitle}>Select a course to continue</h3>
            <p className={styles.emptyText}>
              Once a course is selected, you can switch between lecture uploads and assignment creation.
            </p>
          </motion.section>
        )}

        {course && (
          <>
            <section className={styles.surface}>
              <div className={styles.surfaceHead}>
                <div>
                  <span className={styles.sectionEyebrow}>Step 2</span>
                  <h2 className={styles.sectionTitle}>Choose the material type</h2>
                  <p className={styles.sectionText}>
                    You are working inside <strong>{currentCourse.code}</strong> · {currentCourse.name}.
                  </p>
                </div>
              </div>

              <TypeSelector matType={matType} setMatType={setMatType} accent={currentCourse.color || "#7c3aed"} />
            </section>

            <AnimatePresence mode="wait">
              {matType ? (
                <motion.section
                  key={`${course}-${matType}`}
                  className={styles.formPanel}
                  initial={{ opacity: 0, y: 18 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -12 }}
                  transition={{ duration: 0.24 }}
                  style={{ "--accent": activeAccent }}
                >
                  <div className={styles.formPanelHead}>
                    <div>
                      <span className={styles.sectionEyebrow}>
                        {matType === "lecture" ? "Lecture mode" : "Assignment mode"}
                      </span>
                      <h2 className={styles.formPanelTitle}>
                        {matType === "lecture" ? "Publish a new lecture" : "Create a new assignment"}
                      </h2>
                      <p className={styles.formPanelText}>
                        {currentCourse.icon} {currentCourse.code} · {currentCourse.name}
                      </p>
                    </div>

                    <button type="button" className={styles.ghostAction} onClick={() => setMatType(null)}>
                      Close panel
                    </button>
                  </div>

                  {matType === "lecture" ? (
                    <LectureForm courseId={course} color="#5a67d8" onUploaded={() => setRefreshKey((k) => k + 1)} />
                  ) : (
                    <AssignmentForm courseCode={currentCourse.code} color={currentCourse.color || "#7c3aed"} />
                  )}
                </motion.section>
              ) : (
                <motion.section
                  key={`${course}-idle`}
                  className={styles.idleState}
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                >
                  <div className={styles.idleBadge} style={{ "--accent": currentCourse.color || "#7c3aed" }}>
                    {currentCourse.icon}
                  </div>
                  <h3 className={styles.emptyTitle}>Choose a material type</h3>
                  <p className={styles.emptyText}>
                    Use the cards above to start a lecture upload or create an assignment for {currentCourse.code}.
                  </p>
                </motion.section>
              )}
            </AnimatePresence>

            {matType === "lecture" && (
              <LectureList
                courseId={course}
                color="#5a67d8"
                refreshKey={refreshKey}
                onChanged={() => setRefreshKey((k) => k + 1)}
              />
            )}
          </>
        )}
      </main>
    </div>
  );
}
