import api from "./axiosInstance";

// ── Student Registration ────────────────────────────────────

/**
 * GET /api/student/registration/status
 * Returns { open, allowedYears, currentCredits, maxCredits,
 *           registeredCourses, failedCourses, lockedCourses }
 */
export const getStudentRegistrationStatus = async () => {
  const res = await api.get("/student/registration/status");
  return res.data.data;
};

/**
 * GET /api/student/registration/courses
 * Returns { yearCounts, courses: [{ id, code, name, instructor, schedule,
 *           credits, capacity, enrolled, status, prereqs, color, pattern, year, lockReason }] }
 */
export const getStudentAvailableCourses = async () => {
  const res = await api.get("/student/registration/courses");
  return res.data.data;
};

/**
 * POST /api/student/registration/courses
 * Body: { courseCode }
 * Returns { message, course }
 */
export const registerCourse = async (courseCode) => {
  const res = await api.post("/student/registration/courses", { courseCode });
  return res.data.data;
};

/**
 * DELETE /api/student/registration/courses/{courseCode}
 * Returns { message }
 */
export const dropCourse = async (courseCode) => {
  const res = await api.delete(`/student/registration/courses/${courseCode}`);
  return res.data.data;
};

// ── Transcript ──────────────────────────────────────────────

/**
 * GET /api/student/transcript
 * Returns full academic setup: { student, academicSetup: { currentYear, years: { "1": { semesters: { "1": [...] } } } } }
 * Each course entry: { courseCode, name, credits, selected, total, grade, gpaPoints, isEquivalency }
 * selected=true means the student has completed (passed) that course.
 */
export const getStudentTranscript = async () => {
  const res = await api.get("/student/transcript");
  return res.data.data;
};

// ── Student Courses ─────────────────────────────────────────

/** GET /api/student/courses — returns enrolled course list */
export const getStudentCourses = async () => {
  const res = await api.get("/student/courses");
  return res.data.data;
};

/** GET /api/student/courses/{courseId} — returns full course detail */
export const getStudentCourseDetail = async (courseId) => {
  const res = await api.get(`/student/courses/${courseId}`);
  return res.data.data;
};

// ── Assignments ──────────────────────────────────────────────

/**
 * POST /api/student/assignments/{assignmentId}/submit
 * Multipart form — submits or replaces file before deadline.
 */
export const submitAssignment = async (assignmentId, file) => {
  const form = new FormData();
  form.append("file", file);
  const res = await api.post(`/student/assignments/${assignmentId}/submit`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return res.data.data;
};

/**
 * DELETE /api/student/assignments/{assignmentId}/submit
 * Remove own submission before deadline.
 */
export const removeSubmission = async (assignmentId) => {
  const res = await api.delete(`/student/assignments/${assignmentId}/submit`);
  return res.data.data;
};

// ── Quizzes ──────────────────────────────────────────────────

/**
 * GET /api/quizzes/{quizId}
 * Full quiz with questions (no correct answers exposed).
 */
export const getQuizDetail = async (quizId) => {
  const res = await api.get(`/quizzes/${quizId}`);
  return res.data;
};

/**
 * POST /api/quizzes/{quizId}/submit
 * Body: { quizId, answers: [{ questionId, selectedOptionId }] }
 * Returns { data: score }
 */
export const submitQuiz = async (quizId, answers) => {
  const res = await api.post(`/quizzes/${quizId}/submit`, { quizId, answers });
  return res.data;
};

// ── Notifications ─────────────────────────────────────────────

/**
 * GET /api/student/notifications
 */
export const getStudentNotifications = async () => {
  const res = await api.get("/student/notifications");
  return res.data.data;
};

/**
 * PUT /api/student/notifications/{id}/read
 */
export const markNotificationRead = async (id) => {
  await api.put(`/student/notifications/${id}/read`);
};

/**
 * PUT /api/student/notifications/read-all
 */
export const markAllNotificationsRead = async () => {
  await api.put("/student/notifications/read-all");
};

// ── Final Grades (published only) ───────────────────────────

/**
 * GET /api/student/final-grades
 * Returns published final grades for currently registered courses.
 * Shape: [{ courseId, courseCode, courseName, finalScore, courseworkTotal, total, letterGrade }]
 * Server enforces Published == true — unpublished grades are never returned.
 */
export const getStudentPublishedFinalGrades = async () => {
  const res = await api.get("/student/final-grades");
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

// ── Timetable ───────────────────────────────────────────────

/**
 * GET /api/student/timetable/events
 * Returns a normalized list of assignment / quiz / lecture events for the
 * student's actively-registered courses. Server decides availability —
 * frontend MUST use status / isAvailable / actionUrl as the source of truth.
 *
 * Optional params: { limit, type } where type ∈ "assignment" | "quiz" | "lecture".
 */
export const getStudentTimetableEvents = async (params = {}) => {
  const res = await api.get("/student/timetable/events", { params });
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

// ── Stubs (not yet implemented on backend) ──────────────────
export const getStudentGrades = () => Promise.resolve([]);

export const getStudentProfile = async () => {
  const res = await api.get("/student/profile");
  return res.data.data;
};
export const updateStudentProfile = async (dto) => {
  const res = await api.put("/student/profile", dto);
  return res.data.data;
};
