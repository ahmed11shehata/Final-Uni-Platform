// MindMapTool — Narrow upload sidebar LEFT · Huge visual tree RIGHT
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./MindMapTool.module.css";
const C="#22c55e",G="linear-gradient(135deg,#22c55e,#16a34a)";

function parseMM(text){
  const lines=text.split("\n").filter(l=>l.trim());
  const root={text:"Root",children:[]};let mainNode=null;
  for(const line of lines){
    const t=line.trim().replace(/^#+\s*/,"").replace(/^[-•*]\s*/,"").replace(/\*\*(.*?)\*\*/g,"$1");
    if(!t)continue;
    const ind=line.search(/\S/);
    if(ind<=2||line.match(/^#{1,2}\s/)){
      if(root.text==="Root"&&root.children.length===0){root.text=t;continue;}
      mainNode={text:t,children:[],open:true};root.children.push(mainNode);
    }else if(mainNode){
      mainNode.children.push({text:t,children:[],open:false});
    }
  }
  return root;
}

function TreeNode({node,depth=0,color}){
  const[open,setOpen]=useState(node.open!==false);
  const hasKids=node.children&&node.children.length>0;
  const ind=depth*24;
  return(
    <div className={s.treeNode}>
      <motion.div
        className={`${s.nodeBox} ${depth===0?s.rootBox:depth===1?s.mainBox:s.subBox}`}
        style={depth===0?{background:G,color:"white",borderColor:"transparent",marginLeft:0}:
               depth===1?{borderColor:color,marginLeft:ind}:{marginLeft:ind+24}}
        onClick={()=>hasKids&&setOpen(v=>!v)}
        whileHover={{x:depth>0?5:0,transition:{duration:.12}}}
        initial={{opacity:0,x:-10}} animate={{opacity:1,x:0}} transition={{duration:.3}}>
        {depth===1&&hasKids&&(
          <motion.span className={s.toggle} animate={{rotate:open?90:0}}>{open?"▾":"▸"}</motion.span>
        )}
        <span className={s.nodeTxt}>{node.text}</span>
        {hasKids&&depth>0&&<span className={s.badge}>{node.children.length}</span>}
      </motion.div>
      <AnimatePresence>
        {open&&hasKids&&(
          <motion.div className={s.kids}
            style={{borderLeftColor:depth===0?color:`${color}50`,marginLeft:ind+(depth===0?20:24)}}
            initial={{opacity:0,height:0}} animate={{opacity:1,height:"auto"}} exit={{opacity:0,height:0}}
            transition={{duration:.3,ease:[.22,1,.36,1]}}>
            {node.children.map((child,i)=>(
              <TreeNode key={i} node={child} depth={depth+1} color={color}/>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

export default function MindMapTool(){
  const nav=useNavigate();
  const[file,setFile]=useState(null),[text,setText]=useState(""),[wc,setWC]=useState(0);
  const[ext,setExt]=useState(false),[lang,setLang]=useState("en");
  const[load,setLoad]=useState(false),[mmText,setMM]=useState(null),[err,setErr]=useState("");
  const[view,setView]=useState("tree");
  const[copied,setCopied]=useState(false);
  const ref=useRef();
  const abortRef=useRef(null);
  const cancel=()=>{if(abortRef.current){abortRef.current.abort();abortRef.current=null;}};

  const pick=async f=>{if(!f)return;setFile(f);setErr("");setMM(null);setExt(true);
    const ctrl=new AbortController();abortRef.current=ctrl;
    try{const d=await aiApi.extractText(f,ctrl);setText(d.text);setWC(d.word_count);}
    catch(e){if(e.name!=="AbortError"){setErr(e.message);setFile(null);}}finally{setExt(false);abortRef.current=null;}};
  const gen=async()=>{cancel();setLoad(true);setErr("");setMM(null);
    try{const d=await (()=>{const ctrl=new AbortController();abortRef.current=ctrl;return aiApi.generateMindmap(text,lang,ctrl);})();setMM(d.mindmap);}
    catch(e){setErr(e.message);}finally{setLoad(false);}};
  const copy=()=>{navigator.clipboard.writeText(mmText||"");setCopied(true);setTimeout(()=>setCopied(false),2000);};
  const tree=mmText?parseMM(mmText):null;

  return(
    <div className={s.page}>
      <div className={s.bar}>
        <motion.button className={s.back} onClick={()=>nav("/student/ai-tools")} whileHover={{x:-3}}>← Back</motion.button>
        <div className={s.barC}>
          <span className={s.barIco}>🗺️</span>
          <div><p className={s.barN}>Mind Map</p><p className={s.barS}>Visual knowledge tree from your document</p></div>
        </div>
        {mmText&&(
          <div className={s.viewToggle}>
            {[{k:"tree",ico:"🌳"},{k:"text",ico:"📝"}].map(({k,ico})=>(
              <motion.button key={k} className={`${s.vtBtn} ${view===k?s.vtBtnA:""}`}
                style={view===k?{background:G,color:"white",border:"none"}:{}} onClick={()=>setView(k)}
                whileHover={{scale:1.04}} whileTap={{scale:.96}}>{ico} {k==="tree"?"Visual":"Raw"}</motion.button>
            ))}
          </div>
        )}
      </div>

      <div className={s.split}>
        {/* LEFT sidebar */}
        <motion.aside className={s.sidebar}
          initial={{opacity:0,x:-28}} animate={{opacity:1,x:0}} transition={{duration:.4}}>
          <div className={s.panelTitle}><div className={s.panelDot} style={{background:C}}/>Upload</div>
          <div className={s.uploadCard}>
            <div className={s.topLine} style={{background:G}}/>
            {!file?(
              <div className={s.dz}
                onDragOver={e=>e.preventDefault()} onDrop={e=>{e.preventDefault();pick(e.dataTransfer.files[0]);}}
                onClick={()=>ref.current?.click()}>
                <motion.span className={s.dzIcon} animate={{y:[0,-5,0]}} transition={{repeat:Infinity,duration:2.8}}>📄</motion.span>
                <p className={s.dzT}>Drop here</p>
                <p className={s.dzS}>PDF · DOCX · PPTX · TXT</p>
                <motion.button className={s.dzBtn} style={{background:G}}
                  whileHover={{scale:1.06,boxShadow:`0 8px 24px ${C}55`}} whileTap={{scale:.94}}
                  onClick={e=>{e.stopPropagation();ref.current?.click();}}>Browse</motion.button>
                <input ref={ref} type="file" accept={ACCEPT_STRING} style={{display:"none"}} onChange={e=>pick(e.target.files[0])}/>
              </div>
            ):(
              <motion.div className={s.fp} initial={{opacity:0,scale:.9}} animate={{opacity:1,scale:1}}>
                <span>📎</span>
                <div><p className={s.fpN}>{file.name}</p><p className={s.fpM}>{wc.toLocaleString()} words</p></div>
                <button className={s.fpX} onClick={()=>{setFile(null);setText("");setMM(null);}}>✕</button>
              </motion.div>
            )}
          </div>
          <AnimatePresence>
            {ext&&<motion.div className={s.stRow} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}><span className={s.spin} style={{"--c":C}}/>Extracting...</motion.div>}
            {text&&!ext&&<motion.div className={s.okRow} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}><span className={s.okDot}/>✅ {wc.toLocaleString()} words</motion.div>}
            {err&&<motion.div className={s.errRow} initial={{opacity:0}} animate={{opacity:1}}>⚠️ {err}</motion.div>}
          </AnimatePresence>
          {text&&(
            <motion.div className={s.panelTitle} initial={{opacity:0}} animate={{opacity:1}}><div className={s.panelDot} style={{background:C}}/>Language</motion.div>
          )}
          {text&&(
            <motion.div className={s.langBox} initial={{opacity:0,y:8}} animate={{opacity:1,y:0}}>
              {[{k:"en",l:"🇬🇧 English"},{k:"ar",l:"🇸🇦 Arabic"}].map(({k,l})=>(
                <motion.button key={k} className={`${s.lBtn} ${lang===k?s.lBtnA:""}`}
                  style={lang===k?{background:G,color:"white",border:"none"}:{}} onClick={()=>setLang(k)}
                  whileHover={{scale:1.03}} whileTap={{scale:.96}}>{l}</motion.button>
              ))}
            </motion.div>
          )}
          {text&&(
            <motion.button className={s.genBtn} style={{background:G}} disabled={load} onClick={gen}
              initial={{opacity:0,y:8}} animate={{opacity:1,y:0}}
              whileHover={{scale:1.04,boxShadow:`0 12px 36px ${C}44`}} whileTap={{scale:.97}}>
              {load?<><span className={s.spin} style={{"--c":"white"}}/>Building...</>:"🌳 Build Map"}
            </motion.button>
          )}
          {mmText&&(
            <motion.button className={s.copyBtn} onClick={copy} whileHover={{scale:1.03}}>
              {copied?"✅ Copied":"📋 Copy Text"}
            </motion.button>
          )}
        </motion.aside>

        {/* RIGHT canvas */}
        <motion.main className={s.canvas}
          initial={{opacity:0,x:28}} animate={{opacity:1,x:0}} transition={{duration:.4}}>
          <AnimatePresence mode="wait">
            {!mmText&&!load&&(
              <motion.div key="idle" className={s.idle} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
                <motion.div className={s.idleOrb} style={{background:G}}
                  animate={{scale:[1,1.1,1],opacity:[.4,.7,.4]}} transition={{repeat:Infinity,duration:3}}/>
                <div className={s.idleTree}>
                  {[{w:120,y:0},{w:80,y:50,l:60},{w:80,y:50,r:60},{w:55,y:110,l:30},{w:55,y:110,l:100},{w:55,y:110,r:30},{w:55,y:110,r:100}].map((n,i)=>(
                    <motion.div key={i} className={s.idleNode}
                      style={{width:n.w,top:n.y,left:n.l!=null?n.l:undefined,right:n.r!=null?n.r:undefined}}
                      animate={{opacity:[.2,.5,.2]}} transition={{repeat:Infinity,duration:2.5,delay:i*.25}}/>
                  ))}
                </div>
                <p className={s.idleMsg}>Your mind map will appear here</p>
              </motion.div>
            )}
            {load&&(
              <motion.div key="load" className={s.loadBox} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}>
                <div className={s.loadNodes}>
                  {[0,1,2,3,4].map(i=>(
                    <motion.div key={i} className={s.loadNode} style={{background:G,borderColor:`${C}44`}}
                      animate={{scale:[1,1.15,1],opacity:[.6,1,.6]}} transition={{repeat:Infinity,duration:1.5,delay:i*.2}}/>
                  ))}
                </div>
                <p className={s.loadTxt}>Building mind map...</p>
              </motion.div>
            )}
            {mmText&&view==="tree"&&tree&&(
              <motion.div key="tree" className={s.treeWrap} initial={{opacity:0}} animate={{opacity:1}} transition={{duration:.4}}>
                <div className={s.treeRoot}>
                  <motion.div className={s.rootChip} style={{background:G}}
                    initial={{scale:0}} animate={{scale:1}} transition={{type:"spring",stiffness:250,damping:20}}>
                    {tree.text}
                  </motion.div>
                </div>
                <div className={s.branches}>
                  {tree.children.map((node,i)=>(
                    <TreeNode key={i} node={node} depth={1} color={C}/>
                  ))}
                </div>
              </motion.div>
            )}
            {mmText&&view==="text"&&(
              <motion.pre key="text" className={s.rawText} initial={{opacity:0}} animate={{opacity:1}}>{mmText}</motion.pre>
            )}
          </AnimatePresence>
        </motion.main>
      </div>
    </div>
  );
}