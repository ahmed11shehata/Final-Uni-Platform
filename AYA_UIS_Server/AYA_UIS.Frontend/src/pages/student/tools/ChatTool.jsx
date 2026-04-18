// ChatTool — Full messenger: floating doc sidebar · messages fill center
import { useState, useRef, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./ChatTool.module.css";
const C="#3d8fe0",G="linear-gradient(135deg,#3d8fe0,#1d6ab0)";
const SUGGESTED=["What are the main concepts?","Summarize this document","What are the key points?","Explain the most important topic","Create 3 exam questions"];

export default function ChatTool(){
  const nav=useNavigate();
  const[file,setFile]=useState(null),[docText,setDoc]=useState(""),[wc,setWC]=useState(0);
  const[ext,setExt]=useState(false),[lang,setLang]=useState("en");
  const[msgs,setMsgs]=useState([]),[input,setInput]=useState(""),[streaming,setStream]=useState(false);
  const[err,setErr]=useState(""),[sideOpen,setSide]=useState(true);
  const ref=useRef(),fileRef=useRef(),endRef=useRef();
  const abortRef=useRef(null);
  const cancelStream=()=>{if(abortRef.current){abortRef.current.abort();abortRef.current=null;}};
  useEffect(()=>{endRef.current?.scrollIntoView({behavior:"smooth"});},[msgs,streaming]);

  const pickFile=async f=>{if(!f)return;cancelStream();setFile(f);setErr("");setExt(true);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.extractText(f,ctrl);setDoc(d.text);setWC(d.word_count);
      setMsgs([{role:"assistant",content:`I've read **${f.name}** (${d.word_count.toLocaleString()} words). Ask me anything about it!`}]);}
    catch(e){if(e.name!=="AbortError"){setErr(e.message);setFile(null);}}finally{setExt(false);abortRef.current=null;}};

  const send=async(msg=input)=>{
    if(!msg.trim()||streaming)return;
    const q=msg.trim();setInput("");
    setMsgs(p=>[...p,{role:"user",content:q}]);
    setStream(true);
    try{
      let full="";
      setMsgs(p=>[...p,{role:"assistant",content:"",streaming:true}]);
      const ctrl=new AbortController();abortRef.current=ctrl;
      await aiApi.streamChat(file?.name||"doc",q,docText,lang,chunk=>{
        full+=chunk;setMsgs(p=>{const n=[...p];n[n.length-1]={role:"assistant",content:full,streaming:true};return n;});
      });
      setMsgs(p=>{const n=[...p];n[n.length-1]={role:"assistant",content:full};return n;});
    }catch(e){if(e.name!=="AbortError")setMsgs(p=>[...p,{role:"assistant",content:"⚠️ "+e.message}]);}
    finally{setStream(false);abortRef.current=null;}
  };

  const renderMsg = (txt) => {
    if (!txt) return null;
    const lines = txt.split("\n");
    const elements = [];
    let listItems = [];

    const flushList = () => {
      if (listItems.length > 0) {
        elements.push(
          <ul key={`ul-${elements.length}`} style={{margin:"8px 0 8px 4px",paddingLeft:18,display:"flex",flexDirection:"column",gap:4}}>
            {listItems.map((li, i) => (
              <li key={i} style={{fontSize:14.5,lineHeight:1.6,color:"inherit"}}>{inlineFmt(li)}</li>
            ))}
          </ul>
        );
        listItems = [];
      }
    };

    const inlineFmt = (str) => {
      // bold + code inline
      const parts = str.split(/(\*\*.*?\*\*|`.*?`)/g);
      return parts.map((p, i) => {
        if (p.startsWith("**") && p.endsWith("**")) return <strong key={i} style={{fontWeight:800}}>{p.slice(2,-2)}</strong>;
        if (p.startsWith("`") && p.endsWith("`")) return <code key={i} style={{background:"rgba(0,0,0,.1)",padding:"1px 6px",borderRadius:5,fontSize:13,fontFamily:"monospace"}}>{p.slice(1,-1)}</code>;
        return p;
      });
    };

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i];
      const t = line.trim();
      if (!t) { flushList(); elements.push(<div key={`sp-${i}`} style={{height:6}}/>); continue; }

      // H1
      if (t.startsWith("# ")) {
        flushList();
        elements.push(<div key={i} style={{fontSize:18,fontWeight:900,margin:"14px 0 6px",letterSpacing:"-.3px",borderBottom:"1.5px solid rgba(0,0,0,.1)",paddingBottom:6}}>{inlineFmt(t.slice(2))}</div>);
        continue;
      }
      // H2
      if (t.startsWith("## ")) {
        flushList();
        elements.push(<div key={i} style={{fontSize:15.5,fontWeight:900,margin:"12px 0 5px",color:"inherit"}}>{inlineFmt(t.slice(3))}</div>);
        continue;
      }
      // H3
      if (t.startsWith("### ")) {
        flushList();
        elements.push(<div key={i} style={{fontSize:14,fontWeight:800,margin:"10px 0 4px",opacity:.85}}>{inlineFmt(t.slice(4))}</div>);
        continue;
      }
      // Numbered
      if (/^\d+[\.\)]/.test(t)) {
        flushList();
        elements.push(
          <div key={i} style={{display:"flex",gap:8,padding:"3px 0",alignItems:"flex-start"}}>
            <span style={{fontSize:12,fontWeight:900,minWidth:22,height:22,borderRadius:"50%",background:"rgba(0,0,0,.12)",display:"flex",alignItems:"center",justifyContent:"center",flexShrink:0,marginTop:1}}>
              {t.match(/^(\d+)/)[1]}
            </span>
            <span style={{fontSize:14.5,lineHeight:1.6}}>{inlineFmt(t.replace(/^\d+[\.\)]\s*/,""))}</span>
          </div>
        );
        continue;
      }
      // Bullet
      if (/^[-•*]/.test(t)) {
        listItems.push(t.replace(/^[-•*]\s*/,""));
        continue;
      }
      // Divider ---
      if (/^---+$/.test(t)) {
        flushList();
        elements.push(<hr key={i} style={{border:"none",borderTop:"1px solid rgba(0,0,0,.12)",margin:"8px 0"}}/>);
        continue;
      }
      // Normal paragraph
      flushList();
      elements.push(<p key={i} style={{margin:"3px 0",fontSize:14.5,lineHeight:1.7}}>{inlineFmt(t)}</p>);
    }
    flushList();
    return <div style={{display:"flex",flexDirection:"column",gap:2}}>{elements}</div>;
  };

  return(
    <div className={s.page}>
      <div className={s.bar}>
        <motion.button className={s.back} onClick={()=>nav("/student/ai-tools")} whileHover={{x:-3}}>← Back</motion.button>
        <div className={s.barC}>
          <span className={s.barIco}>💬</span>
          <div><p className={s.barN}>Document Chat</p><p className={s.barS}>Ask anything about your document</p></div>
        </div>
        <div style={{display:"flex",gap:8,alignItems:"center"}}>
          {file&&<div className={s.docChip}><span>📎</span><span className={s.dcN}>{file.name}</span></div>}
          <motion.button className={`${s.sideBtn} ${sideOpen?s.sideBtnA:""}`} onClick={()=>setSide(v=>!v)} whileHover={{scale:1.05}}>📁</motion.button>
        </div>
      </div>

      <div className={s.layout}>
        {/* Doc sidebar */}
        <AnimatePresence>
          {sideOpen&&(
            <motion.aside className={s.docSide}
              initial={{width:0,opacity:0}} animate={{width:260,opacity:1}} exit={{width:0,opacity:0}}
              transition={{duration:.35,ease:[.22,1,.36,1]}}>
              <div className={s.docSideInner}>
                <p className={s.dsTitle}>Document</p>
                {!file?(
                  <div className={s.dz}
                    onDragOver={e=>e.preventDefault()} onDrop={e=>{e.preventDefault();pickFile(e.dataTransfer.files[0]);}}
                    onClick={()=>fileRef.current?.click()}>
                    <motion.span className={s.dzIco} animate={{y:[0,-5,0]}} transition={{repeat:Infinity,duration:2.6}}>📄</motion.span>
                    <p className={s.dzT}>Upload document</p>
                    <p className={s.dzS}>to start chatting</p>
                    <motion.button className={s.dzBtn} style={{background:G}}
                      whileHover={{scale:1.05}} whileTap={{scale:.96}}
                      onClick={e=>{e.stopPropagation();fileRef.current?.click();}}>Choose</motion.button>
                    <input ref={fileRef} type="file" accept={ACCEPT_STRING} style={{display:"none"}} onChange={e=>pickFile(e.target.files[0])}/>
                  </div>
                ):(
                  <motion.div className={s.docInfo} initial={{opacity:0}} animate={{opacity:1}}>
                    <div className={s.diIcon}>📎</div>
                    <p className={s.diName}>{file.name}</p>
                    <p className={s.diMeta}>{wc.toLocaleString()} words</p>
                    <div className={s.diBar}><div className={s.diBarFill} style={{background:G,width:"100%"}}/></div>
                    <button className={s.changeDoc} onClick={()=>{setFile(null);setDoc("");setMsgs([]);fileRef.current?.click();}}>
                      🔄 Change document
                    </button>
                    <input ref={fileRef} type="file" accept={ACCEPT_STRING} style={{display:"none"}} onChange={e=>pickFile(e.target.files[0])}/>
                  </motion.div>
                )}
                {ext&&<div className={s.stRow}><span className={s.spin} style={{"--c":C}}/>Reading...</div>}
                {err&&<div className={s.errRow}>⚠️ {err}</div>}
                {file&&(
                  <div className={s.langBox}>
                    <p className={s.dsLabel}>Language</p>
                    {[{k:"en",l:"🇬🇧 English"},{k:"ar",l:"🇸🇦 Arabic"},{k:"eg",l:"🇪🇬 عامية"}].map(({k,l})=>(
                      <motion.button key={k} className={`${s.lBtn} ${lang===k?s.lBtnA:""}`}
                        style={lang===k?{background:G,color:"white",border:"none"}:{}} onClick={()=>setLang(k)}
                        whileHover={{scale:1.03}} whileTap={{scale:.96}}>{l}</motion.button>
                    ))}
                  </div>
                )}
              </div>
            </motion.aside>
          )}
        </AnimatePresence>

        {/* Messages */}
        <div className={s.chatArea}>
          <div className={s.msgList}>
            {msgs.length===0&&(
              <motion.div className={s.welcome} initial={{opacity:0,y:24}} animate={{opacity:1,y:0}}>
                <div className={s.welcomeOrb} style={{background:G}}/>
                <h2 className={s.welcomeT}>Document Chat</h2>
                <p className={s.welcomeS}>Upload a document, then ask me anything about it</p>
                {file&&(
                  <div className={s.suggestions}>
                    {SUGGESTED.map((q,i)=>(
                      <motion.button key={i} className={s.sugBtn}
                        initial={{opacity:0,y:10}} animate={{opacity:1,y:0}} transition={{delay:i*.06}}
                        whileHover={{scale:1.03,borderColor:C,color:C}} whileTap={{scale:.97}}
                        onClick={()=>send(q)}>{q}</motion.button>
                    ))}
                  </div>
                )}
              </motion.div>
            )}
            {msgs.map((m,i)=>(
              <motion.div key={i} className={`${s.msg} ${m.role==="user"?s.msgUser:s.msgBot}`}
                initial={{opacity:0,y:12,scale:.97}} animate={{opacity:1,y:0,scale:1}} transition={{duration:.25}}>
                {m.role==="assistant"&&<div className={s.botAvatar} style={{background:G}}>AI</div>}
                <div className={`${s.bubble} ${m.role==="user"?s.bubUser:s.bubBot}`}
                  style={m.role==="user"?{background:G}:{}}>
                  {renderMsg(m.content)}
                  {m.streaming&&<motion.span className={s.cursor} animate={{opacity:[1,0,1]}} transition={{repeat:Infinity,duration:.8}}>▌</motion.span>}
                </div>
              </motion.div>
            ))}
            <div ref={endRef}/>
          </div>

          {/* Input */}
          <div className={s.inputArea}>
            {file&&msgs.length===1&&(
              <motion.div className={s.quickSugg} initial={{opacity:0,y:8}} animate={{opacity:1,y:0}}>
                {SUGGESTED.slice(0,3).map((q,i)=>(
                  <motion.button key={i} className={s.qsBtn} onClick={()=>send(q)} whileHover={{scale:1.04,borderColor:C}} whileTap={{scale:.96}}>{q}</motion.button>
                ))}
              </motion.div>
            )}
            {streaming&&(
              <div style={{textAlign:"center",marginBottom:6}}>
                <motion.button style={{padding:"6px 18px",borderRadius:9,border:"1.5px solid rgba(239,68,68,.4)",background:"rgba(239,68,68,.08)",color:"#ef4444",fontFamily:"'Sora',sans-serif",fontWeight:700,fontSize:12,cursor:"pointer"}}
                  onClick={()=>{cancelStream();setStream(false);}}
                  whileHover={{scale:1.04}} whileTap={{scale:.96}}>✕ Stop generating</motion.button>
              </div>
            )}
            <div className={s.inputBox}>
              <input ref={ref} className={s.input} placeholder={file?"Ask anything about the document...":"Upload a document first..."}
                value={input} onChange={e=>setInput(e.target.value)} disabled={!file||streaming}
                onKeyDown={e=>e.key==="Enter"&&!e.shiftKey&&send()}/>
              <motion.button className={s.sendBtn} style={{background:(!input.trim()||!file||streaming)?"var(--card-border)":G}}
                disabled={!input.trim()||!file||streaming} onClick={()=>send()}
                whileHover={{scale:1.07}} whileTap={{scale:.93}}>
                {streaming?<span className={s.spin} style={{"--c":"white"}}/>:<svg width="16" height="16" viewBox="0 0 24 24" fill="white"><path d="M22 2L11 13"/><path d="M22 2L15 22 11 13 2 9l20-7z"/></svg>}
              </motion.button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}