// src/pages/admin/InstructorControlPage.jsx
import { useState, useEffect, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { getInstructorControl, assignInstructors } from "../../services/api/adminApi";
import styles from "./InstructorControlPage.module.css";

const YEAR_COLORS = ["", "#818cf8", "#22c55e", "#f59e0b", "#ef4444"];
const YEAR_LABELS = ["", "Year 1", "Year 2", "Year 3", "Year 4"];

function stableColor(code) {
  let h = 0;
  for (let i = 0; i < code.length; i++) h = (h * 31 + code.charCodeAt(i)) >>> 0;
  return YEAR_COLORS[1 + (h % 4)];
}

function InstructorAvatar({ name, size = 28 }) {
  const letter = (name || "?")[0].toUpperCase();
  const color = stableColor(name || "?");
  return (
    <span
      className={styles.iAvatar}
      style={{
        width: size,
        height: size,
        fontSize: size * 0.44,
        background: `${color}22`,
        color,
        border: `1.5px solid ${color}40`,
      }}
    >
      {letter}
    </span>
  );
}

function AssignModal({ course, allInstructors, onSave, onClose }) {
  const [selected, setSelected] = useState(() => new Set(course.assignedInstructors.map((i) => i.id)));
  const [search, setSearch] = useState("");
  const [saving, setSaving] = useState(false);

  const filtered = allInstructors.filter((i) =>
    i.name.toLowerCase().includes(search.toLowerCase()) ||
    i.email.toLowerCase().includes(search.toLowerCase()) ||
    i.code.toLowerCase().includes(search.toLowerCase())
  );

  const toggle = (id) =>
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  const handleSave = async () => {
    setSaving(true);
    try {
      await onSave(course.id, [...selected]);
    } finally {
      setSaving(false);
    }
  };

  const color = YEAR_COLORS[course.year] || "#818cf8";

  return (
    <div className={styles.overlay} onClick={(e) => e.target === e.currentTarget && onClose()}>
      <motion.div
        className={styles.modal}
        initial={{ opacity: 0, scale: 0.92, y: 24 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.95, y: 18 }}
        transition={{ type: "spring", stiffness: 380, damping: 28 }}
      >
        <div className={styles.modalTopGlow} style={{ background: `radial-gradient(circle, ${color}30 0%, transparent 70%)` }} />

        <div className={styles.modalHeader} style={{ borderBottomColor: `${color}24` }}>
          <div className={styles.modalHeaderLeft}>
            <span className={styles.modalCode} style={{ color, background: `${color}16`, borderColor: `${color}30` }}>
              {course.code}
            </span>
            <h2 className={styles.modalTitle}>{course.name}</h2>
            <div className={styles.modalMetaRow}>
              <span className={styles.modalMetaChip}>{YEAR_LABELS[course.year] || `Year ${course.year}`}</span>
              <span className={styles.modalMetaChip}>{course.credits} Credits</span>
              <span className={styles.modalMetaChip}>
                {course.assignedInstructors.length === 0 ? "No instructors assigned" : `${course.assignedInstructors.length} assigned`}
              </span>
            </div>
          </div>

          <button className={styles.modalClose} onClick={onClose} aria-label="Close popup">
            ✕
          </button>
        </div>

        <div className={styles.modalToolbar}>
          <div className={styles.modalSearch}>
            <svg className={styles.searchIco} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round">
              <circle cx="11" cy="11" r="8" />
              <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
            <input
              className={styles.searchInput}
              placeholder="Search instructors by name, email, or code…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              autoFocus
            />
          </div>

          <div className={styles.modalCountRow}>
            <span className={styles.selCount} style={{ color }}>{selected.size} selected</span>
            {selected.size > 0 && (
              <button className={styles.clearBtn} onClick={() => setSelected(new Set())}>
                Clear all
              </button>
            )}
          </div>
        </div>

        <div className={styles.modalList}>
          {filtered.length === 0 ? (
            <div className={styles.emptyList}>No instructors match your search.</div>
          ) : (
            filtered.map((instructor) => {
              const active = selected.has(instructor.id);
              return (
                <motion.button
                  key={instructor.id}
                  className={`${styles.instructorRow} ${active ? styles.instructorRowActive : ""}`}
                  style={active ? { borderColor: `${color}50`, background: `${color}0d`, boxShadow: `0 12px 28px ${color}18` } : {}}
                  onClick={() => toggle(instructor.id)}
                  whileHover={{ scale: 1.01 }}
                  whileTap={{ scale: 0.99 }}
                >
                  <InstructorAvatar name={instructor.name} size={44} />
                  <div className={styles.iInfo}>
                    <div className={styles.iTopLine}>
                      <span className={styles.iName}>{instructor.name}</span>
                      {instructor.code && <span className={styles.iCodeChip}>{instructor.code}</span>}
                    </div>
                    <div className={styles.iMetaChips}>
                      <span className={styles.iEmailChip}>{instructor.email}</span>
                    </div>
                  </div>
                  <motion.div
                    className={styles.checkbox}
                    style={active ? { background: color, borderColor: color } : {}}
                    animate={{ scale: active ? 1 : 0.86 }}
                    transition={{ type: "spring", stiffness: 480, damping: 22 }}
                  >
                    {active && (
                      <svg viewBox="0 0 12 10" fill="none" stroke="white" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <polyline points="1,5 4,8 11,1" />
                      </svg>
                    )}
                  </motion.div>
                </motion.button>
              );
            })
          )}
        </div>

        <div className={styles.modalFooter}>
          <button className={styles.cancelBtn} onClick={onClose} disabled={saving}>
            Cancel
          </button>
          <motion.button
            className={styles.saveBtn}
            style={{ background: `linear-gradient(135deg, ${color}, ${color}cc)` }}
            onClick={handleSave}
            disabled={saving}
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.97 }}
          >
            {saving ? "Saving…" : "Save Assignments"}
          </motion.button>
        </div>
      </motion.div>
    </div>
  );
}

function CourseCard({ course, onAssign, index }) {
  const color = YEAR_COLORS[course.year] || "#818cf8";
  const assigned = course.assignedInstructors;
  const isAssigned = assigned.length > 0;

  return (
    <motion.div
      className={`${styles.card} ${isAssigned ? styles.cardAssigned : styles.cardUnassigned}`}
      initial={{ opacity: 0, y: 28 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.04 * index, duration: 0.42, ease: [0.22, 1, 0.36, 1] }}
    >
      <div className={styles.cardGlow} style={{ background: `radial-gradient(circle, ${color}20 0%, transparent 70%)` }} />
      <div className={styles.cardStripe} style={{ background: color }} />

      <div className={styles.cardBody}>
        <div className={styles.cardHead}>
          <div className={styles.cardHeadLeft}>
            <span className={styles.cardCode} style={{ color, background: `${color}18`, borderColor: `${color}2f` }}>
              {course.code}
            </span>
            <span className={styles.cardYear} style={{ color: `${color}d0` }}>
              {YEAR_LABELS[course.year] || `Year ${course.year}`}
            </span>
          </div>

          <span
            className={styles.cardStatus}
            style={{
              color: isAssigned ? "#22c55e" : "#f59e0b",
              background: isAssigned ? "rgba(34,197,94,.12)" : "rgba(245,158,11,.12)",
              borderColor: isAssigned ? "rgba(34,197,94,.24)" : "rgba(245,158,11,.24)",
            }}
          >
            {isAssigned ? `${assigned.length} Assigned` : "Pending"}
          </span>
        </div>

        <h3 className={styles.cardName}>{course.name}</h3>

        <div className={styles.cardMetaRow}>
          <span className={styles.cardCredits}>{course.credits} Credits</span>
          <span className={styles.cardMetaDivider}>•</span>
          <span className={styles.cardInfoText}>
            {assigned.length === 0 ? "Choose responsible instructor(s)" : "Ready for academic control"}
          </span>
        </div>

        <div className={styles.assignedSection}>
          {assigned.length === 0 ? (
            <div className={styles.unassignedBox}>
              <span className={styles.unassignedIcon}>+</span>
              <span className={styles.unassigned}>No instructor assigned yet</span>
            </div>
          ) : (
            <div className={styles.assignedList}>
              {assigned.map((i) => (
                <div key={i.id} className={styles.assignedItem}>
                  <InstructorAvatar name={i.name} size={30} />
                  <div className={styles.assignedText}>
                    <span className={styles.assignedName}>{i.name}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <motion.button
        className={styles.assignBtn}
        style={{ color, borderColor: `${color}28` }}
        whileHover={{ background: `${color}12`, borderColor: `${color}60` }}
        onClick={() => onAssign(course)}
      >
        {assigned.length === 0 ? "Choose Responsible Instructor" : "Manage Responsible Instructors"} →
      </motion.button>
    </motion.div>
  );
}

export default function InstructorControlPage() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filter, setFilter] = useState("all");
  const [search, setSearch] = useState("");
  const [modal, setModal] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getInstructorControl();
      setData(result);
    } catch {
      setError("Failed to load instructor control data.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const handleSave = async (courseId, instructorIds) => {
    await assignInstructors(courseId, instructorIds);
    setModal(null);
    const result = await getInstructorControl();
    setData(result);
  };

  const courses = data?.courses ?? [];

  const displayed = courses.filter((c) => {
    const q = search.toLowerCase();
    const matchSearch = c.name.toLowerCase().includes(q) || c.code.toLowerCase().includes(q);
    const matchFilter =
      filter === "all" ? true : filter === "unassigned" ? c.assignedInstructors.length === 0 : filter === String(c.year);
    return matchSearch && matchFilter;
  });

  const stats = {
    total: courses.length,
    assigned: courses.filter((c) => c.assignedInstructors.length > 0).length,
    unassigned: courses.filter((c) => c.assignedInstructors.length === 0).length,
  };

  return (
    <div className={styles.page}>
      <motion.div
        className={styles.pageHeader}
        initial={{ opacity: 0, y: -18 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}
      >
        <div>
          <h1 className={styles.pageTitle}>Instructor Control</h1>
          <p className={styles.pageSubtitle}>
            {data?.isOpen ? "Assign and manage responsible instructors for open registration courses" : "No active registration cycle"}
          </p>
        </div>

        {!loading && (
          <span
            className={styles.statusBadge}
            style={{
              background: data?.isOpen ? "rgba(34,197,94,0.12)" : "rgba(148,163,184,0.12)",
              color: data?.isOpen ? "#22c55e" : "#94a3b8",
              border: `1px solid ${data?.isOpen ? "rgba(34,197,94,0.3)" : "rgba(148,163,184,0.25)"}`,
            }}
          >
            {data?.isOpen ? "● Registration Open" : "● Registration Closed"}
          </span>
        )}
      </motion.div>

      {error && (
        <motion.div className={styles.errorBox} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          ⚠️ {error}
        </motion.div>
      )}

      {!loading && data?.isOpen && (
        <motion.div
          className={styles.statsRow}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.08 }}
        >
          {[
            { label: "Open Courses", value: stats.total, color: "#818cf8" },
            { label: "Assigned", value: stats.assigned, color: "#22c55e" },
            { label: "Unassigned", value: stats.unassigned, color: "#f59e0b" },
            { label: "Instructors", value: data.allInstructors.length, color: "#0ea5e9" },
          ].map((s, i) => (
            <motion.div
              key={s.label}
              className={styles.statCard}
              initial={{ opacity: 0, scale: 0.88 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.12 + i * 0.05, type: "spring", stiffness: 380, damping: 24 }}
            >
              <span className={styles.statValue} style={{ color: s.color }}>{s.value}</span>
              <span className={styles.statLabel}>{s.label}</span>
            </motion.div>
          ))}
        </motion.div>
      )}

      {!loading && data?.isOpen && (
        <motion.div
          className={styles.controls}
          initial={{ opacity: 0, y: 8 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.14 }}
        >
          <div className={styles.searchBox}>
            <svg className={styles.searchIco} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round">
              <circle cx="11" cy="11" r="8" />
              <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
            <input
              className={styles.searchInput}
              placeholder="Search courses…" 
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>

          <div className={styles.filterPills}>
            {[
              { key: "all", label: "All" },
              { key: "unassigned", label: "Unassigned" },
              { key: "1", label: "Year 1" },
              { key: "2", label: "Year 2" },
              { key: "3", label: "Year 3" },
              { key: "4", label: "Year 4" },
            ].map((f) => (
              <button
                key={f.key}
                className={`${styles.pill} ${filter === f.key ? styles.pillActive : ""}`}
                onClick={() => setFilter(f.key)}
              >
                {f.label}
              </button>
            ))}
          </div>
        </motion.div>
      )}

      {loading ? (
        <div className={styles.grid}>
          {[...Array(6)].map((_, i) => (
            <motion.div
              key={i}
              className={styles.skeleton}
              animate={{ opacity: [0.45, 0.8, 0.45] }}
              transition={{ duration: 1.5, delay: i * 0.1, repeat: Infinity }}
            />
          ))}
        </div>
      ) : !data?.isOpen ? (
        <motion.div className={styles.empty} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <span style={{ fontSize: 52 }}>📋</span>
          <p>No active registration cycle.</p>
          <span className={styles.emptyHint}>Open registration in the Registration Manager to assign instructors.</span>
        </motion.div>
      ) : displayed.length === 0 ? (
        <motion.div className={styles.empty} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
          <span style={{ fontSize: 52 }}>🔍</span>
          <p>No courses match your filter.</p>
        </motion.div>
      ) : (
        <AnimatePresence mode="wait">
          <motion.div key={filter + search} className={styles.grid}>
            {displayed.map((c, i) => (
              <CourseCard key={c.id} course={c} index={i} onAssign={setModal} />
            ))}
          </motion.div>
        </AnimatePresence>
      )}

      <AnimatePresence>
        {modal && (
          <AssignModal
            course={modal}
            allInstructors={data?.allInstructors ?? []}
            onSave={handleSave}
            onClose={() => setModal(null)}
          />
        )}
      </AnimatePresence>
    </div>
  );
}