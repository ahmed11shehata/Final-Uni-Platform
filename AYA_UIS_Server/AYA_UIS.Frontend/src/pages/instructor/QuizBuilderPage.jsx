// src/pages/instructor/QuizBuilderPage.jsx
import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./QuizBuilderPage.module.css";

const MY_COURSES = [
  { id:"cs401", code:"CS401", name:"Artificial Intelligence", color:"#f59e0b", icon:"🤖" },
  { id:"cs404", code:"CS404", name:"Expert Systems",          color:"#e05c8a", icon:"🧠" },
];
const LETTERS = ["A","B","C","D","E","F"];
const sp = { type:"spring", stiffness:400, damping:28 };

const makeQ = () => ({
  id: Date.now() + Math.random(),
  text: "",
  answers: [{text:""},{text:""},{text:""},{text:""}],
  correct: 0,
});

/* ── Answer row ── */
function ARow({ a, idx, correct, onText, onCorrect, onRemove, canRemove, color }) {
  return (
    <div className={styles.answerRow}
      style={correct?{borderColor:`${color}55`,background:`${color}0b`}:{}}>
      <div className={styles.answerLetter}
        style={correct?{background:color,borderColor:color,color:"#fff"}:{}}>
        {LETTERS[idx]}
      </div>
      <input className={styles.answerInp}
        placeholder={`Answer ${LETTERS[idx]}…`}
        value={a.text} onChange={e=>onText(e.target.value)}/>
      <button className={`${styles.correctBtn} ${correct?styles.correctBtnOn:""}`}
        style={correct?{background:color,borderColor:color}:{}}
        onClick={onCorrect}>
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
          <polyline points="20 6 9 17 4 12"/>
        </svg>
        {correct ? "✓ Correct" : "Mark correct"}
      </button>
      {canRemove && <button className={styles.removeAnsBtn} onClick={onRemove}>✕</button>}
    </div>
  );
}

/* ── Question card ── */
function QCard({ q, idx, color, onChange, onDelete, canDelete }) {
  return (
    <motion.div className={styles.qCard} layout
      initial={{opacity:0,y:18}} animate={{opacity:1,y:0}}
      exit={{opacity:0,x:-20,scale:.97}} transition={sp}
      style={{"--fc":color}}>

      <div className={styles.qCardHead} style={{borderLeft:`4px solid ${color}`}}>
        <div className={styles.qBadge} style={{background:`${color}20`,color}}>Q{idx+1}</div>
        <div className={styles.qHeadBody}>
          <label className={styles.qCardLabel}>Question {idx+1}</label>
          <textarea className={styles.qTextarea}
            placeholder="Type your question here…"
            value={q.text} rows={2}
            onChange={e=>onChange({...q,text:e.target.value})}/>
        </div>
        {canDelete&&(
          <button className={styles.qDeleteBtn} onClick={onDelete}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              <polyline points="3 6 5 6 21 6"/>
              <path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/>
            </svg>
          </button>
        )}
      </div>

      <div className={styles.answers}>
        <div className={styles.answersTopRow}>
          <span>Choose the correct answer →</span>
          <span>{q.answers.length}/6 options</span>
        </div>
        {q.answers.map((a,ai)=>(
          <ARow key={ai} a={a} idx={ai} correct={q.correct===ai} color={color}
            onText={val=>onChange({...q,answers:q.answers.map((x,i)=>i===ai?{...x,text:val}:x)})}
            onCorrect={()=>onChange({...q,correct:ai})}
            onRemove={()=>{
              const ans=q.answers.filter((_,i)=>i!==ai);
              onChange({...q,answers:ans,correct:q.correct>=ans.length?ans.length-1:q.correct});
            }}
            canRemove={q.answers.length>2}/>
        ))}
        {q.answers.length<6&&(
          <button className={styles.addAnsBtn} style={{color,borderColor:`${color}40`}}
            onClick={()=>onChange({...q,answers:[...q.answers,{text:""}]})}>
            + Add option
          </button>
        )}
      </div>
    </motion.div>
  );
}

/* ── Step 1: Settings ── */
function StepSettings({ data, onChange, color }) {
  const isDurCustom = data.duration && ![10,15,20,30,45,60,90].includes(data.duration);
  const showPreview = data.title && data.startDate && data.startTime && data.duration;

  return (
    <div className={styles.infoGrid}>

      {/* LEFT COL */}

      {/* Title */}
      <div className={styles.settingCard}>
        <div className={styles.settingCardHead}>
          <div className={styles.settingCardIcon} style={{background:`${color}18`,color}}>📋</div>
          <div>
            <div className={styles.settingCardTitle}>Quiz Title</div>
            <div className={styles.settingCardSub}>Name of this quiz</div>
          </div>
        </div>
        <div className={styles.settingCardBody}>
          <div className={styles.field}>
            <label className={styles.label}>Title <span className={styles.req}>*</span></label>
            <input className={styles.inp} style={{"--fc":color}}
              placeholder="e.g. Quiz 3 — ML Fundamentals"
              value={data.title} onChange={e=>onChange({...data,title:e.target.value})}/>
          </div>
        </div>
      </div>

      {/* Duration */}
      <div className={styles.settingCard}>
        <div className={styles.settingCardHead}>
          <div className={styles.settingCardIcon} style={{background:"rgba(239,68,68,.14)",color:"#ef4444"}}>⏱</div>
          <div>
            <div className={styles.settingCardTitle}>Duration</div>
            <div className={styles.settingCardSub}>Timer starts when student clicks Start</div>
          </div>
        </div>
        <div className={styles.settingCardBody}>
          <div className={styles.durGrid}>
            {[10,15,20,30,45,60,90].map(m=>(
              <button key={m}
                className={`${styles.durPill} ${data.duration===m&&!isDurCustom?styles.durPillOn:""}`}
                style={data.duration===m&&!isDurCustom?{borderColor:color,background:`${color}14`,color}:{}}
                onClick={()=>onChange({...data,duration:m})}>
                {m}m
              </button>
            ))}
            <div className={styles.durCustomWrap}>
              <input type="number" min={1} max={180} className={styles.durCustomInp}
                style={isDurCustom?{borderColor:`${color}60`}:{}}
                placeholder="Custom"
                value={isDurCustom?data.duration:""}
                onChange={e=>e.target.value&&onChange({...data,duration:Number(e.target.value)})}/>
              <span className={styles.durUnit}>min</span>
            </div>
          </div>
        </div>
      </div>

      {/* Schedule */}
      <div className={styles.settingCard}>
        <div className={styles.settingCardHead}>
          <div className={styles.settingCardIcon} style={{background:"rgba(14,165,233,.15)",color:"#0ea5e9"}}>📅</div>
          <div>
            <div className={styles.settingCardTitle}>Schedule</div>
            <div className={styles.settingCardSub}>When the quiz opens and closes</div>
          </div>
        </div>
        <div className={styles.settingCardBody}>
          <div className={styles.twoCol}>
            <div className={styles.field}>
              <label className={styles.label}>Start Date <span className={styles.req}>*</span></label>
              <input type="date" className={styles.inp} style={{"--fc":color}}
                value={data.startDate}
                onChange={e=>onChange({...data,startDate:e.target.value})}
                min={new Date().toISOString().split("T")[0]}/>
            </div>
            <div className={styles.field}>
              <label className={styles.label}>Start Time <span className={styles.req}>*</span></label>
              <input type="time" className={styles.inp} style={{"--fc":color}}
                value={data.startTime} onChange={e=>onChange({...data,startTime:e.target.value})}/>
            </div>
            <div className={styles.field}>
              <label className={styles.label}>Deadline Date <span className={styles.req}>*</span></label>
              <p className={styles.hint}>No new starts after this date</p>
              <input type="date" className={styles.inp} style={{"--fc":color}}
                value={data.deadlineDate}
                onChange={e=>onChange({...data,deadlineDate:e.target.value})}
                min={data.startDate||new Date().toISOString().split("T")[0]}/>
            </div>
            <div className={styles.field}>
              <label className={styles.label}>Deadline Time <span className={styles.req}>*</span></label>
              <p className={styles.hint}>Quiz locks at this time</p>
              <input type="time" className={styles.inp} style={{"--fc":color}}
                value={data.deadlineTime} onChange={e=>onChange({...data,deadlineTime:e.target.value})}/>
            </div>
          </div>
        </div>
      </div>

      {/* Options */}
      <div className={styles.settingCard}>
        <div className={styles.settingCardHead}>
          <div className={styles.settingCardIcon} style={{background:"rgba(129,140,248,.15)",color:"#818cf8"}}>⚙️</div>
          <div>
            <div className={styles.settingCardTitle}>Options</div>
            <div className={styles.settingCardSub}>Grading and randomization</div>
          </div>
        </div>
        <div className={styles.settingCardBody}>
          <div className={styles.field}>
            <label className={styles.label}>Points per question</label>
            <input type="number" min={1} max={10} className={styles.inp} style={{"--fc":color}}
              value={data.gradePerQ||""} placeholder="1"
              onChange={e=>onChange({...data,gradePerQ:Number(e.target.value)})}/>
          </div>
          <div className={styles.field} style={{marginTop:14}}>
            <label className={styles.label}>Shuffle</label>
            <div className={styles.toggleRow}>
              <button className={`${styles.toggle} ${data.shuffleQ?styles.toggleOn:""}`}
                style={data.shuffleQ?{borderColor:color,background:`${color}12`,color}:{}}
                onClick={()=>onChange({...data,shuffleQ:!data.shuffleQ})}>
                <span className={styles.toggleDot} style={data.shuffleQ?{background:color}:{}}/>
                Shuffle Questions
              </button>
              <button className={`${styles.toggle} ${data.shuffleA?styles.toggleOn:""}`}
                style={data.shuffleA?{borderColor:color,background:`${color}12`,color}:{}}
                onClick={()=>onChange({...data,shuffleA:!data.shuffleA})}>
                <span className={styles.toggleDot} style={data.shuffleA?{background:color}:{}}/>
                Shuffle Answers
              </button>
            </div>
            {/* Shuffle explanation */}
            <div className={styles.shuffleExplain}>
              <strong>Shuffle Questions</strong> — each student gets questions in a different random order.<br/>
              <strong>Shuffle Answers</strong> — the A/B/C/D options are randomized per student, preventing copying.
            </div>
          </div>
        </div>
      </div>

      {/* Preview — spans full width */}
      <AnimatePresence>
        {showPreview && (
          <motion.div className={`${styles.settingCard} ${styles.infoGridFull}`}
            style={{borderColor:`${color}30`}}
            initial={{opacity:0,y:8}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
            <div className={styles.settingCardHead}>
              <div className={styles.settingCardIcon} style={{background:`${color}18`,color}}>✅</div>
              <div>
                <div className={styles.settingCardTitle}>Quiz Summary</div>
                <div className={styles.settingCardSub}>All good — proceed to add questions</div>
              </div>
            </div>
            <div className={styles.settingCardBody}>
              <div className={styles.previewRow}>
                {[
                  {l:"Title",        v:data.title},
                  {l:"Opens",        v:`${data.startDate} · ${data.startTime}`},
                  {l:"Closes",       v:`${data.deadlineDate||"—"} · ${data.deadlineTime||"—"}`},
                  {l:"Duration",     v:`${data.duration} minutes`},
                  {l:"Pts/Question", v:data.gradePerQ||1},
                  {l:"Shuffle",      v:[data.shuffleQ?"Questions":"",data.shuffleA?"Answers":""].filter(Boolean).join(", ")||"Off"},
                ].map(x=>(
                  <div key={x.l} className={styles.previewItem}>
                    <span className={styles.previewLabel}>{x.l}</span>
                    <span className={styles.previewVal}>{x.v}</span>
                  </div>
                ))}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

/* ── Step 2: Questions ── */
function StepQuestions({ questions, setQuestions, color }) {
  const addQ = () => setQuestions(p=>[...p, makeQ()]);
  const filled = questions.filter(q=>q.text.trim()&&q.answers.some(a=>a.text.trim())).length;
  const pct    = questions.length>0 ? Math.round(filled/questions.length*100) : 0;

  /* bulk add N questions */
  const bulkAdd = (target) => {
    const current = questions.length;
    if(target <= current) return;
    const toAdd = Array.from({length: target-current}, makeQ);
    setQuestions(p=>[...p,...toAdd]);
  };

  return (
    <div className={styles.qLayout}>

      {/* LEFT: questions */}
      <div className={styles.qList}>
        <div className={styles.qListHeader}>
          <div className={styles.qCountInfo}>
            <div className={styles.qCountNum}>{questions.length} Question{questions.length!==1?"s":""}</div>
            <div className={styles.qCountSub}>{filled} filled · mark the correct answer for each</div>
          </div>
          <motion.button className={styles.addQBtn}
            style={{background:`linear-gradient(135deg,${color}cc,${color})`}}
            onClick={addQ} whileHover={{scale:1.03}} whileTap={{scale:.97}}>
            + Add Question
          </motion.button>
        </div>

        {questions.length===0&&(
          <div className={styles.qEmpty}>
            <div className={styles.qEmptyIcon}>❓</div>
            <p className={styles.qEmptyTitle}>No questions yet</p>
            <p className={styles.qEmptyHint}>Use the sidebar to set how many questions you need, or click below</p>
            <motion.button className={styles.addQBtnBig}
              style={{background:`linear-gradient(135deg,${color}cc,${color})`}}
              onClick={addQ} whileHover={{scale:1.02}} whileTap={{scale:.97}}>
              + Add First Question
            </motion.button>
          </div>
        )}

        <AnimatePresence>
          {questions.map((q,i)=>(
            <QCard key={q.id} q={q} idx={i} color={color}
              onChange={nq=>setQuestions(p=>p.map(x=>x.id===q.id?nq:x))}
              onDelete={()=>setQuestions(p=>p.filter(x=>x.id!==q.id))}
              canDelete={questions.length>1}/>
          ))}
        </AnimatePresence>

        {questions.length>0&&(
          <motion.button className={styles.addMoreBtn}
            style={{color,borderColor:`${color}40`}}
            onClick={addQ} initial={{opacity:0}} animate={{opacity:1}}
            whileHover={{scale:1.01}} whileTap={{scale:.98}}>
            + Add Another Question
          </motion.button>
        )}
      </div>

      {/* RIGHT: sidebar */}
      <div className={styles.qSidebar}>

        {/* Question count control */}
        <div className={styles.qSideCard}>
          <div className={styles.qSideHead}>
            🔢 Number of Questions
          </div>
          <div className={styles.qSideBody}>
            <div className={styles.field}>
              <label className={styles.label}>Target count</label>
              <div className={styles.qCountInput}>
                <button className={styles.qCountMinus}
                  onClick={()=>questions.length>1&&setQuestions(p=>p.slice(0,-1))}>−</button>
                <input type="number" min={1} max={50} className={styles.qCountNum2}
                  value={questions.length}
                  onChange={e=>{
                    const n=Number(e.target.value);
                    if(n<1||n>50) return;
                    if(n>questions.length) bulkAdd(n);
                    else if(n<questions.length) setQuestions(p=>p.slice(0,n));
                  }}/>
                <button className={styles.qCountPlus}
                  onClick={()=>questions.length<50&&addQ()}>+</button>
              </div>
              <p className={styles.qCountHint}>Change to add/remove questions in bulk (max 50)</p>
            </div>
          </div>
        </div>

        {/* Progress */}
        <div className={styles.qSideCard}>
          <div className={styles.qSideHead}>📊 Progress</div>
          <div className={styles.qSideBody}>
            <div className={styles.qStatRow}>
              <span className={styles.qStatLabel}>Questions added</span>
              <span className={styles.qStatVal} style={{color}}>{questions.length}</span>
            </div>
            <div className={styles.qStatRow}>
              <span className={styles.qStatLabel}>Filled in</span>
              <span className={styles.qStatVal} style={{color:"#22c55e"}}>{filled}</span>
            </div>
            <div className={styles.qStatRow}>
              <span className={styles.qStatLabel}>Correct marked</span>
              <span className={styles.qStatVal} style={{color:"#818cf8"}}>
                {questions.filter(q=>q.answers[q.correct]?.text.trim()).length}
              </span>
            </div>
            <div className={styles.qFillBar}>
              <div className={styles.qFillFill} style={{width:`${pct}%`,background:pct===100?"#22c55e":color}}/>
            </div>
            <span className={styles.qCountHint}>{pct}% complete</span>
          </div>
        </div>

        {/* Tips */}
        <div className={styles.qSideCard}>
          <div className={styles.qSideHead}>💡 Tips</div>
          <div className={styles.qSideBody}>
            {[
              "Each question needs at least 2 options",
              "Mark one answer as correct per question",
              "Add up to 6 options per question",
              "Use + Add option to add more choices",
            ].map((t,i)=>(
              <div key={i} style={{fontSize:".74rem",color:"var(--text-muted)",display:"flex",gap:8,lineHeight:1.5}}>
                <span style={{color,flexShrink:0,fontWeight:900}}>·</span>{t}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ══ MAIN ══ */
export default function QuizBuilderPage() {
  const [courseId, setCourseId] = useState(MY_COURSES[0].id);
  const [step,     setStep]     = useState(1);
  const [saving,   setSaving]   = useState(false);
  const [done,     setDone]     = useState(false);
  const [info,     setInfo]     = useState({
    title:"", startDate:"", startTime:"", deadlineDate:"", deadlineTime:"",
    duration:20, gradePerQ:1, shuffleQ:false, shuffleA:false,
  });
  const [questions, setQuestions] = useState([makeQ()]);

  const c       = MY_COURSES.find(x=>x.id===courseId)||MY_COURSES[0];
  const infoOk  = info.title&&info.startDate&&info.startTime&&info.deadlineDate&&info.deadlineTime&&info.duration;
  const questOk = questions.length>0&&questions.every(q=>q.text.trim()&&q.answers.some(a=>a.text.trim()));

  const publish = () => {
    if(!infoOk||!questOk) return;
    setSaving(true);
    setTimeout(()=>{ setSaving(false); setDone(true); }, 1600);
  };

  const reset = () => {
    setDone(false); setStep(1);
    setInfo({title:"",startDate:"",startTime:"",deadlineDate:"",deadlineTime:"",duration:20,gradePerQ:1,shuffleQ:false,shuffleA:false});
    setQuestions([makeQ()]);
  };

  if(done) return (
    <div className={styles.page}>
      <motion.div className={styles.successWrap}
        initial={{opacity:0,scale:.92}} animate={{opacity:1,scale:1}} transition={sp}>
        <div className={styles.successIcon} style={{background:`${c.color}18`,color:c.color}}>🎉</div>
        <h2 className={styles.successTitle}>Quiz Published!</h2>
        <p className={styles.successSub}>
          <strong>{info.title}</strong> is live for <strong>{c.code}</strong>.<br/>
          Opens <strong>{info.startDate}</strong> at <strong>{info.startTime}</strong> · Lasts <strong>{info.duration} min</strong>.
        </p>
        <div className={styles.successStats}>
          {[
            {v:questions.length,             l:"Questions"},
            {v:info.duration+"m",            l:"Duration"},
            {v:questions.length*(info.gradePerQ||1), l:"Total Pts"},
          ].map(s=>(
            <div key={s.l} style={{textAlign:"center"}}>
              <span className={styles.sstatV} style={{color:c.color}}>{s.v}</span>
              <span className={styles.sstatL}>{s.l}</span>
            </div>
          ))}
        </div>
        <button className={styles.successBtn} style={{background:c.color}} onClick={reset}>
          Create Another Quiz
        </button>
      </motion.div>
    </div>
  );

  return (
    <div className={styles.page}>

      {/* Header */}
      <div className={styles.header}>
        <div className={styles.headerBg}/>
        <div className={styles.headerContent}>
          <div>
            <h1 className={styles.pageTitle}>Quiz Builder</h1>
            <p className={styles.pageSub}>Build and publish quizzes for your students</p>
          </div>
          <div className={styles.courseRow}>
            {MY_COURSES.map((cr,i)=>(
              <motion.button key={cr.id}
                className={`${styles.cPill} ${courseId===cr.id?styles.cPillOn:""}`}
                style={{"--cp":cr.color}}
                onClick={()=>setCourseId(cr.id)}
                initial={{opacity:0,x:12}} animate={{opacity:1,x:0}}
                transition={{delay:i*.06,...sp}}
                whileHover={{y:-2}} whileTap={{scale:.96}}>
                <span>{cr.icon}</span>
                <div>
                  <span className={styles.cpCode} style={{color:cr.color}}>{cr.code}</span>
                  <span className={styles.cpName}>{cr.name}</span>
                </div>
              </motion.button>
            ))}
          </div>
        </div>
      </div>

      {/* Step tabs */}
      <div className={styles.stepTabs}>
        {[
          {n:1,icon:"📋",title:"Quiz Settings",desc:"Title, schedule & duration"},
          {n:2,icon:"❓",title:"Questions",     desc:`${questions.length} question${questions.length!==1?"s":""}`},
        ].map(s=>{
          const on   = step===s.n;
          const done = step>s.n;
          return (
            <button key={s.n}
              className={`${styles.stepTab} ${on?styles.stepTabOn:""}`}
              style={{"--st":c.color}}
              onClick={()=>done&&setStep(s.n)}>
              {done
                ? <div className={styles.stepTabCheck}>✓</div>
                : <div className={`${styles.stepTabNum} ${on?styles.stepTabNumOn:""}`}>{s.n}</div>
              }
              <div className={styles.stepTabLabel}>
                <span className={styles.stepTabTitle}>{s.icon} {s.title}</span>
                <span className={styles.stepTabDesc}>{s.desc}</span>
              </div>
            </button>
          );
        })}
      </div>

      {/* Body */}
      <div className={styles.body}>
        <AnimatePresence mode="wait">
          {step===1&&(
            <motion.div key="s1"
              initial={{opacity:0,x:20}} animate={{opacity:1,x:0}} exit={{opacity:0,x:-16}} transition={{duration:.22}}>
              <StepSettings data={info} onChange={setInfo} color={c.color}/>
            </motion.div>
          )}
          {step===2&&(
            <motion.div key="s2"
              initial={{opacity:0,x:20}} animate={{opacity:1,x:0}} exit={{opacity:0,x:-16}} transition={{duration:.22}}>
              <StepQuestions questions={questions} setQuestions={setQuestions} color={c.color}/>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Footer */}
      <div className={styles.footer}>
        <div className={styles.footerInfo}>
          {step===1 ? "Step 1 of 2 — Fill quiz settings then proceed to questions"
                    : `Step 2 of 2 — ${questions.length} questions · ${questions.filter(q=>q.text.trim()).length} filled`}
        </div>
        <div className={styles.footerBtns}>
          {step===2&&<button className={styles.footerBack} onClick={()=>setStep(1)}>← Back</button>}
          {step===1&&(
            <motion.button className={styles.footerNext}
              style={{background:`linear-gradient(135deg,${c.color}cc,${c.color})`,opacity:infoOk?1:.4}}
              disabled={!infoOk} onClick={()=>setStep(2)}
              whileHover={infoOk?{scale:1.02}:{}} whileTap={infoOk?{scale:.97}:{}}>
              Next: Add Questions →
            </motion.button>
          )}
          {step===2&&(
            <motion.button className={styles.footerPublish}
              style={{background:`linear-gradient(135deg,${c.color}cc,${c.color})`,opacity:(infoOk&&questOk)?1:.4}}
              disabled={!infoOk||!questOk||saving} onClick={publish}
              whileHover={(infoOk&&questOk)?{scale:1.02}:{}} whileTap={(infoOk&&questOk)?{scale:.97}:{}}>
              {saving
                ? <><motion.span animate={{rotate:360}} transition={{duration:.7,repeat:Infinity,ease:"linear"}}>⟳</motion.span> Publishing…</>
                : <>✅ Publish Quiz ({questions.length}Q)</>
              }
            </motion.button>
          )}
        </div>
      </div>
    </div>
  );
}
