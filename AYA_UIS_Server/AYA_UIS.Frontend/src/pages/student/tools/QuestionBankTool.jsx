// QuestionBankTool — Exam paper aesthetic: upload top · exam sheet result below
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./QuestionBankTool.module.css";
const C="#e05c8a",G="linear-gradient(135deg,#e05c8a,#be185d)";

function ExamSheet({ text }){
  const lines=text.split("\n");
  let qNum=0;
  const sections=[];let cur=null;
  for(const line of lines){
    const t=line.trim();
    if(!t)continue;
    if(/^##/.test(t)){if(cur)sections.push(cur);cur={title:t.replace(/^#+\s*/,""),items:[]};}
    else if(cur){cur.items.push(t);}
    else{if(!cur)cur={title:null,items:[]};cur.items.push(t);}
  }
  if(cur)sections.push(cur);
  return(
    <div className={s.examSheet}>
      {sections.map((sec,si)=>(
        <motion.div key={si} className={s.section}
          initial={{opacity:0,y:14}} animate={{opacity:1,y:0}} transition={{delay:si*.08,duration:.35}}>
          {sec.title&&<div className={s.secHead}><div className={s.secBar} style={{background:G}}/><h2 className={s.secTitle}>{sec.title}</h2></div>}
          {sec.items.map((item,ii)=>{
            const isNum=/^\d+[\.\)]/.test(item);
            const isTF=/true|false/i.test(item)&&item.length<80;
            if(isNum){qNum++;return(
              <div key={ii} className={s.qItem}>
                <span className={s.qNum} style={{borderColor:`${C}30`,color:C}}>{qNum}</span>
                <p className={s.qTxt}>{item.replace(/^\d+[\.\)]\s*/,"")}</p>
              </div>
            );}
            if(item.startsWith("- ")||item.startsWith("• ")){return(
              <div key={ii} className={s.choiceRow}>
                <span className={s.choiceBubble}/>
                <span className={s.choiceTxt}>{item.replace(/^[-•]\s*/,"")}</span>
              </div>
            );}
            return <p key={ii} className={s.plainLine}>{item}</p>;
          })}
        </motion.div>
      ))}
    </div>
  );
}

export default function QuestionBankTool(){
  const nav=useNavigate();
  const[file,setFile]=useState(null),[text,setText]=useState(""),[wc,setWC]=useState(0);
  const[ext,setExt]=useState(false),[lang,setLang]=useState("en"),[numQ,setNumQ]=useState(15);
  const[load,setLoad]=useState(false),[bank,setBank]=useState(null),[err,setErr]=useState("");
  const[copied,setCopied]=useState(false);
  const ref=useRef();
  const abortRef=useRef(null);
  const cancel=()=>{if(abortRef.current){abortRef.current.abort();abortRef.current=null;}};

  const pick=async f=>{if(!f)return;cancel();setFile(f);setErr("");setBank(null);setExt(true);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.extractText(f,ctrl);setText(d.text);setWC(d.word_count);}
    catch(e){if(e.name!=="AbortError"){setErr(e.message);setFile(null);}}finally{setExt(false);abortRef.current=null;}};
  const gen=async()=>{cancel();setLoad(true);setErr("");setBank(null);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.generateQuestionBank(text,numQ,lang,ctrl);setBank(d.question_bank);}
    catch(e){if(e.name!=="AbortError")setErr(e.message);}finally{setLoad(false);abortRef.current=null;}};
  const copy=()=>{navigator.clipboard.writeText(bank||"");setCopied(true);setTimeout(()=>setCopied(false),2000);};

  return(
    <div className={s.page}>
      <div className={s.bar}>
        <motion.button className={s.back} onClick={()=>nav("/student/ai-tools")} whileHover={{x:-3}}>← Back</motion.button>
        <div className={s.barC}>
          <span className={s.barIco}>📚</span>
          <div><p className={s.barN}>Question Bank</p><p className={s.barS}>T/F · MCQ · Short answer exam preparation</p></div>
        </div>
      </div>

      <div className={s.content}>
        {/* Config strip */}
        <motion.div className={s.configStrip} initial={{opacity:0,y:-16}} animate={{opacity:1,y:0}} transition={{duration:.35}}>
          <div className={s.uploadZone}
            onDragOver={e=>e.preventDefault()} onDrop={e=>{e.preventDefault();pick(e.dataTransfer.files[0]);}}
            onClick={()=>ref.current?.click()}>
            {!file?(
              <>
                <motion.span animate={{y:[0,-4,0]}} transition={{repeat:Infinity,duration:2.4}}>📄</motion.span>
                <div><p className={s.uzT}>Drop document</p><p className={s.uzS}>or click to browse</p></div>
              </>
            ):(
              <motion.div className={s.fileChip} initial={{scale:.9}} animate={{scale:1}}>
                <span>📎</span>
                <div><p className={s.fcN}>{file.name}</p><p className={s.fcM}>{wc.toLocaleString()} words</p></div>
                <button className={s.fcX} onClick={e=>{e.stopPropagation();setFile(null);setText("");setBank(null);}}>✕</button>
              </motion.div>
            )}
            <input ref={ref} type="file" accept={ACCEPT_STRING} style={{display:"none"}} onChange={e=>pick(e.target.files[0])}/>
          </div>

          <div className={s.divider}/>

          <div className={s.configRow}>
            <div className={s.cfgGrp}><span className={s.cfgL}>Questions</span>
              <div className={s.nRow}>
                {[10,15,20,30].map(n=>(
                  <motion.button key={n} className={`${s.nBtn} ${numQ===n?s.nBtnA:""}`}
                    style={numQ===n?{background:G,color:"white",border:"none"}:{}} onClick={()=>setNumQ(n)}
                    whileHover={{scale:1.06}} whileTap={{scale:.92}}>{n}</motion.button>
                ))}
                <input type="number" className={s.nInp} min={5} max={50} value={numQ}
                  onChange={e=>setNumQ(Math.min(50,Math.max(5,+e.target.value)))}/>
              </div>
            </div>
            <div className={s.cfgGrp}><span className={s.cfgL}>Language</span>
              <div className={s.lRow}>
                {[{k:"en",l:"🇬🇧 EN"},{k:"ar",l:"🇸🇦 AR"}].map(({k,l})=>(
                  <motion.button key={k} className={`${s.lBtn} ${lang===k?s.lBtnA:""}`}
                    style={lang===k?{background:G,color:"white",border:"none"}:{}} onClick={()=>setLang(k)}
                    whileHover={{scale:1.04}} whileTap={{scale:.95}}>{l}</motion.button>
                ))}
              </div>
            </div>
          </div>

          <motion.button className={s.genBtn} style={{background:G}} disabled={!text||load} onClick={gen}
            whileHover={{scale:1.03,boxShadow:`0 12px 36px ${C}44`}} whileTap={{scale:.97}}>
            {load?<><span className={s.spin} style={{"--c":"white"}}/>Building Bank...</>:"📚 Generate Question Bank"}
          </motion.button>

          <AnimatePresence>
            {ext&&<motion.div className={s.stRow} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}><span className={s.spin} style={{"--c":C}}/>Extracting text...</motion.div>}
            {text&&!ext&&<motion.div className={s.okRow} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}><span className={s.okDot}/>✅ {wc.toLocaleString()} words extracted</motion.div>}
            {err&&<motion.div className={s.errRow} initial={{opacity:0}} animate={{opacity:1}}>⚠️ {err}</motion.div>}
          </AnimatePresence>
        </motion.div>

        {/* Result: exam paper */}
        <AnimatePresence mode="wait">
          {!bank&&!load&&(
            <motion.div key="idle" className={s.idle} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
              <div className={s.idlePaper}>
                <div className={s.idlePaperHead}><div className={s.idlePaperLine} style={{width:"60%"}}/><div className={s.idlePaperLine} style={{width:"40%"}}/></div>
                {[100,85,70,90,75,60,88,65,80].map((w,i)=>(
                  <motion.div key={i} className={`${s.idlePaperLine} ${i%3===0?s.idleQ:""}`} style={{width:`${w}%`}}
                    animate={{opacity:[.2,.4,.2]}} transition={{repeat:Infinity,duration:2.2,delay:i*.15}}/>
                ))}
              </div>
              <p className={s.idleMsg}>Your exam question bank will appear here</p>
            </motion.div>
          )}
          {load&&(
            <motion.div key="load" className={s.loadBox} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
              <motion.div className={s.loadPaper} animate={{y:[0,-6,0]}} transition={{repeat:Infinity,duration:1.8}}>
                {[1,2,3].map(i=><div key={i} className={s.loadLine} style={{width:`${80-i*10}%`,background:`${C}${40-i*10}`,opacity:.6+i*.1}}/>)}
              </motion.div>
              <p className={s.loadTxt}>Generating question bank...</p>
            </motion.div>
          )}
          {bank&&(
            <motion.div key="result" className={s.resultPaper}
              initial={{opacity:0,y:24}} animate={{opacity:1,y:0}} transition={{duration:.45,ease:[.22,1,.36,1]}}>
              <div className={s.paperHeader}>
                <div>
                  <h2 className={s.paperTitle}>📋 Question Bank</h2>
                  <p className={s.paperFile}>{file?.name}</p>
                </div>
                <div style={{display:"flex",gap:8}}>
                  <motion.button className={`${s.actBtn} ${copied?s.actBtnG:""}`} onClick={copy} whileHover={{scale:1.05}} whileTap={{scale:.95}}>
                    {copied?"✅ Copied":"📋 Copy"}
                  </motion.button>
                  <motion.button className={s.actBtn} onClick={()=>setBank(null)} whileHover={{scale:1.05}}>🔄 Redo</motion.button>
                </div>
              </div>
              <ExamSheet text={bank}/>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}