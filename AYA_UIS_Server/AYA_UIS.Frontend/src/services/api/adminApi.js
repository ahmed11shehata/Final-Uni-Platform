import api from "./axiosInstance";

// ── Admin Dashboard Stats ───────────────────────────────────

/** GET /api/admin/stats */
export const getAdminStats = async () => {
  const res = await api.get("/admin/stats");
  return res.data.data;
};

// ── Admin Registration ──────────────────────────────────────

/** GET /api/admin/registration/status */
export const getRegistrationStatus = async () => {
  const res = await api.get("/admin/registration/status");
  return res.data.data;
};

/**
 * POST /api/admin/registration/start
 * Body: { semester, academicYear, startDate, deadline, openedCoursesByYear, maxCredits }
 */
export const startRegistration = async (dto) => {
  const res = await api.post("/admin/registration/start", dto);
  return res.data.data;
};

/** POST /api/admin/registration/stop */
export const stopRegistration = async () => {
  const res = await api.post("/admin/registration/stop");
  return res.data.data;
};

/**
 * PUT /api/admin/registration/settings
 * Body: same as start
 */
export const updateRegistrationSettings = async (dto) => {
  const res = await api.put("/admin/registration/settings", dto);
  return res.data.data;
};

// ── Admin Courses ───────────────────────────────────────────

/** GET /api/admin/courses?year=&semester=&type=&search= */
export const getAdminCourses = async (params = {}) => {
  const res = await api.get("/admin/courses", { params });
  return res.data.data; // array of { code, name, credits, year, semester, type, prerequisites }
};

// ── Admin Email Manager ─────────────────────────────────────

export const getEmails = async () => {
  const res = await api.get("/admin/emails");
  return res.data.data;
};

export const createEmailAccount = async (dto) => {
  const res = await api.post("/admin/emails/create", dto);
  return res.data.data;
};

export const toggleActive = async (id) => {
  const res = await api.put(`/admin/emails/${id}/toggle-active`);
  return res.data.data;
};

export const resetPassword = async (id, newPassword) => {
  const res = await api.put(`/admin/emails/${id}/reset-password`, { newPassword });
  return res.data.data;
};

export const deleteAccount = async (id) => {
  const res = await api.delete(`/admin/emails/${id}`);
  return res.data;
};

export const updateAccount = async (id, dto) => {
  const res = await api.put(`/admin/emails/${id}`, dto);
  return res.data.data;
};

// ── Admin Student Control ────────────────────────────────────

/** GET /api/admin/student/{idOrCode} — accepts GUID or academic code */
export const getAdminStudent = async (idOrCode) => {
  const res = await api.get(`/admin/student/${encodeURIComponent(idOrCode)}`);
  return res.data.data;
};

/** POST /api/admin/student/{studentId}/courses/add */
export const adminAddCourse = async (studentId, courseCode) => {
  const res = await api.post(`/admin/student/${encodeURIComponent(studentId)}/courses/add`, { courseCode });
  return res.data.data;
};

/** DELETE /api/admin/student/{studentId}/courses/{code} */
export const adminRemoveCourse = async (studentId, code) => {
  const res = await api.delete(`/admin/student/${encodeURIComponent(studentId)}/courses/${encodeURIComponent(code)}`);
  return res.data.data;
};

/** PUT /api/admin/student/{studentId}/lock/{code}?reason= */
export const adminLockCourse = async (studentId, code, reason = "") => {
  const res = await api.put(
    `/admin/student/${encodeURIComponent(studentId)}/lock/${encodeURIComponent(code)}`,
    null,
    { params: reason ? { reason } : {} }
  );
  return res.data.data;
};

/** PUT /api/admin/student/{studentId}/unlock/{code} */
export const adminUnlockCourse = async (studentId, code) => {
  const res = await api.put(`/admin/student/${encodeURIComponent(studentId)}/unlock/${encodeURIComponent(code)}`);
  return res.data.data;
};

/** PUT /api/admin/student/{studentId}/max-credits */
export const adminSetMaxCredits = async (studentId, maxCredits) => {
  const res = await api.put(`/admin/student/${encodeURIComponent(studentId)}/max-credits`, { maxCredits });
  return res.data.data;
};

/** GET /api/admin/student/{studentId}/academic-setup */
export const adminGetAcademicSetup = async (studentId) => {
  const res = await api.get(`/admin/student/${encodeURIComponent(studentId)}/academic-setup`);
  return res.data.data;
};

/** PUT /api/admin/student/{studentId}/academic-setup */
export const adminSaveAcademicSetup = async (studentId, dto) => {
  const res = await api.put(`/admin/student/${encodeURIComponent(studentId)}/academic-setup`, dto);
  return res.data.data;
};

// ── Final Grade Audit ────────────────────────────────────────

/**
 * GET /api/admin/final-grade/student/{studentCode}
 * Returns the student + all registered courses with final grade status.
 */
export const adminGetFinalGrades = async (studentCode) => {
  const res = await api.get(`/admin/final-grade/student/${encodeURIComponent(studentCode)}`);
  return res.data.data;
};

/**
 * POST /api/admin/final-grade/publish/{studentId}
 * Publishes all assigned final grades to the student.
 * @param {string} studentId - GUID
 * @param {number[]|null} courseIds - if null, publishes all assigned courses
 */
export const adminPublishFinalGrades = async (studentId, courseIds = null) => {
  const res = await api.post(
    `/admin/final-grade/publish/${encodeURIComponent(studentId)}`,
    { courseIds }
  );
  return res.data.data;
};

/**
 * POST /api/admin/final-grade/notify/{studentId}/{courseId}
 * Sends a warning notification to all instructors of this course
 * about the missing final grade for this student.
 * Backend enforces once-per-day per (instructor, student, course).
 */
export const adminNotifyInstructor = async (studentId, courseId) => {
  const res = await api.post(
    `/admin/final-grade/notify/${encodeURIComponent(studentId)}/${courseId}`
  );
  return res.data.data;
};

/**
 * GET /api/admin/final-grade/students
 * Returns all current-term students grouped by classification:
 * { progress: [...], notCompleted: [...], completed: [...], total, canPublishAll }
 * Each student: { studentId, studentName, studentCode, academicYear, registeredCourses, status }
 */
export const adminGetFinalGradeReviewList = async () => {
  const res = await api.get("/admin/final-grade/students");
  return res.data.data;
};

/**
 * POST /api/admin/final-grade/classify/{studentId}
 * Manually set a student's review classification.
 * @param {"progress"|"not_completed"|"completed"} status
 */
export const adminClassifyStudent = async (studentId, status) => {
  const res = await api.post(
    `/admin/final-grade/classify/${encodeURIComponent(studentId)}`,
    { status }
  );
  return res.data.data;
};

/**
 * POST /api/admin/final-grade/publish-all
 * Globally publishes every assigned final grade for students whose
 * classification is "completed". Backend rejects when any student is
 * still in progress / not_completed.
 */
export const adminPublishAllFinalGrades = async () => {
  const res = await api.post("/admin/final-grade/publish-all");
  return res.data.data;
};

// ── Academic Year Reset ──────────────────────────────────────

/**
 * POST /api/admin/academic-reset/preview
 * Read-only impact preview for the selected students.
 * @param {{ studentIds?: string[], selectAll?: boolean }} dto
 */
export const academicResetPreview = async (dto) => {
  const res = await api.post("/admin/academic-reset/preview", dto);
  return res.data.data;
};

/**
 * POST /api/admin/academic-reset/execute
 * Mutates state. Backend validates confirmationText + resetPassword.
 * @param {{
 *   studentIds?: string[],
 *   selectAll?: boolean,
 *   confirmationText: string,
 *   resetPassword: string,
 *   forceReset?: boolean
 * }} dto
 */
export const academicResetExecute = async (dto) => {
  const res = await api.post("/admin/academic-reset/execute", dto);
  return res.data.data;
};

// ── Permanent Student Delete (Email Manager → Danger Zone) ───

/**
 * GET /api/admin/student-delete/preview/{academicCode}
 * Read-only — returns student preview info before deletion.
 */
export const studentDeletePreview = async (academicCode) => {
  const res = await api.get(`/admin/student-delete/preview/${encodeURIComponent(academicCode)}`);
  return res.data.data;
};

/**
 * POST /api/admin/student-delete/execute
 * Permanently deletes a student account and all rows tied to them.
 * dto: { academicCode, confirmAcademicCode, password }
 * Backend rejects unless password === "StudentDelete@123#".
 */
export const studentDeleteExecute = async (dto) => {
  const res = await api.post("/admin/student-delete/execute", dto);
  return res.data.data;
};

// ── Reset Material ───────────────────────────────────────────

/**
 * GET /api/admin/material-reset/courses
 * Returns every course from the catalog, with per-course material counts.
 * Each row: { id, code, name, credits, department, assignmentCount,
 *             quizCount, lectureCount, hasMaterial }
 */
export const materialResetCourses = async () => {
  const res = await api.get("/admin/material-reset/courses");
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

/**
 * POST /api/admin/material-reset/preview
 * @param {{ courseIds?: number[], selectAll?: boolean }} dto
 */
export const materialResetPreview = async (dto) => {
  const res = await api.post("/admin/material-reset/preview", dto);
  return res.data.data;
};

/**
 * POST /api/admin/material-reset/execute
 * dto must include: { courseIds?: number[], selectAll?: boolean, password: string }
 * Password is validated server-side only.
 */
export const materialResetExecute = async (dto) => {
  const res = await api.post("/admin/material-reset/execute", dto);
  return res.data.data;
};

// ── Instructor Control ────────────────────────────────────────

/** GET /api/admin/instructor-control */
export const getInstructorControl = async () => {
  const res = await api.get("/admin/instructor-control");
  return res.data.data;
};

/** PUT /api/admin/instructor-control/{courseId} */
export const assignInstructors = async (courseId, instructorIds) => {
  const res = await api.put(`/admin/instructor-control/${courseId}`, { instructorIds });
  return res.data;
};
