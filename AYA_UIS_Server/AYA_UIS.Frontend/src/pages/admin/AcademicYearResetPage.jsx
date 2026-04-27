// src/pages/admin/AcademicYearResetPage.jsx
import { useState, useEffect, useMemo, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./AcademicYearResetPage.module.css";
import {
  adminGetFinalGradeReviewList,
  academicResetPreview,
  academicResetExecute,
} from "../../services/api/adminApi";

const sp = { type: "spring", stiffness: 380, damping: 30 };
const STEP_LABELS = ["Select", "Preview", "Execute"];
const REQUIRED_CONFIRM_TEXT = "RESET";

function cx(...classes) {
  return classes.filter(Boolean).join(" ");
}

function initials(name) {
  return (name || "?")
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => word[0] || "")
    .join("")
    .toUpperCase();
}

function Avatar({ name }) {
  return <div className={styles.avatar}>{initials(name)}</div>;
}

function Stepper({ step }) {
  return (
    <div className={styles.stepper} aria-label="Academic reset progress">
      {STEP_LABELS.map((label, index) => {
        const idx = index + 1;
        const isDone = idx < step;
        const isActive = idx === step;
        return (
          <div key={label} className={styles.stepperItem}>
            <div
              className={cx(
                styles.stepDot,
                isDone && styles.stepDotDone,
                isActive && styles.stepDotActive
              )}
            >
              <span className={styles.stepNum}>{isDone ? "✓" : idx}</span>
              <span className={styles.stepLabel}>{label}</span>
            </div>
            {idx < STEP_LABELS.length && <div className={styles.stepBar} />}
          </div>
        );
      })}
    </div>
  );
}

export default function AcademicYearResetPage() {
  const [step, setStep] = useState(1);

  const [toast, setToast] = useState(null);
  const showToast = useCallback((msg, kind = "success") => {
    setToast({ msg, kind });
    setTimeout(() => setToast(null), 3000);
  }, []);

  const [students, setStudents] = useState([]);
  const [listLoading, setListLoading] = useState(true);
  const [listError, setListError] = useState(null);
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState("all");
  const [selectedIds, setSelectedIds] = useState(new Set());

  const [previewLoading, setPreviewLoading] = useState(false);
  const [preview, setPreview] = useState(null);
  const [previewError, setPreviewError] = useState(null);

  const [confirmText, setConfirmText] = useState("");
  const [resetPwd, setResetPwd] = useState("");
  const [forceReset, setForceReset] = useState(false);

  const [executing, setExecuting] = useState(false);
  const [executeRes, setExecuteRes] = useState(null);
  const [executeErr, setExecuteErr] = useState(null);

  const reloadStudents = useCallback(async () => {
    setListLoading(true);
    setListError(null);
    try {
      const data = await adminGetFinalGradeReviewList();
      const flat = [
        ...(data.progress || []).map((s) => ({ ...s, status: "progress" })),
        ...(data.notCompleted || []).map((s) => ({ ...s, status: "not_completed" })),
        ...(data.completed || []).map((s) => ({ ...s, status: "completed" })),
      ];
      setStudents(flat);
    } catch (err) {
      setListError(err?.response?.data?.error?.message || err.message || "Failed to load students.");
    } finally {
      setListLoading(false);
    }
  }, []);

  useEffect(() => { reloadStudents(); }, [reloadStudents]);

  const filteredStudents = useMemo(() => {
    const q = search.trim().toLowerCase();
    return students.filter((s) => {
      if (filter !== "all" && s.status !== filter) return false;
      if (!q) return true;
      const code = (s.studentCode || "").toLowerCase();
      const name = (s.studentName || "").toLowerCase();
      return code.includes(q) || name.includes(q);
    });
  }, [students, search, filter]);

  const allFilteredSelected = useMemo(() => (
    filteredStudents.length > 0 &&
    filteredStudents.every((s) => selectedIds.has(s.studentId))
  ), [filteredStudents, selectedIds]);

  const toggleOne = useCallback((id) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const toggleAllFiltered = useCallback(() => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (allFilteredSelected) {
        filteredStudents.forEach((s) => next.delete(s.studentId));
      } else {
        filteredStudents.forEach((s) => next.add(s.studentId));
      }
      return next;
    });
  }, [allFilteredSelected, filteredStudents]);

  const clearSelection = useCallback(() => setSelectedIds(new Set()), []);

  const goToConfirm = useCallback(async () => {
    if (selectedIds.size === 0) {
      showToast("Select at least one student.", "warn");
      return;
    }
    setPreviewLoading(true);
    setPreviewError(null);
    try {
      const data = await academicResetPreview({
        studentIds: Array.from(selectedIds),
        selectAll: false,
      });
      setPreview(data);
      setStep(2);
      setConfirmText("");
      setResetPwd("");
      setForceReset(false);
    } catch (err) {
      const msg = err?.response?.data?.error?.message || err.message || "Failed to load preview.";
      setPreviewError(msg);
      showToast(msg, "err");
    } finally {
      setPreviewLoading(false);
    }
  }, [selectedIds, showToast]);

  const requiresForce = !!preview?.requiresForceReset;
  const canExecute = (
    confirmText === REQUIRED_CONFIRM_TEXT &&
    resetPwd.length > 0 &&
    (!requiresForce || forceReset)
  );

  const onExecute = useCallback(async () => {
    if (!canExecute) return;
    setExecuting(true);
    setStep(3);
    setExecuteErr(null);
    setExecuteRes(null);
    try {
      const data = await academicResetExecute({
        studentIds: Array.from(selectedIds),
        selectAll: false,
        confirmationText: confirmText,
        resetPassword: resetPwd,
        forceReset: forceReset,
      });
      setExecuteRes(data);
      showToast("Academic year reset complete.", "success");
    } catch (err) {
      const msg = err?.response?.data?.error?.message || err.message || "Reset failed.";
      setExecuteErr(msg);
      showToast(msg, "err");
    } finally {
      setExecuting(false);
    }
  }, [canExecute, selectedIds, confirmText, resetPwd, forceReset, showToast]);

  const goBackToSelect = useCallback(() => {
    setStep(1);
    setPreview(null);
    setPreviewError(null);
  }, []);

  const startOver = useCallback(() => {
    setStep(1);
    setPreview(null);
    setExecuteRes(null);
    setExecuteErr(null);
    setSelectedIds(new Set());
    setConfirmText("");
    setResetPwd("");
    setForceReset(false);
    reloadStudents();
  }, [reloadStudents]);

  return (
    <div className={styles.page}>
      <AnimatePresence>
        {toast && (
          <motion.div
            className={cx(
              styles.toast,
              toast.kind === "err" && styles.toastErr,
              toast.kind === "warn" && styles.toastWarn
            )}
            initial={{ y: -28, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            exit={{ y: -22, opacity: 0 }}
            transition={sp}
          >
            {toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      <section className={styles.hero}>
        <div className={styles.heroAuraOne} />
        <div className={styles.heroAuraTwo} />
        <div className={styles.heroContent}>
          <div className={styles.heroCopy}>
            <span className={styles.eyebrow}>Academic Operations</span>
            <h1 className={styles.heroTitle}>Academic Year Reset</h1>
            <p className={styles.heroSub}>
              Move selected students to the next term, archive their current registrations, freeze grade history,
              and clean old learning activity in one controlled workflow.
            </p>
            <div className={styles.heroBadges}>
              <span>Transactional rollback</span>
              <span>Preview before execute</span>
              <span>Protected by password</span>
            </div>
          </div>

          <div className={styles.heroPanel}>
            <Stepper step={step} />
            <div className={styles.heroMiniStats}>
              <MiniStat value={students.length} label="Students loaded" />
              <MiniStat value={selectedIds.size} label="Selected" />
              <MiniStat value={step} label="Current step" />
            </div>
          </div>
        </div>
      </section>

      <main className={styles.body}>
        <AnimatePresence mode="wait">
          {step === 1 && (
            <motion.div
              key="step1"
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.22 }}
            >
              <SelectStep
                listLoading={listLoading}
                listError={listError}
                students={students}
                filteredStudents={filteredStudents}
                selectedIds={selectedIds}
                search={search}
                setSearch={setSearch}
                filter={filter}
                setFilter={setFilter}
                toggleOne={toggleOne}
                toggleAllFiltered={toggleAllFiltered}
                allFilteredSelected={allFilteredSelected}
                clearSelection={clearSelection}
                onContinue={goToConfirm}
                continueLoading={previewLoading}
              />
            </motion.div>
          )}

          {step === 2 && (
            <motion.div
              key="step2"
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.22 }}
            >
              <ConfirmStep
                preview={preview}
                previewError={previewError}
                confirmText={confirmText}
                setConfirmText={setConfirmText}
                resetPwd={resetPwd}
                setResetPwd={setResetPwd}
                forceReset={forceReset}
                setForceReset={setForceReset}
                requiresForce={requiresForce}
                canExecute={canExecute}
                onBack={goBackToSelect}
                onExecute={onExecute}
              />
            </motion.div>
          )}

          {step === 3 && (
            <motion.div
              key="step3"
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              transition={{ duration: 0.22 }}
            >
              <ResultStep
                executing={executing}
                executeRes={executeRes}
                executeErr={executeErr}
                onBack={goBackToSelect}
                onStartOver={startOver}
              />
            </motion.div>
          )}
        </AnimatePresence>
      </main>
    </div>
  );
}

function MiniStat({ value, label }) {
  return (
    <div className={styles.miniStat}>
      <strong>{value}</strong>
      <span>{label}</span>
    </div>
  );
}

function SelectStep({
  listLoading,
  listError,
  students,
  filteredStudents,
  selectedIds,
  search,
  setSearch,
  filter,
  setFilter,
  toggleOne,
  toggleAllFiltered,
  allFilteredSelected,
  clearSelection,
  onContinue,
  continueLoading,
}) {
  const selectedList = students.filter((s) => selectedIds.has(s.studentId));
  const sumRegistered = selectedList.reduce((total, item) => total + (item.registeredCourses ?? 0), 0);
  const completedCount = selectedList.filter((item) => item.status === "completed").length;
  const pendingCount = selectedList.filter((item) => item.status !== "completed").length;

  const statusCounts = useMemo(() => ({
    all: students.length,
    completed: students.filter((s) => s.status === "completed").length,
    progress: students.filter((s) => s.status === "progress").length,
    not_completed: students.filter((s) => s.status === "not_completed").length,
  }), [students]);

  const filters = [
    ["all", "All", statusCounts.all],
    ["completed", "Complete", statusCounts.completed],
    ["not_completed", "Not Complete", statusCounts.not_completed],
    ["progress", "Progress", statusCounts.progress],
  ];

  return (
    <div className={styles.selectLayout}>
      <aside className={styles.controlPanel}>
        <div className={styles.panelHeader}>
          <span className={styles.panelIcon}>↻</span>
          <div>
            <h2>Reset scope</h2>
            <p>Choose the students that will be moved to the next academic term.</p>
          </div>
        </div>

        <div className={styles.summaryGrid}>
          <SummaryCard value={selectedIds.size} label="Selected" tone="blue" />
          <SummaryCard value={sumRegistered} label="Courses" tone="purple" />
          <SummaryCard value={completedCount} label="Ready" tone="green" />
          <SummaryCard value={pendingCount} label="Needs force" tone="orange" />
        </div>

        <div className={styles.searchBox}>
          <svg className={styles.searchIcon} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            type="text"
            className={styles.searchInput}
            placeholder="Search by student name or code..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button type="button" className={styles.clearSearch} onClick={() => setSearch("")}>
              ×
            </button>
          )}
        </div>

        <div className={styles.safetyNote}>
          <strong>Before reset</strong>
          <span>Review the preview carefully. The backend validates the password, force flag, and final impact again before execution.</span>
        </div>
      </aside>

      <section className={styles.studentPanel}>
        <div className={styles.statusFilterBar}>
          <div className={styles.statusFilterTitle}>
            <span>Review status</span>
            <strong>{filters.find(([key]) => key === filter)?.[1] || "All"}</strong>
          </div>
          <div className={styles.statusFilterActions}>
            {filters.map(([key, label, count]) => (
              <button
                key={key}
                type="button"
                className={cx(styles.statusFilterChip, filter === key && styles.statusFilterChipActive)}
                onClick={() => setFilter(key)}
              >
                <span>{label}</span>
                <strong className={styles.statusFilterCount}>{count}</strong>
              </button>
            ))}
          </div>
        </div>
        <div className={styles.studentPanelTop}>
          <div>
            <span className={styles.kicker}>Student selection</span>
            <h3>Students ready for preview</h3>
            <p>{filteredStudents.length} visible result{filteredStudents.length === 1 ? "" : "s"} from {students.length} total.</p>
          </div>

          <div className={styles.listActions}>
            {selectedIds.size > 0 && (
              <button type="button" className={styles.ghostButton} onClick={clearSelection}>
                Clear ({selectedIds.size})
              </button>
            )}
            <button type="button" className={styles.ghostButton} onClick={toggleAllFiltered}>
              {allFilteredSelected ? "Unselect filtered" : "Select filtered"}
            </button>
          </div>
        </div>

        <div className={styles.studentList}>
          {listLoading ? (
            <LoadingState label="Loading students..." />
          ) : listError ? (
            <EmptyState title="Could not load students" text={listError} />
          ) : filteredStudents.length === 0 ? (
            <EmptyState title="No students found" text="Try a different search keyword or filter." />
          ) : (
            filteredStudents.map((student, index) => {
              const checked = selectedIds.has(student.studentId);
              return (
                <motion.button
                  type="button"
                  key={student.studentId}
                  className={cx(styles.studentRow, checked && styles.studentRowChecked)}
                  onClick={() => toggleOne(student.studentId)}
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.18, delay: Math.min(index * 0.018, 0.12) }}
                >
                  <span className={cx(styles.checkBox, checked && styles.checkBoxOn)}>
                    {checked && <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5"><polyline points="20 6 9 17 4 12" /></svg>}
                  </span>

                  <div className={styles.studentIdentity}>
                    <Avatar name={student.studentName} />
                    <div>
                      <strong>{student.studentName || "Unnamed student"}</strong>
                      <span>{student.studentCode || "No code"}</span>
                    </div>
                  </div>

                  <div className={styles.studentMeta}>
                    <span>Academic year</span>
                    <strong>{student.academicYear ? `${student.academicYear} Year` : "—"}</strong>
                  </div>

                  <div className={styles.studentMeta}>
                    <span>Courses</span>
                    <strong>{student.registeredCourses ?? 0}</strong>
                  </div>

                  <span className={statusClass(student.status)}>{statusLabel(student.status)}</span>
                </motion.button>
              );
            })
          )}
        </div>

        <div className={styles.footerBar}>
          <div>
            <strong>{selectedIds.size}</strong> selected for a protected reset preview.
          </div>
          <motion.button
            type="button"
            className={styles.primaryButton}
            onClick={onContinue}
            disabled={selectedIds.size === 0 || continueLoading}
            whileHover={selectedIds.size > 0 && !continueLoading ? { scale: 1.02 } : {}}
            whileTap={selectedIds.size > 0 && !continueLoading ? { scale: 0.98 } : {}}
          >
            {continueLoading ? "Building preview..." : "Preview selected students"}
            <span>→</span>
          </motion.button>
        </div>
      </section>
    </div>
  );
}

function SummaryCard({ value, label, tone }) {
  return (
    <div className={cx(styles.summaryCard, styles[`summary_${tone}`])}>
      <strong>{value}</strong>
      <span>{label}</span>
    </div>
  );
}

function ConfirmStep({
  preview,
  previewError,
  confirmText,
  setConfirmText,
  resetPwd,
  setResetPwd,
  forceReset,
  setForceReset,
  requiresForce,
  canExecute,
  onBack,
  onExecute,
}) {
  if (previewError) {
    return (
      <div className={styles.resultBlock}>
        <div className={cx(styles.resultIcon, styles.resultIconErr)}>!</div>
        <h3 className={styles.resultTitle}>Preview failed</h3>
        <p className={styles.resultSub}>{previewError}</p>
        <button type="button" className={styles.secondaryButton} onClick={onBack}>← Back to selection</button>
      </div>
    );
  }

  if (!preview) return null;

  const totals = preview.totals || {};

  return (
    <div className={styles.confirmLayout}>
      <section className={styles.impactPanel}>
        <div className={styles.confirmHeader}>
          <span className={styles.confirmIcon}>⚡</span>
          <div>
            <span className={styles.kicker}>Preview generated</span>
            <h2>Reset impact summary</h2>
            <p>Review what will be archived, marked as passed or failed, and moved to history before execution.</p>
          </div>
        </div>

        {preview.warnings && preview.warnings.length > 0 && (
          <div className={styles.warningCard}>
            <div className={styles.warningTitle}>Warnings detected</div>
            <ul>
              {preview.warnings.map((warning, i) => <li key={i}>{warning}</li>)}
            </ul>
          </div>
        )}

        <div className={styles.impactGrid}>
          <ImpactCard value={preview.selectedCount} label="Selected students" />
          <ImpactCard value={totals.registeredCourses ?? 0} label="Current registrations" />
          <ImpactCard value={totals.passedCourses ?? 0} label="Will become passed" tone="green" />
          <ImpactCard value={totals.failedCourses ?? 0} label="Will become failed" tone="red" />
          <ImpactCard value={totals.unassignedGrades ?? 0} label="Unassigned grades" tone="orange" />
          <ImpactCard value={totals.notCompletedReviewCount ?? 0} label="Not completed reviews" tone="orange" />
        </div>

        <div className={styles.transitionPanel}>
          <div className={styles.sectionTitleRow}>
            <div>
              <span className={styles.kicker}>Term movement</span>
              <h3>Student transitions</h3>
            </div>
            <span className={styles.countBadge}>{(preview.perStudent || []).length}</span>
          </div>

          <div className={styles.transitionList}>
            {(preview.perStudent || []).map((student) => (
              <div key={student.studentId} className={styles.transitionItem}>
                <div className={styles.transitionPerson}>
                  <Avatar name={student.studentName} />
                  <div>
                    <strong>{student.studentName || "Unnamed student"}</strong>
                    <span>{student.academicCode || "No code"}</span>
                  </div>
                </div>

                <div className={styles.termMove}>
                  <span>{labelizeLevel(student.currentLevel)} · Semester {student.currentSemester}</span>
                  <b>→</b>
                  <span>{labelizeLevel(student.targetLevel)} · Semester {student.targetSemester}</span>
                </div>

                <div className={styles.transitionStats}>
                  {student.alreadyReset ? (
                    <span className={styles.alreadyTag}>Already reset</span>
                  ) : (
                    <>
                      <span>{student.passedCount} passed</span>
                      <span>{student.failedCount} failed</span>
                      <span>{student.unassignedCount} unassigned</span>
                    </>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <aside className={styles.executePanel}>
        <div className={styles.lockBadge}>Protected action</div>
        <h3>Confirm execution</h3>
        <p>
          This operation updates academic level, archives registrations, preserves grade history,
          and purges current activity data for the selected students.
        </p>

        {requiresForce && (
          <label className={cx(styles.forceBox, forceReset && styles.forceBoxOn)}>
            <input
              type="checkbox"
              checked={forceReset}
              onChange={(e) => setForceReset(e.target.checked)}
            />
            <div>
              <strong>Enable Force Reset</strong>
              <span>Required because the preview contains unassigned grades or incomplete final grade reviews.</span>
            </div>
          </label>
        )}

        <label className={styles.fieldGroup}>
          <span>Type {REQUIRED_CONFIRM_TEXT}</span>
          <input
            type="text"
            className={cx(
              styles.input,
              confirmText === REQUIRED_CONFIRM_TEXT && styles.inputOk,
              confirmText.length > 0 && confirmText !== REQUIRED_CONFIRM_TEXT && styles.inputBad
            )}
            value={confirmText}
            onChange={(e) => setConfirmText(e.target.value)}
            placeholder={REQUIRED_CONFIRM_TEXT}
            autoComplete="off"
            spellCheck={false}
          />
          <small>Case-sensitive confirmation.</small>
        </label>

        <label className={styles.fieldGroup}>
          <span>Reset password</span>
          <input
            type="password"
            className={cx(styles.input, resetPwd.length > 0 && styles.inputOk)}
            value={resetPwd}
            onChange={(e) => setResetPwd(e.target.value)}
            placeholder="Reset password"
            autoComplete="new-password"
          />
          <small>The backend validates this password before any database change.</small>
        </label>

        <div className={styles.transactionNote}>
          <strong>Rollback safe</strong>
          <span>If any write fails, the whole reset is rolled back as one transaction.</span>
        </div>

        <div className={styles.confirmActions}>
          <button type="button" className={styles.secondaryButton} onClick={onBack}>← Back</button>
          <motion.button
            type="button"
            className={styles.dangerButton}
            onClick={onExecute}
            disabled={!canExecute}
            whileHover={canExecute ? { scale: 1.02 } : {}}
            whileTap={canExecute ? { scale: 0.98 } : {}}
          >
            Execute reset
          </motion.button>
        </div>
      </aside>
    </div>
  );
}

function ImpactCard({ value, label, tone }) {
  return (
    <div className={cx(styles.impactCard, tone && styles[`impact_${tone}`])}>
      <strong>{value ?? 0}</strong>
      <span>{label}</span>
    </div>
  );
}

function ResultStep({ executing, executeRes, executeErr, onBack, onStartOver }) {
  if (executing) {
    return (
      <div className={styles.resultBlock}>
        <div className={cx(styles.resultIcon, styles.resultIconRun)}>
          <div className={styles.spinner} />
        </div>
        <h3 className={styles.resultTitle}>Running academic reset...</h3>
        <p className={styles.resultSub}>
          The backend is archiving registrations, freezing grade history, cleaning learning activity,
          and notifying selected students. Keep this page open until the response returns.
        </p>
      </div>
    );
  }

  if (executeErr) {
    return (
      <div className={styles.resultBlock}>
        <div className={cx(styles.resultIcon, styles.resultIconErr)}>✕</div>
        <h3 className={styles.resultTitle}>Reset failed</h3>
        <p className={styles.resultSub}>No partial changes were applied. The transaction was rolled back.</p>
        <div className={styles.errorBox}>{executeErr}</div>
        <div className={styles.resultActions}>
          <button type="button" className={styles.secondaryButton} onClick={onBack}>← Back to preview</button>
          <button type="button" className={styles.primaryButton} onClick={onStartOver}>Start over</button>
        </div>
      </div>
    );
  }

  if (executeRes) {
    const r = executeRes;
    return (
      <div className={styles.resultBlock}>
        <div className={cx(styles.resultIcon, styles.resultIconOk)}>✓</div>
        <h3 className={styles.resultTitle}>Reset completed successfully</h3>
        <p className={styles.resultSub}>
          {r.studentsReset} student{r.studentsReset === 1 ? "" : "s"} moved to the next academic term.
          Backup snapshot reference: <strong>#{r.resetId}</strong>.
        </p>

        <div className={styles.resultStats}>
          <ResultStat value={r.studentsReset} label="Students reset" />
          <ResultStat value={r.archivedRegistrations} label="Archived registrations" />
          <ResultStat value={r.passedCount} label="Passed history" tone="green" />
          <ResultStat value={r.failedCount} label="Failed history" tone="red" />
          <ResultStat value={r.unassignedFailedCount} label="Unassigned → failed" tone="orange" />
          <ResultStat value={r.finalGradesPurged} label="Final grades purged" />
          <ResultStat value={r.submissionsPurged} label="Submissions purged" />
          <ResultStat value={r.quizAttemptsPurged} label="Quiz attempts purged" />
          <ResultStat value={r.midtermsPurged} label="Midterms purged" />
          <ResultStat value={r.notificationsSent} label="Notifications sent" />
        </div>

        {r.limitations && r.limitations.length > 0 && (
          <div className={styles.limitations}>
            <h4>Notes</h4>
            <ul>{r.limitations.map((limitation, i) => <li key={i}>{limitation}</li>)}</ul>
          </div>
        )}

        <div className={styles.resultActions}>
          <button type="button" className={styles.primaryButton} onClick={onStartOver}>Run another reset</button>
        </div>
      </div>
    );
  }

  return null;
}

function ResultStat({ value, label, tone }) {
  return (
    <div className={cx(styles.resultStatCell, tone && styles[`result_${tone}`])}>
      <strong>{value ?? 0}</strong>
      <span>{label}</span>
    </div>
  );
}

function LoadingState({ label }) {
  return (
    <div className={styles.loadingState}>
      <div className={styles.spinner} />
      <span>{label}</span>
    </div>
  );
}

function EmptyState({ title, text }) {
  return (
    <div className={styles.emptyState}>
      <div className={styles.emptyIcon}>⌕</div>
      <strong>{title}</strong>
      <span>{text}</span>
    </div>
  );
}

function statusClass(status) {
  return cx(
    styles.statusPill,
    status === "completed" && styles.statusReady,
    status === "not_completed" && styles.statusMissing,
    status !== "completed" && status !== "not_completed" && styles.statusProgress
  );
}

function statusLabel(status) {
  if (status === "completed") return "Completed";
  if (status === "not_completed") return "Not completed";
  return "Progress";
}

function labelizeLevel(raw) {
  if (!raw) return "—";
  return String(raw).replace(/_/g, " ");
}
