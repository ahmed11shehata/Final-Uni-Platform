// src/pages/instructor/InstructorDashboard.jsx
import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { getInstructorDashboard } from "../../services/api/instructorApi";
import styles from "./InstructorDashboard.module.css";

const spring = { type: "spring", stiffness: 360, damping: 30 };

const COURSE_PALETTE = ["#7c3aed", "#0891b2", "#059669", "#f59e0b", "#e11d48", "#2563eb"];
const GRADE_TONES = ["#f59e0b", "#10b981", "#f43f5e", "#8b5cf6"];

export default function InstructorDashboard() {
  const { user } = useAuth?.() || {};
  const navigate = useNavigate();

  const [time, setTime] = useState(new Date());
  const [course, setCourse] = useState(null);
  const [courses, setCourses] = useState([]);
  const [gradeSummary, setGradeSummary] = useState({});
  const [activity, setActivity] = useState([]);
  const [upcoming, setUpcoming] = useState([]);

  useEffect(() => {
    const timer = setInterval(() => setTime(new Date()), 30000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    getInstructorDashboard()
      .then((data) => {
        const nextCourses = Array.isArray(data?.courses) ? data.courses : [];
        setCourses(nextCourses);
        setGradeSummary(data?.gradeSummary || {});
        setActivity(Array.isArray(data?.recentActivity) ? data.recentActivity : []);
        setUpcoming(Array.isArray(data?.upcoming) ? data.upcoming : []);
        if (nextCourses.length > 0 && !course) setCourse(nextCourses[0].id);
      })
      .catch(() => {});
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const selectedCourse = courses.find((item) => item.id === course) || courses[0] || {
    color: "#8b5cf6",
    code: "—",
    name: "No course selected",
    icon: "🎓",
    students: 0,
    progress: 0,
  };
  const selectedTone = selectedCourse.color || "#8b5cf6";
  const currentGrades = gradeSummary?.[course] || { pending: 0, approved: 0, rejected: 0, avg: "—" };
  const totalStudents = courses.reduce((sum, item) => sum + (item.students || 0), 0);
  const totalPending = courses.reduce((sum, item) => sum + (gradeSummary?.[item.id]?.pending || 0), 0);
  const courseCodes = courses.map((item) => item.code).filter(Boolean).join(" • ") || "Courses will appear here once assigned";

  const statCards = [
    { label: "Total Students", value: totalStudents, icon: "👥", tone: "#22c55e" },
    { label: "Pending Grades", value: totalPending, icon: "📝", tone: "#f59e0b" },
    { label: "Active Courses", value: courses.length, icon: "📚", tone: "#38bdf8" },
  ];

  const quickActions = [
    { label: "Upload Material", path: "/instructor/material", icon: "📤", tone: "#7c3aed", sub: "Lectures & files" },
    { label: "Quiz Builder", path: "/instructor/quiz-builder", icon: "✏️", tone: "#0891b2", sub: "Create assessments" },
    { label: "Grade Submissions", path: "/instructor/grades", icon: "📊", tone: "#059669", sub: "Review results" },
    { label: "My Schedule", path: "/instructor/schedule", icon: "📅", tone: "#f59e0b", sub: "Today & exams" },
  ];

  const gradeCards = [
    { value: currentGrades.pending, label: "Pending", icon: "⏳" },
    { value: currentGrades.approved, label: "Approved", icon: "✅" },
    { value: currentGrades.rejected, label: "Rejected", icon: "⚠️" },
    { value: currentGrades.avg, label: "Avg Grade", icon: "⭐" },
  ];

  return (
    <main className={styles.page}>
      <motion.section
        className={styles.hero}
        initial={{ opacity: 0, y: -18 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.45, ease: [0.22, 1, 0.36, 1] }}
      >
        <div className={styles.heroAuraOne} />
        <div className={styles.heroAuraTwo} />
        <div className={styles.heroGrid} />

        <div className={styles.heroTop}>
          <div className={styles.heroIntro}>
            <span className={styles.eyebrow}>Instructor Workspace</span>
            <h1 className={styles.heroName}>Dr. {user?.name || "Mohamed Farouk"}</h1>
            <p className={styles.heroText}>{courseCodes}</p>
            <div className={styles.courseRibbon}>
              {courses.slice(0, 5).map((item, index) => (
                <span key={item.id || item.code || index} style={{ "--tone": item.color || COURSE_PALETTE[index % COURSE_PALETTE.length] }}>
                  {item.code || "COURSE"}
                </span>
              ))}
              {courses.length === 0 && <span style={{ "--tone": "#8b5cf6" }}>No assigned courses</span>}
            </div>
          </div>

          <motion.div className={styles.clockCard} whileHover={{ y: -4 }} transition={spring}>
            <span className={styles.clockLabel}>Live Clock</span>
            <strong>{time.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</strong>
            <p>{time.toLocaleDateString([], { weekday: "long", month: "short", day: "numeric" })}</p>
          </motion.div>
        </div>

        <div className={styles.statDeck}>
          {statCards.map((stat, index) => (
            <motion.div
              key={stat.label}
              className={styles.statCard}
              style={{ "--tone": stat.tone }}
              initial={{ opacity: 0, y: 14 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.1 + index * 0.06, ...spring }}
            >
              <span className={styles.statIcon}>{stat.icon}</span>
              <div>
                <strong>{stat.value}</strong>
                <span>{stat.label}</span>
              </div>
            </motion.div>
          ))}
        </div>

        <div className={styles.quickActions}>
          {quickActions.map((action, index) => (
            <motion.button
              key={action.label}
              className={styles.quickAction}
              style={{ "--tone": action.tone }}
              onClick={() => navigate(action.path)}
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.18 + index * 0.05, ...spring }}
              whileHover={{ y: -3 }}
              whileTap={{ scale: 0.97 }}
            >
              <span>{action.icon}</span>
              <div>
                <strong>{action.label}</strong>
                <small>{action.sub}</small>
              </div>
            </motion.button>
          ))}
        </div>
      </motion.section>

      <section className={styles.mainGrid}>
        <div className={styles.leftCol}>
          <section className={styles.panel}>
            <div className={styles.sectionHead}>
              <div>
                <span className={styles.sectionKicker}>Teaching Load</span>
                <h2>My Courses</h2>
              </div>
              <span className={styles.sectionBadge}>{courses.length} courses</span>
            </div>

            <div className={styles.courseCards}>
              {courses.length === 0 && (
                <div className={styles.emptyState}>
                  <span>📚</span>
                  <strong>No courses assigned yet.</strong>
                  <p>Your assigned courses will be listed here automatically.</p>
                </div>
              )}

              {courses.map((item, index) => {
                const tone = item.color || COURSE_PALETTE[index % COURSE_PALETTE.length];
                const isActive = course === item.id;
                const pending = gradeSummary?.[item.id]?.pending || 0;

                return (
                  <motion.button
                    key={item.id}
                    className={`${styles.courseCard} ${isActive ? styles.courseCardActive : ""}`}
                    style={{ "--cc": tone }}
                    onClick={() => setCourse(item.id)}
                    initial={{ opacity: 0, x: -18 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: index * 0.055, ...spring }}
                    whileHover={{ x: 5 }}
                    whileTap={{ scale: 0.99 }}
                  >
                    <div className={styles.courseIcon}>{item.icon || "🎓"}</div>
                    <div className={styles.courseBody}>
                      <div className={styles.courseTopLine}>
                        <span>{item.code || "COURSE"}</span>
                        <small>{item.students || 0} students</small>
                      </div>
                      <strong>{item.name || "Untitled Course"}</strong>
                      <div className={styles.progressTrack}>
                        <motion.div
                          className={styles.progressFill}
                          initial={{ width: 0 }}
                          animate={{ width: `${item.progress || 0}%` }}
                          transition={{ delay: 0.25 + index * 0.06, duration: 0.8, ease: "easeOut" }}
                        />
                      </div>
                    </div>
                    <div className={styles.courseSide}>
                      <span>{item.progress || 0}%</span>
                      {pending > 0 && <em>{pending}</em>}
                    </div>
                  </motion.button>
                );
              })}
            </div>
          </section>

          <AnimatePresence mode="wait">
            <motion.section
              key={course || "empty-course"}
              className={styles.panel}
              style={{ "--panelTone": selectedTone }}
              initial={{ opacity: 0, y: 12 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: 8 }}
            >
              <div className={styles.sectionHead}>
                <div>
                  <span className={styles.sectionKicker}>Selected Course</span>
                  <h2>Grade Overview</h2>
                  <p className={styles.sectionHint}>{selectedCourse.code} — {selectedCourse.name}</p>
                </div>
                <span className={styles.activeCoursePill}>{selectedCourse.icon || "🎓"} {selectedCourse.code}</span>
              </div>

              <div className={styles.gradeGrid}>
                {gradeCards.map((item, index) => (
                  <motion.div
                    key={item.label}
                    className={styles.gradeBox}
                    style={{ "--tone": GRADE_TONES[index] }}
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ delay: index * 0.05, ...spring }}
                    whileHover={{ y: -3 }}
                  >
                    <span>{item.icon}</span>
                    <strong>{item.value}</strong>
                    <small>{item.label}</small>
                  </motion.div>
                ))}
              </div>
            </motion.section>
          </AnimatePresence>

          <section className={styles.panel}>
            <div className={styles.sectionHead}>
              <div>
                <span className={styles.sectionKicker}>Latest Updates</span>
                <h2>Recent Activity</h2>
              </div>
              <span className={styles.sectionBadge}>{activity.length} updates</span>
            </div>

            <div className={styles.feed}>
              {activity.length === 0 && (
                <div className={styles.emptyStateCompact}>
                  <span>✨</span>
                  <p>No recent activity.</p>
                </div>
              )}

              {activity.map((item, index) => {
                const tone = item.color || COURSE_PALETTE[index % COURSE_PALETTE.length];
                return (
                  <motion.div
                    key={item.id || index}
                    className={styles.feedItem}
                    style={{ "--tone": tone }}
                    initial={{ opacity: 0, x: -12 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: 0.04 + index * 0.04, ...spring }}
                  >
                    <div className={styles.feedIcon}>{item.icon || "📎"}</div>
                    <div className={styles.feedBody}>
                      <strong>{item.student || item.studentName || "Student"}</strong>
                      <span>{item.detail || item.description || "New update"}</span>
                    </div>
                    <div className={styles.feedMeta}>
                      <b>{item.course || item.courseCode || ""}</b>
                      <small>{item.time || ""}</small>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </section>
        </div>

        <div className={styles.rightCol}>
          <section className={styles.panel}>
            <div className={styles.sectionHead}>
              <div>
                <span className={styles.sectionKicker}>Planning</span>
                <h2>Upcoming</h2>
              </div>
              <span className={styles.sectionBadge}>{upcoming.length} items</span>
            </div>

            <div className={styles.upList}>
              {upcoming.length === 0 && (
                <div className={styles.emptyState}>
                  <span>📅</span>
                  <strong>No upcoming events.</strong>
                  <p>Scheduled lectures, deadlines, and exams will appear here.</p>
                </div>
              )}

              {upcoming.map((item, index) => {
                const tone = item.color || COURSE_PALETTE[index % COURSE_PALETTE.length];
                return (
                  <motion.div
                    key={item.id || index}
                    className={styles.upCard}
                    style={{ "--tone": tone }}
                    initial={{ opacity: 0, y: 12 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.06 + index * 0.06, ...spring }}
                    whileHover={{ x: 4 }}
                  >
                    <div className={styles.upIcon}>{item.icon || "📅"}</div>
                    <div className={styles.upBody}>
                      <strong>{item.title || "Upcoming event"}</strong>
                      <div>
                        <span>📅 {item.date || "—"}</span>
                        <span>⏰ {item.time || "—"}</span>
                        <span>🏛 {item.room || "—"}</span>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </section>

          <section className={`${styles.panel} ${styles.quickPanel}`}>
            <div className={styles.sectionHead}>
              <div>
                <span className={styles.sectionKicker}>Shortcuts</span>
                <h2>Quick Access</h2>
              </div>
            </div>

            <div className={styles.qnGrid}>
              {quickActions.map((item, index) => (
                <motion.button
                  key={item.label}
                  className={styles.qnBtn}
                  style={{ "--tone": item.tone }}
                  onClick={() => navigate(item.path)}
                  initial={{ opacity: 0, scale: 0.92 }}
                  animate={{ opacity: 1, scale: 1 }}
                  transition={{ delay: 0.1 + index * 0.05, ...spring }}
                  whileHover={{ y: -4 }}
                  whileTap={{ scale: 0.97 }}
                >
                  <span>{item.icon}</span>
                  <strong>{item.label}</strong>
                  <small>{item.sub}</small>
                </motion.button>
              ))}
            </div>
          </section>
        </div>
      </section>
    </main>
  );
}
