// GenerateAllTool — Mission control: upload top · 4 result panels reveal one by one
import { useState, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi, ACCEPT_STRING } from "../../../services/aiApi";
import s from "./GenerateAllTool.module.css";

const C = "#a855f7", G = "linear-gradient(135deg,#a855f7,#7c3aed)";
const FEATURES = [
  { id: "summary",       icon: "📄", label: "Summary",       color: "#6366f1" },
  { id: "quiz",          icon: "✏️", label: "Quiz",          color: "#e8a838" },
  { id: "mindmap",       icon: "🗺️", label: "Mind Map",     color: "#22c55e" },
  { id: "question_bank", icon: "📚", label: "Question Bank", color: "#e05c8a" },
];

function RichBody({ text, color }) {
  if (!text) return null;
  return (
    <div className={s.richBody}>
      {text.split("\n").map((line, i) => {
        const t = line.trim(); if (!t) return <div key={i} style={{ height: 6 }} />;
        if (t.startsWith("# "))  return <h1 key={i} className={s.rh1}>{t.slice(2)}</h1>;
        if (t.startsWith("## ")) return <h2 key={i} className={s.rh2}>{t.slice(3)}</h2>;
        if (t.startsWith("### "))return <h3 key={i} className={s.rh3} style={{ color }}>{t.slice(4)}</h3>;
        if (/^[-•]/.test(t))     return <div key={i} className={s.rbul}><span className={s.rdot} style={{ background: color }} /><span>{t.replace(/^[-•]\s*/, "")}</span></div>;
        return <p key={i} className={s.rpara}>{t}</p>;
      })}
    </div>
  );
}

export default function GenerateAllTool() {
  const nav = useNavigate();
  const [file, setFile]       = useState(null);
  const [wc, setWC]           = useState(0);
  const [ext, setExt]         = useState(false);
  const [lang, setLang]       = useState("en");
  const [numQ, setNumQ]       = useState(10);
  const [load, setLoad]       = useState(false);
  const [results, setResults] = useState(null);
  const [err, setErr]         = useState("");
  const [doneSteps, setDone]  = useState([]); // only filled AFTER real response
  const [activeTab, setTab]   = useState("summary");
  const [loadMsg, setLoadMsg] = useState("Processing document...");
  const ref      = useRef();
  const abortRef = useRef(null);

  const cancel = () => {
    if (abortRef.current) {
      abortRef.current.abort();
      abortRef.current = null;
    }
  };

  const handleCancel = () => {
    cancel();
    setLoad(false);
    setExt(false);
    setDone([]);
    setErr("Cancelled.");
  };

  /* ── extract on file pick ── */
  const pick = async (f) => {
    if (!f) return;
    cancel();
    setFile(f); setErr(""); setResults(null); setDone([]); setExt(true);
    const ctrl = new AbortController();
    abortRef.current = ctrl;
    try {
      const d = await aiApi.extractText(f, ctrl);
      setWC(d.word_count);
    } catch (e) {
      if (e.name !== "AbortError") { setErr(e.message); setFile(null); }
    } finally {
      setExt(false);
      abortRef.current = null;
    }
  };

  /* ── generate everything ── */
  const gen = async () => {
    cancel();
    setLoad(true); setErr(""); setResults(null); setDone([]);
    setLoadMsg("Uploading & processing document...");

    const ctrl = new AbortController();
    abortRef.current = ctrl;

    // Simulate progress messages while real call is running
    const msgs = [
      [2000,  "Generating summary..."],
      [8000,  "Creating quiz questions..."],
      [16000, "Building mind map..."],
      [26000, "Compiling question bank..."],
      [40000, "Almost done..."],
    ];
    const timers = msgs.map(([delay, msg]) =>
      setTimeout(() => { if (abortRef.current) setLoadMsg(msg); }, delay)
    );

    try {
      const d = await aiApi.generateAll(file, lang, numQ, 20, ctrl);
      timers.forEach(clearTimeout);
      // Mark ALL steps as done only after real response arrives
      setResults(d);
      setDone(FEATURES.map(f => f.id));
      setTab("summary");
    } catch (e) {
      timers.forEach(clearTimeout);
      if (e.name !== "AbortError") setErr(e.message);
    } finally {
      setLoad(false);
      abortRef.current = null;
    }
  };

  return (
    <div className={s.page}>
      <div className={s.bar}>
        <motion.button className={s.back} onClick={() => nav("/student/ai-tools")} whileHover={{ x: -3 }}>← Back</motion.button>
        <div className={s.barC}>
          <span className={s.barIco}>⚡</span>
          <div><p className={s.barN}>Generate All</p><p className={s.barS}>One upload → Summary · Quiz · Mind Map · Question Bank</p></div>
        </div>
      </div>

      <div className={s.content}>
        {/* TOP: upload + config + features grid */}
        <motion.div className={s.topSection} initial={{ opacity: 0, y: -20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: .4 }}>
          <div className={s.topLeft}>
            <div className={s.uploadArea}
              onDragOver={e => e.preventDefault()}
              onDrop={e => { e.preventDefault(); pick(e.dataTransfer.files[0]); }}
              onClick={() => ref.current?.click()}>
              {!file ? (
                <>
                  <motion.span className={s.uIcon} animate={{ y: [0, -5, 0] }} transition={{ repeat: Infinity, duration: 2.5 }}>📁</motion.span>
                  <div><p className={s.uT}>Drop document here</p><p className={s.uS}>PDF · DOCX · PPTX · TXT · Image</p></div>
                  <motion.button className={s.uBtn} style={{ background: G }} whileHover={{ scale: 1.05 }} whileTap={{ scale: .96 }}
                    onClick={e => { e.stopPropagation(); ref.current?.click(); }}>Choose File</motion.button>
                </>
              ) : (
                <motion.div className={s.fileInfo} initial={{ scale: .92 }} animate={{ scale: 1 }}>
                  <span className={s.fIcon}>📎</span>
                  <div><p className={s.fName}>{file.name}</p><p className={s.fMeta}>{wc.toLocaleString()} words · ready</p></div>
                  <motion.button className={s.fX}
                    onClick={e => { e.stopPropagation(); cancel(); setFile(null); setResults(null); setDone([]); }}
                    whileHover={{ scale: 1.1 }}>✕</motion.button>
                </motion.div>
              )}
              <input ref={ref} type="file" accept={ACCEPT_STRING} style={{ display: "none" }}
                onChange={e => pick(e.target.files[0])} />
            </div>

            <AnimatePresence>
              {ext && (
                <motion.div className={s.stRow} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
                  <span className={s.spin} style={{ "--c": C }} />Extracting...
                </motion.div>
              )}
              {file && wc > 0 && !ext && (
                <motion.div className={s.okRow} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
                  <span className={s.okDot} />✅ {wc.toLocaleString()} words ready
                </motion.div>
              )}
              {err && (
                <motion.div className={s.errRow} initial={{ opacity: 0 }} animate={{ opacity: 1 }}>⚠️ {err}</motion.div>
              )}
            </AnimatePresence>
          </div>

          <div className={s.topRight}>
            <div className={s.featuresGrid}>
              {FEATURES.map((f, i) => (
                <motion.div key={f.id}
                  className={`${s.featCard} ${doneSteps.includes(f.id) ? s.featDone : ""}`}
                  style={{ "--fc": f.color }}
                  initial={{ opacity: 0, scale: .9 }} animate={{ opacity: 1, scale: 1 }} transition={{ delay: i * .07 }}>
                  <span className={s.featIcon}>{f.icon}</span>
                  <span className={s.featLabel}>{f.label}</span>
                  <AnimatePresence>
                    {doneSteps.includes(f.id) && (
                      <motion.span className={s.featCheck} initial={{ scale: 0 }} animate={{ scale: 1 }} exit={{ scale: 0 }}>✓</motion.span>
                    )}
                  </AnimatePresence>
                </motion.div>
              ))}
            </div>

            <div className={s.configRow}>
              <div className={s.cfgG}>
                <span className={s.cfgL}>Questions</span>
                <div style={{ display: "flex", gap: 6 }}>
                  {[5, 10, 15, 20].map(n => (
                    <motion.button key={n} className={`${s.nBtn} ${numQ === n ? s.nBtnA : ""}`}
                      style={numQ === n ? { background: G, color: "white", border: "none" } : {}}
                      onClick={() => setNumQ(n)} whileHover={{ scale: 1.07 }} whileTap={{ scale: .93 }}>{n}</motion.button>
                  ))}
                </div>
              </div>
              <div className={s.cfgG}>
                <span className={s.cfgL}>Language</span>
                <div style={{ display: "flex", gap: 6 }}>
                  {[{ k: "en", l: "🇬🇧 EN" }, { k: "ar", l: "🇸🇦 AR" }].map(({ k, l }) => (
                    <motion.button key={k} className={`${s.lBtn} ${lang === k ? s.lBtnA : ""}`}
                      style={lang === k ? { background: G, color: "white", border: "none" } : {}}
                      onClick={() => setLang(k)} whileHover={{ scale: 1.04 }} whileTap={{ scale: .95 }}>{l}</motion.button>
                  ))}
                </div>
              </div>
            </div>

            <motion.button className={s.genBtn} style={{ background: G }}
              disabled={!file || !wc || load} onClick={gen}
              whileHover={{ scale: 1.03, boxShadow: `0 16px 50px ${C}44` }} whileTap={{ scale: .97 }}>
              {load
                ? <><span className={s.spin} style={{ "--c": "white" }} />Generating all...</>
                : "⚡ Generate Everything"}
            </motion.button>
          </div>
        </motion.div>

        {/* ── Loading status (real messages) ── */}
        <AnimatePresence>
          {load && (
            <motion.div className={s.progressSection}
              initial={{ opacity: 0, height: 0 }} animate={{ opacity: 1, height: "auto" }} exit={{ opacity: 0, height: 0 }}>
              <div style={{ display: "flex", alignItems: "center", gap: 16, flexWrap: "wrap" }}>
                <div className={s.progressTrack} style={{ flex: 1, minWidth: 140 }}>
                  <motion.div className={s.progressFill} style={{ background: G }}
                    animate={{ width: ["0%", "90%"] }}
                    transition={{ duration: 50, ease: "linear" }} />
                </div>
                <motion.span
                  key={loadMsg}
                  style={{ fontSize: 12.5, fontWeight: 700, color: "var(--text-secondary)", minWidth: 220 }}
                  initial={{ opacity: 0, y: 4 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: .3 }}>
                  {loadMsg}
                </motion.span>
                <motion.button
                  style={{ padding: "7px 18px", borderRadius: 9, border: "1.5px solid rgba(239,68,68,.4)", background: "rgba(239,68,68,.08)", color: "#ef4444", fontFamily: "'Sora',sans-serif", fontWeight: 700, fontSize: 12.5, cursor: "pointer", flexShrink: 0 }}
                  onClick={handleCancel}
                  whileHover={{ scale: 1.04 }} whileTap={{ scale: .96 }}>
                  ✕ Cancel
                </motion.button>
              </div>
              <div className={s.progressLabels}>
                {FEATURES.map((f) => (
                  <div key={f.id} className={s.progStep} style={{ color: "var(--text-muted)" }}>
                    <span>{f.icon}</span>
                    <span>{f.label}</span>
                  </div>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Results tabs */}
        <AnimatePresence>
          {results && (
            <motion.div className={s.resultsCard}
              initial={{ opacity: 0, y: 24 }} animate={{ opacity: 1, y: 0 }}
              transition={{ duration: .4, ease: [.22, 1, .36, 1] }}>
              <div className={s.tabBar}>
                {FEATURES.map(f => (
                  <motion.button key={f.id}
                    className={`${s.tab} ${activeTab === f.id ? s.tabA : ""}`}
                    style={activeTab === f.id ? { borderBottomColor: f.color, color: f.color } : {}}
                    onClick={() => setTab(f.id)} whileHover={{ y: -1 }}>
                    {f.icon} {f.label}
                  </motion.button>
                ))}
              </div>
              <AnimatePresence mode="wait">
                <motion.div key={activeTab} className={s.tabContent}
                  initial={{ opacity: 0, x: 14 }} animate={{ opacity: 1, x: 0 }}
                  exit={{ opacity: 0, x: -14 }} transition={{ duration: .22 }}>
                  <RichBody
                    text={results[activeTab] || results[activeTab.replace("_bank", "") + "_bank"] || ""}
                    color={FEATURES.find(f => f.id === activeTab)?.color || C}
                  />
                </motion.div>
              </AnimatePresence>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}