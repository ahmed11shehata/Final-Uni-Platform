import api from "./axiosInstance";

// ── Courses ──────────────────────────────────────────────────
/** GET /api/instructor/courses — courses this instructor is assigned to */
export const getInstructorCourses = async () => {
  const res = await api.get("/instructor/courses");
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

/** GET /api/instructor/dashboard — full dashboard data (courses, gradeSummary, activity, upcoming) */
export const getInstructorDashboard = async () => {
  const res = await api.get("/instructor/dashboard");
  const d = res.data?.data || {};
  return {
    courses:       Array.isArray(d.courses)       ? d.courses       : [],
    gradeSummary:  d.gradeSummary  || {},
    recentActivity:Array.isArray(d.recentActivity)? d.recentActivity: [],
    upcoming:      Array.isArray(d.upcoming)       ? d.upcoming       : [],
  };
};

/** GET /api/instructor/courses/{courseId}/students */
export const getCourseStudents = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/students`);
  return res.data.data;
};

// ── Assignments ──────────────────────────────────────────────
/** GET /api/instructor/assignments?courseId={id} */
export const getInstructorAssignments = async (courseId) => {
  const res = await api.get("/instructor/assignments", { params: { courseId } });
  return res.data.data;
};

/**
 * POST /api/instructor/assignments  (multipart/form-data)
 * dto: { title, description, courseCode, deadline (ISO), maxGrade, allowedFormats[] }
 * file: optional File object — instructor attachment/starter file
 */
export const createAssignment = async (dto, file = null) => {
  const form = new FormData();
  form.append("title", dto.title ?? "");
  if (dto.description) form.append("description", dto.description);
  form.append("courseCode", dto.courseCode ?? "");
  form.append("deadline", dto.deadline ?? "");
  form.append("maxGrade", String(dto.maxGrade ?? 20));
  if (dto.releaseDate) form.append("releaseDate", dto.releaseDate);
  (dto.allowedFormats ?? []).forEach((f) => form.append("allowedFormats", f));
  if (file) form.append("attachmentFile", file);

  const res = await api.post("/instructor/assignments", form);
  return res.data.data;
};

/**
 * PUT /api/instructor/assignments/{id}  (multipart/form-data)
 * Supports replacing the attachment file. If `file` is omitted and
 * `dto.removeAttachment === true`, the existing attachment is dropped.
 *
 * `dto` shape: { title?, description?, deadline? (ISO), releaseDate?,
 *                clearReleaseDate?, maxGrade?, removeAttachment? }
 */
export const updateAssignment = async (id, dto, file = null) => {
  const form = new FormData();
  if (dto.title       != null) form.append("title",       dto.title);
  if (dto.description != null) form.append("description", dto.description);
  if (dto.deadline    != null) form.append("deadline",    dto.deadline);
  if (dto.releaseDate)         form.append("releaseDate", dto.releaseDate);
  if (dto.clearReleaseDate)    form.append("clearReleaseDate", "true");
  if (dto.maxGrade    != null) form.append("maxGrade",    String(dto.maxGrade));
  if (dto.removeAttachment)    form.append("removeAttachment", "true");
  if (file)                    form.append("attachmentFile", file);

  const res = await api.put(`/instructor/assignments/${id}`, form);
  return res.data.data;
};

/** DELETE /api/instructor/assignments/{id} */
export const deleteAssignment = async (id) => {
  const res = await api.delete(`/instructor/assignments/${id}`);
  return res.data;
};

// ── Submissions ──────────────────────────────────────────────
/** GET /api/instructor/assignments/{id}/submissions */
export const getSubmissions = async (assignmentId) => {
  const res = await api.get(`/instructor/assignments/${assignmentId}/submissions`);
  return res.data.data;
};

/** POST /api/instructor/assignments/{aId}/submissions/{sId}/accept */
export const acceptSubmission = async (assignmentId, submissionId) => {
  const res = await api.post(
    `/instructor/assignments/${assignmentId}/submissions/${submissionId}/accept`
  );
  return res.data.data;
};

/**
 * POST /api/instructor/assignments/{aId}/submissions/{sId}/reject
 * body: { reason }
 */
export const rejectSubmission = async (assignmentId, submissionId, reason) => {
  const res = await api.post(
    `/instructor/assignments/${assignmentId}/submissions/${submissionId}/reject`,
    { reason }
  );
  return res.data.data;
};

// ── Quizzes ──────────────────────────────────────────────────
/** GET /api/instructor/courses/{courseId}/quizzes */
export const getInstructorQuizzes = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/quizzes`);
  return res.data.data;
};

/**
 * POST /api/instructor/courses/{courseId}/quizzes
 * dto: { title, duration, startTime (ISO), endTime (ISO), gradePerQ, questions[] }
 * questions[]: { text, imageUrl?, answers: [{text}], correct: index }
 */
export const createQuiz = async (courseId, dto) => {
  const res = await api.post(`/instructor/courses/${courseId}/quizzes`, dto);
  return res.data.data;
};

/** GET /api/instructor/courses/{courseId}/quizzes/{quizId}/attempts */
export const getQuizAttempts = async (courseId, quizId) => {
  const res = await api.get(`/instructor/courses/${courseId}/quizzes/${quizId}/attempts`);
  return res.data.data;
};

/**
 * PUT /api/instructor/courses/{courseId}/quizzes/{quizId}
 * dto: { title?, startTime? (ISO), endTime? (ISO), questions?: [{ text, answers: [{text}], correct }] }
 * If attempts already exist, omit `questions` (backend rejects question changes after attempts).
 */
export const updateQuiz = async (courseId, quizId, dto) => {
  const res = await api.put(`/instructor/courses/${courseId}/quizzes/${quizId}`, dto);
  return res.data.data;
};

/** DELETE /api/instructor/courses/{courseId}/quizzes/{quizId} */
export const deleteQuiz = async (courseId, quizId) => {
  const res = await api.delete(`/instructor/courses/${courseId}/quizzes/${quizId}`);
  return res.data;
};

// ── Materials / Lectures ─────────────────────────────────────
/** GET /api/instructor/courses/{courseId}/materials */
export const getCourseMaterials = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/materials`);
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

/**
 * POST /api/instructor/courses/{courseId}/materials  (multipart)
 * @param {number} courseId
 * @param {{ title: string, description?: string, week?: number, releaseDate?: string|null }} meta
 * @param {File} file
 */
export const uploadLecture = async (courseId, meta, file) => {
  const form = new FormData();
  form.append("title", meta.title);
  if (meta.description) form.append("description", meta.description);
  if (meta.week != null)  form.append("week", String(meta.week));
  if (meta.releaseDate)   form.append("releaseDate", meta.releaseDate);
  if (file)               form.append("file", file);
  const res = await api.post(`/instructor/courses/${courseId}/materials`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return res.data.data;
};

/**
 * PUT /api/instructor/courses/{courseId}/materials/{id}  (multipart)
 * Replacing `file` deletes the old physical file from storage.
 * meta: { title?, description?, type?, week?, releaseDate? }
 */
export const updateLecture = async (courseId, materialId, meta, file = null) => {
  const form = new FormData();
  if (meta.title       != null) form.append("title",       meta.title);
  if (meta.description != null) form.append("description", meta.description);
  if (meta.type        != null) form.append("type",        meta.type);
  if (meta.week        != null) form.append("week",        String(meta.week));
  if (meta.releaseDate)         form.append("releaseDate", meta.releaseDate);
  if (file)                     form.append("file", file);
  const res = await api.put(`/instructor/courses/${courseId}/materials/${materialId}`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return res.data.data;
};

/** DELETE /api/instructor/courses/{courseId}/materials/{id} */
export const deleteLecture = async (courseId, materialId) => {
  const res = await api.delete(`/instructor/courses/${courseId}/materials/${materialId}`);
  return res.data;
};

// ── Final Grade ──────────────────────────────────────────────
/** GET /api/instructor/courses/{courseId}/final-grade/status */
export const getFinalGradeStatus = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/final-grade/status`);
  return res.data.data;
};

/** GET /api/instructor/courses/{courseId}/final-grade/students */
export const getFinalGradeStudents = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/final-grade/students`);
  return Array.isArray(res.data?.data) ? res.data.data : [];
};

/**
 * PUT /api/instructor/courses/{courseId}/final-grade/{studentId}
 * dto: { finalScore (0-60), bonus (0-10) }
 */
export const setFinalGrade = async (courseId, studentId, dto) => {
  const res = await api.put(`/instructor/courses/${courseId}/final-grade/${studentId}`, dto);
  return res.data.data;
};

// ── Midterm ──────────────────────────────────────────────────
/** GET /api/instructor/courses/{courseId}/midterm — all student grades */
export const getMidtermGrades = async (courseId) => {
  const res = await api.get(`/instructor/courses/${courseId}/midterm`);
  return res.data.data;
};

/**
 * PUT /api/instructor/courses/{courseId}/midterm/{studentId}
 * dto: { grade, max, published }
 */
export const setMidtermGrade = async (courseId, studentId, dto) => {
  const res = await api.put(`/instructor/courses/${courseId}/midterm/${studentId}`, dto);
  return res.data.data;
};

/**
 * PATCH /api/instructor/courses/{courseId}/midterm/publish?publish=true|false
 */
export const publishMidterm = async (courseId, publish = true) => {
  const res = await api.patch(`/instructor/courses/${courseId}/midterm/publish`, null, {
    params: { publish },
  });
  return res.data;
};
