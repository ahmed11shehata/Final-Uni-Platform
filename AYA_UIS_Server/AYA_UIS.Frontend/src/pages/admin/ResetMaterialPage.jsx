// src/pages/admin/ResetMaterialPage.jsx
//
// Admin "Reset Material" page.
// Distinct from the existing Academic Year Reset page (year/term reset).
// Lets the admin select courses, preview impact, and execute the reset
// after a server-side password confirmation (validated only on the backend).

import { useEffect, useMemo, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import {
  materialResetCourses,
  materialResetPreview,
  materialResetExecute,
} from "../../services/api/adminApi";

const sp = { type: "spring", stiffness: 360, damping: 28 };

/* ── Lightweight inline styles — match existing admin dark theme ── */
const S = {
  page:    { padding: "24px 32px", fontFamily: "Sora, sans-serif", color: "var(--text-primary)" },
  hero:    {
    padding: "20px 24px",
    borderRadius: 18,
    background: "linear-gradient(135deg, rgba(239,68,68,.18), rgba(245,158,11,.10))",
    border: "1px solid rgba(239,68,68,.30)",
    marginBottom: 22,
  },
  heroTitle: { fontSize: "1.45rem", fontWeight: 900, margin: "0 0 6px", letterSpacing: "-.02em" },
  heroSub:   { fontSize: ".88rem", color: "var(--text-secondary)", margin: 0, lineHeight: 1.5 },
  card:    {
    background: "var(--card-bg)", border: "1px solid var(--border)",
    borderRadius: 16, padding: 18, marginBottom: 16,
  },
  cardTitle: { fontSize: ".94rem", fontWeight: 800, margin: "0 0 10px", color: "var(--text-primary)" },
  rowBetween: { display: "flex", alignItems: "center", justifyContent: "space-between", gap: 12, flexWrap: "wrap" },
  btn:     {
    padding: "10px 18px", borderRadius: 11, border: "1px solid var(--border)",
    background: "var(--hover-bg)", color: "var(--text-primary)",
    fontFamily: "inherit", fontSize: 13, fontWeight: 700, cursor: "pointer",
  },
  btnPrimary: {
    padding: "10px 18px", borderRadius: 11, border: "none",
    background: "linear-gradient(135deg,#4338ca,#818cf8)", color: "#fff",
    fontFamily: "inherit", fontSize: 13, fontWeight: 800, cursor: "pointer",
  },
  btnDanger: {
    padding: "10px 18px", borderRadius: 11, border: "none",
    background: "linear-gradient(135deg,#b91c1c,#ef4444)", color: "#fff",
    fontFamily: "inherit", fontSize: 13, fontWeight: 800, cursor: "pointer",
  },
  input:   {
    width: "100%", boxSizing: "border-box",
    padding: "10px 12px", borderRadius: 11, border: "1.5px solid var(--border)",
    background: "var(--card-bg)", color: "var(--text-primary)",
    fontFamily: "inherit", fontSize: 13.5, outline: "none",
  },
  pill:    {
    padding: "5px 11px", borderRadius: 99, fontSize: 11.5, fontWeight: 800,
    background: "rgba(129,140,248,.10)", color: "#818cf8",
    border: "1px solid rgba(129,140,248,.28)",
  },
  metric:  {
    padding: "12px 14px", borderRadius: 12,
    background: "var(--hover-bg)", border: "1px solid var(--border)",
  },
  metricVal: { fontSize: "1.5rem", fontWeight: 900, color: "var(--text-primary)", lineHeight: 1 },
  metricLbl: { fontSize: 11, fontWeight: 700, textTransform: "uppercase", letterSpacing: ".07em", color: "var(--text-muted)", marginTop: 4 },
  blocked: {
    background: "rgba(239,68,68,.07)", border: "1px solid rgba(239,68,68,.30)",
    borderRadius: 14, padding: 16, marginBottom: 16,
  },
  blockedTitle: { fontSize: ".95rem", fontWeight: 800, color: "#ef4444", margin: "0 0 8px" },
  blockedRow: {
    padding: "10px 12px", borderRadius: 10, background: "rgba(0,0,0,0.04)",
    border: "1px solid var(--border)", marginBottom: 8,
  },
};

function CourseTile({ c, selected, onToggle, color }) {
  const hasMaterial = !!c.hasMaterial;
  const aCount = c.assignmentCount ?? 0;
  const qCount = c.quizCount ?? 0;
  const lCount = c.lectureCount ?? 0;
  const badge = (label, n, bg, fg) => (
    <span style={{
      padding: "2px 7px", borderRadius: 99, fontSize: 10.5, fontWeight: 800,
      background: bg, color: fg, lineHeight: 1.4,
    }}>{label} {n}</span>
  );
  return (
    <motion.button
      type="button"
      onClick={() => onToggle(c.id)}
      whileHover={{ y: -2 }}
      whileTap={{ scale: 0.97 }}
      style={{
        position: "relative",
        textAlign: "left",
        padding: "12px 14px",
        borderRadius: 14,
        border: selected ? `1.5px solid ${color}` : "1.5px solid var(--border)",
        background: selected ? `${color}10` : "var(--card-bg)",
        cursor: "pointer", fontFamily: "inherit",
        display: "flex", flexDirection: "column", gap: 6,
      }}
    >
      <span style={{
        fontSize: 12, fontWeight: 800, color: selected ? color : "var(--text-secondary)",
      }}>{c.code}</span>
      <span style={{ fontSize: 13, fontWeight: 700, color: "var(--text-primary)", lineHeight: 1.3 }}>
        {c.name}
      </span>

      <div style={{ display: "flex", gap: 4, flexWrap: "wrap", marginTop: 2 }}>
        {hasMaterial ? (
          <>
            {aCount > 0 && badge("asn",  aCount, "rgba(129,140,248,.10)", "#818cf8")}
            {qCount > 0 && badge("quiz", qCount, "rgba(245,158,11,.12)",  "#f59e0b")}
            {lCount > 0 && badge("lec",  lCount, "rgba(34,197,94,.12)",   "#22c55e")}
          </>
        ) : (
          <span style={{
            padding: "2px 7px", borderRadius: 99, fontSize: 10.5, fontWeight: 800,
            background: "rgba(148,163,184,.10)", color: "var(--text-muted)",
            border: "1px dashed var(--border)",
          }}>No material yet</span>
        )}
      </div>

      {selected && (
        <span style={{
          position: "absolute", top: 8, right: 10,
          width: 18, height: 18, borderRadius: 99,
          background: color, color: "#fff", fontSize: 11, fontWeight: 800,
          display: "flex", alignItems: "center", justifyContent: "center",
        }}>✓</span>
      )}
    </motion.button>
  );
}

function PasswordModal({ onConfirm, onClose, busy, errorMessage, onClearError }) {
  const [pwd, setPwd] = useState("");
  const [localError, setLocalError] = useState("");

  const isEmpty = pwd.trim().length === 0;
  const displayError = localError || errorMessage || "";

  const handleChange = (e) => {
    setPwd(e.target.value);
    if (localError) setLocalError("");
    if (errorMessage && onClearError) onClearError();
  };

  const handleSubmit = () => {
    if (isEmpty) {
      setLocalError("Password is required.");
      return;
    }
    setLocalError("");
    onConfirm(pwd);
  };

  return (
    <motion.div
      onClick={onClose}
      initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
      style={{
        position: "fixed", inset: 0, background: "rgba(0,0,0,0.55)", backdropFilter: "blur(8px)",
        display: "flex", alignItems: "center", justifyContent: "center", zIndex: 999, padding: 16,
      }}>
      <motion.div
        onClick={(e) => e.stopPropagation()}
        initial={{ scale: 0.92, y: 18 }} animate={{ scale: 1, y: 0 }} exit={{ scale: 0.95 }}
        transition={sp}
        style={{
          width: "min(480px, 100%)", background: "var(--card-bg)",
          borderRadius: 18, overflow: "hidden",
          border: "1px solid var(--border)", boxShadow: "0 24px 60px rgba(0,0,0,0.32)",
        }}>
        <div style={{ height: 4, background: "linear-gradient(135deg,#b91c1c,#ef4444)" }} />
        <div style={{ padding: 22 }}>
          <h3 style={{ margin: "0 0 8px", fontSize: "1.1rem", fontWeight: 800 }}>Confirm Reset Material</h3>
          <p style={{ margin: "0 0 12px", fontSize: 13.5, color: "var(--text-secondary)", lineHeight: 1.5 }}>
            This action will <strong>archive</strong> assignments and quizzes (DB rows preserved for grade
            integrity) and <strong>permanently delete</strong> lecture rows + all physical files for the
            selected courses. Type the reset password to continue.
          </p>
          <form autoComplete="off" onSubmit={(e) => { e.preventDefault(); handleSubmit(); }}>
            {/* Hidden honeypot field discourages browser autofill from targeting the real input */}
            <input
              type="password"
              name="fake-password-autofill-trap"
              autoComplete="new-password"
              tabIndex={-1}
              aria-hidden="true"
              style={{ position: "absolute", opacity: 0, height: 0, width: 0, pointerEvents: "none" }}
              readOnly
            />
            <input
              type="password"
              name="material_reset_confirm_token"
              autoComplete="new-password"
              autoCorrect="off"
              autoCapitalize="off"
              spellCheck={false}
              autoFocus
              placeholder="Reset password"
              value={pwd}
              onChange={handleChange}
              style={S.input}
            />
          </form>
          {displayError && (
            <div style={{
              marginTop: 10, padding: "8px 10px",
              borderRadius: 9, background: "rgba(239,68,68,.08)",
              border: "1px solid rgba(239,68,68,.30)", color: "#ef4444",
              fontSize: 12.5, fontWeight: 700,
            }}>
              ⚠ {displayError}
            </div>
          )}
          <div style={{ display: "flex", gap: 10, justifyContent: "flex-end", marginTop: 16 }}>
            <button type="button" onClick={onClose} disabled={busy} style={S.btn}>Cancel</button>
            <button
              type="button"
              onClick={handleSubmit}
              disabled={busy || isEmpty}
              style={{ ...S.btnDanger, opacity: !busy && !isEmpty ? 1 : 0.5 }}
            >
              {busy ? "Resetting…" : "🧹 Reset Material"}
            </button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

export default function ResetMaterialPage() {
  const [courses, setCourses] = useState([]);
  const [loadingCourses, setLoadingCourses] = useState(true);
  const [selected, setSelected] = useState(new Set());
  const [search, setSearch] = useState("");

  const [preview, setPreview] = useState(null);
  const [previewing, setPreviewing] = useState(false);
  const [showPwd, setShowPwd] = useState(false);
  const [executing, setExecuting] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);
  const [pwdError, setPwdError] = useState("");

  const openPwdModal = () => {
    setPwdError("");
    setError(null);
    setShowPwd(true);
  };

  const closePwdModal = () => {
    if (executing) return;
    setPwdError("");
    setShowPwd(false);
  };

  const accent = "#818cf8";

  useEffect(() => {
    setLoadingCourses(true);
    materialResetCourses()
      .then((rows) => {
        // Backend already returns id + per-course counts. Keep every row from the
        // catalog — even courses with no material remain selectable (they no-op
        // safely on execute).
        setCourses(Array.isArray(rows) ? rows : []);
      })
      .catch(() => setCourses([]))
      .finally(() => setLoadingCourses(false));
  }, []);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return courses;
    return courses.filter((c) =>
      (c.code || "").toLowerCase().includes(q) || (c.name || "").toLowerCase().includes(q));
  }, [courses, search]);

  const allSelected = courses.length > 0 && selected.size === courses.length;

  const toggle = (id) => {
    setPreview(null);
    setResult(null);
    setError(null);
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  };

  const selectAll = () => {
    setPreview(null); setResult(null); setError(null);
    setSelected(allSelected ? new Set() : new Set(courses.map((c) => c.id)));
  };

  const runPreview = async () => {
    setError(null);
    setResult(null);
    setPreviewing(true);
    try {
      const data = await materialResetPreview({
        courseIds: Array.from(selected),
        selectAll: allSelected,
      });
      setPreview(data);
    } catch (e) {
      setError(e?.response?.data?.error?.message || "Preview failed");
    } finally {
      setPreviewing(false);
    }
  };

  const runExecute = async (pwd) => {
    setError(null);
    setPwdError("");
    setExecuting(true);
    try {
      const data = await materialResetExecute({
        courseIds: Array.from(selected),
        selectAll: allSelected,
        password: pwd,
      });
      setResult(data);
      setPreview(null);
      setPwdError("");
      setShowPwd(false);
    } catch (e) {
      const code = e?.response?.data?.error?.code;
      if (code === "INVALID_PASSWORD") {
        // Keep modal open; do not leak any hint about the real value.
        setPwdError("Incorrect reset password.");
      } else {
        setError(e?.response?.data?.error?.message || "Reset failed");
        setShowPwd(false);
      }
    } finally {
      setExecuting(false);
    }
  };

  const blocked = preview?.blocked === true;

  return (
    <div style={S.page}>
      {/* Hero */}
      <div style={S.hero}>
        <h1 style={S.heroTitle}>🧹 Reset Material</h1>
        <p style={S.heroSub}>
          Select one or more courses and clean their old material. Assignments and quizzes are
          archived (DB rows preserved); lecture rows and all physical files are permanently
          deleted. Pending submissions block execution until instructors have reviewed them.
        </p>
      </div>

      {/* Course selection */}
      <div style={S.card}>
        <div style={S.rowBetween}>
          <h2 style={S.cardTitle}>1. Select courses</h2>
          <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
            <input
              type="text"
              placeholder="Search code or name…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{ ...S.input, width: 240 }}
            />
            <button type="button" onClick={selectAll} disabled={loadingCourses || courses.length === 0} style={S.btn}>
              {allSelected ? "Clear all" : "Select all"}
            </button>
          </div>
        </div>

        {loadingCourses ? (
          <div style={{ padding: 32, textAlign: "center", color: "var(--text-muted)" }}>Loading courses…</div>
        ) : courses.length === 0 ? (
          <div style={{ padding: 24, textAlign: "center", color: "var(--text-muted)" }}>No courses available.</div>
        ) : (
          <div style={{
            marginTop: 12,
            display: "grid", gap: 10,
            gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
            maxHeight: 360, overflowY: "auto", paddingRight: 4,
          }}>
            {filtered.map((c) => (
              <CourseTile key={c.id} c={c} selected={selected.has(c.id)} onToggle={toggle} color={accent} />
            ))}
            {filtered.length === 0 && (
              <div style={{ padding: 16, color: "var(--text-muted)", fontSize: 13 }}>
                No courses match "{search}".
              </div>
            )}
          </div>
        )}
        <div style={{ marginTop: 14, fontSize: 12.5, color: "var(--text-secondary)" }}>
          <strong>{selected.size}</strong> course{selected.size !== 1 ? "s" : ""} selected
          {allSelected && " (all)"}
        </div>
      </div>

      {/* Actions */}
      <div style={S.card}>
        <div style={S.rowBetween}>
          <h2 style={S.cardTitle}>2. Preview impact</h2>
          <div style={{ display: "flex", gap: 10 }}>
            <button
              type="button"
              onClick={runPreview}
              disabled={previewing || (selected.size === 0 && !allSelected)}
              style={{ ...S.btnPrimary, opacity: (selected.size === 0 && !allSelected) || previewing ? 0.55 : 1 }}
            >
              {previewing ? "Previewing…" : "🔎 Run Preview"}
            </button>
            <button
              type="button"
              onClick={openPwdModal}
              disabled={!preview || blocked || executing}
              style={{ ...S.btnDanger, opacity: !preview || blocked || executing ? 0.55 : 1 }}
            >
              🧹 Execute Reset
            </button>
          </div>
        </div>

        {error && (
          <div style={{
            marginTop: 12, padding: "10px 12px",
            borderRadius: 10, background: "rgba(239,68,68,.07)",
            border: "1px solid rgba(239,68,68,.30)", color: "#ef4444",
            fontSize: 13, fontWeight: 700,
          }}>
            ⚠ {error}
          </div>
        )}
      </div>

      {/* Preview output */}
      <AnimatePresence>
        {preview && (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
          >
            {blocked && (
              <div style={S.blocked}>
                <h3 style={S.blockedTitle}>⛔ {preview.blockReason}</h3>
                {preview.pendingSubmissions.map((p) => (
                  <div key={`${p.assignmentId}`} style={S.blockedRow}>
                    <div style={{ fontSize: 13, fontWeight: 800 }}>
                      {p.courseCode} — {p.assignmentTitle}
                    </div>
                    <div style={{ fontSize: 12, color: "var(--text-secondary)", marginTop: 3 }}>
                      <strong>{p.pendingCount}</strong> pending submission{p.pendingCount !== 1 ? "s" : ""}
                      {p.studentNames?.length > 0 && (
                        <> · {p.studentNames.slice(0, 5).join(", ")}{p.studentNames.length > 5 ? "…" : ""}</>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}

            <div style={S.card}>
              <h2 style={S.cardTitle}>Impact preview</h2>
              <div style={{
                display: "grid", gap: 10, marginTop: 8,
                gridTemplateColumns: "repeat(auto-fit, minmax(140px, 1fr))",
              }}>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.selectedCourseCount}</div>
                  <div style={S.metricLbl}>Selected courses</div>
                </div>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.totals.assignments}</div>
                  <div style={S.metricLbl}>Assignments</div>
                </div>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.totals.quizzes}</div>
                  <div style={S.metricLbl}>Quizzes</div>
                </div>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.totals.lectures}</div>
                  <div style={S.metricLbl}>Lectures</div>
                </div>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.totals.pendingSubmissions}</div>
                  <div style={S.metricLbl}>Pending subs</div>
                </div>
                <div style={S.metric}>
                  <div style={S.metricVal}>{preview.totals.instructorsAffected}</div>
                  <div style={S.metricLbl}>Instructors</div>
                </div>
              </div>

              {preview.perCourse.length > 0 && (
                <div style={{ marginTop: 14 }}>
                  <h3 style={{ ...S.cardTitle, fontSize: ".82rem", textTransform: "uppercase", color: "var(--text-muted)", letterSpacing: ".06em" }}>
                    Per course
                  </h3>
                  <div style={{ display: "grid", gap: 6 }}>
                    {preview.perCourse.map((c) => (
                      <div key={c.courseId} style={{
                        display: "grid",
                        gridTemplateColumns: "1.4fr repeat(4, 1fr) auto",
                        gap: 10, alignItems: "center",
                        padding: "8px 12px", borderRadius: 10,
                        background: c.hasMaterial ? "var(--hover-bg)" : "transparent",
                        border: "1px solid var(--border)",
                        fontSize: 12.5,
                      }}>
                        <span style={{ fontWeight: 700 }}>
                          <span style={{ color: accent, fontWeight: 800 }}>{c.courseCode}</span> — {c.courseName}
                        </span>
                        <span><span style={{ color: "var(--text-muted)" }}>asn</span> {c.assignments}</span>
                        <span><span style={{ color: "var(--text-muted)" }}>quiz</span> {c.quizzes}</span>
                        <span><span style={{ color: "var(--text-muted)" }}>lec</span> {c.lectures}</span>
                        <span><span style={{ color: "var(--text-muted)" }}>pend</span> {c.pendingSubmissions}</span>
                        {!c.hasMaterial && (
                          <span style={S.pill}>nothing to clean</span>
                        )}
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Result after execute */}
      <AnimatePresence>
        {result && (
          <motion.div
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            style={{
              ...S.card,
              borderColor: "rgba(34,197,94,.4)",
              background: "rgba(34,197,94,.06)",
            }}
          >
            <h2 style={{ ...S.cardTitle, color: "#22c55e" }}>✅ Reset complete (batch #{result.batchId})</h2>
            <div style={{
              display: "grid", gap: 10, marginTop: 8,
              gridTemplateColumns: "repeat(auto-fit, minmax(140px, 1fr))",
            }}>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.assignmentsArchived}</div>
                <div style={S.metricLbl}>Assignments archived</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.assignmentFilesPurged}</div>
                <div style={S.metricLbl}>Assignment files</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.submissionFilesPurged}</div>
                <div style={S.metricLbl}>Submission files</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.quizzesArchived}</div>
                <div style={S.metricLbl}>Quizzes archived</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.lecturesDeleted}</div>
                <div style={S.metricLbl}>Lectures deleted</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.lectureFilesDeleted}</div>
                <div style={S.metricLbl}>Lecture files</div>
              </div>
              <div style={S.metric}>
                <div style={S.metricVal}>{result.counts.instructorsNotified}</div>
                <div style={S.metricLbl}>Instructors notified</div>
              </div>
            </div>
            {result.warnings?.length > 0 && (
              <div style={{ marginTop: 12, fontSize: 12.5, color: "var(--text-secondary)" }}>
                {result.warnings.map((w, i) => (
                  <div key={i}>• {w}</div>
                ))}
              </div>
            )}
          </motion.div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {showPwd && (
          <PasswordModal
            busy={executing}
            errorMessage={pwdError}
            onClearError={() => setPwdError("")}
            onClose={closePwdModal}
            onConfirm={runExecute}
          />
        )}
      </AnimatePresence>
    </div>
  );
}
