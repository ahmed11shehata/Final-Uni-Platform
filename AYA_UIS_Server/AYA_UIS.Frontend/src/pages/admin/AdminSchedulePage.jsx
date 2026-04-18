// src/pages/admin/AdminSchedulePage.jsx
import { useState, useMemo, useCallback, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { COURSE_DB } from "../../services/mock/mockData";
import { getAdminCourses } from "../../services/api/adminApi";
import {
  getAdminSessions, addSession as apiAddSession, removeSession as apiRemoveSession,
  getAdminExams,   addExam   as apiAddExam,     removeExam   as apiRemoveExam,
  publishSchedule,
} from "../../services/api/scheduleApi";
import styles from "./AdminSchedulePage.module.css";

/* ─── Constants ─── */
const DAYS       = ["Saturday","Sunday","Monday","Tuesday","Wednesday","Thursday"];
const HOURS      = [8,9,10,11,12,13,14,15,16];
const PX_PER_HOUR = 96;
const YEAR_LABELS = ["","First Year","Second Year","Third Year","Fourth Year"];
const YEAR_COLORS = ["","#818cf8","#22c55e","#f59e0b","#ef4444"];
const DURATIONS   = [{v:1,l:"1 hour"},{v:1.5,l:"1.5 hours"},{v:2,l:"2 hours"}];

function fmtH(h){
  const hr=Math.floor(h),m=h%1===0.5?"30":"00",ap=hr>=12?"PM":"AM";
  const d=hr>12?hr-12:hr===0?12:hr;
  return `${d}:${m} ${ap}`;
}

/* Parse "10:30 AM" → 10.5, "2:00 PM" → 14.0 */
function parseTimeStr(str){
  if(!str) return 9;
  const [t,ap]=(str+" ").split(" ");
  const [h,m]=(t||"9:00").split(":").map(Number);
  let hr=h||9;
  if(ap==="PM"&&hr!==12) hr+=12;
  if(ap==="AM"&&hr===12) hr=0;
  return hr+(m||0)/60;
}

/* ── Minimal icons ── */
const Ic={
  plus:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>,
  close: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>,
  check: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>,
  trash: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/></svg>,
  spin:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className={styles.spinIcon}><circle cx="12" cy="12" r="10" strokeOpacity="0.25"/><path d="M12 2a10 10 0 0 1 10 10" strokeLinecap="round"/></svg>,
};

/* ── Field wrapper ── */
function Field({ label, required, error, half, children }) {
  return (
    <div className={`${styles.field} ${half?styles.fieldHalf:""}`}>
      <label className={styles.fieldLabel}>{label}{required&&<span className={styles.req}> *</span>}</label>
      {children}
      {error && <span className={styles.fieldError}>{error}</span>}
    </div>
  );
}

/* ── Modal shell ── */
function Modal({ title, sub, onClose, footer, children }) {
  return (
    <motion.div className={styles.overlay}
      initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}} onClick={onClose}>
      <motion.div className={styles.modal}
        initial={{scale:0.92,y:20,opacity:0}} animate={{scale:1,y:0,opacity:1}}
        exit={{scale:0.95,opacity:0}} transition={{type:"spring",stiffness:400,damping:28}}
        onClick={e=>e.stopPropagation()}>
        <div className={styles.modalHead}>
          <div>
            <h3 className={styles.modalTitle}>{title}</h3>
            {sub&&<p className={styles.modalSub}>{sub}</p>}
          </div>
          <button className={styles.modalClose} onClick={onClose}>{Ic.close}</button>
        </div>
        <div className={styles.modalBody}>{children}</div>
        {footer && <div className={styles.modalFoot}>{footer}</div>}
      </motion.div>
    </motion.div>
  );
}

/* ═══════════════════════════════════════════════
   PAGE
═══════════════════════════════════════════════ */
export default function AdminSchedulePage() {
  const [yr,      setYr]      = useState(1);
  const [grp,     setGrp]     = useState("A");
  const [view,    setView]    = useState("weekly");
  const [examT,   setExamT]   = useState("midterm");
  const [slotM,   setSlotM]   = useState(null);
  const [addM,    setAddM]    = useState(false);
  const [examM,   setExamM]   = useState(false);
  const [toast,   setToast]   = useState(null);
  const [publishing,  setPublishing]  = useState(false);

  /* ── Data state ── */
  const [courses,  setCourses]  = useState([]);
  const [sessions, setSessions] = useState([]);
  const [exams,    setExams]    = useState([]);
  const [loadingSess, setLoadingSess] = useState(false);
  const [loadingExam, setLoadingExam] = useState(false);

  const showToast = useCallback((msg, type="success")=>{
    setToast({msg,type}); setTimeout(()=>setToast(null),3000);
  },[]);

  /* ── Helpers to normalise API shapes ── */
  const normSession = (s) => ({
    ...s,
    start: s.startTime ?? s.start,
    end:   s.endTime   ?? s.end,
    color: s.color || "#818cf8",
  });
  const normExam = (e) => ({
    ...e,
    time: e.startTime != null ? fmtH(e.startTime) : (e.time || "—"),
    hall: e.location ?? e.hall ?? "—",
  });

  /* ── Load courses for year (API, fallback mock) ── */
  useEffect(()=>{
    let cancelled=false;
    async function load(){
      try{
        const data = await getAdminCourses({ year: yr });
        if(!cancelled && data?.length) setCourses(data);
        else if(!cancelled) setCourses(COURSE_DB.filter(c=>c.year===yr));
      }catch{
        if(!cancelled) setCourses(COURSE_DB.filter(c=>c.year===yr));
      }
    }
    load();
    return()=>{ cancelled=true; };
  },[yr]);

  /* ── Load sessions ── */
  const fetchSessions = useCallback(async()=>{
    setLoadingSess(true);
    try{
      const data = await getAdminSessions(yr, grp);
      setSessions((data||[]).map(normSession));
    }catch{
      setSessions([]);
    }finally{ setLoadingSess(false); }
  },[yr,grp]);

  useEffect(()=>{ fetchSessions(); },[fetchSessions]);

  /* ── Load exams ── */
  const fetchExams = useCallback(async()=>{
    setLoadingExam(true);
    try{
      const data = await getAdminExams(yr, examT);
      setExams((data||[]).map(normExam));
    }catch{
      setExams([]);
    }finally{ setLoadingExam(false); }
  },[yr,examT]);

  useEffect(()=>{ fetchExams(); },[fetchExams]);

  /* ── Add sessions (one or two for "both groups") ── */
  const onAddSess = useCallback(async(sessArr)=>{
    try{
      await Promise.all(sessArr.map(s=>apiAddSession({
        year:       yr,
        group:      s.group,
        day:        s.day,
        startTime:  s.start,
        endTime:    s.end,
        courseCode: s.code,
        type:       s.type,
        instructor: s.instructor,
        room:       s.room,
      })));
      await fetchSessions();
      setSlotM(null); setAddM(false);
      showToast(`${sessArr[0].code} added · ${YEAR_LABELS[yr]}`);
    }catch(err){
      const msg = err?.response?.data?.error || "Failed to add session";
      showToast(msg,"error");
    }
  },[yr,fetchSessions,showToast]);

  /* ── Remove session ── */
  const onRemSess = useCallback(async(id)=>{
    try{
      await apiRemoveSession(id);
      setSessions(p=>p.filter(s=>s.id!==id));
    }catch{
      showToast("Failed to remove session","error");
    }
  },[showToast]);

  /* ── Add exam ── */
  const onAddExam = useCallback(async(data)=>{
    try{
      await apiAddExam({
        year:       yr,
        courseCode: data.code,
        type:       examT,
        date:       new Date(data.date+"T12:00"),
        startTime:  parseTimeStr(data.time),
        duration:   data.duration,
        location:   data.hall,
      });
      await fetchExams();
      setExamM(false);
      showToast(`Exam added: ${data.code}`);
    }catch(err){
      const msg = err?.response?.data?.error || "Failed to add exam";
      showToast(msg,"error");
    }
  },[yr,examT,fetchExams,showToast]);

  /* ── Remove exam ── */
  const onRemExam = useCallback(async(id)=>{
    try{
      await apiRemoveExam(id);
      setExams(p=>p.filter(e=>e.id!==id));
    }catch{
      showToast("Failed to remove exam","error");
    }
  },[showToast]);

  /* ── Publish ── */
  const handlePublish = async()=>{
    setPublishing(true);
    try{
      const type = view==="weekly" ? "weekly" : examT;
      await publishSchedule(yr,type);
      showToast(`${YEAR_LABELS[yr]} ${type} schedule published ✓`);
    }catch{
      showToast(`Publish failed — check API`,"error");
    }finally{ setPublishing(false); }
  };

  const yc = YEAR_COLORS[yr];

  return (
    <div className={styles.page}>

      {/* Toast */}
      <AnimatePresence>
        {toast&&(
          <motion.div className={`${styles.toast} ${styles[`toast_${toast.type}`]}`}
            initial={{opacity:0,y:-18,x:"-50%"}} animate={{opacity:1,y:0,x:"-50%"}}
            exit={{opacity:0,x:"-50%"}} transition={{type:"spring",stiffness:440,damping:28}}>
            <span className={styles.toastIcon}>{toast.type==="error" ? Ic.close : Ic.check}</span>{toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      {/* Modals */}
      <AnimatePresence>
        {slotM&&<SlotModal slot={slotM} courses={courses} yr={yr} onAdd={onAddSess} onClose={()=>setSlotM(null)}/>}
      </AnimatePresence>
      <AnimatePresence>
        {addM&&<AddModal courses={courses} yr={yr} onAdd={onAddSess} onClose={()=>setAddM(false)}/>}
      </AnimatePresence>
      <AnimatePresence>
        {examM&&<ExamModal courses={courses} yr={yr} onAdd={onAddExam} onClose={()=>setExamM(false)}/>}
      </AnimatePresence>

      {/* ── Header ── */}
      <motion.div className={styles.header} initial={{opacity:0,y:-10}} animate={{opacity:1,y:0}}>
        <div className={styles.headerL}>
          <h1 className={styles.headerTitle}>Schedule Manager</h1>
          <span className={styles.headerSub}>Draft Editor</span>
        </div>
        <div className={styles.headerR}>
          {view==="weekly"&&(
            <motion.button className={styles.addBtn} onClick={()=>setAddM(true)} whileTap={{scale:0.97}}>
              {Ic.plus} Add Lecture
            </motion.button>
          )}
          {view==="exams"&&(
            <motion.button className={styles.addExamBtn} onClick={()=>setExamM(true)} whileTap={{scale:0.97}}>
              {Ic.plus} Add Exam
            </motion.button>
          )}
          <motion.button
            className={styles.saveBtn}
            onClick={handlePublish}
            disabled={publishing}
            whileTap={{scale:0.97}}
          >
            {publishing ? "Publishing…" : "Publish"}
          </motion.button>
        </div>
      </motion.div>

      {/* ── Controls ── */}
      <div className={styles.controls}>
        <div className={styles.ctrlL}>
          <div className={styles.yearTabs}>
            {[1,2,3,4].map(y=>(
              <button key={y}
                className={`${styles.yearTab} ${yr===y?styles.yearTabOn:""}`}
                style={yr===y?{"--yc":YEAR_COLORS[y]}:{}}
                onClick={()=>setYr(y)}>
                <span className={styles.ytNum}>Year {y}</span>
              </button>
            ))}
          </div>
        </div>
        <div className={styles.ctrlR}>
          <div className={styles.seg}>
            {[{k:"weekly",l:"Weekly"},{k:"exams",l:"Exams"}].map(v=>(
              <button key={v.k} className={`${styles.segBtn} ${view===v.k?styles.segOn:""}`} onClick={()=>setView(v.k)}>{v.l}</button>
            ))}
          </div>
          {view==="weekly"&&(
            <div className={styles.seg}>
              {["A","B"].map(g=>(
                <button key={g} className={`${styles.segBtn} ${grp===g?styles.segOn:""}`} onClick={()=>setGrp(g)}>Group {g}</button>
              ))}
            </div>
          )}
          {view==="exams"&&(
            <div className={styles.seg}>
              {[{k:"midterm",l:"Midterm"},{k:"final",l:"Final"}].map(t=>(
                <button key={t.k} className={`${styles.segBtn} ${examT===t.k?styles.segOn:""}`} onClick={()=>setExamT(t.k)}>{t.l}</button>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* ── Year banner ── */}
      <div className={styles.yrBanner} style={{"--yc":yc}}>
        <span className={styles.yrBannerDot}/>
        <span className={styles.yrBannerText}>
          {YEAR_LABELS[yr]}
          {view==="weekly" ? ` · Group ${grp}` : ` · ${examT==="midterm"?"Midterm":"Final"} Exams`}
        </span>
        <span className={styles.yrBannerCount}>
          {view==="weekly"
            ? `${sessions.length} session${sessions.length!==1?"s":""} in draft`
            : `${exams.length} exam${exams.length!==1?"s":""} in draft`}
        </span>
        {view==="weekly"&&<span className={styles.yrBannerHint}>Click empty slot to quick-add</span>}
        {(loadingSess||loadingExam)&&<span className={styles.yrBannerHint}>{Ic.spin}</span>}
      </div>

      {/* ── Weekly view ── */}
      {view==="weekly"&&(
        <motion.div className={styles.scheduleWrap} initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} transition={{delay:0.04}}>
          <div className={styles.grid}>
            <div className={styles.timeCol}>
              <div className={styles.timeColHead}>Time</div>
              {HOURS.map(h=>(
                <div key={h} className={styles.timeSlot} style={{height:PX_PER_HOUR}}>
                  <span className={styles.timeLabel}>{fmtH(h)}</span>
                </div>
              ))}
            </div>
            {DAYS.map(day=>(
              <DayCol key={day} day={day}
                sessions={sessions.filter(s=>s.day===day)}
                yc={yc}
                onClickSlot={start=>setSlotM({day,start})}
                onRemove={onRemSess}/>
            ))}
          </div>
          {!loadingSess && sessions.length===0&&(
            <motion.div className={styles.empty} initial={{opacity:0,y:8}} animate={{opacity:1,y:0}}>
              <p className={styles.emptyTitle}>No sessions yet for {YEAR_LABELS[yr]}</p>
              <p className={styles.emptyHint}>Click a time slot or use <strong>Add Lecture</strong></p>
            </motion.div>
          )}
        </motion.div>
      )}

      {/* ── Exams view ── */}
      {view==="exams"&&(
        <motion.div className={styles.examsWrap} initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} transition={{delay:0.04}}>
          {!loadingExam && exams.length===0?(
            <motion.div className={styles.empty} initial={{opacity:0,y:8}} animate={{opacity:1,y:0}}>
              <p className={styles.emptyTitle}>No {examT} exams for {YEAR_LABELS[yr]}</p>
              <p className={styles.emptyHint}>Use <strong>Add Exam</strong> to schedule one</p>
            </motion.div>
          ):(
            <motion.div className={styles.examsGrid} initial="h" animate="s" variants={{s:{transition:{staggerChildren:0.05}}}}>
              {exams.map(ex=><ExamCard key={ex.id} exam={ex} onRemove={()=>onRemExam(ex.id)}/>)}
            </motion.div>
          )}
        </motion.div>
      )}
    </div>
  );
}

/* ── Day Column ── */
function DayCol({day,sessions,yc,onClickSlot,onRemove}){
  const SH=8;
  return(
    <div className={styles.dayCol}>
      <div className={styles.dayHead}>{day.slice(0,3)}</div>
      <div className={styles.dayBody} style={{height:HOURS.length*PX_PER_HOUR}}
        onClick={e=>{
          const r=e.currentTarget.getBoundingClientRect();
          const raw=SH+(e.clientY-r.top)/PX_PER_HOUR;
          const sn=Math.max(SH,Math.min(Math.round(raw*2)/2,15.5));
          onClickSlot(sn);
        }}>
        {HOURS.map(h=><div key={h} className={styles.hourLine} style={{top:(h-SH)*PX_PER_HOUR}}/>)}
        {sessions.map(s=>{
          const top=(s.start-SH)*PX_PER_HOUR+3;
          const ht=(s.end-s.start)*PX_PER_HOUR-6;
          const isL=s.type==="Lecture";
          const c=s.color||yc;
          return(
            <motion.div key={s.id} className={styles.sessBlock}
              style={{top,height:ht,borderLeftColor:c,"--sc":c,background:`color-mix(in srgb, ${c} 8%, var(--card-bg))`}}
              initial={{opacity:0,scaleY:0.6}} animate={{opacity:1,scaleY:1}}
              transition={{type:"spring",stiffness:400,damping:28}}
              onClick={e=>e.stopPropagation()}>
              <div className={styles.sessTop}>
                <span className={styles.sessCode} style={{background:c}}>{s.code}</span>
                <span className={`${styles.sessType} ${isL?styles.sessL:styles.sessS}`}>{isL?"LEC":s.type==="Lab"?"LAB":"SEC"}</span>
              </div>
              {ht>=40&&<div className={styles.sessName}>{s.name}</div>}
              {ht>=60&&s.instructor&&<div className={styles.sessMeta}>{s.instructor}</div>}
              {ht>=76&&s.room&&<div className={styles.sessMeta}>{s.room}</div>}
              {ht>=90&&<div className={styles.sessTime}>{fmtH(s.start)} – {fmtH(s.end)}</div>}
              <span className={styles.sessGrp} style={{background:`${c}18`,color:c}}>{s.group}</span>
              <motion.button className={styles.sessRemove} onClick={e=>{e.stopPropagation();onRemove(s.id);}}
                whileTap={{scale:0.85}}>{Ic.close}</motion.button>
            </motion.div>
          );
        })}
      </div>
    </div>
  );
}

/* ── Shared form helpers ── */
function useForm(initialErrors){
  const [touched,setTouched]=useState({});
  const touch=(f)=>setTouched(p=>({...p,[f]:true}));
  const touchAll=()=>setTouched(Object.fromEntries(Object.keys(initialErrors).map(k=>[k,true])));
  return{touched,touch,touchAll};
}

/* ── Add Lecture Modal ── */
function AddModal({courses,yr,onAdd,onClose}){
  const [code,setCode]=useState(courses[0]?.code||"");
  const [inst,setInst]=useState(courses[0]?.instructor||"");
  const [day,setDay]=useState("Saturday");
  const [sh,setSh]=useState(8);
  const [sm,setSm]=useState(0);
  const [dur,setDur]=useState(1.5);
  const [type,setType]=useState("Lecture");
  const [room,setRoom]=useState("");
  const [roomB,setRoomB]=useState("");
  const [grpMode,setGrpMode]=useState("both");
  const [saving,setSaving]=useState(false);

  const course=courses.find(c=>c.code===code);
  const startDec=Math.round((sh+sm/60)*4)/4;
  const errMap={code:!code?"Required":null,inst:!inst.trim()?"Required":null,room:!room.trim()?"Required":null};
  const valid=!Object.values(errMap).some(Boolean);
  const {touched,touch,touchAll}=useForm(errMap);

  const onChangeCourse=c=>{setCode(c);const f=courses.find(x=>x.code===c);if(f?.instructor)setInst(f.instructor);};

  const handleAdd=async()=>{
    touchAll();
    if(!valid||saving)return;
    setSaving(true);
    const base={day,start:startDec,end:startDec+dur,code,name:course?.name||code,type,color:course?.color||"#818cf8",instructor:inst.trim(),duration:dur};
    const arr=grpMode==="both"
      ?[{...base,group:"A",room:room.trim()},{...base,group:"B",room:roomB.trim()||room.trim()}]
      :[{...base,group:grpMode,room:room.trim()}];
    await onAdd(arr);
    setSaving(false);
  };

  return(
    <Modal title="Add Lecture" sub={`${YEAR_LABELS[yr]} · Fill all required fields`} onClose={onClose}
      footer={<><button className={styles.btnGhost} onClick={onClose}>Cancel</button>
        <motion.button className={styles.btnPrimary} onClick={handleAdd} disabled={saving} whileTap={{scale:0.97}}>
          {saving?Ic.spin:Ic.plus} {saving?"Saving…":"Add to Schedule"}
        </motion.button></>}>
      <div className={styles.formGrid}>
        <Field label="Course" required error={touched.code&&errMap.code}>
          <select className={`${styles.inp} ${touched.code&&errMap.code?styles.inpErr:""}`} value={code}
            onChange={e=>{onChangeCourse(e.target.value);touch("code");}}>
            {courses.length===0&&<option>No courses for Year {yr}</option>}
            {courses.map(c=><option key={c.code} value={c.code}>{c.code} — {c.name}</option>)}
          </select>
        </Field>
        <Field label="Instructor Name" required error={touched.inst&&errMap.inst}>
          <input className={`${styles.inp} ${touched.inst&&errMap.inst?styles.inpErr:""}`}
            placeholder="e.g. Dr. Ahmed Hassan" value={inst}
            onChange={e=>{setInst(e.target.value);touch("inst");}}/>
        </Field>
        <div className={styles.formRow}>
          <Field label="Day" required half>
            <select className={styles.inp} value={day} onChange={e=>setDay(e.target.value)}>
              {DAYS.map(d=><option key={d}>{d}</option>)}
            </select>
          </Field>
          <Field label="Session Type" required half>
            <select className={styles.inp} value={type} onChange={e=>setType(e.target.value)}>
              <option>Lecture</option><option>Section</option><option>Lab</option>
            </select>
          </Field>
        </div>
        <div className={styles.formRow}>
          <Field label="Start Time" required half>
            <div className={styles.timePick}>
              <select className={styles.inpSm} value={sh} onChange={e=>setSh(Number(e.target.value))}>
                {HOURS.map(h=>{const d=h>12?h-12:h===0?12:h,ap=h>=12?"PM":"AM";return<option key={h} value={h}>{d}:00 {ap}</option>;})}
              </select>
              <select className={styles.inpSm} value={sm} onChange={e=>setSm(Number(e.target.value))}>
                <option value={0}>:00</option><option value={30}>:30</option>
              </select>
            </div>
          </Field>
          <Field label="Duration" required half>
            <select className={styles.inp} value={dur} onChange={e=>setDur(Number(e.target.value))}>
              {DURATIONS.map(d=><option key={d.v} value={d.v}>{d.l}</option>)}
            </select>
          </Field>
        </div>
        <div className={styles.timePrev}>
          <strong>{day}</strong>
          <span>{fmtH(startDec)} → {fmtH(startDec+dur)}</span>
          <span className={styles.timePrevDur}>{DURATIONS.find(d=>d.v===dur)?.l}</span>
        </div>
        <Field label="Apply to Group" required>
          <div className={styles.grpSel}>
            {[["both","Both Groups (A & B)"],["A","Group A only"],["B","Group B only"]].map(([g,l])=>(
              <button key={g} className={`${styles.grpBtn} ${grpMode===g?styles.grpOn:""}`} onClick={()=>setGrpMode(g)}>{l}</button>
            ))}
          </div>
        </Field>
        <div className={styles.formRow}>
          <Field label={grpMode==="both"?"Room / Hall (Group A)":"Room / Hall"} required error={touched.room&&errMap.room} half>
            <input className={`${styles.inp} ${touched.room&&errMap.room?styles.inpErr:""}`}
              placeholder={grpMode==="both"?"Group A room":"e.g. Hall A, Lab 2"} value={room}
              onChange={e=>{setRoom(e.target.value);touch("room");}}/>
          </Field>
          {grpMode==="both"&&(
            <Field label="Room / Hall (Group B)" half>
              <input className={styles.inp} placeholder="Optional — defaults to Group A" value={roomB} onChange={e=>setRoomB(e.target.value)}/>
            </Field>
          )}
        </div>
      </div>
    </Modal>
  );
}

/* ── Slot Modal ── */
function SlotModal({slot,courses,yr,onAdd,onClose}){
  const [code,setCode]=useState(courses[0]?.code||"");
  const [inst,setInst]=useState(courses[0]?.instructor||"");
  const [dur,setDur]=useState(1.5);
  const [type,setType]=useState("Lecture");
  const [room,setRoom]=useState("");
  const [roomB,setRoomB]=useState("");
  const [grpMode,setGrpMode]=useState("both");
  const [saving,setSaving]=useState(false);

  const course=courses.find(c=>c.code===code);
  const errMap={code:!code?"Required":null,inst:!inst.trim()?"Required":null,room:!room.trim()?"Required":null};
  const valid=!Object.values(errMap).some(Boolean);
  const {touched,touch,touchAll}=useForm(errMap);

  const onChangeCourse=c=>{setCode(c);const f=courses.find(x=>x.code===c);if(f?.instructor)setInst(f.instructor);};

  const handleAdd=async()=>{
    touchAll();
    if(!valid||saving)return;
    setSaving(true);
    const base={day:slot.day,start:slot.start,end:slot.start+dur,code,name:course?.name||code,type,color:course?.color||"#818cf8",instructor:inst.trim(),duration:dur};
    const arr=grpMode==="both"
      ?[{...base,group:"A",room:room.trim()},{...base,group:"B",room:roomB.trim()||room.trim()}]
      :[{...base,group:grpMode,room:room.trim()}];
    await onAdd(arr);
    setSaving(false);
  };

  return(
    <Modal title="Add Session" sub={`${slot.day} · Starting at ${fmtH(slot.start)}`} onClose={onClose}
      footer={<><button className={styles.btnGhost} onClick={onClose}>Cancel</button>
        <motion.button className={styles.btnPrimary} onClick={handleAdd} disabled={saving} whileTap={{scale:0.97}}>
          {saving?Ic.spin:Ic.plus} {saving?"Saving…":"Add Session"}
        </motion.button></>}>
      <div className={styles.formGrid}>
        <Field label="Course" required error={touched.code&&errMap.code}>
          <select className={`${styles.inp} ${touched.code&&errMap.code?styles.inpErr:""}`} value={code}
            onChange={e=>{onChangeCourse(e.target.value);touch("code");}}>
            {courses.length===0&&<option>No courses for Year {yr}</option>}
            {courses.map(c=><option key={c.code} value={c.code}>{c.code} — {c.name}</option>)}
          </select>
        </Field>
        <Field label="Instructor Name" required error={touched.inst&&errMap.inst}>
          <input className={`${styles.inp} ${touched.inst&&errMap.inst?styles.inpErr:""}`}
            placeholder="e.g. Dr. Ahmed Hassan" value={inst}
            onChange={e=>{setInst(e.target.value);touch("inst");}}/>
        </Field>
        <div className={styles.formRow}>
          <Field label="Session Type" required half>
            <select className={styles.inp} value={type} onChange={e=>setType(e.target.value)}>
              <option>Lecture</option><option>Section</option><option>Lab</option>
            </select>
          </Field>
          <Field label="Duration" required half>
            <select className={styles.inp} value={dur} onChange={e=>setDur(Number(e.target.value))}>
              {DURATIONS.map(d=><option key={d.v} value={d.v}>{d.l}</option>)}
            </select>
          </Field>
        </div>
        <div className={styles.timePrev}>
          <strong>{slot.day}</strong>
          <span>{fmtH(slot.start)} → {fmtH(slot.start+dur)}</span>
          <span className={styles.timePrevDur}>{DURATIONS.find(d=>d.v===dur)?.l}</span>
        </div>
        <Field label="Apply to Group" required>
          <div className={styles.grpSel}>
            {[["both","Both Groups (A & B)"],["A","Group A only"],["B","Group B only"]].map(([g,l])=>(
              <button key={g} className={`${styles.grpBtn} ${grpMode===g?styles.grpOn:""}`} onClick={()=>setGrpMode(g)}>{l}</button>
            ))}
          </div>
        </Field>
        <div className={styles.formRow}>
          <Field label={grpMode==="both"?"Room / Hall (Group A)":"Room / Hall"} required error={touched.room&&errMap.room} half>
            <input className={`${styles.inp} ${touched.room&&errMap.room?styles.inpErr:""}`}
              placeholder="e.g. Hall A, Lab 2" value={room}
              onChange={e=>{setRoom(e.target.value);touch("room");}}/>
          </Field>
          {grpMode==="both"&&(
            <Field label="Room / Hall (Group B)" half>
              <input className={styles.inp} placeholder="Optional" value={roomB} onChange={e=>setRoomB(e.target.value)}/>
            </Field>
          )}
        </div>
      </div>
    </Modal>
  );
}

/* ── Exam Modal ── */
function ExamModal({courses,yr,onAdd,onClose}){
  const [code,setCode]=useState(courses[0]?.code||"");
  const [date,setDate]=useState("");
  const [time,setTime]=useState("10:00");
  const [hall,setHall]=useState("");
  const [dur,setDur]=useState(2);
  const [saving,setSaving]=useState(false);

  const errMap={code:!code?"Required":null,date:!date?"Required":null,time:!time?"Required":null,hall:!hall.trim()?"Required":null};
  const valid=!Object.values(errMap).some(Boolean);
  const {touched,touch,touchAll}=useForm(errMap);
  const course=courses.find(c=>c.code===code);

  const handleAdd=async()=>{
    touchAll(); if(!valid||saving) return;
    setSaving(true);
    const [hr,mn]=time.split(":");const h=parseInt(hr),ap=h>=12?"PM":"AM",d=h>12?h-12:h===0?12:h;
    await onAdd({code,name:course?.name||code,color:course?.color||"#818cf8",date,time:`${d}:${mn||"00"} ${ap}`,hall:hall.trim(),duration:dur});
    setSaving(false);
  };

  return(
    <Modal title="Schedule Exam" sub={`${YEAR_LABELS[yr]} · Add exam entry`} onClose={onClose}
      footer={<><button className={styles.btnGhost} onClick={onClose}>Cancel</button>
        <motion.button className={styles.btnPrimary} onClick={handleAdd} disabled={saving} whileTap={{scale:0.97}}>
          {saving?Ic.spin:Ic.plus} {saving?"Saving…":"Add Exam"}
        </motion.button></>}>
      <div className={styles.formGrid}>
        <Field label="Course" required error={touched.code&&errMap.code}>
          <select className={`${styles.inp} ${touched.code&&errMap.code?styles.inpErr:""}`} value={code}
            onChange={e=>{setCode(e.target.value);touch("code");}}>
            {courses.map(c=><option key={c.code} value={c.code}>{c.code} — {c.name}</option>)}
          </select>
        </Field>
        <div className={styles.formRow}>
          <Field label="Date" required error={touched.date&&errMap.date} half>
            <input type="date" className={`${styles.inp} ${touched.date&&errMap.date?styles.inpErr:""}`} value={date}
              onChange={e=>{setDate(e.target.value);touch("date");}}/>
          </Field>
          <Field label="Time" required error={touched.time&&errMap.time} half>
            <input type="time" className={`${styles.inp} ${touched.time&&errMap.time?styles.inpErr:""}`} value={time}
              onChange={e=>{setTime(e.target.value);touch("time");}}/>
          </Field>
        </div>
        <div className={styles.formRow}>
          <Field label="Hall / Room" required error={touched.hall&&errMap.hall} half>
            <input className={`${styles.inp} ${touched.hall&&errMap.hall?styles.inpErr:""}`}
              placeholder="e.g. Main Hall A" value={hall} onChange={e=>{setHall(e.target.value);touch("hall");}}/>
          </Field>
          <Field label="Duration" required half>
            <select className={styles.inp} value={dur} onChange={e=>setDur(Number(e.target.value))}>
              {[1,1.5,2,2.5,3].map(d=><option key={d} value={d}>{d} hr{d!==1?"s":""}</option>)}
            </select>
          </Field>
        </div>
      </div>
    </Modal>
  );
}

/* ── Exam Card ── */
const fadeUp={hidden:{opacity:0,y:12},show:{opacity:1,y:0,transition:{duration:0.35,ease:[0.22,1,0.36,1]}}};
function ExamCard({exam,onRemove}){
  const d=exam.date?new Date(exam.date+"T12:00"):null;
  const mo=d?d.toLocaleDateString("en-US",{month:"short"}):"—";
  const dy=d?d.getDate():"—";
  const wk=d?d.toLocaleDateString("en-US",{weekday:"short"}):"—";
  const dl=d?Math.ceil((d-new Date())/86400000):null;
  const cd=dl===null?null:dl<0?{l:"Passed",c:"#6b7280",bg:"rgba(107,114,128,0.08)"}:dl===0?{l:"Today!",c:"#ef4444",bg:"rgba(239,68,68,0.08)"}:dl<=5?{l:`${dl}d`,c:"#f59e0b",bg:"rgba(245,158,11,0.08)"}:{l:`${dl}d`,c:exam.color,bg:`${exam.color}14`};
  return(
    <motion.div className={styles.examCard} variants={fadeUp}>
      <div className={styles.examStripe} style={{background:exam.color}}/>
      <div className={styles.examBody}>
        <div className={styles.examTile} style={{background:`${exam.color}0c`,borderColor:`${exam.color}28`}}>
          <div className={styles.examMo} style={{color:exam.color}}>{mo}</div>
          <div className={styles.examDy}>{dy}</div>
          <div className={styles.examWk}>{wk}</div>
        </div>
        <div className={styles.examInfo}>
          <div className={styles.examCode} style={{color:exam.color}}>{exam.code}</div>
          <div className={styles.examName}>{exam.name}</div>
          <div className={styles.examMeta}><span>{exam.time}</span><span>{exam.hall}</span><span>{exam.duration}h</span></div>
        </div>
        <div className={styles.examR}>
          {cd&&<div className={styles.examCd} style={{color:cd.c,background:cd.bg,borderColor:`${cd.c}28`}}>{cd.l}</div>}
          <motion.button className={styles.examDel} onClick={onRemove} whileTap={{scale:0.85}}>{Ic.trash}</motion.button>
        </div>
      </div>
    </motion.div>
  );
}
