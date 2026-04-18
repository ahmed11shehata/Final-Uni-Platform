// src/pages/admin/AdminCoursesPage.jsx
import { useState, useMemo } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { COURSE_DB } from "../../services/mock/mockData";
import styles from "./AdminCoursesPage.module.css";

/* ── SVG Icons ── */
const Ic = {
  Book:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M4 19.5A2.5 2.5 0 016.5 17H20"/><path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z"/></svg>,
  Lock:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>,
  Check:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>,
  Users:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75"/></svg>,
  Info:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>,
  Filter:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/></svg>,
  Search:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>,
  Globe:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 010 20M12 2a15.3 15.3 0 000 20"/></svg>,
  Save:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M19 21H5a2 2 0 01-2-2V5a2 2 0 012-2h11l5 5v11a2 2 0 01-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>,
};

/* ── SVG patterns (same as CoursesPage) ── */
function getPatternBg(pattern, color) {
  const enc = encodeURIComponent;
  const p = {
    mosaic:   `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='56' height='56'%3E%3Crect x='2' y='2' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.45'/%3E%3Crect x='30' y='2' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.25'/%3E%3Crect x='2' y='30' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.25'/%3E%3Crect x='30' y='30' width='24' height='24' rx='3' fill='${enc(color)}' opacity='.45'/%3E%3C/svg%3E")`,
    circles:  `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='80'%3E%3Ccircle cx='40' cy='40' r='28' fill='none' stroke='${enc(color)}' stroke-width='18' opacity='.28'/%3E%3Ccircle cx='0' cy='0' r='18' fill='none' stroke='${enc(color)}' stroke-width='12' opacity='.18'/%3E%3Ccircle cx='80' cy='80' r='18' fill='none' stroke='${enc(color)}' stroke-width='12' opacity='.18'/%3E%3C/svg%3E")`,
    squares:  `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='72' height='72'%3E%3Crect x='8' y='8' width='56' height='56' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.22'/%3E%3Crect x='18' y='18' width='36' height='36' fill='none' stroke='${enc(color)}' stroke-width='2.5' opacity='.18'/%3E%3Crect x='28' y='28' width='16' height='16' fill='none' stroke='${enc(color)}' stroke-width='2' opacity='.15'/%3E%3C/svg%3E")`,
    diamonds: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='64' height='64'%3E%3Cpolygon points='32,4 60,32 32,60 4,32' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.26'/%3E%3Cpolygon points='32,16 48,32 32,48 16,32' fill='none' stroke='${enc(color)}' stroke-width='2.5' opacity='.2'/%3E%3C/svg%3E")`,
    waves:    `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='80' height='40'%3E%3Cpath d='M0 20 Q20 0 40 20 Q60 40 80 20' fill='none' stroke='${enc(color)}' stroke-width='3' opacity='.25'/%3E%3C/svg%3E")`,
    dots:     `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='32' height='32'%3E%3Ccircle cx='4' cy='4' r='2.5' fill='${enc(color)}' opacity='.35'/%3E%3Ccircle cx='20' cy='4' r='1.5' fill='${enc(color)}' opacity='.2'/%3E%3Ccircle cx='4' cy='20' r='1.5' fill='${enc(color)}' opacity='.2'/%3E%3Ccircle cx='20' cy='20' r='2.5' fill='${enc(color)}' opacity='.35'/%3E%3C/svg%3E")`,
  };
  return p[pattern] || p.mosaic;
}

const YEAR_LABELS = ["", "First Year", "Second Year", "Third Year", "Fourth Year"];
const YEAR_COLORS = ["", "#818cf8", "#22c55e", "#f59e0b", "#ef4444"];
const DEPT_COLORS = {
  "Computer Science": "#6366f1",
  "Mathematics":      "#f59e0b",
  "Humanities":       "#22c55e",
  "Engineering":      "#ef4444",
  "General":          "#14b8a6",
};

const fadeUp = { hidden:{opacity:0,y:18}, show:{opacity:1,y:0,transition:{duration:0.4,ease:[0.22,1,0.36,1]}} };
const stagger = { show:{ transition:{ staggerChildren:0.04 } } };

/* ══════════════════════════════════════════════════════════════
   PAGE
══════════════════════════════════════════════════════════════ */
export default function AdminCoursesPage() {
  // Which years are open for registration (admin selects)
  const [openYears, setOpenYears] = useState([3, 4]);
  // Which specific courses are enabled this semester
  const [enabledCourses, setEnabledCourses] = useState(
    COURSE_DB.filter(c => [3,4].includes(c.year)).map(c => c.code)
  );

  const [activeYear,  setActiveYear]  = useState(3); // current tab
  const [typeFilter,  setTypeFilter]  = useState("all"); // all/mandatory/elective
  const [search,      setSearch]      = useState("");
  const [toast,       setToast]       = useState(null);
  const [saved,       setSaved]       = useState(false);

  const showToast = (msg, type = "success") => {
    setToast({ msg, type });
    setTimeout(() => setToast(null), 3000);
  };

  const toggleYear = (yr) => {
    setOpenYears(p =>
      p.includes(yr) ? p.filter(y => y !== yr)
                     : [...p, yr]
    );
    // When enabling year, auto-enable all its courses
    if (!openYears.includes(yr)) {
      const yrCourses = COURSE_DB.filter(c => c.year === yr).map(c => c.code);
      setEnabledCourses(p => [...new Set([...p, ...yrCourses])]);
    }
  };

  const toggleCourse = (code) => {
    setEnabledCourses(p =>
      p.includes(code) ? p.filter(c => c !== code) : [...p, code]
    );
  };

  const toggleAllInYear = (yr, enable) => {
    const yrCourses = COURSE_DB.filter(c => c.year === yr).map(c => c.code);
    if (enable) setEnabledCourses(p => [...new Set([...p, ...yrCourses])]);
    else        setEnabledCourses(p => p.filter(c => !yrCourses.includes(c)));
  };

  const handleSave = () => {
    setSaved(true);
    showToast(`Saved — ${openYears.length} year(s) open, ${enabledCourses.length} courses active`);
    setTimeout(() => setSaved(false), 2000);
  };

  // Courses for active year tab
  const yearCourses = useMemo(() => {
    let list = COURSE_DB.filter(c => c.year === activeYear);
    if (typeFilter !== "all") list = list.filter(c => c.type === typeFilter);
    if (search.trim()) {
      const q = search.toLowerCase();
      list = list.filter(c =>
        c.name.toLowerCase().includes(q) ||
        c.code.toLowerCase().includes(q) ||
        c.instructor.toLowerCase().includes(q)
      );
    }
    return list;
  }, [activeYear, typeFilter, search]);

  // Stats
  const totalEnabled  = enabledCourses.length;
  const totalCourses  = COURSE_DB.length;
  const openYearCount = openYears.length;

  return (
    <div className={styles.page}>

      {/* Toast */}
      <AnimatePresence>
        {toast && (
          <motion.div className={`${styles.toast} ${styles[`toast_${toast.type}`]}`}
            initial={{opacity:0,y:-16,x:"-50%"}} animate={{opacity:1,y:0,x:"-50%"}}
            exit={{opacity:0,y:-8,x:"-50%"}} transition={{type:"spring",stiffness:400,damping:28}}>
            <span className={styles.toastDot}/>{toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── Header ── */}
      <motion.div className={styles.header} initial={{opacity:0,y:-12}} animate={{opacity:1,y:0}}>
        <div className={styles.headerLeft}>
          <div className={styles.headerIcon}>{Ic.Book}</div>
          <div>
            <h1 className={styles.headerTitle}>Courses Management</h1>
            <p className={styles.headerSub}>Control which courses are open for registration this semester</p>
          </div>
        </div>
        <motion.button className={`${styles.saveBtn} ${saved?styles.saveBtnDone:""}`}
          onClick={handleSave} whileHover={{scale:1.02}} whileTap={{scale:0.97}}>
          {Ic.Save}
          {saved ? "Saved ✓" : "Save Settings"}
        </motion.button>
      </motion.div>

      {/* ── Stats row ── */}
      <motion.div className={styles.statsRow} initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} transition={{delay:0.07}}>
        {[
          { icon:Ic.Globe, val:openYearCount, label:"Years Open",    color:"#818cf8" },
          { icon:Ic.Book,  val:totalEnabled,  label:"Active Courses",color:"#22c55e" },
          { icon:Ic.Users, val:`${totalCourses}`,label:"Total Courses", color:"#f59e0b" },
          { icon:Ic.Check, val:`${Math.round((totalEnabled/totalCourses)*100)}%`,label:"Coverage", color:"#14b8a6" },
        ].map((s,i) => (
          <motion.div key={s.label} className={styles.statCard}
            initial={{opacity:0,y:16}} animate={{opacity:1,y:0}} transition={{delay:0.1+i*0.06}}>
            <div className={styles.statIcon} style={{background:`${s.color}14`, color:s.color}}>
              {s.icon}
            </div>
            <div>
              <div className={styles.statVal} style={{color:s.color}}>{s.val}</div>
              <div className={styles.statLabel}>{s.label}</div>
            </div>
          </motion.div>
        ))}
      </motion.div>

      {/* ── Year Open/Close toggles ── */}
      <motion.div className={styles.yearToggles} initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} transition={{delay:0.12}}>
        <div className={styles.yearTogglesLabel}>
          {Ic.Filter}
          <span>Open Years for Registration</span>
        </div>
        <div className={styles.yearToggleRow}>
          {[1,2,3,4].map(yr => {
            const isOpen   = openYears.includes(yr);
            const yrCount  = COURSE_DB.filter(c => c.year === yr).length;
            const enCount  = COURSE_DB.filter(c => c.year === yr && enabledCourses.includes(c.code)).length;
            return (
              <motion.button key={yr}
                className={`${styles.yearToggle} ${isOpen?styles.yearToggleOn:""}`}
                style={{"--yc": YEAR_COLORS[yr]}}
                onClick={() => toggleYear(yr)}
                whileHover={{scale:1.03}} whileTap={{scale:0.96}}>
                <div className={styles.yearToggleTop}>
                  <span className={styles.yearNum}>Year {yr}</span>
                  <span className={`${styles.yearDot} ${isOpen?styles.yearDotOn:""}`}/>
                </div>
                <div className={styles.yearToggleLabel}>{YEAR_LABELS[yr]}</div>
                <div className={styles.yearToggleCount}>{enCount}/{yrCount} courses</div>
                <div className={styles.yearToggleStatus}>{isOpen ? "Open ✓" : "Closed"}</div>
              </motion.button>
            );
          })}
        </div>
      </motion.div>

      {/* ── Year tabs + filters ── */}
      <div className={styles.controls}>
        <div className={styles.yearTabs}>
          {[1,2,3,4].map(yr => (
            <button key={yr}
              className={`${styles.yearTab} ${activeYear===yr?styles.yearTabActive:""}`}
              style={activeYear===yr ? {"--ytc": YEAR_COLORS[yr]} : {}}
              onClick={() => setActiveYear(yr)}>
              <span style={{color: activeYear===yr ? YEAR_COLORS[yr] : "inherit"}}>Year {yr}</span>
              {!openYears.includes(yr) && <span className={styles.yearTabClosed}>Closed</span>}
            </button>
          ))}
        </div>
        <div className={styles.filters}>
          {["all","mandatory","elective"].map(f => (
            <button key={f}
              className={`${styles.chip} ${typeFilter===f?styles.chipOn:""}`}
              onClick={() => setTypeFilter(f)}>
              {f==="all" ? "All" : f==="mandatory" ? "Mandatory" : "Elective"}
            </button>
          ))}
        </div>
        <div className={styles.searchBox}>
          {Ic.Search}
          <input className={styles.searchIn} placeholder="Search courses…"
            value={search} onChange={e=>setSearch(e.target.value)}/>
        </div>
      </div>

      {/* ── Bulk toggles for active year ── */}
      <div className={styles.bulkRow}>
        <span className={styles.bulkLabel}>
          {YEAR_LABELS[activeYear]} — {yearCourses.length} courses
        </span>
        <div className={styles.bulkBtns}>
          <button className={styles.bulkBtn} onClick={() => toggleAllInYear(activeYear, true)}>
            Enable All
          </button>
          <button className={`${styles.bulkBtn} ${styles.bulkBtnDis}`}
            onClick={() => toggleAllInYear(activeYear, false)}>
            Disable All
          </button>
        </div>
      </div>

      {/* ── Courses Grid ── */}
      <motion.div className={styles.grid} variants={stagger} initial="hidden" animate="show">
        <AnimatePresence mode="popLayout">
          {yearCourses.map((course, i) => {
            const isEnabled = enabledCourses.includes(course.code);
            const isYearOpen = openYears.includes(course.year);
            return (
              <CourseCard key={course.code} course={course} index={i}
                isEnabled={isEnabled} isYearOpen={isYearOpen}
                onToggle={() => toggleCourse(course.code)}
              />
            );
          })}
        </AnimatePresence>
        {yearCourses.length === 0 && (
          <div className={styles.empty}>
            <div className={styles.emptyIcon}>{Ic.Search}</div>
            <p>No courses match</p>
          </div>
        )}
      </motion.div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════
   COURSE CARD
══════════════════════════════════════════════════════════════ */
function CourseCard({ course, isEnabled, isYearOpen, onToggle }) {
  const pat = getPatternBg(course.pattern, course.color);
  const deptColor = DEPT_COLORS[course.dept] || "#818cf8";

  return (
    <motion.article
      className={`${styles.card} ${isEnabled?styles.cardOn:styles.cardOff}`}
      variants={fadeUp} layout
      whileHover={{ y: -4, transition:{duration:0.18} }}
    >
      {/* Cover */}
      <div className={styles.cover}
        style={{
          background: course.color,
          backgroundImage: pat,
          backgroundSize: "64px 64px",
          opacity: isEnabled ? 1 : 0.55,
        }}>
        <div className={styles.coverDark}/>
        <div className={styles.coverTop}>
          <span className={styles.codeTag}>{course.code}</span>
          <span className={styles.crTag}>{course.credits} cr</span>
        </div>
        <div className={styles.coverBot}>
          <span className={`${styles.typeBadge} ${course.type==="elective"?styles.typeBadgeElec:""}`}>
            {course.type === "mandatory" ? "Mandatory" : "Elective"}
          </span>
          {/* Toggle switch */}
          <motion.button
            className={`${styles.toggle} ${isEnabled?styles.toggleOn:""}`}
            onClick={onToggle}
            whileTap={{scale:0.9}}
            style={isEnabled ? {background:course.color, borderColor:course.color} : {}}>
            <motion.div className={styles.toggleThumb}
              layout transition={{type:"spring",stiffness:500,damping:30}}/>
          </motion.button>
        </div>
      </div>

      {/* Body */}
      <div className={styles.cardBody}>
        <h3 className={styles.courseName}>{course.name}</h3>

        <div className={styles.metaRow}>
          <span className={styles.deptBadge} style={{color:deptColor, background:`${deptColor}12`}}>
            {course.dept}
          </span>
        </div>

        <div className={styles.metaRow}>
          <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/>
          </svg>
          <span className={styles.metaText}>{course.instructor}</span>
        </div>

        {course.prereqs?.length > 0 && (
          <div className={styles.prereqRow}>
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="#f59e0b" strokeWidth="2">
              <rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
            </svg>
            <span>Requires: {course.prereqs.join(", ")}</span>
          </div>
        )}
      </div>

      {/* Footer status */}
      <div className={`${styles.cardFoot} ${isEnabled && isYearOpen ? styles.cardFootOn : isEnabled && !isYearOpen ? styles.cardFootWarn : styles.cardFootOff}`}>
        {isEnabled && isYearOpen  && <><span className={styles.footDot}/> Active for registration</>}
        {isEnabled && !isYearOpen && <><span className={styles.footDot} style={{background:"#f59e0b"}}/> Enabled but year is closed</>}
        {!isEnabled               && <><span className={styles.footDot} style={{background:"#6b7280"}}/> Disabled this semester</>}
      </div>
    </motion.article>
  );
}
