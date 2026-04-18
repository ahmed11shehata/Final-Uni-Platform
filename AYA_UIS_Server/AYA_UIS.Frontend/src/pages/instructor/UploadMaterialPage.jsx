// src/pages/instructor/UploadMaterialPage.jsx
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./UploadMaterialPage.module.css";

const MY_COURSES = [
  { id:"cs401", code:"CS401", name:"Artificial Intelligence", color:"#f59e0b", icon:"🤖" },
  { id:"cs404", code:"CS404", name:"Expert Systems",          color:"#e05c8a", icon:"🧠" },
];

const WEEKS = Array.from({length:16},(_,i)=>`Week ${i+1}`);

const sp = { type:"spring", stiffness:400, damping:28 };

/* ── Drag-drop file zone ── */
function FileZone({ accept, onFile, file, color }) {
  const ref = useRef();
  const [drag, setDrag] = useState(false);

  const handle = (f) => { if(f) onFile(f); };
  return (
    <div
      className={`${styles.fileZone} ${drag?styles.fileZoneDrag:""} ${file?styles.fileZoneHas:""}`}
      style={file||drag?{borderColor:color,background:`${color}08`}:{}}
      onDragOver={e=>{e.preventDefault();setDrag(true);}}
      onDragLeave={()=>setDrag(false)}
      onDrop={e=>{e.preventDefault();setDrag(false);handle(e.dataTransfer.files[0]);}}
      onClick={()=>ref.current?.click()}>
      <input ref={ref} type="file" accept={accept} style={{display:"none"}}
        onChange={e=>handle(e.target.files[0])}/>
      {file ? (
        <div className={styles.fileHas}>
          <span className={styles.fileHasIco}>📎</span>
          <div>
            <div className={styles.fileHasName}>{file.name}</div>
            <div className={styles.fileHasSize}>{(file.size/1024/1024).toFixed(2)} MB</div>
          </div>
          <button className={styles.fileRemove} onClick={e=>{e.stopPropagation();onFile(null);}}>✕</button>
        </div>
      ) : (
        <div className={styles.fileEmpty}>
          <div className={styles.fileEmptyIcon} style={{color}}>☁️</div>
          <div className={styles.fileEmptyText}>Drop file here or click to browse</div>
          <div className={styles.fileEmptyAccept}>{accept}</div>
        </div>
      )}
    </div>
  );
}

/* ── Lecture Form ── */
function LectureForm({ courseId, color }) {
  const [title,    setTitle]    = useState("");
  const [week,     setWeek]     = useState("");
  const [desc,     setDesc]     = useState("");
  const [file,     setFile]     = useState(null);
  const [relDate,  setRelDate]  = useState("");
  const [relNow,   setRelNow]   = useState(true);
  const [done,     setDone]     = useState(false);
  const [loading,  setLoading]  = useState(false);

  const valid = title.trim() && week && file && (relNow || relDate);

  const submit = () => {
    if(!valid) return;
    setLoading(true);
    setTimeout(()=>{ setLoading(false); setDone(true); }, 1500);
  };

  if(done) return (
    <motion.div className={styles.successWrap}
      initial={{opacity:0,scale:.9}} animate={{opacity:1,scale:1}} transition={sp}>
      <div className={styles.successIcon} style={{background:`${color}18`,color}}>🎬</div>
      <h3 className={styles.successTitle}>Lecture Uploaded!</h3>
      <p className={styles.successSub}><strong>{title}</strong> is {relNow?"now visible":"scheduled for "+relDate} to students.</p>
      <button className={styles.successReset} style={{background:color}} onClick={()=>{setDone(false);setTitle("");setWeek("");setDesc("");setFile(null);setRelDate("");setRelNow(true);}}>
        Upload Another
      </button>
    </motion.div>
  );

  return (
    <div className={styles.form}>
      {/* Title */}
      <div className={styles.field}>
        <label className={styles.label}>Lecture Title <span className={styles.req}>*</span></label>
        <input className={styles.inp} style={title?{borderColor:`${color}60`}:{}}
          placeholder="e.g. Deep Learning & CNNs"
          value={title} onChange={e=>setTitle(e.target.value)}/>
      </div>

      {/* Week + Course row */}
      <div className={styles.twoCol}>
        <div className={styles.field}>
          <label className={styles.label}>Week <span className={styles.req}>*</span></label>
          <select className={styles.sel} style={week?{borderColor:`${color}60`}:{}}
            value={week} onChange={e=>setWeek(e.target.value)}>
            <option value="">Select week…</option>
            {WEEKS.map(w=><option key={w} value={w}>{w}</option>)}
          </select>
        </div>
        <div className={styles.field}>
          <label className={styles.label}>Type</label>
          <select className={styles.sel}>
            <option>Video Lecture</option>
            <option>PDF Slides</option>
            <option>Lab Session</option>
          </select>
        </div>
      </div>

      {/* Description */}
      <div className={styles.field}>
        <label className={styles.label}>Description <span className={styles.opt}>(optional)</span></label>
        <textarea className={styles.textarea} rows={3}
          placeholder="Brief overview of what students will learn…"
          value={desc} onChange={e=>setDesc(e.target.value)}/>
      </div>

      {/* File upload */}
      <div className={styles.field}>
        <label className={styles.label}>File <span className={styles.req}>*</span></label>
        <FileZone accept=".mp4,.pdf,.pptx,.zip" onFile={setFile} file={file} color={color}/>
      </div>

      {/* Release schedule */}
      <div className={styles.releaseWrap}>
        <div className={styles.releaseHead}>
          <span className={styles.releaseTitle}>📅 Release Schedule</span>
          <span className={styles.releaseSub}>When should students see this lecture?</span>
        </div>
        <div className={styles.releaseOpts}>
          <button
            className={`${styles.releaseBtn} ${relNow?styles.releaseBtnOn:""}`}
            style={relNow?{background:color,borderColor:color}:{}}
            onClick={()=>setRelNow(true)}>
            ⚡ Publish Immediately
          </button>
          <button
            className={`${styles.releaseBtn} ${!relNow?styles.releaseBtnOn:""}`}
            style={!relNow?{background:color,borderColor:color}:{}}
            onClick={()=>setRelNow(false)}>
            📅 Schedule for Later
          </button>
        </div>
        <AnimatePresence>
          {!relNow&&(
            <motion.div className={styles.dateWrap}
              initial={{height:0,opacity:0}} animate={{height:"auto",opacity:1}}
              exit={{height:0,opacity:0}} transition={{duration:.22}}>
              <input type="date" className={styles.dateInput}
                style={relDate?{borderColor:`${color}60`}:{}}
                value={relDate} onChange={e=>setRelDate(e.target.value)}
                min={new Date().toISOString().split("T")[0]}/>
              <p className={styles.dateHint}>Students won't see this until the selected date</p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Submit */}
      <motion.button className={styles.submitBtn}
        style={{background:`linear-gradient(135deg,${color}cc,${color})`, opacity:valid?1:.45}}
        disabled={!valid||loading}
        onClick={submit}
        whileHover={valid?{scale:1.02,filter:"brightness(1.08)"}:{}}
        whileTap={valid?{scale:.97}:{}}>
        {loading ? (
          <motion.span animate={{rotate:360}} transition={{duration:.8,repeat:Infinity,ease:"linear"}}>⟳</motion.span>
        ) : "🎬 Upload Lecture"}
      </motion.button>
    </div>
  );
}

/* ── Assignment Form ── */
function AssignmentForm({ courseId, color }) {
  const [title,    setTitle]    = useState("");
  const [desc,     setDesc]     = useState("");
  const [deadline, setDeadline] = useState("");
  const [releaseDate,setReleaseDate]=useState("");
  const [relNow,   setRelNow]   = useState(true);
  const [maxPts,   setMaxPts]   = useState("20");
  const [file,     setFile]     = useState(null);
  const [allowFmt, setAllowFmt] = useState(["pdf"]);
  const [done,     setDone]     = useState(false);
  const [loading,  setLoading]  = useState(false);

  const FMTS = ["pdf","zip","docx","py","cpp","java","mp4"];
  const toggleFmt = f => setAllowFmt(p=>p.includes(f)?p.filter(x=>x!==f):[...p,f]);
  const valid = title.trim() && deadline && allowFmt.length > 0 && (relNow || releaseDate);

  const submit = () => {
    if(!valid) return;
    setLoading(true);
    setTimeout(()=>{ setLoading(false); setDone(true); }, 1500);
  };

  if(done) return (
    <motion.div className={styles.successWrap}
      initial={{opacity:0,scale:.9}} animate={{opacity:1,scale:1}} transition={sp}>
      <div className={styles.successIcon} style={{background:`${color}18`,color}}>📋</div>
      <h3 className={styles.successTitle}>Assignment Created!</h3>
      <p className={styles.successSub}><strong>{title}</strong> will be {relNow?"visible now":"released on "+releaseDate}. Deadline: {deadline}.</p>
      <button className={styles.successReset} style={{background:color}} onClick={()=>{setDone(false);setTitle("");setDesc("");setDeadline("");setReleaseDate("");setRelNow(true);setMaxPts("20");setFile(null);setAllowFmt(["pdf"]);}}>
        Create Another
      </button>
    </motion.div>
  );

  return (
    <div className={styles.form}>
      {/* Title */}
      <div className={styles.field}>
        <label className={styles.label}>Assignment Title <span className={styles.req}>*</span></label>
        <input className={styles.inp} style={title?{borderColor:`${color}60`}:{}}
          placeholder="e.g. Neural Network from Scratch"
          value={title} onChange={e=>setTitle(e.target.value)}/>
      </div>

      {/* Description */}
      <div className={styles.field}>
        <label className={styles.label}>Instructions <span className={styles.opt}>(optional)</span></label>
        <textarea className={styles.textarea} rows={3}
          placeholder="Describe what students need to do…"
          value={desc} onChange={e=>setDesc(e.target.value)}/>
      </div>

      {/* Deadline + Max pts */}
      <div className={styles.twoCol}>
        <div className={styles.field}>
          <label className={styles.label}>Deadline <span className={styles.req}>*</span></label>
          <input type="date" className={styles.inp} style={deadline?{borderColor:`${color}60`}:{}}
            value={deadline} onChange={e=>setDeadline(e.target.value)}
            min={new Date().toISOString().split("T")[0]}/>
        </div>
        <div className={styles.field}>
          <label className={styles.label}>Max Grade (1–5)</label>
          <div className={styles.starRow}>
            {[1,2,3,4,5].map(n=>(
              <button key={n}
                className={`${styles.starBtn} ${Number(maxPts)===n?styles.starBtnOn:""}`}
                style={Number(maxPts)===n?{background:color,borderColor:color}:{}}
                onClick={()=>setMaxPts(String(n))}>
                {n}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Allowed formats */}
      <div className={styles.field}>
        <label className={styles.label}>Allowed File Formats <span className={styles.req}>*</span></label>
        <div className={styles.fmtRow}>
          {FMTS.map(f=>(
            <button key={f}
              className={`${styles.fmtBtn} ${allowFmt.includes(f)?styles.fmtBtnOn:""}`}
              style={allowFmt.includes(f)?{background:color,borderColor:color}:{}}
              onClick={()=>toggleFmt(f)}>
              .{f}
            </button>
          ))}
        </div>
      </div>

      {/* Attachment (optional) */}
      <div className={styles.field}>
        <label className={styles.label}>Attachment <span className={styles.opt}>(starter file, optional)</span></label>
        <FileZone accept=".pdf,.zip,.docx" onFile={setFile} file={file} color={color}/>
      </div>

      {/* Release schedule */}
      <div className={styles.releaseWrap}>
        <div className={styles.releaseHead}>
          <span className={styles.releaseTitle}>📅 Release to Students</span>
          <span className={styles.releaseSub}>When should this assignment be visible?</span>
        </div>
        <div className={styles.releaseOpts}>
          <button
            className={`${styles.releaseBtn} ${relNow?styles.releaseBtnOn:""}`}
            style={relNow?{background:color,borderColor:color}:{}}
            onClick={()=>setRelNow(true)}>
            ⚡ Publish Now
          </button>
          <button
            className={`${styles.releaseBtn} ${!relNow?styles.releaseBtnOn:""}`}
            style={!relNow?{background:color,borderColor:color}:{}}
            onClick={()=>setRelNow(false)}>
            📅 Schedule Release
          </button>
        </div>
        <AnimatePresence>
          {!relNow&&(
            <motion.div className={styles.dateWrap}
              initial={{height:0,opacity:0}} animate={{height:"auto",opacity:1}}
              exit={{height:0,opacity:0}} transition={{duration:.22}}>
              <input type="date" className={styles.dateInput}
                style={releaseDate?{borderColor:`${color}60`}:{}}
                value={releaseDate} onChange={e=>setReleaseDate(e.target.value)}
                min={new Date().toISOString().split("T")[0]}/>
              <p className={styles.dateHint}>Students won't see this assignment until this date</p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Submit */}
      <motion.button className={styles.submitBtn}
        style={{background:`linear-gradient(135deg,${color}cc,${color})`, opacity:valid?1:.45}}
        disabled={!valid||loading}
        onClick={submit}
        whileHover={valid?{scale:1.02,filter:"brightness(1.08)"}:{}}
        whileTap={valid?{scale:.97}:{}}>
        {loading ? (
          <motion.span animate={{rotate:360}} transition={{duration:.8,repeat:Infinity,ease:"linear"}}>⟳</motion.span>
        ) : "📋 Create Assignment"}
      </motion.button>
    </div>
  );
}

/* ════════════ MAIN PAGE ════════════ */
export default function UploadMaterialPage() {
  const [course,  setCourse]  = useState(MY_COURSES[0].id);
  const [matType, setMatType] = useState(null); // null | "lecture" | "assignment"

  const c = MY_COURSES.find(x=>x.id===course)||MY_COURSES[0];

  return (
    <div className={styles.page}>

      {/* ── Hero header ── */}
      <motion.div className={styles.hero}
        initial={{opacity:0,y:-18}} animate={{opacity:1,y:0}}
        transition={{duration:.45,ease:[.22,1,.36,1]}}>
        <div className={styles.heroBg}/>
        <div className={styles.heroContent}>
          <div>
            <h1 className={styles.heroTitle}>Upload Material</h1>
            <p className={styles.heroSub}>Add lectures and assignments for your students</p>
          </div>
          {/* Course selector */}
          <div className={styles.courseRow}>
            {MY_COURSES.map((cr,i)=>(
              <motion.button key={cr.id}
                className={`${styles.coursePill} ${course===cr.id?styles.coursePillOn:""}`}
                style={{"--cp":cr.color}}
                onClick={()=>{setCourse(cr.id);setMatType(null);}}
                initial={{opacity:0,x:16}} animate={{opacity:1,x:0}}
                transition={{delay:i*.07,type:"spring",stiffness:400,damping:26}}
                whileHover={{y:-2}} whileTap={{scale:.96}}>
                <span>{cr.icon}</span>
                <div>
                  <span className={styles.cpCode}>{cr.code}</span>
                  <span className={styles.cpName}>{cr.name}</span>
                </div>
              </motion.button>
            ))}
          </div>
        </div>
      </motion.div>

      {/* ── Type selector ── */}
      <div className={styles.typeRow}>
        {[
          {
            key:"lecture",
            icon:"🎬",
            title:"Upload Lecture",
            sub:"Video, PDF slides, or lab materials",
            bullet:["📁 Supports MP4, PDF, PPTX, ZIP","📅 Schedule release date","📚 Assign to a specific week"],
            color:"#6366f1",
          },
          {
            key:"assignment",
            icon:"📋",
            title:"Create Assignment",
            sub:"Homework, projects & submissions",
            bullet:["📅 Set deadline & release date","📎 Attach starter files","🎯 Grade out of 1–5"],
            color:c.color,
          },
        ].map((t,i)=>(
          <motion.button key={t.key}
            className={`${styles.typeCard} ${matType===t.key?styles.typeCardOn:""}`}
            style={{"--tc":t.color}}
            onClick={()=>setMatType(p=>p===t.key?null:t.key)}
            initial={{opacity:0,y:22}} animate={{opacity:1,y:0}}
            transition={{delay:i*.08,type:"spring",stiffness:380,damping:28}}
            whileHover={matType!==t.key?{y:-4,boxShadow:`0 16px 40px ${t.color}22`}:{}}
            whileTap={{scale:.98}}>

            <motion.div className={styles.typeAccent} style={{background:t.color}}
              animate={{scaleX:matType===t.key?1:0}} transition={{duration:.28,ease:[.22,1,.36,1]}}/>

            <div className={styles.typeTop}>
              <div className={styles.typeIcon}
                style={{background:`${t.color}18`, border:`2px solid ${t.color}28`, color:t.color}}>
                {t.icon}
              </div>
              <div className={styles.typeGlow}
                style={{background:`radial-gradient(circle at 50% 50%, ${t.color}15, transparent 70%)`}}
                hidden={matType!==t.key}/>
              <div className={styles.typeChev}
                style={matType===t.key?{color:t.color}:{}}>
                {matType===t.key?"▼":"▶"}
              </div>
            </div>
            <div className={styles.typeTitle}
              style={matType===t.key?{color:t.color}:{}}>{t.title}</div>
            <div className={styles.typeSub}>{t.sub}</div>
            <ul className={styles.typeBullets}>
              {t.bullet.map(b=><li key={b}>{b}</li>)}
            </ul>
          </motion.button>
        ))}
      </div>

      {/* ── Form panel ── */}
      <AnimatePresence mode="wait">
        {matType && (
          <motion.div key={`${course}-${matType}`} className={styles.formPanel}
            style={{"--fp": matType==="lecture"?"#6366f1":c.color}}
            initial={{opacity:0,y:24}} animate={{opacity:1,y:0}}
            exit={{opacity:0,y:-16}} transition={{duration:.28,ease:[.22,1,.36,1]}}>

            <div className={styles.formPanelHead}
              style={{borderTop:`3px solid ${matType==="lecture"?"#6366f1":c.color}`}}>
              <span className={styles.formPanelIcon}>{matType==="lecture"?"🎬":"📋"}</span>
              <div>
                <div className={styles.formPanelTitle}>
                  {matType==="lecture"?"New Lecture":"New Assignment"}
                </div>
                <div className={styles.formPanelSub}>{c.icon} {c.code} · {c.name}</div>
              </div>
              <button className={styles.formClose}
                onClick={()=>setMatType(null)}>✕ Cancel</button>
            </div>

            {matType==="lecture"&&(
              <LectureForm key={`${course}-lec`} courseId={course} color="#6366f1"/>
            )}
            {matType==="assignment"&&(
              <AssignmentForm key={`${course}-asn`} courseId={course} color={c.color}/>
            )}
          </motion.div>
        )}
      </AnimatePresence>

      {/* Idle */}
      {!matType&&(
        <motion.div className={styles.idle}
          initial={{opacity:0}} animate={{opacity:1}}>
          <div className={styles.idleIcon} style={{background:`${c.color}14`,color:c.color}}>{c.icon}</div>
          <p className={styles.idleTitle}>Select a material type above</p>
          <p className={styles.idleSub}>Choose Lecture or Assignment to start uploading content for <strong>{c.code}</strong></p>
        </motion.div>
      )}
    </div>
  );
}