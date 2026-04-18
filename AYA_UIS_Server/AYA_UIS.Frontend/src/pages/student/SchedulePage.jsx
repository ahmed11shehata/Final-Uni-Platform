import { useContext, useMemo, useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { AuthContext } from "../../context/AuthContext";
import { getPublishedSchedule, getPublishInfo } from "../../services/api/scheduleApi";
import styles from "./SchedulePage.module.css";

const DAYS = ["Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday"];
const HOURS = [8, 9, 10, 11, 12, 13, 14, 15, 16];
const PX_PER_HOUR = 96;
const YEAR_LABELS = ["", "First Year", "Second Year", "Third Year", "Fourth Year"];
const YEAR_COLORS = ["", "#818cf8", "#22c55e", "#f59e0b", "#ef4444"];

const Ic = {
  spin: (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className={styles.spinIcon}>
      <circle cx="12" cy="12" r="10" strokeOpacity="0.25" />
      <path d="M12 2a10 10 0 0 1 10 10" strokeLinecap="round" />
    </svg>
  ),
};

function fmtH(h) {
  const hr = Math.floor(h);
  const m = h % 1 === 0.5 ? "30" : "00";
  const ap = hr >= 12 ? "PM" : "AM";
  const d = hr > 12 ? hr - 12 : hr === 0 ? 12 : hr;
  return `${d}:${m} ${ap}`;
}

/* ── Fallback mock data ── */
const FALLBACK = {
  3: {
    weekly: [
      { id: 1, day: "Saturday", start: 8, end: 9.5, code: "CS401", name: "Artificial Intelligence", type: "Lecture", room: "Hall A", instructor: "Dr. Sara Mahmoud", color: "#e8a838", group: "A" },
      { id: 2, day: "Saturday", start: 9.5, end: 11, code: "CS401", name: "Artificial Intelligence", type: "Lecture", room: "Hall C", instructor: "Dr. Sara Mahmoud", color: "#e8a838", group: "B" },
      { id: 3, day: "Saturday", start: 11, end: 12.5, code: "CS402", name: "Compiler Design", type: "Lecture", room: "Hall B", instructor: "Dr. Khaled Omar", color: "#7c6fc4", group: "A" },
      { id: 4, day: "Saturday", start: 12.5, end: 14, code: "CS402", name: "Compiler Design", type: "Lecture", room: "Hall D", instructor: "Dr. Khaled Omar", color: "#7c6fc4", group: "B" },
      { id: 5, day: "Sunday", start: 8, end: 9.5, code: "CS403", name: "Image Processing", type: "Lecture", room: "Lab 1", instructor: "Dr. Mona Hassan", color: "#78909c", group: "A" },
      { id: 6, day: "Sunday", start: 9.5, end: 11, code: "CS403", name: "Image Processing", type: "Lecture", room: "Lab 2", instructor: "Dr. Mona Hassan", color: "#78909c", group: "B" },
      { id: 7, day: "Sunday", start: 12, end: 13, code: "CS401", name: "Artificial Intelligence", type: "Section", room: "Room 12", instructor: "Eng. Ahmed Tarek", color: "#e8a838", group: "A" },
      { id: 8, day: "Sunday", start: 13, end: 14, code: "CS401", name: "Artificial Intelligence", type: "Section", room: "Room 14", instructor: "Eng. Ahmed Tarek", color: "#e8a838", group: "B" },
      { id: 9, day: "Monday", start: 9, end: 10.5, code: "CS404", name: "Expert Systems", type: "Lecture", room: "Hall C", instructor: "Dr. Rania Farid", color: "#e05c8a", group: "A" },
      { id: 10, day: "Monday", start: 10.5, end: 12, code: "CS404", name: "Expert Systems", type: "Lecture", room: "Hall C", instructor: "Dr. Rania Farid", color: "#e05c8a", group: "B" },
      { id: 11, day: "Monday", start: 13, end: 14, code: "CS402", name: "Compiler Design", type: "Section", room: "Lab 2", instructor: "Eng. Youssef Ali", color: "#7c6fc4", group: "A" },
      { id: 12, day: "Monday", start: 14, end: 15, code: "CS402", name: "Compiler Design", type: "Section", room: "Lab 3", instructor: "Eng. Youssef Ali", color: "#7c6fc4", group: "B" },
      { id: 13, day: "Tuesday", start: 8, end: 9.5, code: "CS405", name: "NLP", type: "Lecture", room: "Hall A", instructor: "Dr. Sara Mahmoud", color: "#5b9fb5", group: "A" },
      { id: 14, day: "Tuesday", start: 9.5, end: 11, code: "CS405", name: "NLP", type: "Lecture", room: "Hall A", instructor: "Dr. Sara Mahmoud", color: "#5b9fb5", group: "B" },
      { id: 15, day: "Tuesday", start: 11, end: 12.5, code: "CS406", name: "OS Theory", type: "Lecture", room: "Hall D", instructor: "Dr. Tamer Ali", color: "#3d8fe0", group: "A" },
      { id: 16, day: "Tuesday", start: 12.5, end: 14, code: "CS406", name: "OS Theory", type: "Lecture", room: "Hall D", instructor: "Dr. Tamer Ali", color: "#3d8fe0", group: "B" },
      { id: 17, day: "Wednesday", start: 8, end: 9, code: "CS403", name: "Image Processing", type: "Section", room: "Lab 3", instructor: "Eng. Nour Hamed", color: "#78909c", group: "A" },
      { id: 18, day: "Wednesday", start: 9, end: 10, code: "CS403", name: "Image Processing", type: "Section", room: "Lab 4", instructor: "Eng. Nour Hamed", color: "#78909c", group: "B" },
      { id: 19, day: "Wednesday", start: 11, end: 12, code: "CS404", name: "Expert Systems", type: "Section", room: "Room 8", instructor: "Eng. Dina Samir", color: "#e05c8a", group: "A" },
      { id: 20, day: "Wednesday", start: 12, end: 13, code: "CS404", name: "Expert Systems", type: "Section", room: "Room 9", instructor: "Eng. Dina Samir", color: "#e05c8a", group: "B" },
      { id: 21, day: "Thursday", start: 9, end: 10, code: "CS405", name: "NLP", type: "Section", room: "Lab 1", instructor: "Eng. Kareem Nabil", color: "#5b9fb5", group: "A" },
      { id: 22, day: "Thursday", start: 10, end: 11, code: "CS405", name: "NLP", type: "Section", room: "Lab 1", instructor: "Eng. Kareem Nabil", color: "#5b9fb5", group: "B" },
      { id: 23, day: "Thursday", start: 13, end: 14, code: "CS406", name: "OS Theory", type: "Section", room: "Room 5", instructor: "Eng. Hany Fouad", color: "#3d8fe0", group: "A" },
      { id: 24, day: "Thursday", start: 14, end: 15, code: "CS406", name: "OS Theory", type: "Section", room: "Room 6", instructor: "Eng. Hany Fouad", color: "#3d8fe0", group: "B" },
    ],
    midterm: [
      { id: 1, code: "CS401", name: "Artificial Intelligence", date: "2026-04-05", time: "10:00 AM", hall: "Hall A — 2nd Floor", duration: 2, color: "#e8a838" },
      { id: 2, code: "CS402", name: "Compiler Design", date: "2026-04-07", time: "12:00 PM", hall: "Hall B — 1st Floor", duration: 2, color: "#7c6fc4" },
      { id: 3, code: "CS403", name: "Image Processing", date: "2026-04-08", time: "10:00 AM", hall: "Lab Hall — 3rd Floor", duration: 2.5, color: "#78909c" },
      { id: 4, code: "CS404", name: "Expert Systems", date: "2026-04-09", time: "08:00 AM", hall: "Hall C — 2nd Floor", duration: 2, color: "#e05c8a" },
      { id: 5, code: "CS405", name: "Natural Language Processing", date: "2026-04-10", time: "10:00 AM", hall: "Hall A — 2nd Floor", duration: 2, color: "#5b9fb5" },
      { id: 6, code: "CS406", name: "OS Theory", date: "2026-04-12", time: "02:00 PM", hall: "Hall D — 1st Floor", duration: 1.5, color: "#3d8fe0" },
    ],
    final: [
      { id: 1, code: "CS401", name: "Artificial Intelligence", date: "2026-06-14", time: "09:00 AM", hall: "Main Hall A", duration: 3, color: "#e8a838" },
      { id: 2, code: "CS402", name: "Compiler Design", date: "2026-06-16", time: "11:00 AM", hall: "Main Hall B", duration: 3, color: "#7c6fc4" },
      { id: 3, code: "CS403", name: "Image Processing", date: "2026-06-17", time: "09:00 AM", hall: "Lab Hall 1", duration: 3, color: "#78909c" },
      { id: 4, code: "CS404", name: "Expert Systems", date: "2026-06-18", time: "11:00 AM", hall: "Main Hall C", duration: 3, color: "#e05c8a" },
      { id: 5, code: "CS405", name: "Natural Language Processing", date: "2026-06-19", time: "09:00 AM", hall: "Main Hall A", duration: 3, color: "#5b9fb5" },
      { id: 6, code: "CS406", name: "OS Theory", date: "2026-06-21", time: "01:00 PM", hall: "Main Hall D", duration: 2.5, color: "#3d8fe0" },
    ],
  },
};

export default function SchedulePage() {
  const { user } = useContext(AuthContext) ?? {};
  const [yr, setYr] = useState(user?.studyYear ?? 3);
  const [grp, setGrp] = useState("A");
  const [view, setView] = useState("weekly");
  const [examT, setExamT] = useState("midterm");
  const [scheduleData, setScheduleData] = useState({});
  const [publishInfo, setPublishInfo] = useState([]);
  const [loadingSched, setLoadingSched] = useState(false);

  useEffect(() => {
    if (user?.studyYear && ![1,2,3,4].includes(yr)) setYr(user.studyYear);
  }, [user, yr]);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoadingSched(true);
      try {
        const type = view === "weekly" ? "weekly" : examT;
        const res = await getPublishedSchedule(yr, type);
        if (!cancelled && res?.data) {
          setScheduleData((prev) => ({
            ...prev,
            [`${yr}_${type}`]: { data: res.data, publishedAt: res.publishedAt },
          }));
        }
      } catch {
        // fallback data below
      } finally {
        if (!cancelled) setLoadingSched(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, [yr, view, examT]);

  useEffect(() => {
    let cancelled = false;
    async function loadInfo() {
      try {
        const info = await getPublishInfo(yr);
        if (!cancelled && info) setPublishInfo(info);
      } catch {}
    }
    loadInfo();
    return () => { cancelled = true; };
  }, [yr]);

  const dataKey = `${yr}_${view === "weekly" ? "weekly" : examT}`;
  const apiData = scheduleData[dataKey];
  const fallbackYear = FALLBACK[yr] || { weekly: [], midterm: [], final: [] };

  const currentList = useMemo(() => {
    if (apiData?.data && Array.isArray(apiData.data) && apiData.data.length > 0) {
      if (view === "weekly") {
        return apiData.data.map((s) => ({
          ...s,
          start: s.startTime ?? s.start,
          end: s.endTime ?? s.end,
          color: s.color || YEAR_COLORS[yr],
        }));
      }
      return apiData.data.map((e) => ({
        ...e,
        time: e.startTime != null ? fmtH(e.startTime) : (e.time || "—"),
        hall: e.location ?? e.hall ?? "—",
        color: e.color || YEAR_COLORS[yr],
      }));
    }
    return view === "weekly" ? fallbackYear.weekly : examT === "midterm" ? fallbackYear.midterm : fallbackYear.final;
  }, [apiData, view, examT, fallbackYear, yr]);

  const sessions = useMemo(
    () => (view === "weekly" ? currentList.filter((s) => s.group === grp) : []),
    [currentList, grp, view]
  );

  const exams = useMemo(
    () => (view === "exams" ? currentList : []),
    [currentList, view]
  );

  const yc = YEAR_COLORS[yr];
  const pubType = view === "weekly" ? "weekly" : examT;
  const pub = publishInfo.find?.((p) => p.type === pubType);
  const freshnessText = useMemo(() => {
    if (!pub?.publishedAt) return "Published view";
    const diff = (Date.now() - new Date(pub.publishedAt).getTime()) / 86400000;
    if (diff <= 3) return "Recently updated";
    return `Published ${Math.floor(diff)}d ago`;
  }, [pub]);

  return (
    <div className={styles.page}>
      <motion.div className={styles.header} initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }}>
        <div className={styles.headerL}>
          <h1 className={styles.headerTitle}>Schedule</h1>
          <span className={styles.headerSub}>Published View</span>
        </div>
      </motion.div>

      <div className={styles.controls}>
        <div className={styles.ctrlL}>
          <div className={styles.yearTabs}>
            {[1, 2, 3, 4].map((y) => (
              <button
                key={y}
                className={`${styles.yearTab} ${yr === y ? styles.yearTabOn : ""}`}
                style={yr === y ? { "--yc": YEAR_COLORS[y] } : {}}
                onClick={() => setYr(y)}
              >
                <span className={styles.ytNum}>Year {y}</span>
              </button>
            ))}
          </div>
        </div>

        <div className={styles.ctrlR}>
          <div className={styles.seg}>
            {[{ k: "weekly", l: "Weekly" }, { k: "exams", l: "Exams" }].map((item) => (
              <button
                key={item.k}
                className={`${styles.segBtn} ${view === item.k ? styles.segOn : ""}`}
                onClick={() => setView(item.k)}
              >
                {item.l}
              </button>
            ))}
          </div>

          {view === "weekly" && (
            <div className={styles.seg}>
              {["A", "B"].map((g) => (
                <button
                  key={g}
                  className={`${styles.segBtn} ${grp === g ? styles.segOn : ""}`}
                  onClick={() => setGrp(g)}
                >
                  Group {g}
                </button>
              ))}
            </div>
          )}

          {view === "exams" && (
            <div className={styles.seg}>
              {[{ k: "midterm", l: "Midterm" }, { k: "final", l: "Final" }].map((t) => (
                <button
                  key={t.k}
                  className={`${styles.segBtn} ${examT === t.k ? styles.segOn : ""}`}
                  onClick={() => setExamT(t.k)}
                >
                  {t.l}
                </button>
              ))}
            </div>
          )}
        </div>
      </div>

      <div className={styles.yrBanner} style={{ "--yc": yc }}>
        <span className={styles.yrBannerDot} />
        <span className={styles.yrBannerText}>
          {YEAR_LABELS[yr]}
          {view === "weekly" ? ` · Group ${grp}` : ` · ${examT === "midterm" ? "Midterm" : "Final"} Exams`}
        </span>
        <span className={styles.yrBannerCount}>
          {view === "weekly"
            ? `${sessions.length} session${sessions.length !== 1 ? "s" : ""} published`
            : `${exams.length} exam${exams.length !== 1 ? "s" : ""} published`}
        </span>
        <span className={styles.yrBannerHint}>
          {loadingSched ? Ic.spin : freshnessText}
        </span>
      </div>

      <AnimatePresence mode="wait">
        {view === "weekly" ? (
          <motion.div
            key={`weekly-${yr}-${grp}`}
            className={styles.scheduleWrap}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
          >
            {sessions.length === 0 && !loadingSched ? (
              <div className={styles.empty}>
                <p className={styles.emptyTitle}>No sessions published for {YEAR_LABELS[yr]}</p>
                <p className={styles.emptyHint}>There is no published weekly schedule for this group right now.</p>
              </div>
            ) : (
              <div className={styles.grid}>
                <div className={styles.timeCol}>
                  <div className={styles.timeColHead}>Time</div>
                  {HOURS.map((h) => (
                    <div key={h} className={styles.timeSlot} style={{ height: PX_PER_HOUR }}>
                      <span className={styles.timeLabel}>{fmtH(h)}</span>
                    </div>
                  ))}
                </div>

                {DAYS.map((day) => (
                  <DayCol key={day} day={day} sessions={sessions.filter((s) => s.day === day)} yc={yc} />
                ))}
              </div>
            )}
          </motion.div>
        ) : (
          <motion.div
            key={`exams-${yr}-${examT}`}
            className={styles.examsWrap}
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
          >
            {exams.length === 0 && !loadingSched ? (
              <div className={styles.empty}>
                <p className={styles.emptyTitle}>No {examT} exams published for {YEAR_LABELS[yr]}</p>
                <p className={styles.emptyHint}>There is no published exam schedule for this year yet.</p>
              </div>
            ) : (
              <motion.div className={styles.examsGrid} initial="hidden" animate="show" variants={{ show: { transition: { staggerChildren: 0.05 } } }}>
                {exams.map((exam) => (
                  <ExamCard key={exam.id} exam={exam} />
                ))}
              </motion.div>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

function DayCol({ day, sessions, yc }) {
  const SH = 8;
  return (
    <div className={styles.dayCol}>
      <div className={styles.dayHead}>{day.slice(0, 3)}</div>
      <div className={styles.dayBody} style={{ height: HOURS.length * PX_PER_HOUR }}>
        {HOURS.map((h) => (
          <div key={h} className={styles.hourLine} style={{ top: (h - SH) * PX_PER_HOUR }} />
        ))}

        {sessions.map((s) => {
          const top = (s.start - SH) * PX_PER_HOUR + 3;
          const ht = (s.end - s.start) * PX_PER_HOUR - 6;
          const isLecture = s.type === "Lecture";
          const c = s.color || yc;

          return (
            <motion.div
              key={s.id}
              className={styles.sessBlock}
              style={{
                top,
                height: ht,
                borderLeftColor: c,
                "--sc": c,
                background: `color-mix(in srgb, ${c} 8%, var(--card-bg))`,
              }}
              initial={{ opacity: 0, scaleY: 0.7 }}
              animate={{ opacity: 1, scaleY: 1 }}
              transition={{ type: "spring", stiffness: 380, damping: 28 }}
            >
              <div className={styles.sessTop}>
                <span className={styles.sessCode} style={{ background: c }}>{s.code}</span>
                <span className={`${styles.sessType} ${isLecture ? styles.sessL : styles.sessS}`}>
                  {isLecture ? "LEC" : s.type === "Lab" ? "LAB" : "SEC"}
                </span>
              </div>

              {ht >= 40 && <div className={styles.sessName}>{s.name}</div>}
              {ht >= 60 && s.instructor && <div className={styles.sessMeta}>{s.instructor}</div>}
              {ht >= 76 && s.room && <div className={styles.sessMeta}>{s.room}</div>}
              {ht >= 90 && <div className={styles.sessTime}>{fmtH(s.start)} – {fmtH(s.end)}</div>}

              <span className={styles.sessGrp} style={{ background: `${c}18`, color: c }}>{s.group}</span>
            </motion.div>
          );
        })}
      </div>
    </div>
  );
}

const fadeUp = {
  hidden: { opacity: 0, y: 12 },
  show: { opacity: 1, y: 0, transition: { duration: 0.35, ease: [0.22, 1, 0.36, 1] } },
};

function ExamCard({ exam }) {
  const d = exam.date ? new Date(`${exam.date}T12:00`) : null;
  const mo = d ? d.toLocaleDateString("en-US", { month: "short" }) : "—";
  const dy = d ? d.getDate() : "—";
  const wk = d ? d.toLocaleDateString("en-US", { weekday: "short" }) : "—";
  const dl = d ? Math.ceil((d - new Date()) / 86400000) : null;
  const cd = dl === null
    ? null
    : dl < 0
      ? { l: "Passed", c: "#6b7280", bg: "rgba(107,114,128,0.08)" }
      : dl === 0
        ? { l: "Today!", c: "#ef4444", bg: "rgba(239,68,68,0.08)" }
        : dl <= 5
          ? { l: `${dl}d`, c: "#f59e0b", bg: "rgba(245,158,11,0.08)" }
          : { l: `${dl}d`, c: exam.color, bg: `${exam.color}14` };

  return (
    <motion.div className={styles.examCard} variants={fadeUp}>
      <div className={styles.examStripe} style={{ background: exam.color }} />
      <div className={styles.examBody}>
        <div className={styles.examTile} style={{ background: `${exam.color}0c`, borderColor: `${exam.color}28` }}>
          <div className={styles.examMo} style={{ color: exam.color }}>{mo}</div>
          <div className={styles.examDy}>{dy}</div>
          <div className={styles.examWk}>{wk}</div>
        </div>

        <div className={styles.examInfo}>
          <div className={styles.examCode} style={{ color: exam.color }}>{exam.code}</div>
          <div className={styles.examName}>{exam.name}</div>
          <div className={styles.examMeta}>
            <span>{exam.time}</span>
            <span>{exam.hall}</span>
            <span>{exam.duration}h</span>
          </div>
        </div>

        <div className={styles.examR}>
          {cd && <div className={styles.examCd} style={{ color: cd.c, background: cd.bg, borderColor: `${cd.c}28` }}>{cd.l}</div>}
        </div>
      </div>
    </motion.div>
  );
}
