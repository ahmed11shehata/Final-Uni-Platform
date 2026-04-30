// src/pages/student/TimetablePage.jsx
import { useState, useRef, useCallback, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import styles from "./TimetablePage.module.css";
import { getStudentTimetableEvents } from "../../services/api/studentApi";

// ── Type config ──────────────────────────────────────────────────
const TYPE = {
  quiz: {
    label:"Quiz", color:"#7c3aed", light:"rgba(124,58,237,0.08)", border:"rgba(124,58,237,0.2)",
    route:"/student/quizzes",
    icon:(
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11"/>
      </svg>
    ),
  },
  assignment: {
    label:"Assignment", color:"#e11d48", light:"rgba(225,29,72,0.07)", border:"rgba(225,29,72,0.2)",
    route:"/student/courses",
    icon:(
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/>
        <polyline points="14 2 14 8 20 8"/>
        <line x1="16" y1="13" x2="8" y2="13"/><line x1="13" y1="17" x2="8" y2="17"/>
      </svg>
    ),
  },
  lecture: {
    label:"New Lecture", color:"#0369a1", light:"rgba(3,105,161,0.07)", border:"rgba(3,105,161,0.2)",
    route:"/student/courses",
    icon:(
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <path d="M2 3h6a4 4 0 014 4v14a3 3 0 00-3-3H2z"/>
        <path d="M22 3h-6a4 4 0 00-4 4v14a3 3 0 013-3h7z"/>
      </svg>
    ),
  },
};

const MONTHS    = ["January","February","March","April","May","June","July","August","September","October","November","December"];
const DAYS_S    = ["Sun","Mon","Tue","Wed","Thu","Fri","Sat"];
const DAYS_FULL = ["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"];

const toDate   = (s) => new Date(s + "T00:00:00");
const daysLeft = (s) => { const t=new Date(); t.setHours(0,0,0,0); return Math.ceil((toDate(s)-t)/86400000); };
const sameDay  = (a,b) => a.getFullYear()===b.getFullYear()&&a.getMonth()===b.getMonth()&&a.getDate()===b.getDate();
const isAbsoluteUrl = (s) => typeof s === "string" && /^https?:\/\//i.test(s);

// Calm palette for course-based coloring — consistent per course code/id.
const COURSE_PALETTE = [
  "#6366f1", // indigo
  "#10b981", // emerald
  "#f59e0b", // amber
  "#ec4899", // pink
  "#0ea5e9", // sky
  "#8b5cf6", // violet
  "#14b8a6", // teal
  "#f97316", // orange
  "#a855f7", // purple
  "#22c55e", // green
  "#3b82f6", // blue
  "#eab308", // yellow
];
function hashKey(s) {
  let h = 0;
  const str = String(s ?? "");
  for (let i = 0; i < str.length; i++) h = ((h * 31) + str.charCodeAt(i)) >>> 0;
  return h;
}
function getCourseColor(courseCode, courseId) {
  const key = courseCode || courseId;
  if (!key) return COURSE_PALETTE[0];
  return COURSE_PALETTE[hashKey(key) % COURSE_PALETTE.length];
}

// Calendar placement uses publish/release/start date — never deadline.
// "Publish now" assignments arrive without a release timestamp; place them
// on today (or, for already-expired ones, fall back to deadline so the
// chip is still visible).
function pickPrimaryIso(raw) {
  const todayIso = (() => {
    const t = new Date(); t.setHours(0,0,0,0); return t.toISOString();
  })();

  if (raw.type === "assignment") {
    if (raw.releaseAt) return raw.releaseAt;
    if (raw.isExpired && raw.deadlineAt) return raw.deadlineAt;
    return todayIso;
  }
  if (raw.type === "quiz") {
    return raw.startAt || raw.releaseAt || todayIso;
  }
  if (raw.type === "lecture") {
    return raw.releaseAt || raw.startAt || todayIso;
  }
  return raw.startAt || raw.releaseAt || raw.deadlineAt || todayIso;
}
function fmtTimeLocal(iso) {
  if (!iso) return "";
  return new Date(iso).toLocaleTimeString([], { hour:"numeric", minute:"2-digit" });
}
function isoToLocalDateString(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  const y = d.getFullYear();
  const m = String(d.getMonth()+1).padStart(2,"0");
  const dd= String(d.getDate()).padStart(2,"0");
  return `${y}-${m}-${dd}`;
}
function durationMinutes(startIso, endIso) {
  if (!startIso || !endIso) return null;
  const mins = Math.round((new Date(endIso) - new Date(startIso)) / 60000);
  return mins > 0 ? `${mins} min` : null;
}
function normalizeEvent(raw) {
  const primaryIso = pickPrimaryIso(raw);
  return {
    id:             raw.id,
    sourceId:       raw.sourceId,
    type:           raw.type,
    title:          raw.title,
    courseId:       raw.courseId ?? null,
    courseCode:     raw.courseCode || "",
    course:         raw.courseName || raw.courseCode || "",
    date:           isoToLocalDateString(primaryIso),
    time:           raw.type === "lecture" ? "Uploaded" : fmtTimeLocal(primaryIso),
    duration:       raw.type === "quiz" ? durationMinutes(raw.startAt, raw.endAt) : null,
    status:         raw.status,
    isAvailable:    !!raw.isAvailable,
    isExpired:      !!raw.isExpired,
    actionUrl:      raw.actionUrl || null,
    attachmentUrl:  raw.attachmentUrl || null,
    lockedMessage:  raw.lockedMessage || null,
    expiredMessage: raw.expiredMessage || null,
    description:    raw.description || "",
    releaseAt:      raw.releaseAt || null,
    startAt:        raw.startAt   || null,
    endAt:          raw.endAt     || null,
    deadlineAt:     raw.deadlineAt|| null,
  };
}

// ── Status badge ─────────────────────────────────────────────────
function StatusBadge({ days, type, ev }) {
  // Authoritative status comes from backend when present
  if (ev?.status === "completed") return <span className={`${styles.badge} ${styles.bWeek}`}>Completed</span>;
  if (ev?.isExpired)              return <span className={`${styles.badge} ${styles.bExp}`}>Expired</span>;
  if (type==="lecture")           return <span className={`${styles.badge} ${styles.bNew}`}>New Upload</span>;
  if (days < 0)                   return <span className={`${styles.badge} ${styles.bExp}`}>Expired</span>;
  if (days === 0)                 return <span className={`${styles.badge} ${styles.bToday}`}>Due Today!</span>;
  if (days === 1)                 return <span className={`${styles.badge} ${styles.bTmrw}`}>Tomorrow</span>;
  if (days <= 3)                  return <span className={`${styles.badge} ${styles.bSoon}`}>In {days} days</span>;
  if (days <= 7)                  return <span className={`${styles.badge} ${styles.bWeek}`}>This week</span>;
  return <span className={`${styles.badge} ${styles.bFar}`}>{days} days left</span>;
}

// ── Event Popup ──────────────────────────────────────────────────
function EventPopup({ ev, onClose }) {
  const navigate = useNavigate();
  const tm      = TYPE[ev.type] || TYPE.lecture;
  const days    = daysLeft(ev.date);
  const d       = toDate(ev.date);
  const expired = ev.isExpired;
  const urgPct  = expired ? 100 : Math.max(5, Math.min(95, 100 - (days/30)*100));
  const urgCol  = days<=0 ? "#ef4444" : days<=3 ? "#f59e0b" : days<=7 ? "#a78bfa" : tm.color;

  const ctaLabel =
    ev.type === "quiz"       ? (ev.status === "completed" ? "Review Quiz" : "Open Quiz") :
    ev.type === "assignment" ? "Open Assignment" :
    ev.type === "lecture"    ? "Open Material"   : "Open";

  const handleAction = () => {
    if (!ev.actionUrl) return;
    onClose();
    if (isAbsoluteUrl(ev.actionUrl) || ev.actionUrl.startsWith("/uploads") || ev.actionUrl.startsWith("/wwwroot")) {
      window.open(ev.actionUrl, "_blank", "noopener,noreferrer");
    } else {
      navigate(ev.actionUrl);
    }
  };

  // Action gating mirrors backend authority
  const canOpen   = ev.isAvailable && !!ev.actionUrl;
  const canReview = ev.status === "completed" && !!ev.actionUrl;
  const lockedMsg = ev.lockedMessage || ev.expiredMessage ||
    (ev.status === "completed" ? "You have submitted this quiz." : "This event has passed. Contact your instructor if needed.");

  return (
    <motion.div className={styles.overlay}
      initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}
      transition={{duration:0.15}} onClick={onClose}
    >
      <motion.div className={styles.popup}
        initial={{opacity:0, scale:0.8, y:32}}
        animate={{opacity:1, scale:1,   y:0 }}
        exit={{  opacity:0, scale:0.9,  y:16}}
        transition={{type:"spring", stiffness:460, damping:32}}
        onClick={e=>e.stopPropagation()}
      >
        {/* Top color bar */}
        <div className={styles.popBar} style={{background:`linear-gradient(90deg,${tm.color},${tm.color}88)`}}/>

        {/* Header */}
        <div className={styles.popHead}>
          <div className={styles.popIconBox} style={{background:`${tm.color}14`, border:`1.5px solid ${tm.color}28`, color:tm.color}}>
            {tm.icon}
          </div>
          <div style={{flex:1,minWidth:0}}>
            <div className={styles.popTags}>
              <span className={styles.popTypeBadge} style={{background:`${tm.color}15`, color:tm.color}}>{tm.label}</span>
              <StatusBadge days={days} type={ev.type} ev={ev}/>
            </div>
            <h2 className={styles.popTitle}>{ev.title}</h2>
          </div>
          <motion.button className={styles.popClose} onClick={onClose}
            whileHover={{scale:1.12, rotate:90}} whileTap={{scale:0.88}} transition={{duration:0.15}}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </motion.button>
        </div>

        {/* Info cells */}
        <div className={styles.popGrid}>
          <div className={styles.popCell}>
            <span className={styles.popCellIco}>📚</span>
            <div><div className={styles.popLbl}>Course</div><div className={styles.popVal}>{ev.course}</div><div className={styles.popSub}>{ev.courseCode}</div></div>
          </div>
          <div className={styles.popCell}>
            <span className={styles.popCellIco}>📅</span>
            <div><div className={styles.popLbl}>Date</div><div className={styles.popVal}>{DAYS_FULL[d.getDay()]}, {d.getDate()} {MONTHS[d.getMonth()]}</div><div className={styles.popSub}>{d.getFullYear()}</div></div>
          </div>
          <div className={styles.popCell}>
            <span className={styles.popCellIco}>🕐</span>
            <div><div className={styles.popLbl}>Time</div><div className={styles.popVal}>{ev.time}</div>{ev.duration&&<div className={styles.popSub}>{ev.duration}</div>}</div>
          </div>
          {ev.type!=="lecture" && (
            <div className={styles.popCell}>
              <span className={styles.popCellIco}>⏳</span>
              <div>
                <div className={styles.popLbl}>Deadline</div>
                <div className={styles.popVal} style={{color: expired?"#ef4444": days<=3?"#f59e0b": "#1a1235"}}>
                  {expired?"Expired": days===0?"Today!": days===1?"Tomorrow":`${days} days`}
                </div>
                <div className={styles.popSub}>{expired?"Past due":"Remaining"}</div>
              </div>
            </div>
          )}
        </div>

        {/* Urgency bar */}
        {ev.type !== "lecture" && (
          <div className={styles.urgWrap}>
            <div className={styles.urgTrack}>
              <motion.div className={styles.urgFill} style={{background:urgCol}}
                initial={{width:0}} animate={{width:`${urgPct}%`}}
                transition={{duration:0.65, ease:[0.22,1,0.36,1], delay:0.1}}
              />
            </div>
            <p className={styles.urgTxt} style={{color:urgCol}}>
              {expired?"⛔ This deadline has passed":
               days===0?"⚠️ Due today — don't miss it!":
               days===1?"⏰ Only 1 day remaining!":
               `📅 ${days} days remaining`}
            </p>
          </div>
        )}

        {/* CTA / locked-or-expired note */}
        {canOpen ? (
          <div className={styles.popFoot}>
            <motion.button className={styles.ctaBtn}
              style={{background:`linear-gradient(135deg,${tm.color} 0%,${tm.color}bb 100%)`}}
              onClick={handleAction}
              whileHover={{scale:1.02, filter:"brightness(1.09)"}} whileTap={{scale:0.97}}>
              <span className={styles.ctaIco}>{tm.icon}</span>
              {ctaLabel}
              <svg className={styles.ctaArr} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                <line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/>
              </svg>
            </motion.button>
            {ev.attachmentUrl && (
              <a href={ev.attachmentUrl} target="_blank" rel="noreferrer"
                 style={{
                   marginTop:10, display:"inline-flex", alignItems:"center", gap:8,
                   padding:"9px 14px", borderRadius:10, fontSize:13, fontWeight:700,
                   color:tm.color, background:`${tm.color}10`, border:`1px solid ${tm.color}30`,
                   textDecoration:"none", alignSelf:"flex-start",
                 }}>
                📎 Open attachment
              </a>
            )}
          </div>
        ) : canReview ? (
          <div className={styles.popFoot}>
            <motion.button className={styles.ctaBtn}
              style={{background:`linear-gradient(135deg,#22c55e 0%,#16a34a 100%)`}}
              onClick={handleAction}
              whileHover={{scale:1.02}} whileTap={{scale:0.97}}>
              ✓ Already submitted — Review
            </motion.button>
          </div>
        ) : (
          <div className={styles.expNote}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" width="14" height="14">
              <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
            </svg>
            {lockedMsg}
          </div>
        )}
      </motion.div>
    </motion.div>
  );
}

// ── Page-level floating tooltip ───────────────────────────────────
function FloatingTooltip({ tooltip }) {
  if (!tooltip) return null;
  const { ev, x, y } = tooltip;
  const tm      = TYPE[ev.type] || TYPE.lecture;
  const days    = daysLeft(ev.date);
  const expired = ev.isExpired;

  return (
    <motion.div
      className={styles.floatTip}
      style={{ left: x, top: y, "--tc": tm.color }}
      initial={{ opacity:0, scale:0.88, y:4 }}
      animate={{ opacity:1, scale:1,    y:0 }}
      exit={{    opacity:0, scale:0.9,  y:4 }}
      transition={{ duration:0.16, ease:[0.22,1,0.36,1] }}
    >
      <span className={styles.ftDot} style={{background:tm.color}}/>
      <span className={styles.ftType} style={{background:`${tm.color}18`, color:tm.color}}>{tm.label}</span>
      <span className={styles.ftTitle}>{ev.title}</span>
      <span className={styles.ftCode}>{ev.courseCode}</span>
      {expired && <span className={styles.ftExp}>Expired</span>}
      {!expired && ev.type!=="lecture" && days<=7 && <StatusBadge days={days} type={ev.type} ev={ev}/>}
    </motion.div>
  );
}

// ── Main ──────────────────────────────────────────────────────────
export default function TimetablePage() {
  const todayObj = new Date(); todayObj.setHours(0,0,0,0);
  const [year,    setYear]    = useState(todayObj.getFullYear());
  const [month,   setMonth]   = useState(todayObj.getMonth());
  const [active,  setActive]  = useState(null);
  const [tooltip, setTooltip] = useState(null);
  const [dir,     setDir]     = useState(1);
  const tipTimer = useRef(null);

  const [events,  setEvents]  = useState([]);
  const [loading, setLoading] = useState(true);
  const [loadErr, setLoadErr] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    getStudentTimetableEvents()
      .then((list) => {
        if (cancelled) return;
        setEvents((Array.isArray(list) ? list : []).map(normalizeEvent));
      })
      .catch((e) => { if (!cancelled) setLoadErr(e?.response?.data?.error?.message || "Could not load timetable."); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, []);

  const startDay    = new Date(year,month,1).getDay();
  const daysInMonth = new Date(year,month+1,0).getDate();
  const prevCount   = new Date(year,month,0).getDate();
  const totalCells  = Math.ceil((startDay+daysInMonth)/7)*7;
  const numWeeks    = totalCells/7;

  const cells = Array.from({length:totalCells},(_,i)=>{
    const off = i-startDay;
    if (off<0)            return {day:prevCount+off+1,   cur:false, date:new Date(year,month-1,prevCount+off+1)};
    if (off>=daysInMonth) return {day:off-daysInMonth+1, cur:false, date:new Date(year,month+1,off-daysInMonth+1)};
    return {day:off+1, cur:true, date:new Date(year,month,off+1)};
  });

  const eventsOn = (date) => events.filter(e=>e.date && sameDay(toDate(e.date),date));
  const isToday  = (date) => sameDay(date,todayObj);

  const nav = (d) => {
    setDir(d);
    if(d===1){month===11?(setMonth(0),setYear(y=>y+1)):setMonth(m=>m+1);}
    else     {month===0 ?(setMonth(11),setYear(y=>y-1)):setMonth(m=>m-1);}
  };
  const goToday = () => {
    setDir(todayObj.getMonth()>month||todayObj.getFullYear()>year?1:-1);
    setMonth(todayObj.getMonth()); setYear(todayObj.getFullYear());
  };

  // Tooltip handlers — fixed position relative to page
  const handleEnter = useCallback((e, ev) => {
    clearTimeout(tipTimer.current);
    const rect = e.currentTarget.getBoundingClientRect();
    setTooltip({ ev, x: rect.left + rect.width/2, y: rect.top - 8 });
  }, []);
  const handleLeave = useCallback(() => {
    tipTimer.current = setTimeout(() => setTooltip(null), 80);
  }, []);

  const mEvs = events.filter(e=>{
    if (!e.date) return false;
    const d=toDate(e.date);
    return d.getMonth()===month && d.getFullYear()===year;
  });
  const STATS = [
    {n:mEvs.filter(e=>e.type==="quiz").length,       label:"Quizzes",   color:"#7c3aed"},
    {n:mEvs.filter(e=>e.type==="assignment").length, label:"Deadlines", color:"#e11d48"},
    {n:mEvs.filter(e=>e.type==="lecture").length,    label:"Lectures",  color:"#0369a1"},
  ];

  const slide = {
    enter:(d)=>({opacity:0,x:d*50}),
    center:   ({opacity:1,x:0}),
    exit: (d)=>({opacity:0,x:-d*50}),
  };

  return (
    <div className={styles.page}>

      {/* Header */}
      <motion.div className={styles.topBar}
        initial={{opacity:0,y:-16}} animate={{opacity:1,y:0}}
        transition={{duration:0.4,ease:[0.22,1,0.36,1]}}
      >
        <div>
          <h1 className={styles.pgTitle}>Academic Timetable</h1>
          <p className={styles.pgSub}>{MONTHS[month]} {year} · Hover any event for a preview · Click to open details</p>
        </div>
        <div className={styles.statsRow}>
          {STATS.map((s,i)=>(
            <motion.div key={s.label} className={styles.statChip} style={{"--c":s.color}}
              initial={{opacity:0,y:-8}} animate={{opacity:1,y:0}}
              transition={{delay:0.06+i*0.06}}
              whileHover={{y:-4,boxShadow:`0 8px 20px ${s.color}28`}}
            >
              <span className={styles.scN}>{s.n}</span>
              <span className={styles.scL}>{s.label}</span>
              <div className={styles.scBar}/>
            </motion.div>
          ))}
        </div>
      </motion.div>

      {/* Calendar */}
      <motion.div className={styles.calCard}
        initial={{opacity:0,y:18}} animate={{opacity:1,y:0}}
        transition={{delay:0.07,duration:0.45,ease:[0.22,1,0.36,1]}}
      >
        {/* Month nav */}
        <div className={styles.monthBar}>
          <motion.button className={styles.arrBtn} onClick={()=>nav(-1)} whileHover={{scale:1.14}} whileTap={{scale:0.86}}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round"><polyline points="15 18 9 12 15 6"/></svg>
          </motion.button>
          <div className={styles.monthWrap}>
            <AnimatePresence mode="wait" custom={dir}>
              <motion.h2 key={`${year}-${month}`} className={styles.monthTxt}
                custom={dir} variants={slide} initial="enter" animate="center" exit="exit"
                transition={{duration:0.18,ease:[0.22,1,0.36,1]}}
              >
                {MONTHS[month]} <span className={styles.yrTxt}>{year}</span>
              </motion.h2>
            </AnimatePresence>
          </div>
          <motion.button className={styles.arrBtn} onClick={()=>nav(1)} whileHover={{scale:1.14}} whileTap={{scale:0.86}}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round"><polyline points="9 18 15 12 9 6"/></svg>
          </motion.button>
          <motion.button className={styles.todayBtn} onClick={goToday} whileHover={{scale:1.05}} whileTap={{scale:0.95}}>Today</motion.button>
        </div>

        {/* Day headers */}
        <div className={styles.dayRow}>
          {DAYS_S.map(d=>(
            <div key={d} className={`${styles.dayHdr} ${(d==="Sat"||d==="Sun")?styles.wknd:""}`}>{d}</div>
          ))}
        </div>

        {/* Grid */}
        <AnimatePresence mode="wait" custom={dir}>
          <motion.div key={`${year}-${month}`} className={styles.grid}
            style={{"--wks":numWeeks}}
            custom={dir} variants={slide} initial="enter" animate="center" exit="exit"
            transition={{duration:0.2,ease:[0.22,1,0.36,1]}}
          >
            {cells.map((cell,i)=>{
              const evs   = eventsOn(cell.date);
              const today = isToday(cell.date);
              const MAX   = 6;
              return (
                <div key={i} className={`
                  ${styles.cell}
                  ${!cell.cur?styles.cellGhost:""}
                  ${today    ?styles.cellToday:""}
                  ${(cell.date.getDay()===0||cell.date.getDay()===6)&&cell.cur?styles.cellWknd:""}
                `}>
                  {/* Day num */}
                  <div className={styles.cellTop}>
                    {today
                      ? <motion.span className={styles.todayNum}
                          initial={{scale:0.4}} animate={{scale:1}}
                          transition={{delay:i*0.003+0.12,type:"spring",stiffness:440,damping:22}}>
                          {cell.day}
                        </motion.span>
                      : <span className={`${styles.numTxt} ${!cell.cur?styles.numGhost:""}`}>{cell.day}</span>
                    }
                  </div>

                  {/* Events */}
                  {cell.cur && evs.length > 0 && (
                    <div className={styles.evStack}>
                      {evs.slice(0,MAX).map((ev,ei)=>{
                        const tm      = TYPE[ev.type] || TYPE.lecture;
                        const expired = ev.isExpired;
                        const cColor  = getCourseColor(ev.courseCode, ev.courseId);
                        return (
                          <motion.button key={ev.id}
                            className={`${styles.evChip} ${expired?styles.evExpired:""}`}
                            style={{"--c":cColor}}
                            onClick={()=>setActive(ev)}
                            onMouseEnter={(e)=>handleEnter(e,ev)}
                            onMouseLeave={handleLeave}
                            initial={{opacity:0,scale:0.9}}
                            animate={{opacity:1,scale:1}}
                            transition={{delay:i*0.003+ei*0.035+0.06}}
                            whileHover={{y:-1}}
                            whileTap={{scale:0.92}}
                          >
                            <span className={styles.evIco} style={{color:expired?"#94a3b8":cColor}}>{tm.icon}</span>
                            <span className={styles.evName}>{ev.title}</span>
                          </motion.button>
                        );
                      })}
                      {evs.length>MAX && (
                        <motion.button className={styles.moreBtn}
                          onClick={()=>setActive(evs[MAX])}
                          whileHover={{x:3}}
                        >
                          +{evs.length-MAX} more
                        </motion.button>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </motion.div>
        </AnimatePresence>

        {/* Legend */}
        <div className={styles.legend}>
          {Object.entries(TYPE).map(([k,v])=>(
            <div key={k} className={styles.legItem}>
              <span className={styles.legDot} style={{background:v.color}}/>
              <span className={styles.legIco} style={{color:v.color}}>{v.icon}</span>
              <span>{v.label}</span>
            </div>
          ))}
          <div className={styles.legItem}>
            <span className={styles.legDot} style={{background:"#94a3b8",opacity:.5}}/>
            <span style={{color:"#94a3b8"}}>Expired</span>
          </div>
          {(loading || loadErr || (!loading && events.length === 0)) && (
            <span style={{
              marginLeft:"auto", fontSize:11, color: loadErr ? "#ef4444" : "var(--text-muted, #94a3b8)",
              fontWeight:600,
            }}>
              {loading ? "Loading timetable…"
                : loadErr ? loadErr
                : "No activities yet."}
            </span>
          )}
        </div>
      </motion.div>

      {/* Page-level floating tooltip — never clipped */}
      <AnimatePresence>
        {tooltip && <FloatingTooltip tooltip={tooltip}/>}
      </AnimatePresence>

      {/* Popup */}
      <AnimatePresence>
        {active && <EventPopup ev={active} onClose={()=>setActive(null)}/>}
      </AnimatePresence>
    </div>
  );
}
