import api from "./axiosInstance";

// ══════════ ADMIN — Draft sessions ══════════

/** GET /api/schedule/admin/sessions?year=&group= */
export const getAdminSessions = async (year, group) => {
  const params = {};
  if (year)  params.year  = year;
  if (group) params.group = group;
  const res = await api.get("/schedule/admin/sessions", { params });
  return res.data.data;
};

/** POST /api/schedule/admin/sessions */
export const addSession = async (dto) => {
  const res = await api.post("/schedule/admin/sessions", dto);
  return res.data.data;
};

/** DELETE /api/schedule/admin/sessions/{id} */
export const removeSession = async (id) => {
  await api.delete(`/schedule/admin/sessions/${id}`);
};

// ══════════ ADMIN — Draft exams ══════════

/** GET /api/schedule/admin/exams?year=&type= */
export const getAdminExams = async (year, examType) => {
  const params = {};
  if (year)     params.year = year;
  if (examType) params.type = examType;
  const res = await api.get("/schedule/admin/exams", { params });
  return res.data.data;
};

/** POST /api/schedule/admin/exams */
export const addExam = async (dto) => {
  const res = await api.post("/schedule/admin/exams", dto);
  return res.data.data;
};

/** DELETE /api/schedule/admin/exams/{id} */
export const removeExam = async (id) => {
  await api.delete(`/schedule/admin/exams/${id}`);
};

// ══════════ ADMIN — Publish ══════════

/** POST /api/schedule/admin/publish?year=&type= */
export const publishSchedule = async (year, type = "weekly") => {
  const res = await api.post("/schedule/admin/publish", null, { params: { year, type } });
  return res.data;
};

// ══════════ PUBLIC — Published schedule (student/instructor) ══════════

/** GET /api/schedule/published?year=&type= */
export const getPublishedSchedule = async (year, type = "weekly") => {
  const res = await api.get("/schedule/published", { params: { year, type } });
  return res.data; // { data: [...], publishedAt: "..." }
};

/** GET /api/schedule/publish-info?year= */
export const getPublishInfo = async (year) => {
  const params = {};
  if (year) params.year = year;
  const res = await api.get("/schedule/publish-info", { params });
  return res.data.data; // [{ year, type, publishedAt, publishedBy }]
};
