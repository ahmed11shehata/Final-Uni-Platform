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
