import { motion, useScroll, useTransform } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";
import { Link } from "react-router-dom";
import { useState, useEffect, useRef } from "react";
import styles from "./StudentDashboard.module.css";
import { getStudentTimetableEvents } from "../../services/api/studentApi";

// ── Animated Counter ─────────────────────────────
function Counter({ value }) {
  const [count, setCount] = useState(0);
  const num = parseFloat(value);
  const isFloat = String(value).includes(".");
  useEffect(() => {
    let s = 0; const steps = 50;
    const t = setInterval(() => {
      s += num / steps;
      if (s >= num) { setCount(num); clearInterval(t); }
      else setCount(s);
    }, 30);
    return () => clearInterval(t);
  }, [num]);
  return <>{isFloat ? count.toFixed(2) : Math.floor(count)}</>;
}

// ── Floating Particles ───────────────────────────
function Particles() {
  const pts = Array.from({ length: 16 }, (_, i) => ({
    id: i, x: Math.random() * 100, y: Math.random() * 100,
    size: Math.random() * 3 + 2,
    dur: Math.random() * 5 + 4,
    delay: Math.random() * 3,
  }));
  return (
    <div className={styles.particles}>
      {pts.map(p => (
        <motion.div key={p.id} className={styles.particle}
          style={{ left:`${p.x}%`, top:`${p.y}%`, width:p.size, height:p.size }}
          animate={{ y:[-15,15,-15], opacity:[0.1,0.5,0.1], scale:[1,1.5,1] }}
          transition={{ duration:p.dur, repeat:Infinity, delay:p.delay, ease:"easeInOut" }}
        />
      ))}
    </div>
  );
}

// ── Data ─────────────────────────────────────────
const STATS = [
  { label:"Current GPA",      value:"3.85", icon:"🎯", color:"#7c3aed", sub:"+0.12 this term" },
  { label:"Enrolled Courses", value:"6",    icon:"📚", color:"#0891b2", sub:"This semester"   },
  { label:"Assignments Due",  value:"3",    icon:"📋", color:"#dc2626", sub:"This week"       },
  { label:"Quizzes Pending",  value:"2",    icon:"✏️", color:"#d97706", sub:"Upcoming"        },
];

const TYPE_COLORS = {
  assignment: "#dc2626",
  quiz:       "#7c3aed",
  lecture:    "#0891b2",
};

function relativeDayLabel(iso) {
  if (!iso) return "—";
  const target = new Date(iso); target.setHours(0,0,0,0);
  const today  = new Date();   today.setHours(0,0,0,0);
  const diff   = Math.round((target - today) / 86400000);
  if (diff < 0)  return target.toLocaleDateString();
  if (diff === 0) return "Today";
  if (diff === 1) return "Tomorrow";
  if (diff <= 7) return `In ${diff} days`;
  return target.toLocaleDateString();
}

function formatTime(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleTimeString([], { hour:"numeric", minute:"2-digit" });
}

function eventDateIso(ev) {
  if (ev.type === "assignment" && ev.deadlineAt) return ev.deadlineAt;
  if (ev.type === "quiz"       && ev.startAt)    return ev.startAt;
  if (ev.type === "lecture"    && ev.startAt)    return ev.startAt;
  return ev.startAt || ev.endAt || ev.deadlineAt;
}

const COURSES = [
  { id:1, name:"Physics 201",      instructor:"Dr. Ahmed",  progress:72, color:"#7c3aed", icon:"⚡" },
  { id:2, name:"Mathematics 301",  instructor:"Dr. Sara",   progress:85, color:"#0891b2", icon:"📐" },
  { id:3, name:"Computer Science", instructor:"Dr. Khaled", progress:60, color:"#10b981", icon:"💻" },
  { id:4, name:"Chemistry 201",    instructor:"Dr. Mona",   progress:45, color:"#d97706", icon:"🧪" },
];

const VALUES = [
  { icon:"🎓", title:"Academic Excellence", desc:"Striving for the highest standards in education and research" },
  { icon:"❤️", title:"Integrity",           desc:"Maintaining ethical principles in all our endeavors"         },
  { icon:"🤝", title:"Diversity & Inclusion",desc:"Embracing cultural diversity and inclusive education"        },
  { icon:"🚀", title:"Innovation",          desc:"Fostering creativity and technological advancement"           },
];

const MILESTONES = [
  { year:"1999",    text:"Academy established by ministerial decree",             color:"#7c3aed" },
  { year:"2000s",   text:"Expansion of engineering and business programs",        color:"#0891b2" },
  { year:"2010",    text:"Development of research and training facilities",       color:"#d97706" },
  { year:"2020",    text:"Digital transformation and smart campus initiatives",   color:"#dc2626" },
  { year:"Present", text:"Continuous growth and strong industry partnerships",    color:"#10b981" },
];

const CONTACT = [
  { icon:"📍", label:"Address", value:"Industrial Zone – 4th Industrial Area – 6th of October City – Giza – Egypt" },
  { icon:"📞", label:"Phone",   value:"+20 2 38336005 / +20 2 38347120" },
  { icon:"📠", label:"Fax",     value:"+202 0123 45 6789" },
  { icon:"✉️", label:"Mail",    value:"info@akhbaracademy.edu.eg" },
];

export default function StudentDashboard() {
  const { user } = useAuth();
  const heroRef = useRef(null);
  const { scrollYProgress } = useScroll({ target: heroRef, offset:["start start","end start"] });
  const heroY = useTransform(scrollYProgress, [0,1], [0, 50]);

  const [upcoming, setUpcoming] = useState([]);
  const [upcomingLoading, setUpcomingLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    setUpcomingLoading(true);
    getStudentTimetableEvents({ limit: 5 })
      .then((list) => {
        if (cancelled) return;
        const mapped = (Array.isArray(list) ? list : [])
          .filter((ev) => !ev.isExpired)
          .slice(0, 5)
          .map((ev) => ({
            id:     ev.id,
            type:   ev.type,
            title:  ev.title,
            course: ev.courseCode || ev.courseName,
            date:   relativeDayLabel(eventDateIso(ev)),
            time:   formatTime(eventDateIso(ev)),
            color:  TYPE_COLORS[ev.type] || "#7c3aed",
          }));
        setUpcoming(mapped);
      })
      .catch(() => { if (!cancelled) setUpcoming([]); })
      .finally(() => { if (!cancelled) setUpcomingLoading(false); });
    return () => { cancelled = true; };
  }, []);

  return (
    <div className={styles.page}>

      {/* ══ HERO ══════════════════════════════════ */}
      <motion.div ref={heroRef} className={styles.hero} style={{ y: heroY }}>
        <Particles />
        <div className={styles.heroGrid} />

        <div className={styles.heroInner}>
          <div className={styles.heroLeft}>
            <motion.span className={styles.heroBadge}
              initial={{ opacity:0, scale:0.8 }} animate={{ opacity:1, scale:1 }}
              transition={{ delay:0.1, type:"spring" }}
            >
              <span className={styles.heroPulse} /> Academic Year 2024–2025
            </motion.span>

            <motion.h1 className={styles.heroTitle}
              initial={{ opacity:0, y:30 }} animate={{ opacity:1, y:0 }}
              transition={{ delay:0.2, duration:0.7, ease:[0.22,1,0.36,1] }}
            >
              Welcome to <span className={styles.heroAccent}>Akhbar Elyom</span><br />Academy
            </motion.h1>

            <motion.p className={styles.heroTagline}
              initial={{ opacity:0 }} animate={{ opacity:1 }} transition={{ delay:0.3 }}
            >
              A Smart Lead Academic Platform
            </motion.p>

            <motion.p className={styles.heroSub}
              initial={{ opacity:0 }} animate={{ opacity:1 }} transition={{ delay:0.4 }}
            >
              Leading the future of education, innovation, research, and excellence.
            </motion.p>

            <motion.div className={styles.heroBtns}
              initial={{ opacity:0, y:20 }} animate={{ opacity:1, y:0 }} transition={{ delay:0.5 }}
            >
              <Link to="/student/courses" className={styles.btnPrimary}>
                Browse Courses
                <motion.span animate={{ x:[0,5,0] }} transition={{ duration:1.4, repeat:Infinity }}>→</motion.span>
              </Link>
              <Link to="/student/register-courses" className={styles.btnGhost}>
                Register Now
              </Link>
            </motion.div>
          </div>

          <motion.div className={styles.heroRight}
            initial={{ opacity:0, x:40, scale:0.9 }}
            animate={{ opacity:1, x:0, scale:1 }}
            transition={{ delay:0.35, duration:0.7, ease:[0.22,1,0.36,1] }}
          >
            <div className={styles.heroCard}>
              <div className={styles.heroCardGlow} />
              <motion.span className={styles.heroEmoji}
                animate={{ y:[0,-12,0], rotate:[0,6,-6,0] }}
                transition={{ duration:4, repeat:Infinity, ease:"easeInOut" }}
              >🎓</motion.span>
              <p className={styles.heroCardName}>{user?.name}</p>
              <p className={styles.heroCardId}>ID: {user?.id}</p>
              <div className={styles.heroCardLine} />
              <div className={styles.heroCardRow}>
                {[{v:"3.85",l:"GPA"},{v:"6",l:"Courses"},{v:"Yr 3",l:"Year"}].map(x=>(
                  <div key={x.l} className={styles.heroCardStat}>
                    <strong>{x.v}</strong><span>{x.l}</span>
                  </div>
                ))}
              </div>
            </div>
            <motion.div className={styles.floatA}
              animate={{ y:[0,-10,0], rotate:[-2,2,-2] }}
              transition={{ duration:3.5, repeat:Infinity }}
            >🏆 Top 10%</motion.div>
            <motion.div className={styles.floatB}
              animate={{ y:[0,10,0], rotate:[2,-2,2] }}
              transition={{ duration:4, repeat:Infinity, delay:0.6 }}
            >⚡ 3 Due Soon</motion.div>
          </motion.div>
        </div>
      </motion.div>

      {/* ══ STATS ═════════════════════════════════ */}
      <motion.div className={styles.statsGrid}
        initial="hidden" animate="visible"
        variants={{ hidden:{}, visible:{ transition:{ staggerChildren:0.09 } } }}
      >
        {STATS.map((s,i) => (
          <motion.div key={s.label} className={styles.statCard}
            variants={{ hidden:{opacity:0,y:24}, visible:{opacity:1,y:0,transition:{duration:0.5,ease:[0.22,1,0.36,1]}} }}
            whileHover={{ y:-7, boxShadow:`0 16px 40px ${s.color}22` }}
            style={{ "--c": s.color }}
          >
            <div className={styles.statRing}>
              <span className={styles.statEmoji}>{s.icon}</span>
            </div>
            <div className={styles.statBody}>
              <span className={styles.statVal}><Counter value={s.value} /></span>
              <span className={styles.statLbl}>{s.label}</span>
              <span className={styles.statSub}>{s.sub}</span>
            </div>
            <div className={styles.statBar} />
          </motion.div>
        ))}
      </motion.div>

      {/* ══ MISSION + VISION ══════════════════════ */}
      <div className={styles.twoCol}>
        <motion.div className={styles.card}
          initial={{ opacity:0, x:-30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55, ease:[0.22,1,0.36,1] }}
        >
          <div className={styles.cardTopBar} style={{ background:"linear-gradient(90deg,#7c3aed,#4f46e5)" }} />
          <span className={styles.cardBigIcon}>🎯</span>
          <h3 className={styles.cardTitle}>Our Mission</h3>
          <p className={styles.cardText}>
            Our mission is to equip students with strong academic knowledge, professional skills, and ethical
            values that prepare them to become successful leaders in engineering, business, technology, and media.
            We strive to create a learning environment that connects education with real industry needs.
          </p>
        </motion.div>

        <motion.div className={styles.card}
          initial={{ opacity:0, x:30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55, ease:[0.22,1,0.36,1] }}
        >
          <div className={styles.cardTopBar} style={{ background:"linear-gradient(90deg,#0891b2,#0e7490)" }} />
          <span className={styles.cardBigIcon}>👁️</span>
          <h3 className={styles.cardTitle}>Our Vision</h3>
          <p className={styles.cardText}>
            To become a leading institution in academic innovation and excellence, bridging the gap between
            academic teaching and real industry experience through training opportunities, internships, and
            partnerships with companies and institutions across the region and the world.
          </p>
        </motion.div>
      </div>

      {/* ══ UPCOMING + COURSES ════════════════════ */}
      <div className={styles.twoCol}>
        <motion.div className={styles.card}
          initial={{ opacity:0, x:-30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55 }}
        >
          <div className={styles.cardHeader}>
            <div className={styles.cardHeaderLeft}>
              <span className={styles.cardIcon}>📅</span>
              <div>
                <h3 className={styles.cardTitle}>Next Academic Activities</h3>
                <p className={styles.cardMeta}>
                  {upcomingLoading
                    ? "Loading…"
                    : upcoming.length === 0
                      ? "No upcoming activities yet."
                      : `${upcoming.length} item${upcoming.length !== 1 ? "s" : ""} on the way`}
                </p>
              </div>
            </div>
            <Link to="/student/timetable" className={styles.viewAll}>Open Timetable →</Link>
          </div>
          <div className={styles.upList}>
            {!upcomingLoading && upcoming.length === 0 && (
              <div style={{ padding:"18px 4px", color:"var(--text-muted, #94a3b8)", fontSize:13.5 }}>
                No upcoming activities yet. Check back when an instructor publishes new material.
              </div>
            )}
            {upcoming.map((item,i) => (
              <motion.div key={item.id} className={styles.upRow}
                initial={{ opacity:0, x:-16 }} whileInView={{ opacity:1, x:0 }}
                viewport={{ once:true }} transition={{ delay:i*0.08 }}
                whileHover={{ x:6 }}
                style={{ "--c": item.color }}
              >
                <div className={styles.upIcon}>
                  {item.type === "quiz" ? "✏️" : item.type === "lecture" ? "🎬" : "📋"}
                </div>
                <div className={styles.upMid}>
                  <strong>{item.title}</strong>
                  <span>{item.course}</span>
                </div>
                <div className={styles.upEnd}>
                  <span className={styles.upDate}>{item.date}</span>
                  <span className={styles.upTime}>{item.time}</span>
                </div>
              </motion.div>
            ))}
          </div>
        </motion.div>

        <motion.div className={styles.card}
          initial={{ opacity:0, x:30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55 }}
        >
          <div className={styles.cardHeader}>
            <div className={styles.cardHeaderLeft}>
              <span className={styles.cardIcon}>📚</span>
              <div>
                <h3 className={styles.cardTitle}>My Courses</h3>
                <p className={styles.cardMeta}>Current semester</p>
              </div>
            </div>
            <Link to="/student/courses" className={styles.viewAll}>View all →</Link>
          </div>
          <div className={styles.courseList}>
            {COURSES.map((c,i) => (
              <motion.div key={c.id} className={styles.courseRow}
                initial={{ opacity:0, y:12 }} whileInView={{ opacity:1, y:0 }}
                viewport={{ once:true }} transition={{ delay:i*0.08 }}
                whileHover={{ x:4 }}
                style={{ "--c": c.color }}
              >
                <div className={styles.courseEmoji}>{c.icon}</div>
                <div className={styles.courseMid}>
                  <strong>{c.name}</strong>
                  <span>{c.instructor}</span>
                  <div className={styles.track}>
                    <motion.div className={styles.trackFill}
                      initial={{ width:0 }}
                      whileInView={{ width:`${c.progress}%` }}
                      viewport={{ once:true }}
                      transition={{ duration:1, delay:0.3+i*0.1, ease:"easeOut" }}
                    />
                  </div>
                </div>
                <span className={styles.coursePct}>{c.progress}%</span>
              </motion.div>
            ))}
          </div>
        </motion.div>
      </div>

      {/* ══ OUR STORY ════════════════════════════ */}
      <motion.div className={styles.storySection}
        initial={{ opacity:0, y:30 }} whileInView={{ opacity:1, y:0 }}
        viewport={{ once:true }} transition={{ duration:0.6 }}
      >
        <div className={styles.storyLeft}>
          <span className={styles.sectionEyebrow}>📖 Our Story</span>
          <h2 className={styles.sectionTitle}>Building Leaders Since 1999</h2>
          <p>
            Since its establishment, Akhbar El-Yom Academy has focused on offering high-quality education
            aligned with the needs of the Egyptian and regional labor markets. The academy aims to bridge
            the gap between academic teaching and real industry experience through training opportunities,
            internships, and partnerships with companies and institutions.
          </p>
          <p>
            Over the years, the academy has graduated thousands of students who now work across fields such
            as engineering, technology, media, business, and communication.
          </p>
          <p>
            Today, the academy stands as a testament to the power of education to transform lives. As we
            look to the future, we remain committed to our founding mission while adapting to the evolving
            needs of society through strategic partnerships, technological innovation, and a student-center
            approach to set new standards in higher education.
          </p>
        </div>
        <div className={styles.storyRight}>
          <div className={styles.storyImgWrap}>
            <div className={styles.storyImgPlaceholder}>
              <motion.div
                animate={{ scale:[1,1.05,1] }}
                transition={{ duration:4, repeat:Infinity, ease:"easeInOut" }}
                className={styles.storyBuilding}
              >
                🏛️
              </motion.div>
              <p>Akhbar Elyom Academy</p>
              <span>6th of October City, Giza</span>
            </div>
          </div>
        </div>
      </motion.div>

      {/* ══ CORE VALUES ══════════════════════════ */}
      <div>
        <div className={styles.sectionHead}>
          <span className={styles.sectionEyebrow}>✨ What We Stand For</span>
          <h2 className={styles.sectionTitle}>Our Core Values</h2>
          <p className={styles.sectionSub}>The principles that guide everything we do</p>
        </div>
        <motion.div className={styles.valuesGrid}
          initial="hidden" whileInView="visible" viewport={{ once:true }}
          variants={{ hidden:{}, visible:{ transition:{ staggerChildren:0.1 } } }}
        >
          {VALUES.map((v,i) => (
            <motion.div key={v.title} className={styles.valueCard}
              variants={{ hidden:{opacity:0,y:28}, visible:{opacity:1,y:0,transition:{duration:0.5,ease:[0.22,1,0.36,1]}} }}
              whileHover={{ y:-10, scale:1.03 }}
            >
              <motion.span className={styles.valueEmoji}
                animate={{ rotate:[0,8,-8,0] }}
                transition={{ duration:3, repeat:Infinity, delay:i*0.7 }}
              >{v.icon}</motion.span>
              <h4>{v.title}</h4>
              <p>{v.desc}</p>
            </motion.div>
          ))}
        </motion.div>
      </div>

      {/* ══ MILESTONES ═══════════════════════════ */}
      <div>
        <div className={styles.sectionHead}>
          <span className={styles.sectionEyebrow}>🏆 Our Journey</span>
          <h2 className={styles.sectionTitle}>Key Milestones</h2>
          <p className={styles.sectionSub}>The principles that guide everything we do</p>
        </div>
        <div className={styles.milestoneList}>
          {MILESTONES.map((m,i) => (
            <motion.div key={m.year} className={styles.milestoneRow}
              initial={{ opacity:0, x:-40 }} whileInView={{ opacity:1, x:0 }}
              viewport={{ once:true }} transition={{ delay:i*0.1, ease:[0.22,1,0.36,1] }}
              whileHover={{ x:8 }}
              style={{ "--c": m.color }}
            >
              <div className={styles.milestoneYear}>{m.year}</div>
              <div className={styles.milestoneDot} />
              <p className={styles.milestoneText}>{m.text}</p>
            </motion.div>
          ))}
        </div>
      </div>

      {/* ══ LEADERSHIP ═══════════════════════════ */}
      <motion.div className={styles.leaderSection}
        initial={{ opacity:0, y:30 }} whileInView={{ opacity:1, y:0 }}
        viewport={{ once:true }} transition={{ duration:0.6 }}
      >
        <div className={styles.leaderLeft}>
          <motion.div className={styles.leaderIcon}
            animate={{ rotate:[0,10,-10,0] }}
            transition={{ duration:4, repeat:Infinity }}
          >🤝</motion.div>
          <h2>Leadership Commitment</h2>
          <p>
            Our leadership team is dedicated to fostering an environment of academic excellence, innovation,
            and integrity. We are committed to providing every student with the tools and opportunities
            they need to succeed in a rapidly changing world.
          </p>
          <p>
            Through strategic partnerships, state-of-the-art facilities, and world-class faculty, we ensure
            that Akhbar Elyom Academy remains at the forefront of higher education in Egypt and the region.
          </p>
        </div>
        <div className={styles.leaderRight}>
          {[
            { num:"20+", label:"Years of Excellence" },
            { num:"5000+", label:"Graduates" },
            { num:"50+", label:"Industry Partners" },
            { num:"98%", label:"Employment Rate" },
          ].map((x,i) => (
            <motion.div key={x.label} className={styles.leaderStat}
              initial={{ opacity:0, scale:0.8 }} whileInView={{ opacity:1, scale:1 }}
              viewport={{ once:true }} transition={{ delay:i*0.1, type:"spring" }}
              whileHover={{ scale:1.05 }}
            >
              <strong>{x.num}</strong>
              <span>{x.label}</span>
            </motion.div>
          ))}
        </div>
      </motion.div>

      {/* ══ CONTACT + MAP ════════════════════════ */}
      <div className={styles.contactSection}>
        <motion.div className={styles.contactLeft}
          initial={{ opacity:0, x:-30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55 }}
        >
          <h3 className={styles.contactTitle}>🏢 The Academy Office</h3>
          <div className={styles.contactList}>
            {CONTACT.map(c => (
              <div key={c.label} className={styles.contactRow}>
                <span className={styles.contactIcon}>{c.icon}</span>
                <div>
                  <span className={styles.contactLabel}>{c.label}:</span>
                  <span className={styles.contactVal}>{c.value}</span>
                </div>
              </div>
            ))}
          </div>
        </motion.div>

        <motion.div className={styles.contactRight}
          initial={{ opacity:0, x:30 }} whileInView={{ opacity:1, x:0 }}
          viewport={{ once:true }} transition={{ duration:0.55 }}
        >
          <h3 className={styles.contactTitle}>📍 Our Location</h3>
          <div className={styles.mapWrap}>
            {/* Google Maps embed — 6th of October City, Giza */}
            <motion.div className={styles.mapFrame}
              initial={{ opacity:0, scale:0.97 }}
              whileInView={{ opacity:1, scale:1 }}
              viewport={{ once:true }}
              transition={{ duration:0.6, ease:[0.22,1,0.36,1] }}>
              <iframe
                title="Akhbar Elyom Academy Location"
                className={styles.mapIframe}
                src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3456.9!2d30.9800!3d29.9600!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMjnCsDU3JzM2LjAiTiAzMMKwNTgnNDguMCJF!5e0!3m2!1sen!2seg!4v1234567890!5m2!1sen!2seg"
                loading="lazy"
                referrerPolicy="no-referrer-when-downgrade"
                allowFullScreen
              />
              {/* Overlay with animation on first reveal */}
              <motion.div className={styles.mapRevealOverlay}
                initial={{ scaleY:1 }}
                whileInView={{ scaleY:0 }}
                viewport={{ once:true }}
                transition={{ duration:0.7, delay:0.3, ease:[0.22,1,0.36,1] }}
                style={{ originY:0 }}/>
            </motion.div>
            <motion.a
              href="https://maps.app.goo.gl/FtdqeLGivJSQnqBD9"
              target="_blank" rel="noopener noreferrer"
              className={styles.mapOpenBtn}
              whileHover={{ scale:1.04, y:-2 }} whileTap={{ scale:0.97 }}>
              <span>🗺️</span> Open in Google Maps
            </motion.a>
          </div>
        </motion.div>
      </div>

    </div>
  );
}