// src/pages/instructor/UploadMaterialPage.jsx
import { useEffect, useMemo, useRef, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import styles from "./UploadMaterialPage.module.css";
import {
  createAssignment,
  getInstructorCourses,
  uploadLecture,
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

function LectureForm({ courseId, color }) {
  const [title, setTitle] = useState("");
  const [week, setWeek] = useState("");
  const [desc, setDesc] = useState("");
  const [file, setFile] = useState(null);
  const [relDate, setRelDate] = useState("");
  const [relNow, setRelNow] = useState(true);
  const [done, setDone] = useState(false);
  const [loading, setLoading] = useState(false);

  const valid = title.trim() && file && (relNow || relDate);

  const reset = () => {
    setDone(false);
    setTitle("");
    setWeek("");
    setDesc("");
    setFile(null);
    setRelDate("");
    setRelNow(true);
  };

  const submit = async () => {
    if (!valid) return;
    setLoading(true);
    try {
      const weekNum = week ? parseInt(week.replace("Week ", ""), 10) : undefined;
      await uploadLecture(
        courseId,
        {
          title,
          description: desc || undefined,
          week: weekNum,
          releaseDate: relNow ? null : relDate || null,
        },
        file,
      );
      setDone(true);
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

export default function UploadMaterialPage() {
  const [courses, setCourses] = useState([]);
  const [course, setCourse] = useState(null);
  const [matType, setMatType] = useState(null);

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
                    <LectureForm courseId={course} color="#5a67d8" />
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
          </>
        )}
      </main>
    </div>
  );
}
