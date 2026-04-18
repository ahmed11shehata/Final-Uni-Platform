// SummaryTool — LEFT sidebar upload · RIGHT large document result
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./SummaryTool.module.css";

const C = "#6366f1", G = "linear-gradient(135deg,#6366f1,#4338ca)";

function RenderDoc({ text }) {
  const bold = str => str.split(/\*\*(.*?)\*\*/g).map((p, i) =>
    i % 2 === 1 ? <strong key={i} style={{ color: C }}>{p}</strong> : p
  );
  return (
    <div className={s.doc}>
      {text.split("\n").map((line, i) => {
        const t = line.trim();
        if (!t) return <div key={i} style={{ height: 8 }} />;
        if (t.startsWith("# "))   return <h1 key={i} className={s.h1}>{t.slice(2)}</h1>;
        if (t.startsWith("## "))  return <h2 key={i} className={s.h2}>{t.slice(3)}</h2>;
        if (t.startsWith("### ")) return <h3 key={i} className={s.h3}>{t.slice(4)}</h3>;
        if (/^[-•*]/.test(t))     return <div key={i} className={s.bul}><span className={s.bdot} /><span>{bold(t.replace(/^[-•*]\s*/, ""))}</span></div>;
        return <p key={i} className={s.para}>{bold(t)}</p>;
      })}
    </div>
  );
}

export default function SummaryTool() {
  const nav = useNavigate();
  const [file, setFile]       = useState(null);
  const [text, setText]       = useState("");
  const [wc, setWC]           = useState(0);
  const [ext, setExt]         = useState(false);
  const [lang, setLang]       = useState("en");
  const [load, setLoad]       = useState(false);
  const [sum, setSum]         = useState(null);
  const [err, setErr]         = useState("");
  const [copied, setCopied]   = useState(false);
  const ref      = useRef();
  const abortRef = useRef(null); // holds current AbortController

  /* ── cancel any running request ── */
  const cancel = () => {
    if (abortRef.current) {
      abortRef.current.abort();
      abortRef.current = null;
    }
  };

  /* ── pick + extract ── */
  const pick = async (f) => {
    if (!f) return;
    cancel(); // cancel any previous extract
    setFile(f); setErr(""); setSum(null); setExt(true);
    const ctrl = new AbortController();
    abortRef.current = ctrl;
    try {
      const d = await aiApi.extractText(f, ctrl);
      setText(d.text);
      setWC(d.word_count);
    } catch (e) {
      if (e.name !== "AbortError") {
        setErr(e.message);
        setFile(null);
      }
    } finally {
      setExt(false);
      abortRef.current = null;
    }
  };

  /* ── generate summary ── */
  const gen = async () => {
    cancel();
    setLoad(true); setErr(""); setSum(null);
    const ctrl = new AbortController();
    abortRef.current = ctrl;
    try {
      const d = await aiApi.generateSummary(text, lang, ctrl);
      setSum(d.summary);
    } catch (e) {
      if (e.name !== "AbortError") setErr(e.message);
    } finally {
      setLoad(false);
      abortRef.current = null;
    }
  };

  /* ── cancel + reset ── */
  const handleCancel = () => {
    cancel();
    setLoad(false);
    setExt(false);
    setErr("Cancelled.");
  };

  const copy = () => {
    navigator.clipboard.writeText(sum || "");
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className={s.page}>
      {/* TOP BAR */}
      <div className={s.bar}>
        <motion.button className={s.back} onClick={() => nav("/student/ai-tools")} whileHover={{ x: -3 }}>
          ← Back
        </motion.button>
        <div className={s.barMeta}>
          <span className={s.barEmoji}>📄</span>
          <div>
            <p className={s.barName}>Smart Summary</p>
            <p className={s.barSub}>AI-generated structured document summaries</p>
          </div>
        </div>
      </div>

      {/* SPLIT */}
      <div className={s.split}>

        {/* ── LEFT sidebar ── */}
        <motion.aside className={s.sidebar}
          initial={{ opacity: 0, x: -32 }} animate={{ opacity: 1, x: 0 }}
          transition={{ duration: .4, ease: [.22, 1, .36, 1] }}>

          {/* Drop zone */}
          <div className={s.uploadBox}>
            <div className={s.uploadStripe} style={{ background: G }} />
            {!file ? (
              <div className={s.dz}
                onDragOver={e => e.preventDefault()}
                onDrop={e => { e.preventDefault(); pick(e.dataTransfer.files[0]); }}
                onClick={() => ref.current?.click()}>
                <motion.div className={s.dzIcon}
                  animate={{ y: [0, -6, 0] }}
                  transition={{ repeat: Infinity, duration: 2.6, ease: "easeInOut" }}>📄</motion.div>
                <p className={s.dzT}>Drop document here</p>
                <p className={s.dzS}>PDF · DOCX · PPTX · TXT · Image</p>
                <motion.button className={s.dzBtn} style={{ background: G }}
                  whileHover={{ scale: 1.05, boxShadow: `0 8px 24px ${C}55` }}
                  whileTap={{ scale: .96 }}
                  onClick={e => { e.stopPropagation(); ref.current?.click(); }}>
                  Choose File
                </motion.button>
                <input ref={ref} type="file" accept={ACCEPT_STRING} style={{ display: "none" }}
                  onChange={e => pick(e.target.files[0])} />
              </div>
            ) : (
              <motion.div className={s.fp} initial={{ opacity: 0, scale: .92 }} animate={{ opacity: 1, scale: 1 }}>
                <span className={s.fpIcon}>📎</span>
                <div className={s.fpInfo}>
                  <p className={s.fpName}>{file.name}</p>
                  <p className={s.fpMeta}>{(file.size / 1024 / 1024).toFixed(2)} MB</p>
                </div>
                <button className={s.fpX} onClick={() => { cancel(); setFile(null); setText(""); setSum(null); setErr(""); }}>✕</button>
              </motion.div>
            )}
          </div>

          {/* Status */}
          <AnimatePresence>
            {ext && (
              <motion.div className={s.sRow} initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}>
                <span className={s.spin} style={{ "--c": C }} />
                Extracting text...
              </motion.div>
            )}
            {text && !ext && (
              <motion.div className={s.okRow} initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}>
                <span className={s.okDot} />✅ {wc.toLocaleString()} words extracted
              </motion.div>
            )}
            {err && (
              <motion.div className={s.errRow} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                ⚠️ {err}
              </motion.div>
            )}
          </AnimatePresence>

          {/* Cancel button — visible while extracting or generating */}
          <AnimatePresence>
            {(ext || load) && (
              <motion.button className={s.cancelBtn}
                initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
                onClick={handleCancel}>
                ✕ Cancel
              </motion.button>
            )}
          </AnimatePresence>

          {/* Config */}
          {text && (
            <motion.div className={s.cfg} initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: .1 }}>
              <p className={s.cfgL}>Language</p>
              <div className={s.langs}>
                {[{ k: "en", f: "🇬🇧", l: "English" }, { k: "ar", f: "🇸🇦", l: "العربية" }].map(({ k, f, l }) => (
                  <motion.button key={k} className={`${s.lb} ${lang === k ? s.lbA : ""}`}
                    style={lang === k ? { background: G, color: "white", border: "none" } : {}}
                    onClick={() => setLang(k)} whileHover={{ scale: 1.04 }} whileTap={{ scale: .96 }}>
                    {f} {l}
                  </motion.button>
                ))}
              </div>
            </motion.div>
          )}

          {/* Generate */}
          {text && (
            <motion.button className={s.genBtn} style={{ background: G }} disabled={load} onClick={gen}
              initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: .2 }}
              whileHover={{ scale: 1.04, boxShadow: `0 14px 40px ${C}44` }} whileTap={{ scale: .97 }}>
              {load
                ? <><span className={s.spin} style={{ "--c": "white" }} />Summarizing...</>
                : "✨ Generate Summary"}
            </motion.button>
          )}
        </motion.aside>

        {/* ── RIGHT result ── */}
        <motion.main className={s.main}
          initial={{ opacity: 0, x: 28 }} animate={{ opacity: 1, x: 0 }}
          transition={{ duration: .4, ease: [.22, 1, .36, 1] }}>
          <AnimatePresence mode="wait">
            {!sum && !load && (
              <motion.div key="idle" className={s.idle} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
                <div className={s.idleIllu}>
                  {[92, 65, 84, 48, 76, 55, 88, 40].map((w, i) => (
                    <motion.div key={i} className={s.idleLine} style={{ width: `${w}%` }}
                      animate={{ opacity: [.15, .4, .15] }} transition={{ repeat: Infinity, duration: 2.4, delay: i * .2 }} />
                  ))}
                </div>
                <motion.div className={s.idleBadge} animate={{ opacity: [.5, 1, .5] }} transition={{ repeat: Infinity, duration: 3 }}>
                  Your summary will appear here
                </motion.div>
              </motion.div>
            )}
            {load && (
              <motion.div key="load" className={s.loadBox} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
                <motion.div className={s.loadOrb} style={{ background: G }}
                  animate={{ scale: [1, 1.18, 1], opacity: [.6, 1, .6] }} transition={{ repeat: Infinity, duration: 1.7 }} />
                <p className={s.loadTxt}>Analyzing &amp; summarizing...</p>
                <p className={s.loadSub}>This can take up to 90 seconds for large files</p>
                <motion.button
                  style={{ padding: "9px 28px", borderRadius: 10, border: "1.5px solid rgba(239,68,68,.4)", background: "rgba(239,68,68,.08)", color: "#ef4444", fontFamily: "'Sora',sans-serif", fontWeight: 700, fontSize: 13, cursor: "pointer" }}
                  onClick={handleCancel}
                  whileHover={{ scale: 1.04 }}
                  whileTap={{ scale: .96 }}
                >
                  ✕ Cancel
                </motion.button>
              </motion.div>
            )}
            {sum && (
              <motion.div key="result" className={s.result}
                initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: .45, ease: [.22, 1, .36, 1] }}>
                <div className={s.rBar}>
                  <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                    <motion.span className={s.rPulse} style={{ background: C }}
                      animate={{ scale: [1, 1.4, 1], opacity: [1, .5, 1] }} transition={{ repeat: Infinity, duration: 2 }} />
                    <span className={s.rTitle}>Summary Ready</span>
                  </div>
                  <div style={{ display: "flex", gap: 8 }}>
                    <motion.button className={`${s.rBtn} ${copied ? s.rBtnG : ""}`} onClick={copy} whileHover={{ scale: 1.06 }} whileTap={{ scale: .94 }}>
                      {copied ? "✅ Copied" : "📋 Copy"}
                    </motion.button>
                    <motion.button className={s.rBtn} onClick={() => setSum(null)} whileHover={{ scale: 1.06 }}>🔄</motion.button>
                  </div>
                </div>
                <div className={s.rBody}><RenderDoc text={sum} /></div>
              </motion.div>
            )}
          </AnimatePresence>
        </motion.main>
      </div>
    </div>
  );
}