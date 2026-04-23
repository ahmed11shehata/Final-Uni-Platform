// src/pages/instructor/InstructorDashboard.jsx
import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../hooks/useAuth";
import { getInstructorDashboard } from "../../services/api/instructorApi";
import styles from "./InstructorDashboard.module.css";

const sp = { type:"spring", stiffness:400, damping:28 };

/* grade box colors — solid vivid */
const GRADE_COLORS = ["#f59e0b","#22c55e","#ef4444","#818cf8"];
const GRADE_BG     = ["#b45309","#15803d","#b91c1c","#4f46e5"];

export default function InstructorDashboard() {
  const { user }  = useAuth?.() || {};
  const navigate  = useNavigate();
  const [time,         setTime]         = useState(new Date());
  const [course,       setCourse]       = useState(null);
  const [courses,      setCourses]      = useState([]);
  const [gradeSummary, setGradeSummary] = useState({});
  const [activity,     setActivity]     = useState([]);
  const [upcoming,     setUpcoming]     = useState([]);

  useEffect(()=>{ const t=setInterval(()=>setTime(new Date()),30000); return()=>clearInterval(t); },[]);

  useEffect(()=>{
    getInstructorDashboard()
      .then(d=>{
        setCourses(d.courses);
        setGradeSummary(d.gradeSummary);
        setActivity(d.recentActivity);
        setUpcoming(d.upcoming);
        if(d.courses.length>0 && !course) setCourse(d.courses[0].id);
      })
      .catch(()=>{});
  // eslint-disable-next-line react-hooks/exhaustive-deps
  },[]);

  const hr    = time.getHours();
  const greet = hr<12?"Good morning ☀️":hr<17?"Good afternoon 🌤":"Good evening 🌙";
  const c     = courses.find(x=>x.id===course)||courses[0]||{color:"#818cf8",code:"—",name:"—",icon:"📚",students:0,progress:0};
  const gs    = gradeSummary?.[course] || { pending:0, approved:0, rejected:0, avg:"—" };
  const totalPending = courses.reduce((s,x)=>s+(gradeSummary?.[x.id]?.pending||0),0);

  return (
    <div className={styles.page}>

      {/* ══ HERO ══ */}
      <motion.div className={styles.hero}
        initial={{opacity:0,y:-18}} animate={{opacity:1,y:0}}
        transition={{duration:.44,ease:[.22,1,.36,1]}}>
        <div className={styles.heroBg}/>
        <div className={styles.heroMesh}/>

        <div className={styles.heroContent}>
          <div className={styles.heroLeft}>
            <div className={styles.heroGreeting}>{greet}</div>
            <h1 className={styles.heroName}>Dr. {user?.name||"Mohamed Farouk"}</h1>
            <p className={styles.heroRole}>{courses.map(x=>x.code).filter(Boolean).join(" & ")||"—"}</p>
            <div className={styles.heroStats}>
              {[
                {val:courses.reduce((s,x)=>s+(x.students||0),0), label:"Total Students", c:"#6ee7b7"},
                {val:totalPending,                                  label:"Pending Grades", c:"#fcd34d"},
                {val:courses.length,                                label:"Courses",        c:"#a5b4fc"},
              ].map((s,i)=>(
                <motion.div key={i} className={styles.heroStat}
                  initial={{opacity:0,y:12}} animate={{opacity:1,y:0}}
                  transition={{delay:.12+i*.07,...sp}}>
                  <span className={styles.heroStatVal} style={{color:s.c}}>{s.val}</span>
                  <span className={styles.heroStatLabel}>{s.label}</span>
                </motion.div>
              ))}
            </div>
          </div>

          <div className={styles.heroClock}>
            <div className={styles.heroTime}>{time.toLocaleTimeString([],{hour:"2-digit",minute:"2-digit"})}</div>
            <div className={styles.heroDate}>{time.toLocaleDateString([],{weekday:"long",month:"short",day:"numeric"})}</div>
          </div>
        </div>

        <div className={styles.quickActions}>
          {[
            {label:"Upload Material",   path:"/instructor/material",    icon:"📤",c:"#818cf8"},
            {label:"Quiz Builder",       path:"/instructor/quiz-builder", icon:"✏️",c:"#38bdf8"},
            {label:"Grade Submissions",  path:"/instructor/grades",       icon:"📊",c:"#34d399"},
            {label:"My Schedule",        path:"/instructor/schedule",     icon:"📅",c:"#fbbf24"},
          ].map((a,i)=>(
            <motion.button key={a.label} className={styles.qaBtn}
              style={{"--qa":a.c}}
              onClick={()=>navigate(a.path)}
              initial={{opacity:0,y:10}} animate={{opacity:1,y:0}}
              transition={{delay:.2+i*.06,...sp}}
              whileHover={{y:-3}} whileTap={{scale:.96}}>
              <span className={styles.qaIcon} style={{background:`${a.c}28`,color:a.c}}>{a.icon}</span>
              {a.label}
            </motion.button>
          ))}
        </div>
      </motion.div>

      {/* ══ MAIN GRID ══ */}
      <div className={styles.mainGrid}>

        {/* ── LEFT ── */}
        <div className={styles.leftCol}>

          {/* My Courses */}
          <div className={styles.sec}>
            <h2 className={styles.secTitle}>
              <span className={styles.secTitleDot} style={{background:"#818cf8"}}/>
              My Courses
            </h2>
            <div className={styles.courseCards}>
              {courses.length===0&&<p style={{color:"var(--text-muted)",fontSize:".83rem"}}>No courses assigned yet.</p>}
              {courses.map((cr,i)=>(
                <motion.button key={cr.id}
                  className={`${styles.courseCard} ${course===cr.id?styles.courseCardOn:""}`}
                  style={{"--cc":cr.color, background: course===cr.id ? `color-mix(in srgb,${cr.color} 8%,var(--card-bg))` : ""}}
                  onClick={()=>setCourse(cr.id)}
                  initial={{opacity:0,x:-20}} animate={{opacity:1,x:0}}
                  transition={{delay:i*.07,...sp}} whileHover={{x:5}}>
                  <div className={styles.ccStripe} style={{background:cr.color}}/>
                  <div className={styles.ccBody}>
                    <div className={styles.ccTop}>
                      <span className={styles.ccIcon}>{cr.icon}</span>
                      <span className={styles.ccCode} style={{color:cr.color}}>{cr.code}</span>
                    </div>
                    <div className={styles.ccName}>{cr.name}</div>
                    <div className={styles.ccMeta}>
                      <span>👥 {cr.students} students</span>
                      <span>•</span>
                      <span>{cr.progress}% complete</span>
                    </div>
                    <div className={styles.ccBar}>
                      <motion.div className={styles.ccFill} style={{background:cr.color}}
                        initial={{width:0}} animate={{width:`${cr.progress}%`}}
                        transition={{delay:.3+i*.1,duration:.85,ease:"easeOut"}}/>
                    </div>
                  </div>
                  {(gradeSummary?.[cr.id]?.pending||0)>0&&(
                    <span className={styles.ccBadge} style={{background:cr.color}}>
                      {gradeSummary[cr.id].pending}
                    </span>
                  )}
                </motion.button>
              ))}
            </div>
          </div>

          {/* Grade Overview — SOLID colored boxes */}
          <AnimatePresence mode="wait">
            <motion.div key={course} className={styles.sec}
              initial={{opacity:0,y:10}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
              <h2 className={styles.secTitle}>
                <span className={styles.secTitleDot} style={{background:"#22c55e"}}/>
                Grade Overview — {c.code}
              </h2>
              <div className={styles.gradeGrid}>
                {[
                  {val:gs.pending,  label:"Pending"},
                  {val:gs.approved, label:"Approved"},
                  {val:gs.rejected, label:"Rejected"},
                  {val:gs.avg,      label:"Avg Grade"},
                ].map((g,i)=>(
                  <motion.div key={g.label} className={styles.gradeBox}
                    style={{background:`linear-gradient(135deg,${GRADE_COLORS[i]},${GRADE_BG[i]})`}}
                    initial={{opacity:0,scale:.88}} animate={{opacity:1,scale:1}}
                    transition={{delay:i*.05,...sp}}
                    whileHover={{scale:1.04,y:-2}}>
                    <div className={styles.gradeBoxVal}>{g.val}</div>
                    <div className={styles.gradeBoxLabel}>{g.label}</div>
                  </motion.div>
                ))}
              </div>
            </motion.div>
          </AnimatePresence>

          {/* Activity */}
          <div className={styles.sec}>
            <h2 className={styles.secTitle}>
              <span className={styles.secTitleDot} style={{background:"#0ea5e9"}}/>
              Recent Activity
            </h2>
            <div className={styles.feed}>
              {activity.length===0&&(
                <p style={{color:"var(--text-muted)",fontSize:".83rem",padding:"12px 0"}}>No recent activity.</p>
              )}
              {activity.map((a,i)=>(
                <motion.div key={a.id||i} className={styles.feedItem}
                  initial={{opacity:0,x:-14}} animate={{opacity:1,x:0}}
                  transition={{delay:.05+i*.05,...sp}}>
                  <div className={styles.feedStripe} style={{background:a.color||"#818cf8"}}/>
                  <div className={styles.feedIcon} style={{background:a.color||"#818cf8",color:"#fff"}}>{a.icon||"📎"}</div>
                  <div className={styles.feedBody}>
                    <div className={styles.feedName}>{a.student||a.studentName||"—"}</div>
                    <div className={styles.feedDetail}>{a.detail||a.description||"—"}</div>
                  </div>
                  <div className={styles.feedMeta}>
                    <span className={styles.feedCourse} style={{color:a.color||"#818cf8"}}>{a.course||a.courseCode||""}</span>
                    <span className={styles.feedTime}>{a.time||""}</span>
                  </div>
                </motion.div>
              ))}
            </div>
          </div>
        </div>

        {/* ── RIGHT ── */}
        <div className={styles.rightCol}>

          {/* Upcoming */}
          <div className={styles.sec}>
            <h2 className={styles.secTitle}>
              <span className={styles.secTitleDot} style={{background:"#ef4444"}}/>
              Upcoming
            </h2>
            <div className={styles.upList}>
              {upcoming.length===0&&(
                <p style={{color:"var(--text-muted)",fontSize:".83rem",padding:"12px 0"}}>No upcoming events.</p>
              )}
              {upcoming.map((u,i)=>{
                const uc=u.color||"#818cf8";
                return (
                  <motion.div key={u.id||i} className={styles.upCard} style={{"--uc":uc}}
                    initial={{opacity:0,y:12}} animate={{opacity:1,y:0}}
                    transition={{delay:.08+i*.07,...sp}}>
                    <div className={styles.upAccent} style={{background:uc}}/>
                    <div className={styles.upIcon} style={{background:uc,color:"#fff"}}>{u.icon||"📅"}</div>
                    <div className={styles.upBody}>
                      <div className={styles.upTitle}>{u.title||"—"}</div>
                      <div className={styles.upMeta}>
                        <span>📅 {u.date||"—"}</span>
                        <span>⏰ {u.time||"—"}</span>
                        <span>🏛 {u.room||"—"}</span>
                      </div>
                    </div>
                  </motion.div>
                );
              })}
            </div>
          </div>

          {/* Quick Access — SOLID icon buttons */}
          <div className={styles.sec}>
            <h2 className={styles.secTitle}>
              <span className={styles.secTitleDot} style={{background:"#f59e0b"}}/>
              Quick Access
            </h2>
            <div className={styles.qnGrid}>
              {[
                {label:"Quiz Builder",    path:"/instructor/quiz-builder", icon:"✏️",  c:"#0ea5e9", sub:"Create & schedule quizzes"},
                {label:"Grade Mgmt",      path:"/instructor/grades",       icon:"📊",  c:"#22c55e", sub:"Review submissions"},
                {label:"Upload Material", path:"/instructor/material",     icon:"📤",  c:"#6366f1", sub:"Lectures & assignments"},
                {label:"Schedule",        path:"/instructor/schedule",     icon:"📅",  c:"#f59e0b", sub:"View your timetable"},
              ].map((n,i)=>(
                <motion.button key={n.label} className={styles.qnBtn} style={{"--qn":n.c}}
                  onClick={()=>navigate(n.path)}
                  initial={{opacity:0,scale:.9}} animate={{opacity:1,scale:1}}
                  transition={{delay:.16+i*.06,...sp}}
                  whileHover={{y:-4}} whileTap={{scale:.96}}>
                  <div className={styles.qnIcon} style={{background:n.c,color:"#fff"}}>{n.icon}</div>
                  <div className={styles.qnLabel}>{n.label}</div>
                  <div className={styles.qnSub}>{n.sub}</div>
                </motion.button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
