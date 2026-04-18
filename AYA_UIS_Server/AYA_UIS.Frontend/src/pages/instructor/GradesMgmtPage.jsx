// src/pages/instructor/GradesMgmtPage.jsx
import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./GradesMgmtPage.module.css";

/* ─── Data ─────────────────────────────────────────────── */
const MY_COURSES = [
  { id:"cs401", code:"CS401", name:"Artificial Intelligence", color:"#f59e0b", dark:"#b45309", icon:"🤖" },
  { id:"cs404", code:"CS404", name:"Expert Systems",          color:"#e05c8a", dark:"#9d174d", icon:"🧠" },
];

const EXAM_SCHEDULE = {
  cs401: { midterm:{ date:"2026-02-20", published:true  }, final:{ date:"2026-05-15", published:true  } },
  cs404: { midterm:{ date:"2026-03-22", published:true  }, final:{ date:"2026-05-17", published:false } },
};

const MOCK_SUBMISSIONS = {
  cs401:[
    {id:"a1",assignment:"Search Algorithm",      studentName:"Ahmed Hassan", studentId:"20210001",submittedAt:"Feb 26 · 10:32 PM",file:"ahmed_search.pdf",fileSize:"1.2 MB",maxGrade:15,grade:null,status:"pending"},
    {id:"a2",assignment:"Knowledge Base Design", studentName:"Sara Mohamed", studentId:"20210002",submittedAt:"Mar 12 · 09:15 PM",file:"sara_kb.pdf",     fileSize:"2.8 MB",maxGrade:15,grade:13,  status:"approved"},
    {id:"a3",assignment:"Neural Network",        studentName:"Omar Khalil",  studentId:"20210003",submittedAt:"Apr 1  · 11:58 PM",file:"omar_nn.pdf",      fileSize:"3.5 MB",maxGrade:15,grade:null,status:"pending"},
    {id:"a4",assignment:"Neural Network",        studentName:"Nour Ibrahim", studentId:"20210004",submittedAt:"Apr 1  · 08:44 AM",file:"nour_nn.zip",      fileSize:"5.1 MB",maxGrade:15,grade:null,status:"rejected",rejectionReason:"Suspected AI-generated content"},
    {id:"a5",assignment:"Knowledge Base Design", studentName:"Laila Hassan", studentId:"20210005",submittedAt:"Mar 13 · 07:20 PM",file:"laila_kb.pdf",     fileSize:"1.9 MB",maxGrade:15,grade:14,  status:"approved"},
  ],
  cs404:[
    {id:"b1",assignment:"Expert System Design",  studentName:"Laila Samir",  studentId:"20210010",submittedAt:"Mar 5  · 06:20 PM",file:"laila_expert.pdf",fileSize:"1.8 MB",maxGrade:15,grade:null,status:"pending"},
    {id:"b2",assignment:"Expert System Design",  studentName:"Youssef Ali",  studentId:"20210011",submittedAt:"Mar 4  · 11:30 PM",file:"youssef_es.pdf",  fileSize:"2.2 MB",maxGrade:15,grade:12,  status:"approved"},
    {id:"b3",assignment:"Knowledge Repr.",       studentName:"Mona Karim",   studentId:"20210012",submittedAt:"Mar 18 · 03:10 PM",file:"mona_kr.pdf",      fileSize:"0.9 MB",maxGrade:15,grade:null,status:"pending"},
  ],
};

const STUDENT_META = {
  "20210001":{ year:4, batch:"2021", dept:"CS" }, "20210002":{ year:4, batch:"2021", dept:"CS" },
  "20210003":{ year:4, batch:"2021", dept:"CS" }, "20210004":{ year:4, batch:"2021", dept:"CS" },
  "20210005":{ year:4, batch:"2021", dept:"CS" }, "20210010":{ year:4, batch:"2021", dept:"CS" },
  "20210011":{ year:4, batch:"2021", dept:"CS" }, "20210012":{ year:4, batch:"2021", dept:"CS" },
};

const MOCK_STUDENTS = {
  cs401:[
    {id:"20210001",name:"Ahmed Hassan",  midterm:17, final:null},
    {id:"20210002",name:"Sara Mohamed",  midterm:18, final:null},
    {id:"20210003",name:"Omar Khalil",   midterm:15, final:null},
    {id:"20210004",name:"Nour Ibrahim",  midterm:14, final:null},
    {id:"20210005",name:"Laila Hassan",  midterm:16, final:null},
  ],
  cs404:[
    {id:"20210010",name:"Laila Samir",  midterm:19, final:null},
    {id:"20210011",name:"Youssef Ali",  midterm:17, final:null},
    {id:"20210012",name:"Mona Karim",   midterm:13, final:null},
  ],
};

const REJECT_REASONS = [
  {icon:"🤖", label:"AI-generated",    value:"Suspected AI-generated content"},
  {icon:"📋", label:"Wrong format",    value:"Incorrect submission format"},
  {icon:"📄", label:"Plagiarized",     value:"Plagiarism detected"},
  {icon:"📦", label:"Incomplete",      value:"Submission is incomplete"},
  {icon:"🔗", label:"Wrong assignment",value:"Wrong assignment submitted"},
  {icon:"⚠️", label:"Corrupted file",  value:"File is corrupted or unreadable"},
];

const sp = { type:"spring", stiffness:400, damping:28 };

/* ── Avatar ── */
function Av({ name, color, size=44 }) {
  const ini = name.split(" ").slice(0,2).map(w=>w[0]).join("");
  return (
    <div style={{
      width:size, height:size, borderRadius: size > 40 ? 14 : 10, flexShrink:0,
      display:"flex", alignItems:"center", justifyContent:"center",
      background:`${color}22`, color, border:`1.5px solid ${color}40`,
      fontSize: size > 48 ? "1rem" : ".78rem", fontWeight:800, letterSpacing:"-.02em",
    }}>{ini}</div>
  );
}

/* ════════════ REJECT MODAL ════════════ */
function RejectModal({ sub, onConfirm, onClose }) {
  const [preset, setPreset] = useState("");
  const [custom, setCustom] = useState("");
  const reason = custom.trim() || preset;

  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
      <motion.div className={styles.modal} onClick={e=>e.stopPropagation()}
        initial={{opacity:0,scale:.88,y:28}} animate={{opacity:1,scale:1,y:0}}
        exit={{opacity:0,scale:.95}} transition={sp}>
        <div className={styles.modalBand} style={{background:"linear-gradient(135deg,#b91c1c,#ef4444)"}}/>
        <div className={styles.modalBody}>
          <div className={styles.modalTitle}>Reject Submission</div>
          <div className={styles.modalSub}>{sub.studentName} · {sub.assignment}</div>
          <div className={styles.reasonGrid}>
            {REJECT_REASONS.map(r=>(
              <button key={r.value}
                className={`${styles.reasonBtn} ${preset===r.value&&!custom.trim()?styles.reasonOn:""}`}
                onClick={()=>{setPreset(r.value);setCustom("");}}>
                <span>{r.icon}</span>{r.label}
              </button>
            ))}
          </div>
          <div className={styles.customWrap}>
            <label className={styles.customLabel}>Or write a custom reason:</label>
            <textarea className={styles.customInput} rows={3}
              placeholder="Describe the specific issue…"
              value={custom} onChange={e=>{setCustom(e.target.value);if(e.target.value.trim())setPreset("");}}/>
          </div>
          <div className={styles.mActions}>
            <button className={styles.mCancel} onClick={onClose}>Cancel</button>
            <motion.button className={styles.mReject}
              disabled={!reason} style={{opacity:reason?1:.4}}
              onClick={()=>reason&&onConfirm(reason)}
              whileHover={reason?{scale:1.02}:{}} whileTap={reason?{scale:.97}:{}}>
              Reject Submission
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ════════════ APPROVE MODAL ════════════ */
function ApproveModal({ sub, onConfirm, onClose }) {
  const [grade, setGrade] = useState(sub.grade ?? null);

  return (
    <motion.div className={styles.overlay} onClick={onClose}
      initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
      <motion.div className={styles.modal} onClick={e=>e.stopPropagation()}
        initial={{opacity:0,scale:.88,y:28}} animate={{opacity:1,scale:1,y:0}}
        exit={{opacity:0,scale:.95}} transition={sp}>
        <div className={styles.modalBand} style={{background:"linear-gradient(135deg,#15803d,#22c55e)"}}/>
        <div className={styles.modalBody}>
          <div className={styles.modalTitle}>✓ Approve & Grade</div>
          <div className={styles.modalSub}>{sub.studentName} · {sub.assignment}</div>
          <div style={{margin:"18px 0 6px"}}>
            <div style={{fontSize:".72rem",fontWeight:800,color:"var(--text-muted)",textTransform:"uppercase",letterSpacing:".07em",marginBottom:12}}>
              Assignment Grade (1–5)
            </div>
            <div className={styles.starRow}>
              {[1,2,3,4,5].map(n=>(
                <button key={n}
                  className={`${styles.starBtn} ${grade===n?styles.starBtnOn:""}`}
                  onClick={()=>setGrade(n)}>
                  {n}
                </button>
              ))}
              <span className={styles.starLabel}>out of 5</span>
            </div>
          </div>
          {grade!==null&&(
            <motion.div style={{
              marginTop:14,padding:"12px 16px",borderRadius:12,
              background:grade>=4?"#f0fdf4":grade>=3?"#fef9c3":"#fef2f2",
              border:`1px solid ${grade>=4?"#bbf7d0":grade>=3?"#fde68a":"#fecaca"}`,
              color:grade>=4?"#15803d":grade>=3?"#92400e":"#b91c1c",
              fontSize:".82rem",fontWeight:700,
            }}
            initial={{opacity:0,y:6}} animate={{opacity:1,y:0}}>
              {grade===5?"🌟 Excellent":grade===4?"✅ Good":grade===3?"👍 Average":grade===2?"⚠️ Below average":"❌ Poor"} — Grade: {grade}/5
            </motion.div>
          )}
          <div className={styles.mActions} style={{marginTop:20}}>
            <button className={styles.mCancel} onClick={onClose}>Cancel</button>
            <motion.button className={styles.mApprove}
              disabled={grade===null} style={{opacity:grade!==null?1:.4}}
              onClick={()=>grade!==null&&onConfirm(grade)}
              whileHover={grade!==null?{scale:1.02}:{}} whileTap={grade!==null?{scale:.97}:{}}>
              ✓ Approve Submission
            </motion.button>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ════════════ ASSIGNMENTS SECTION ════════════ */
function AssignmentsSection({ courseId, color }) {
  const [subs,     setSubs]     = useState(MOCK_SUBMISSIONS[courseId]||[]);
  const [filter,   setFilter]   = useState("all");
  const [selected, setSelected] = useState(new Set());
  const [rejectT,  setRejectT]  = useState(null);
  const [approveT, setApproveT] = useState(null);
  const [toast,    setToast]    = useState(null);

  const toast$ = (msg,t="ok") => { setToast({msg,t}); setTimeout(()=>setToast(null),2400); };

  const counts = {
    all:subs.length, pending:subs.filter(s=>s.status==="pending").length,
    approved:subs.filter(s=>s.status==="approved").length,
    rejected:subs.filter(s=>s.status==="rejected").length,
  };
  const show = subs.filter(s=> filter==="all" ? true : s.status===filter);
  const pendSubs = subs.filter(s=>s.status==="pending");
  const allSel   = pendSubs.length>0 && pendSubs.every(s=>selected.has(s.id));
  const toggle   = id => setSelected(p=>{const n=new Set(p);n.has(id)?n.delete(id):n.add(id);return n;});

  const doApprove = (sub,g) => { setSubs(p=>p.map(s=>s.id===sub.id?{...s,status:"approved",grade:g}:s)); setApproveT(null); toast$(`✓ Approved · ${g}/${sub.maxGrade}`); };
  const doReject  = (sub,r) => { setSubs(p=>p.map(s=>s.id===sub.id?{...s,status:"rejected",grade:0,rejectionReason:r}:s)); setRejectT(null); toast$("✗ Submission rejected","err"); };

  return (
    <div className={styles.section}>
      <AnimatePresence>
        {toast&&(
          <motion.div className={`${styles.toast} ${toast.t==="err"?styles.toastErr:""}`}
            initial={{opacity:0,y:-18}} animate={{opacity:1,y:0}} exit={{opacity:0}}
            transition={sp}>{toast.msg}</motion.div>
        )}
      </AnimatePresence>

      {/* Filter bar */}
      <div className={styles.filterBar}>
        <div className={styles.filters}>
          {[["all","All"],["pending","Pending"],["approved","Approved"],["rejected","Rejected"]].map(([k,l])=>(
            <button key={k}
              className={`${styles.pill} ${filter===k?styles.pillOn:""}`}
              style={filter===k?{color,borderColor:`${color}60`,background:`${color}12`}:{}}
              onClick={()=>setFilter(k)}>
              {l} <span className={styles.pillCount}>{counts[k]}</span>
            </button>
          ))}
        </div>
        {pendSubs.length>0&&(
          <button className={styles.selAll}
            onClick={()=>setSelected(allSel?new Set():new Set(pendSubs.map(s=>s.id)))}>
            {allSel?"Deselect All":"Select All Pending"}
          </button>
        )}
      </div>

      {/* Bulk bar */}
      <AnimatePresence>
        {selected.size>0&&(
          <motion.div className={styles.bulkBar}
            initial={{height:0,opacity:0}} animate={{height:"auto",opacity:1}}
            exit={{height:0,opacity:0}} transition={{duration:.2}}>
            <div className={styles.bulkInner}>
              <span className={styles.bulkSel}>{selected.size} selected</span>
              <button className={styles.bApprove} onClick={()=>{const s=subs.find(x=>selected.has(x.id)&&x.status==="pending");if(s)setApproveT(s);}}>✓ Approve Selected</button>
              <button className={styles.bReject}  onClick={()=>{const s=subs.find(x=>selected.has(x.id)&&x.status==="pending");if(s)setRejectT(s);}}>✗ Reject Selected</button>
              <button className={styles.bClear} onClick={()=>setSelected(new Set())}>Clear</button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* List */}
      <div className={styles.subList}>
        <AnimatePresence initial={false}>
          {show.map((s,i)=>(
            <motion.div key={s.id}
              className={`${styles.subCard} ${selected.has(s.id)?styles.subCardSel:""}`}
              initial={{opacity:0,y:12}} animate={{opacity:1,y:0}}
              exit={{opacity:0,x:-20}} transition={{delay:i*.04,...sp}}>

              {s.status==="pending"&&(
                <input type="checkbox" className={styles.cb}
                  checked={selected.has(s.id)} onChange={()=>toggle(s.id)}/>
              )}

              <Av name={s.studentName} color={color}/>

              <div className={styles.subInfo}>
                <div className={styles.subHead}>
                  <span className={styles.subName}>{s.studentName}</span>
                  <span className={styles.subStudId}>{s.studentId}</span>
                </div>
                <div className={styles.subTitle}>{s.assignment}</div>
                <div className={styles.subMeta}>
                  <span>🕐 {s.submittedAt}</span>
                  <span>📎 {s.file} · {s.fileSize}</span>
                </div>
                {s.status==="rejected"&&s.rejectionReason&&(
                  <span className={styles.rejectTag}>⛔ {s.rejectionReason}</span>
                )}
              </div>

              <div className={styles.subActions}>
                {s.status==="pending"&&(
                  <>
                    <span className={styles.sPending}>⏳ Pending</span>
                    <div className={styles.subBtns}>
                      <motion.button className={styles.btnApp} onClick={()=>setApproveT(s)}
                        whileHover={{scale:1.04}} whileTap={{scale:.95}}>✓ Approve</motion.button>
                      <motion.button className={styles.btnRej} onClick={()=>setRejectT(s)}
                        whileHover={{scale:1.04}} whileTap={{scale:.95}}>✗ Reject</motion.button>
                    </div>
                  </>
                )}
                {s.status==="approved"&&(
                  <>
                    <div className={styles.scoreDisplay}>
                      <span className={styles.scoreNum} style={{color}}>{s.grade}</span>
                      <span className={styles.scoreOf}>/5</span>
                    </div>
                    <span className={styles.sApproved}>✅ Approved</span>
                    <button className={styles.btnEdit} onClick={()=>setApproveT(s)}>✎ Edit</button>
                  </>
                )}
                {s.status==="rejected"&&(
                  <span className={styles.sRejected}>✗ Rejected</span>
                )}
              </div>
            </motion.div>
          ))}
        </AnimatePresence>
        {show.length===0&&(
          <div className={styles.empty}><span>📭</span><p>No submissions here</p></div>
        )}
      </div>

      <AnimatePresence>
        {approveT&&<ApproveModal sub={approveT} onConfirm={(g)=>doApprove(approveT,g)} onClose={()=>setApproveT(null)}/>}
        {rejectT &&<RejectModal  sub={rejectT}  onConfirm={(r)=>doReject(rejectT,r)}   onClose={()=>setRejectT(null)}/>}
      </AnimatePresence>
    </div>
  );
}

/* ════════════ EXAM GRADES SECTION ════════════ */
function ExamSection({ courseId, color, examType }) {
  const schedule = EXAM_SCHEDULE[courseId]?.[examType];
  const MAX = examType==="final" ? 60 : 40;

  const [students,  setStudents]  = useState(MOCK_STUDENTS[courseId]?.map(s=>({...s}))||[]);
  const [maxPts,    setMaxPts]    = useState(examType==="final" ? "60" : "20");
  const [maxSet,    setMaxSet]    = useState(examType==="final");
  const [query,     setQuery]     = useState("");
  const [draft,     setDraft]     = useState("");
  const [toast,     setToast]     = useState(null);

  const toast$ = (msg,t="ok") => { setToast({msg,t}); setTimeout(()=>setToast(null),2400); };

  const now      = new Date();
  const examDate = schedule?.date ? new Date(schedule.date) : null;
  const canGrade = examDate && now > new Date(examDate.getTime()+86400000);
  const key      = examType==="final" ? "final" : "midterm";
  const max      = Number(maxPts||MAX);

  const found    = query.trim() ? students.find(s=>s.id===query.trim()) : null;

  const save = () => {
    if(!found) return;
    const v = Number(draft);
    if(isNaN(v)||v<0||v>max) return;
    setStudents(p=>p.map(s=>s.id===found.id?{...s,[key]:v}:s));
    setDraft(""); toast$(`✓ Grade saved for ${found.name}`);
  };

  if(!schedule?.published) {
    return (
      <div className={styles.section}>
        <div className={styles.notYet}>
          <div className={styles.nyIcon}>📅</div>
          <h3>Schedule Not Published</h3>
          <p>The admin hasn't published the exam schedule yet. Check back later.</p>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.section}>
      <AnimatePresence>
        {toast&&(
          <motion.div className={`${styles.toast} ${toast.t==="err"?styles.toastErr:""}`}
            initial={{opacity:0,y:-18}} animate={{opacity:1,y:0}} exit={{opacity:0}}
            transition={sp}>{toast.msg}</motion.div>
        )}
      </AnimatePresence>

      {/* Exam info banner */}
      <div className={styles.examBanner} style={{borderColor:`${color}30`,background:`${color}07`}}>
        <div className={styles.examBannerL}>
          <div className={styles.examDate}>📅 Exam Date: <strong>{schedule.date}</strong></div>
          {canGrade
            ? <span className={styles.gradingOpen}>✅ Grading is open</span>
            : <span className={styles.gradingWait}>⏳ Opens 1 day after exam</span>
          }
        </div>
        {examType==="midterm" && !maxSet && (
          <div className={styles.setMax}>
            <span>Total points:</span>
            <input type="number" min={1} max={MAX} value={maxPts}
              onChange={e=>setMaxPts(e.target.value)}
              className={styles.maxIn}/>
            <span>/ 100</span>
            <button className={styles.maxBtn} style={{background:color}}
              onClick={()=>setMaxSet(true)}>Confirm</button>
          </div>
        )}
        {examType==="midterm" && maxSet && (
          <div className={styles.setMax}>
            <span className={styles.examDate}>📊 Exam out of <strong>{maxPts} pts</strong></span>
            <button className={styles.maxEdit} onClick={()=>setMaxSet(false)}>Edit</button>
          </div>
        )}
        {examType==="final" && (
          <span className={styles.examDate}>🏁 Fixed at <strong>60 points</strong></span>
        )}
      </div>

      {!canGrade ? (
        <div className={styles.notYet}>
          <div className={styles.nyIcon}>⏳</div>
          <h3>Grading Not Available Yet</h3>
          <p>You can record grades 1 day after the exam date ({schedule.date}).</p>
        </div>
      ) : (
        <>
          {/* Search */}
          <div className={styles.searchWrap}>
            <div className={styles.searchBox}>
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" className={styles.searchIco}>
                <circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/>
              </svg>
              <input className={styles.searchIn}
                placeholder={`Type student ID… e.g. ${(MOCK_STUDENTS[courseId]||[])[0]?.id||"20210001"}`}
                value={query} onChange={e=>{setQuery(e.target.value);setDraft("");}}/>
              {query&&<button className={styles.searchX} onClick={()=>{setQuery("");setDraft("");}}>✕</button>}
            </div>
          </div>

          {/* Result */}
          <AnimatePresence mode="wait">
            {!query.trim() && (
              <motion.div key="idle" className={styles.examIdle}
                initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
                <div className={styles.examIdleIcon} style={{background:`${color}14`,color}}>
                  {examType==="midterm"?"📝":"🏁"}
                </div>
                <p className={styles.examIdleT}>Enter a student ID to record their grade</p>
                <div className={styles.examHints}>
                  {(MOCK_STUDENTS[courseId]||[]).map(s=>(
                    <button key={s.id} className={styles.hint} onClick={()=>setQuery(s.id)}>
                      <Av name={s.name} color={color} size={28}/>
                      <span>{s.name}</span>
                      <span className={styles.hintId}>{s.id}</span>
                      {s[key]!==null&&s[key]!==undefined&&(
                        <span className={styles.hintGrade} style={{color}}>{s[key]}/{max}</span>
                      )}
                    </button>
                  ))}
                </div>
              </motion.div>
            )}

            {query.trim() && !found && (
              <motion.div key="nf" className={styles.notFound}
                initial={{opacity:0,scale:.96}} animate={{opacity:1,scale:1}} exit={{opacity:0}}>
                <span>🔍</span>
                <p>No student found with ID <strong>{query.trim()}</strong></p>
              </motion.div>
            )}

            {found && (
              <motion.div key={found.id} className={styles.stuCard}
                style={{borderColor:`${color}35`}}
                initial={{opacity:0,y:20,scale:.97}} animate={{opacity:1,y:0,scale:1}}
                exit={{opacity:0,y:-10}} transition={sp}>

                {/* Student header */}
                <div className={styles.stuHead} style={{background:`${color}0d`,borderBottom:`1px solid ${color}25`}}>
                  <Av name={found.name} color={color} size={58}/>
                  <div className={styles.stuInfo}>
                    <div className={styles.stuName}>{found.name}</div>
                    <div className={styles.stuChips}>
                      {[
                        ["🆔", found.id],
                        ["🎓", `Batch ${STUDENT_META[found.id]?.batch||"2021"}`],
                        ["📚", `Year ${STUDENT_META[found.id]?.year||4}`],
                        ["💻", STUDENT_META[found.id]?.dept||"CS"],
                      ].map(([ic,val])=>(
                        <span key={val} className={styles.chip}>{ic} {val}</span>
                      ))}
                    </div>
                  </div>

                  {/* Existing grade circle */}
                  {found[key]!==null&&found[key]!==undefined&&draft===""&&(()=>{
                    const p  = Math.round(found[key]/max*100);
                    const gc = p>=80?"#22c55e":p>=60?"#f59e0b":"#ef4444";
                    const dashArr = 2*Math.PI*40;
                    return (
                      <div className={styles.existCircle}>
                        <svg viewBox="0 0 100 100" className={styles.circSvg}>
                          <circle cx="50" cy="50" r="40" fill="none" stroke="var(--prog-track)" strokeWidth="8"/>
                          <motion.circle cx="50" cy="50" r="40" fill="none"
                            stroke={gc} strokeWidth="8" strokeLinecap="round"
                            strokeDasharray={dashArr}
                            initial={{strokeDashoffset:dashArr}}
                            animate={{strokeDashoffset:dashArr-dashArr*(p/100)}}
                            transition={{duration:1,ease:"easeOut",delay:.1}}
                            style={{transform:"rotate(-90deg)",transformOrigin:"50px 50px"}}/>
                        </svg>
                        <div className={styles.circInner}>
                          <span className={styles.circGrade} style={{color:gc}}>{found[key]}</span>
                          <span className={styles.circMax}>/{max}</span>
                        </div>
                        <div className={styles.circPct} style={{color:gc}}>{p}%</div>
                      </div>
                    );
                  })()}
                </div>

                {/* Grade entry */}
                <div className={styles.stuBody}>
                  <div className={styles.entryLabel}>
                    {examType==="midterm" ? "Midterm Grade" : "Final Exam Grade"}
                    <span className={styles.entryOf}>/ {max} pts</span>
                  </div>

                  {/* Current grade bar */}
                  {found[key]!==null&&found[key]!==undefined&&draft===""&&(()=>{
                    const p  = Math.round(found[key]/max*100);
                    const gc = p>=80?"#22c55e":p>=60?"#f59e0b":"#ef4444";
                    return (
                      <div className={styles.curGrade} style={{borderColor:`${gc}22`,background:`${gc}07`}}>
                        <span className={styles.curLabel}>Current:</span>
                        <span className={styles.curVal} style={{color:gc}}>{found[key]}</span>
                        <span className={styles.curOf}>/{max}</span>
                        <div className={styles.curBar}>
                          <motion.div style={{background:gc,height:"100%",borderRadius:99}}
                            initial={{width:0}} animate={{width:`${p}%`}} transition={{duration:.8}}/>
                        </div>
                        <span className={styles.curPct} style={{color:gc}}>{p}%</span>
                      </div>
                    );
                  })()}

                  <div className={styles.entryRow}>
                    <input type="number" min={0} max={max}
                      className={styles.gradeIn}
                      style={draft?{borderColor:`${color}70`,boxShadow:`0 0 0 4px ${color}18`}:{}}
                      placeholder={found[key]!==null&&found[key]!==undefined?String(found[key]):"0"}
                      value={draft}
                      onChange={e=>setDraft(e.target.value)}
                      onKeyDown={e=>e.key==="Enter"&&save()}/>
                    <span className={styles.entryOf} style={{fontSize:"1rem"}}>/ {max}</span>
                    <motion.button className={styles.saveBtn} style={{background:color}}
                      disabled={!draft||Number(draft)<0||Number(draft)>max}
                      style={{background:color, opacity:draft&&Number(draft)>=0&&Number(draft)<=max?1:.4}}
                      onClick={save}
                      whileHover={{scale:1.03,filter:"brightness(1.08)"}}
                      whileTap={{scale:.97}}>
                      {found[key]!==null&&found[key]!==undefined?"✎ Update Grade":"+ Save Grade"}
                    </motion.button>
                  </div>
                  {draft&&(Number(draft)<0||Number(draft)>max)&&(
                    <p className={styles.err}>Grade must be 0 – {max}</p>
                  )}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </>
      )}
    </div>
  );
}

/* ════════════ MAIN PAGE ════════════ */
export default function GradesMgmtPage() {
  const [course,    setCourse]    = useState(MY_COURSES[0].id);
  const [activeTab, setActiveTab] = useState(null);

  const c         = MY_COURSES.find(x=>x.id===course)||MY_COURSES[0];
  const pending   = (MOCK_SUBMISSIONS[course]||[]).filter(s=>s.status==="pending").length;
  const approved  = (MOCK_SUBMISSIONS[course]||[]).filter(s=>s.status==="approved").length;
  const rejected  = (MOCK_SUBMISSIONS[course]||[]).filter(s=>s.status==="rejected").length;

  const TABS = [
    {
      key:"assignments", icon:"📋",
      title:"Assignments", sub:"Review & grade submissions",
      color:c.color,
      stats:[
        {val:pending,  label:"Pending",  c:"#f59e0b"},
        {val:approved, label:"Approved", c:"#22c55e"},
        {val:rejected, label:"Rejected", c:"#ef4444"},
      ],
      available:true,
    },
    {
      key:"midterm", icon:"📝",
      title:"Midterm Grades", sub:"Record exam grades · max 40 pts",
      color:"#0ea5e9",
      stats: EXAM_SCHEDULE[course]?.midterm?.published
        ? [{val:EXAM_SCHEDULE[course].midterm.date, label:"Exam Date", c:"#0ea5e9"}]
        : [{val:"—", label:"Not Scheduled", c:"#94a3b8"}],
      available: EXAM_SCHEDULE[course]?.midterm?.published,
    },
    {
      key:"final", icon:"🏁",
      title:"Final Grades", sub:"Record exam grades · fixed 60 pts",
      color:"#818cf8",
      stats: EXAM_SCHEDULE[course]?.final?.published
        ? [{val:EXAM_SCHEDULE[course].final.date, label:"Exam Date", c:"#818cf8"}]
        : [{val:"—", label:"Not Scheduled", c:"#94a3b8"}],
      available: EXAM_SCHEDULE[course]?.final?.published,
    },
  ];

  return (
    <div className={styles.page}>

      {/* ════ HERO HEADER ════ */}
      <motion.div className={styles.hero}
        initial={{opacity:0,y:-18}} animate={{opacity:1,y:0}}
        transition={{duration:.45, ease:[.22,1,.36,1]}}>

        {/* Gradient background */}
        <div className={styles.heroBg}
          style={{background:`radial-gradient(ellipse 60% 100% at 10% 50%, ${c.color}18 0%, transparent 70%), radial-gradient(ellipse 40% 80% at 90% 20%, ${c.color}10 0%, transparent 70%)`}}/>

        <div className={styles.heroContent}>
          <div>
            <h1 className={styles.heroTitle}>Grade Management</h1>
            <p className={styles.heroSub}>Manage assignments & exam grades for your courses</p>
          </div>

          {/* Course selector */}
          <div className={styles.courseRow}>
            {MY_COURSES.map((cr,i)=>(
              <motion.button key={cr.id}
                className={`${styles.coursePill} ${course===cr.id?styles.coursePillOn:""}`}
                style={{"--cp":cr.color}}
                onClick={()=>{setCourse(cr.id);setActiveTab(null);}}
                initial={{opacity:0,x:16}} animate={{opacity:1,x:0}}
                transition={{delay:i*.06, type:"spring", stiffness:400, damping:26}}
                whileHover={{y:-2}} whileTap={{scale:.96}}>
                <span className={styles.cpIcon}>{cr.icon}</span>
                <div className={styles.cpText}>
                  <span className={styles.cpCode} style={{color:cr.color}}>{cr.code}</span>
                  <span className={styles.cpName}>{cr.name}</span>
                </div>
                {(MOCK_SUBMISSIONS[cr.id]||[]).filter(s=>s.status==="pending").length>0&&(
                  <span className={styles.cpBadge} style={{background:cr.color}}>
                    {(MOCK_SUBMISSIONS[cr.id]||[]).filter(s=>s.status==="pending").length}
                  </span>
                )}
              </motion.button>
            ))}
          </div>
        </div>
      </motion.div>

      {/* ════ SECTION TABS ════ */}
      <div className={styles.tabRow}>
        {TABS.map((t,i)=>(
          <motion.button key={t.key}
            className={`${styles.tab} ${activeTab===t.key?styles.tabOn:""} ${!t.available&&t.key!=="assignments"?styles.tabLocked:""}`}
            style={{"--tc":t.color}}
            onClick={()=>t.available&&setActiveTab(p=>p===t.key?null:t.key)}
            initial={{opacity:0,y:20}} animate={{opacity:1,y:0}}
            transition={{delay:.1+i*.06, type:"spring", stiffness:380, damping:28}}
            whileHover={t.available?{y:-4}:{}}
            whileTap={t.available?{scale:.98}:{}}>

            {/* Active glow border */}
            <motion.div className={styles.tabGlow}
              style={{background:`linear-gradient(135deg,${t.color}40,${t.color}00)`}}
              animate={{opacity: activeTab===t.key ? 1 : 0}}
              transition={{duration:.22}}/>

            {/* Top accent */}
            <motion.div className={styles.tabAccent} style={{background:t.color}}
              animate={{scaleX: activeTab===t.key ? 1 : 0}}
              transition={{duration:.28, ease:[.22,1,.36,1]}}/>

            <div className={styles.tabInner}>
              <div className={styles.tabIcon}
                style={{background:`${t.color}18`, border:`2px solid ${t.color}28`, color:t.color}}>
                {t.icon}
              </div>
              <div className={styles.tabMid}>
                <div className={styles.tabTitle}
                  style={activeTab===t.key?{color:t.color}:{}}>{t.title}</div>
                <div className={styles.tabSub}>{t.sub}</div>
              </div>
              <div className={styles.tabStats}>
                {t.stats.map((s,si)=>(
                  <div key={si} className={styles.tabStat}>
                    <span className={styles.tabStatVal} style={{color:s.c}}>{s.val}</span>
                    <span className={styles.tabStatLabel}>{s.label}</span>
                  </div>
                ))}
              </div>
            </div>

            {!t.available&&t.key!=="assignments"&&(
              <div className={styles.lockedOverlay}>
                <span>🔒</span><span>Not Scheduled</span>
              </div>
            )}
          </motion.button>
        ))}
      </div>

      {/* ════ CONTENT PANEL ════ */}
      <AnimatePresence mode="wait">
        {!activeTab && (
          <motion.div key="idle" className={styles.idleWrap}
            initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}
            transition={{duration:.18}}>
            <div className={styles.idleIcon}
              style={{background:`${c.color}14`,color:c.color}}>{c.icon}</div>
            <h3 className={styles.idleTitle}>Select a section above</h3>
            <p className={styles.idleSub}>Click Assignments, Midterm, or Final to start grading</p>
          </motion.div>
        )}

        {activeTab && (
          <motion.div key={`${course}-${activeTab}`}
            className={styles.panel}
            initial={{opacity:0,y:18}} animate={{opacity:1,y:0}}
            exit={{opacity:0,y:-12}} transition={{duration:.24, ease:[.22,1,.36,1]}}>

            {/* Panel header */}
            <div className={styles.panelHead}
              style={{borderTop:`3px solid ${TABS.find(t=>t.key===activeTab)?.color||c.color}`}}>
              <div className={styles.panelHeadL}>
                <span className={styles.panelIco}>
                  {TABS.find(t=>t.key===activeTab)?.icon}
                </span>
                <div>
                  <div className={styles.panelTitle}>
                    {activeTab==="assignments"?"Manage Assignments":activeTab==="midterm"?"Manage Midterm Grades":"Manage Final Grades"}
                  </div>
                  <div className={styles.panelSub}>{c.code} · {c.name}</div>
                </div>
              </div>
              <button className={styles.panelClose}
                onClick={()=>setActiveTab(null)}>✕ Close</button>
            </div>

            {/* Panel content */}
            {activeTab==="assignments" && (
              <AssignmentsSection key={course} courseId={course} color={c.color}/>
            )}
            {(activeTab==="midterm"||activeTab==="final") && (
              <ExamSection
                key={`${course}-${activeTab}`}
                courseId={course}
                color={activeTab==="midterm"?"#0ea5e9":"#818cf8"}
                examType={activeTab}/>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
