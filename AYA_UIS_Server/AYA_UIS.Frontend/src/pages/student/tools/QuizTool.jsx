// QuizTool — Full game arena: setup grid + immersive quiz
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./QuizTool.module.css";
const C="#e8a838",G="linear-gradient(135deg,#e8a838,#c07818)";
const OPTS=["A","B","C","D"];

export default function QuizTool(){
  const nav=useNavigate();
  const [file,setFile]=useState(null),[text,setText]=useState(""),[wc,setWC]=useState(0);
  const [ext,setExt]=useState(false),[lang,setLang]=useState("en"),[numQ,setNumQ]=useState(10);
  const [load,setLoad]=useState(false),[qs,setQs]=useState(null),[err,setErr]=useState("");
  const [phase,setPhase]=useState("setup");
  const [idx,setIdx]=useState(0),[answers,setAnswers]=useState([]),[revealed,setRev]=useState(false);
  const ref=useRef();
  const abortRef=useRef(null);
  const cancel=()=>{if(abortRef.current){abortRef.current.abort();abortRef.current=null;}};

  const pick=async f=>{if(!f)return;cancel();setFile(f);setErr("");setQs(null);setExt(true);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.extractText(f,ctrl);setText(d.text);setWC(d.word_count);}
    catch(e){if(e.name!=="AbortError"){setErr(e.message);setFile(null);}}finally{setExt(false);abortRef.current=null;}};
  const gen=async()=>{cancel();setLoad(true);setErr("");setQs(null);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.generateQuiz(text,numQ,lang,ctrl);setQs(d.questions);setAnswers(new Array(d.questions.length).fill(null));}
    catch(e){if(e.name!=="AbortError")setErr(e.message);}finally{setLoad(false);abortRef.current=null;}};
  const start=()=>{setIdx(0);setAnswers(new Array(qs.length).fill(null));setPhase("quiz");setRev(false);};
  const choose=i=>{if(revealed)return;const n=[...answers];n[idx]=i;setAnswers(n);};
  const next=()=>{setRev(false);if(idx<qs.length-1)setIdx(idx+1);else setPhase("results");};
  const score=qs?answers.reduce((s,a,i)=>s+(a===qs[i].correct_answer?1:0),0):0;
  const pct=qs?Math.round((score/qs.length)*100):0;
  const grade=pct>=90?"A+":pct>=80?"A":pct>=70?"B":pct>=60?"C":"D";
  const prog=phase==="quiz"?((idx+1)/qs.length)*100:100;

  return(
    <div className={s.page}>
      <div className={s.bar}>
        <motion.button className={s.back} onClick={()=>nav("/student/ai-tools")} whileHover={{x:-3}}>← Back</motion.button>
        <div className={s.barC}>
          <span className={s.barIco}>✏️</span>
          <div><p className={s.barN}>Quiz Generator</p><p className={s.barS}>Interactive MCQ quiz from your documents</p></div>
        </div>
        {qs&&phase!=="setup"&&(
          <div className={s.barProg}>
            <span className={s.barProgTxt}>{phase==="quiz"?`${idx+1} / ${qs.length}`:`${score} / ${qs.length} correct`}</span>
            <div className={s.barProgTrack}><motion.div className={s.barProgFill} style={{background:G}} animate={{width:`${prog}%`}} transition={{duration:.4}}/></div>
          </div>
        )}
      </div>

      <AnimatePresence mode="wait">
        {/* SETUP */}
        {phase==="setup"&&(
          <motion.div key="setup" className={s.setup} initial={{opacity:0,y:20}} animate={{opacity:1,y:0}} exit={{opacity:0,y:-20}}>
            <div className={s.grid}>
              <motion.div className={s.card} initial={{opacity:0,y:24}} animate={{opacity:1,y:0}} transition={{delay:.05}}>
                <div className={s.cardStripe} style={{background:G}}/>
                <div className={s.cardBody}>
                  <h3 className={s.cardH}>📁 Upload Document</h3>
                  {!file?(
                    <div className={s.dz}
                      onDragOver={e=>e.preventDefault()} onDrop={e=>{e.preventDefault();pick(e.dataTransfer.files[0]);}}
                      onClick={()=>ref.current?.click()}>
                      <motion.span animate={{y:[0,-6,0]}} transition={{repeat:Infinity,duration:2.4}}>📄</motion.span>
                      <p>Drop here or click</p>
                      <div className={s.pills}>{["PDF","DOCX","PPTX","TXT"].map(t=><span key={t} className={s.pill}>{t}</span>)}</div>
                      <input ref={ref} type="file" accept={ACCEPT_STRING} style={{display:"none"}} onChange={e=>pick(e.target.files[0])}/>
                    </div>
                  ):(
                    <motion.div className={s.filebox} initial={{scale:.92}} animate={{scale:1}}>
                      <span>📎</span>
                      <div><p className={s.fn}>{file.name}</p><p className={s.fm}>{wc.toLocaleString()} words</p></div>
                      <button onClick={()=>{setFile(null);setText("");setQs(null);}}>✕</button>
                    </motion.div>
                  )}
                  {ext&&<div className={s.stRow}><span className={s.spin} style={{"--c":C}}/>Extracting...</div>}
                  {err&&<div className={s.eRow}>⚠️ {err}</div>}
                </div>
              </motion.div>

              <motion.div className={s.card} initial={{opacity:0,y:24}} animate={{opacity:1,y:0}} transition={{delay:.1}}>
                <div className={s.cardStripe} style={{background:G}}/>
                <div className={s.cardBody}>
                  <h3 className={s.cardH}>⚙️ Quiz Settings</h3>
                  <div className={s.setting}><p className={s.setLabel}>Questions</p>
                    <div className={s.numRow}>
                      {[5,10,15,20].map(n=>(
                        <motion.button key={n} className={`${s.nBtn} ${numQ===n?s.nBtnA:""}`}
                          style={numQ===n?{background:G,color:"white",border:"none"}:{}} onClick={()=>setNumQ(n)} whileHover={{scale:1.08}} whileTap={{scale:.92}}>{n}</motion.button>
                      ))}
                      <input type="number" className={s.nInp} min={3} max={30} value={numQ}
                        onChange={e=>setNumQ(Math.min(30,Math.max(3,+e.target.value)))}/>
                    </div>
                  </div>
                  <div className={s.setting}><p className={s.setLabel}>Language</p>
                    <div className={s.lRow}>
                      {[{k:"en",l:"🇬🇧 English"},{k:"ar",l:"🇸🇦 العربية"}].map(({k,l})=>(
                        <motion.button key={k} className={`${s.lBtn} ${lang===k?s.lBtnA:""}`}
                          style={lang===k?{background:G,color:"white",border:"none"}:{}} onClick={()=>setLang(k)} whileHover={{scale:1.03}} whileTap={{scale:.96}}>{l}</motion.button>
                      ))}
                    </div>
                  </div>
                </div>
              </motion.div>
            </div>

            {!qs?(
              text&&(
                <motion.button className={s.bigBtn} style={{background:G}} disabled={load} onClick={gen}
                  whileHover={{scale:1.03,boxShadow:`0 16px 50px ${C}44`}} whileTap={{scale:.97}}>
                  {load?<><span className={s.spin} style={{"--c":"white"}}/>Generating... <span style={{fontSize:11,opacity:.7,marginLeft:4}} onClick={e=>{e.stopPropagation();cancel();setLoad(false);}}>(cancel)</span></>:`✨ Generate ${numQ} Questions`}
                </motion.button>
              )
            ):(
              <motion.div className={s.readyBanner} initial={{opacity:0,scale:.96}} animate={{opacity:1,scale:1}}>
                <div className={s.readyLeft}>
                  <motion.span className={s.readyNum} style={{color:C}} initial={{scale:0}} animate={{scale:1}} transition={{type:"spring",stiffness:300,damping:18}}>{qs.length}</motion.span>
                  <div><p className={s.readyT}>Questions Ready</p><p className={s.readyS}>{lang==="en"?"English":"Arabic"} · MCQ</p></div>
                </div>
                <div style={{display:"flex",gap:10}}>
                  <motion.button className={s.bigBtn} style={{background:G}} onClick={start} whileHover={{scale:1.04,boxShadow:`0 14px 44px ${C}44`}}>🚀 Start Quiz</motion.button>
                  <motion.button className={s.outBtn} onClick={()=>setQs(null)} whileHover={{scale:1.04}}>🔄 Redo</motion.button>
                </div>
              </motion.div>
            )}
          </motion.div>
        )}

        {/* QUIZ */}
        {phase==="quiz"&&qs&&(
          <motion.div key={`q${idx}`} className={s.quizArea}
            initial={{opacity:0,x:40}} animate={{opacity:1,x:0}} exit={{opacity:0,x:-40}} transition={{duration:.28}}>
            <div className={s.qCard}>
              <div className={s.qCardHead}>
                <span className={s.qTag} style={{background:`${C}18`,color:C}}>Question {idx+1} of {qs.length}</span>
                <p className={s.qText}>{qs[idx].question}</p>
              </div>
              <div className={s.qCardBody}>
                <div className={s.opts}>
                  {(qs[idx].options||[]).map((opt,oi)=>{
                    const picked=answers[idx]===oi,corr=qs[idx].correct_answer,show=revealed;
                    return(
                      <motion.button key={oi}
                        className={`${s.opt} ${picked&&!show?s.optPicked:""} ${show&&oi===corr?s.optRight:""} ${show&&picked&&oi!==corr?s.optWrong:""}`}
                        style={picked&&!show?{borderColor:C,background:`${C}12`}:{}}
                        onClick={()=>choose(oi)} whileHover={!revealed?{x:5}:{}} whileTap={!revealed?{scale:.98}:{}}>
                        <div className={s.optL} style={picked&&!show?{background:C,color:"white",border:"none"}:{}}>{OPTS[oi]}</div>
                        <span className={s.optT}>{opt}</span>
                        {show&&oi===corr&&<motion.span initial={{scale:0}} animate={{scale:1}} className={s.ck}>✓</motion.span>}
                        {show&&picked&&oi!==corr&&<motion.span initial={{scale:0}} animate={{scale:1}} className={s.cx}>✗</motion.span>}
                      </motion.button>
                    );
                  })}
                </div>
                <AnimatePresence>
                  {revealed&&qs[idx].explanation&&(
                    <motion.div className={s.expl} initial={{opacity:0,y:8,height:0}} animate={{opacity:1,y:0,height:"auto"}} exit={{opacity:0,height:0}}>
                      💡 {qs[idx].explanation}
                    </motion.div>
                  )}
                </AnimatePresence>
                <div className={s.nav}>
                  {!revealed&&answers[idx]!==null&&(
                    <motion.button className={s.revBtn} onClick={()=>setRev(true)} whileHover={{scale:1.03}}>👁 Reveal Answer</motion.button>
                  )}
                  {revealed&&(
                    <motion.button className={s.nextBtn} style={{background:G}} onClick={next}
                      whileHover={{scale:1.04,boxShadow:`0 8px 28px ${C}44`}} whileTap={{scale:.97}}
                      initial={{opacity:0,x:12}} animate={{opacity:1,x:0}}>
                      {idx<qs.length-1?"Next →":"Finish ✓"}
                    </motion.button>
                  )}
                </div>
              </div>
            </div>
            <div className={s.dots}>
              {qs.map((_,i)=>(
                <motion.button key={i} className={s.dot}
                  style={{background:i===idx?C:answers[i]!==null?`${C}55`:"var(--card-border)"}}
                  onClick={()=>{setIdx(i);setRev(false);}} whileHover={{scale:1.5}}/>
              ))}
            </div>
          </motion.div>
        )}

        {/* RESULTS */}
        {phase==="results"&&qs&&(
          <motion.div key="results" className={s.results} initial={{opacity:0,scale:.94}} animate={{opacity:1,scale:1}}>
            <motion.div className={s.scoreRing} initial={{scale:0}} animate={{scale:1}} transition={{type:"spring",stiffness:200,damping:22}}>
              <svg width="170" height="170" viewBox="0 0 170 170" style={{transform:"rotate(-90deg)"}}>
                <circle cx="85" cy="85" r="72" fill="none" stroke="var(--prog-track,#f1f5f9)" strokeWidth="13"/>
                <motion.circle cx="85" cy="85" r="72" fill="none" stroke={C} strokeWidth="13" strokeLinecap="round"
                  strokeDasharray={2*Math.PI*72}
                  initial={{strokeDashoffset:2*Math.PI*72}}
                  animate={{strokeDashoffset:2*Math.PI*72*(1-pct/100)}}
                  transition={{delay:.3,duration:1.5,ease:[.22,1,.36,1]}}/>
              </svg>
              <div className={s.scoreInner}>
                <motion.span className={s.scoreNum} style={{color:C}} initial={{opacity:0}} animate={{opacity:1}} transition={{delay:.5}}>{score}</motion.span>
                <span className={s.scoreOf}>/{qs.length}</span>
              </div>
            </motion.div>
            <motion.div className={s.gradeChip} style={{background:`${C}18`,borderColor:`${C}30`,color:C}}
              initial={{opacity:0,y:10}} animate={{opacity:1,y:0}} transition={{delay:.4}}>Grade {grade} · {pct}%</motion.div>
            <motion.p className={s.resMsg} initial={{opacity:0}} animate={{opacity:1}} transition={{delay:.5}}>
              {pct>=90?"🏆 Outstanding!":pct>=70?"🌟 Great work!":pct>=50?"👍 Keep it up!":"📚 Practice more!"}
            </motion.p>
            <motion.div className={s.resBtns} initial={{opacity:0,y:10}} animate={{opacity:1,y:0}} transition={{delay:.6}}>
              <motion.button className={s.bigBtn} style={{background:G}} onClick={start} whileHover={{scale:1.04}}>🔄 Try Again</motion.button>
              <motion.button className={s.outBtn} onClick={()=>{setPhase("setup");setQs(null);}} whileHover={{scale:1.04}}>📄 New Quiz</motion.button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}