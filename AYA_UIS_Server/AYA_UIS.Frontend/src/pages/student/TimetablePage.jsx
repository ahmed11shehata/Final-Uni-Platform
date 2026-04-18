// src/pages/student/TimetablePage.jsx
import { useState, useRef, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import styles from "./TimetablePage.module.css";

/*
// ── REAL API — uncomment when backend ready ─────────────────
import axios from "axios";
export const fetchTimetableEvents = async (token) => {
  const res = await axios.get("/api/student/timetable", {
    headers: { Authorization: `Bearer ${token}` },
  });
  return res.data;
};
*/

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
    route:"/student/assignments",
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
  video: {
    label:"Video Session", color:"#047857", light:"rgba(4,120,87,0.07)", border:"rgba(4,120,87,0.2)",
    route:"/student/courses",
    icon:(
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
        <polygon points="23 7 16 12 23 17 23 7"/>
        <rect x="1" y="5" width="15" height="14" rx="2"/>
      </svg>
    ),
  },
};

// ── Mock events ──────────────────────────────────────────────────
const MOCK_EVENTS = [
  { id:1,  type:"quiz",       title:"Quiz 1 – Search Algorithms",          courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-03", time:"10:00 AM", duration:"20 min" },
  { id:2,  type:"assignment", title:"Linear Algebra Problem Set",           courseCode:"MATH302", course:"Mathematics",             date:"2026-03-03", time:"11:59 PM", duration:null     },
  { id:3,  type:"lecture",    title:"Intro to Neural Networks",             courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-03", time:"Uploaded",  duration:null     },
  { id:4,  type:"quiz",       title:"Quiz 2 – Circuit Analysis",            courseCode:"EE301",   course:"Electronics",             date:"2026-03-05", time:"11:00 AM", duration:"20 min" },
  { id:5,  type:"assignment", title:"Op-Amp Circuit Design",                courseCode:"EE301",   course:"Electronics",             date:"2026-03-05", time:"11:59 PM", duration:null     },
  { id:6,  type:"video",      title:"Electronics Live Q&A",                 courseCode:"EE301",   course:"Electronics",             date:"2026-03-05", time:"03:00 PM", duration:"60 min" },
  { id:7,  type:"lecture",    title:"Operational Amplifiers Pt.2",          courseCode:"EE301",   course:"Electronics",             date:"2026-03-10", time:"Uploaded",  duration:null     },
  { id:8,  type:"assignment", title:"Search Algorithm Implementation",      courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-10", time:"11:59 PM", duration:null     },
  { id:9,  type:"quiz",       title:"Midterm – Calculus II",                courseCode:"MATH302", course:"Mathematics",             date:"2026-03-12", time:"09:00 AM", duration:"90 min" },
  { id:10, type:"quiz",       title:"Quiz 3 – ML Fundamentals",             courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-12", time:"11:00 AM", duration:"20 min" },
  { id:11, type:"assignment", title:"Database Schema Design",               courseCode:"CS302",   course:"Database Systems",        date:"2026-03-12", time:"11:59 PM", duration:null     },
  { id:12, type:"video",      title:"AI Concepts Live Session",             courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-12", time:"04:00 PM", duration:"75 min" },
  { id:13, type:"lecture",    title:"Deep Learning & CNNs",                 courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-17", time:"Uploaded",  duration:null     },
  { id:14, type:"assignment", title:"Op-Amp Design Report",                 courseCode:"EE301",   course:"Electronics",             date:"2026-03-17", time:"11:59 PM", duration:null     },
  { id:15, type:"quiz",       title:"Quiz 4 – Knowledge Representation",    courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-19", time:"10:00 AM", duration:"20 min" },
  { id:16, type:"lecture",    title:"SQL Joins & Subqueries",               courseCode:"CS302",   course:"Database Systems",        date:"2026-03-19", time:"Uploaded",  duration:null     },
  { id:17, type:"assignment", title:"Neural Network from Scratch",          courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-03-24", time:"11:59 PM", duration:null     },
  { id:18, type:"video",      title:"Calculus Review Session",              courseCode:"MATH302", course:"Mathematics",             date:"2026-03-24", time:"11:00 AM", duration:"60 min" },
  { id:19, type:"quiz",       title:"Final Exam – Electronics",             courseCode:"EE301",   course:"Electronics",             date:"2026-03-26", time:"09:00 AM", duration:"120 min"},
  { id:20, type:"assignment", title:"Final Project Submission",             courseCode:"CS302",   course:"Database Systems",        date:"2026-03-26", time:"11:59 PM", duration:null     },
  { id:21, type:"lecture",    title:"Integration Techniques",               courseCode:"MATH302", course:"Mathematics",             date:"2026-03-26", time:"Uploaded",  duration:null     },
  { id:22, type:"assignment", title:"AI Final Project",                     courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-04-02", time:"11:59 PM", duration:null     },
  { id:23, type:"video",      title:"Project Showcase Session",             courseCode:"CS401",   course:"Artificial Intelligence", date:"2026-04-07", time:"02:00 PM", duration:"120 min"},
];

const MONTHS    = ["January","February","March","April","May","June","July","August","September","October","November","December"];
const DAYS_S    = ["Sun","Mon","Tue","Wed","Thu","Fri","Sat"];
const DAYS_FULL = ["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"];

const toDate   = (s) => new Date(s + "T00:00:00");
const daysLeft = (s) => { const t=new Date(); t.setHours(0,0,0,0); return Math.ceil((toDate(s)-t)/86400000); };
const sameDay  = (a,b) => a.getFullYear()===b.getFullYear()&&a.getMonth()===b.getMonth()&&a.getDate()===b.getDate();

// ── Status badge ─────────────────────────────────────────────────
function StatusBadge({ days, type }) {
  if (type==="lecture")   return <span className={`${styles.badge} ${styles.bNew}`}>New Upload</span>;
  if (days < 0)           return <span className={`${styles.badge} ${styles.bExp}`}>Expired</span>;
  if (days === 0)         return <span className={`${styles.badge} ${styles.bToday}`}>Due Today!</span>;
  if (days === 1)         return <span className={`${styles.badge} ${styles.bTmrw}`}>Tomorrow</span>;
  if (days <= 3)          return <span className={`${styles.badge} ${styles.bSoon}`}>In {days} days</span>;
  if (days <= 7)          return <span className={`${styles.badge} ${styles.bWeek}`}>This week</span>;
  return <span className={`${styles.badge} ${styles.bFar}`}>{days} days left</span>;
}

// ── Event Popup ──────────────────────────────────────────────────
function EventPopup({ ev, onClose }) {
  const navigate = useNavigate();
  const tm      = TYPE[ev.type];
  const days    = daysLeft(ev.date);
  const d       = toDate(ev.date);
  const expired = days < 0 && ev.type !== "lecture";
  const urgPct  = expired ? 100 : Math.max(5, Math.min(95, 100 - (days/30)*100));
  const urgCol  = days<=0 ? "#ef4444" : days<=3 ? "#f59e0b" : days<=7 ? "#a78bfa" : tm.color;

  const ctaLabel =
    ev.type==="quiz"       ? "Go to Quiz"      :
    ev.type==="assignment" ? "View Assignment" :
    ev.type==="lecture"    ? "Watch Lecture"   : "Join Session";

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
              <StatusBadge days={days} type={ev.type}/>
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

        {/* CTA / expired note */}
        {!expired ? (
          <div className={styles.popFoot}>
            <motion.button className={styles.ctaBtn}
              style={{background:`linear-gradient(135deg,${tm.color} 0%,${tm.color}bb 100%)`}}
              onClick={()=>{onClose(); navigate(tm.route);}}
              whileHover={{scale:1.02, filter:"brightness(1.09)"}} whileTap={{scale:0.97}}>
              <span className={styles.ctaIco}>{tm.icon}</span>
              {ctaLabel}
              <svg className={styles.ctaArr} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                <line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/>
              </svg>
            </motion.button>
          </div>
        ) : (
          <div className={styles.expNote}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" width="14" height="14">
              <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
            </svg>
            This event has passed. Contact your instructor if needed.
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
  const tm      = TYPE[ev.type];
  const days    = daysLeft(ev.date);
  const expired = days < 0 && ev.type !== "lecture";

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
      {!expired && ev.type!=="lecture" && days<=7 && <StatusBadge days={days} type={ev.type}/>}
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

  const eventsOn = (date) => MOCK_EVENTS.filter(e=>sameDay(toDate(e.date),date));
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

  const mEvs = MOCK_EVENTS.filter(e=>{const d=toDate(e.date);return d.getMonth()===month&&d.getFullYear()===year;});
  const STATS = [
    {n:mEvs.filter(e=>e.type==="quiz").length,       label:"Quizzes",   color:"#7c3aed"},
    {n:mEvs.filter(e=>e.type==="assignment").length, label:"Deadlines", color:"#e11d48"},
    {n:mEvs.filter(e=>e.type==="lecture").length,    label:"Lectures",  color:"#0369a1"},
    {n:mEvs.filter(e=>e.type==="video").length,      label:"Sessions",  color:"#047857"},
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
              const MAX   = 3;
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
                        const tm      = TYPE[ev.type];
                        const days    = daysLeft(ev.date);
                        const expired = days<0 && ev.type!=="lecture";
                        return (
                          <motion.button key={ev.id}
                            className={`${styles.evChip} ${expired?styles.evExpired:""}`}
                            style={{"--c":tm.color,"--bg":tm.light,"--br":tm.border}}
                            onClick={()=>setActive(ev)}
                            onMouseEnter={(e)=>handleEnter(e,ev)}
                            onMouseLeave={handleLeave}
                            initial={{opacity:0,x:-5}}
                            animate={{opacity:1,x:0}}
                            transition={{delay:i*0.003+ei*0.035+0.06}}
                            whileHover={{x:2}}
                            whileTap={{scale:0.94}}
                          >
                            <span className={styles.evStripe} style={{background:expired?"#94a3b8":tm.color}}/>
                            <span className={styles.evName}>{ev.title}</span>
                            <span className={styles.evIco} style={{color:expired?"#94a3b8":tm.color}}>{tm.icon}</span>
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