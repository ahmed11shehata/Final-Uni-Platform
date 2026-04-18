// LoginPage.jsx — Akhbar Elyoum Academy · v6
// Font: Outfit | Intro: typewriter + orbital logo + stars | Split: panel-open animation
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import toast from "react-hot-toast";
import { useAuth } from "../../hooks/useAuth";
import { useTheme } from "../../context/ThemeContext";
import { loginUser } from "../../services/api/authApi";
import s from "./LoginPage.module.css";

// ─────────────────────────────────────────
// PALETTE
// ─────────────────────────────────────────
const PALETTE = [
  { id: "amber", dot: "#f5a623", bg1: "#0b0400", bg2: "#1c0b00", accent: "#f5a623", glow: "rgba(245,166,35,0.48)" },
  { id: "navy", dot: "#4f7fff", bg1: "#02080e", bg2: "#071230", accent: "#4f7fff", glow: "rgba(79,127,255,0.4)" },
  { id: "violet", dot: "#9b59e8", bg1: "#060112", bg2: "#110530", accent: "#9b59e8", glow: "rgba(155,89,232,0.4)" },
  { id: "emerald", dot: "#10c97a", bg1: "#000d07", bg2: "#011b0e", accent: "#10c97a", glow: "rgba(16,201,122,0.4)" },
  { id: "rose", dot: "#f4587e", bg1: "#0e0107", bg2: "#1f0210", accent: "#f4587e", glow: "rgba(244,88,126,0.4)" },
  { id: "teal", dot: "#0dd3f5", bg1: "#000c12", bg2: "#011522", accent: "#0dd3f5", glow: "rgba(13,211,245,0.4)" },
  { id: "crimson", dot: "#f87171", bg1: "#0b0000", bg2: "#180000", accent: "#f87171", glow: "rgba(248,113,113,0.4)" },
  { id: "slate", dot: "#94a3b8", bg1: "#05070c", bg2: "#0a0e16", accent: "#94a3b8", glow: "rgba(148,163,184,0.38)" },
];

const ROLES = [
  { value: "student", label: "Student", emoji: "🎓" },
  { value: "instructor", label: "Instructor", emoji: "🖥️" },
  { value: "admin", label: "Admin", emoji: "🛡️" },
];

// ─────────────────────────────────────────
// STARS — deterministic positions
// ─────────────────────────────────────────
const STARS = Array.from({ length: 60 }, (_, i) => ({
  x: (i * 37 + 11) % 98,
  y: (i * 53 + 7) % 95,
  r: 0.7 + (i % 4) * 0.45,
  dur: 1.2 + (i % 7) * 0.3,
  del: i * 0.04,
}));

// ─────────────────────────────────────────
// GRAD CAP — 3-D shaded
// ─────────────────────────────────────────
function GradCap({ accent = "#f5a623", size = 80 }) {
  return (
    <svg viewBox="0 0 110 95" fill="none" style={{ width: size, height: size * 0.86 }}>
      <polygon points="55,6 104,28 55,50 6,28" fill="#f0f0f4" />
      <polygon points="6,28 10,33 55,55 55,50" fill="#c8c8d0" opacity="0.9" />
      <polygon points="104,28 100,33 55,55 55,50" fill="#d8d8e0" opacity="0.9" />
      <path d="M18,36 L18,62 Q18,77 55,77 Q92,77 92,62 L92,36 L55,50 Z" fill="#e2e2e8" />
      <path d="M18,36 L18,42 L55,56 L92,42 L92,36 L55,50 Z" fill="#c0c0cc" opacity="0.55" />
      <line x1="104" y1="28" x2="104" y2="57" stroke={accent} strokeWidth="3.5" strokeLinecap="round" />
      <circle cx="104" cy="61" r="5.5" fill={accent}
        style={{ filter: `drop-shadow(0 0 7px ${accent})` }} />
      <line x1="101" y1="65" x2="99" y2="75" stroke={accent} strokeWidth="1.5" strokeLinecap="round" opacity="0.8" />
      <line x1="104" y1="66" x2="104" y2="76" stroke={accent} strokeWidth="1.5" strokeLinecap="round" opacity="0.8" />
      <line x1="107" y1="65" x2="109" y2="75" stroke={accent} strokeWidth="1.5" strokeLinecap="round" opacity="0.8" />
    </svg>
  );
}

// ─────────────────────────────────────────
// ORBITAL INTRO LOGO
// Two rings, rotating dots, cap in centre
// ─────────────────────────────────────────
function IntroLogo() {
  return (
    <div className={s.iLogoWrap}>
      {/* Warm radial glow behind everything */}
      <div className={s.iLogoGlow} />

      {/* Outer orbit — slow clockwise, 2 dots */}
      <motion.div className={s.iOrbitOuter}
        animate={{ rotate: 360 }}
        transition={{ duration: 7, repeat: Infinity, ease: "linear" }}>
        <svg viewBox="0 0 148 148" fill="none" style={{ width: "100%", height: "100%" }}>
          <circle cx="74" cy="74" r="70"
            stroke="rgba(245,166,35,0.22)" strokeWidth="1" />
          {/* Main dot — top */}
          <circle cx="74" cy="4" r="5.5" fill="#f5a623"
            style={{ filter: "drop-shadow(0 0 6px #f5a623)" }} />
          {/* Secondary dot — right */}
          <circle cx="144" cy="74" r="3" fill="#f5a623" opacity="0.45" />
        </svg>
      </motion.div>

      {/* Inner orbit — counter-clockwise, dashed, 1 dot */}
      <motion.div className={s.iOrbitInner}
        animate={{ rotate: -360 }}
        transition={{ duration: 4.5, repeat: Infinity, ease: "linear" }}>
        <svg viewBox="0 0 108 108" fill="none" style={{ width: "100%", height: "100%" }}>
          <circle cx="54" cy="54" r="50"
            stroke="rgba(245,166,35,0.12)" strokeWidth="1"
            strokeDasharray="5 10" />
          <circle cx="54" cy="4" r="3.5" fill="#f5a623" opacity="0.7" />
        </svg>
      </motion.div>

      {/* Central disk */}
      <div className={s.iLogoDisk}>
        <motion.div
          animate={{ y: [0, -5, 0] }}
          transition={{ duration: 3.2, repeat: Infinity, ease: "easeInOut" }}>
          <GradCap accent="#f5a623" size={52} />
        </motion.div>
      </div>
    </div>
  );
}

// ─────────────────────────────────────────
// INTRO SCREEN
// Logo → typewriter name → academy → rule → shimmer
// Total: ~4 s. Skip available after 0.8 s.
// ─────────────────────────────────────────
function IntroScreen({ onDone }) {
  const [typed, setTyped] = useState("");
  const [cursorOn, setCursorOn] = useState(true);
  const [nameDone, setNameDone] = useState(false);

  const FULL = "Akhbar Elyoum";

  useEffect(() => {
    // Auto-close at 4.2 s
    const autoClose = setTimeout(onDone, 4200);

    // Cursor blink
    const blink = setInterval(() => setCursorOn(c => !c), 520);

    // Typewriter — 85 ms per char, starts at 0.38 s
    let i = 0;
    const typer = setTimeout(() => {
      const tick = setInterval(() => {
        i++;
        setTyped(FULL.slice(0, i));
        if (i >= FULL.length) {
          clearInterval(tick);
          // Name done
          setTimeout(() => {
            setNameDone(true);
            setTimeout(() => setCursorOn(false), 700);
          }, 120);
        }
      }, 85);
      return () => clearInterval(tick);
    }, 380);

    return () => {
      clearTimeout(autoClose);
      clearTimeout(typer);
      clearInterval(blink);
    };
  }, [onDone]);

  return (
    <motion.div className={s.intro}
      exit={{ opacity: 0, scale: 1.02, filter: "blur(10px)" }}
      transition={{ duration: 0.5, ease: [0.4, 0, 0.2, 1] }}>

      {/* ── BG ── */}
      <div className={s.introBg} />

      {/* ── STARFIELD ── */}
      <div className={s.iStarsLayer}>
        {STARS.map((st, i) => (
          <motion.div key={i}
            className={s.iStar}
            style={{ left: `${st.x}%`, top: `${st.y}%`, width: st.r * 2, height: st.r * 2 }}
            animate={{ opacity: [0.06, 0.75, 0.06], scale: [0.5, 1.6, 0.5] }}
            transition={{ duration: st.dur, repeat: Infinity, delay: st.del, ease: "easeInOut" }} />
        ))}
      </div>

      {/* ── CENTRAL GLOW ── */}
      <div className={s.introHalo} />

      {/* ── ORBITAL LOGO ── */}
      <motion.div
        initial={{ scale: 0.2, opacity: 0 }}
        animate={{ scale: 1, opacity: 1 }}
        transition={{ delay: 0.08, duration: 0.65, type: "spring", stiffness: 190, damping: 18 }}>
        <IntroLogo />
      </motion.div>

      {/* ── TYPEWRITER NAME ── */}
      <motion.div className={s.iTypeRow}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.3, duration: 0.3 }}>
        <span className={s.iTypedName}>{typed}</span>
        <motion.span
          className={s.iCursor}
          animate={{ opacity: cursorOn ? 1 : 0 }}
          transition={{ duration: 0 }}>
          |
        </motion.span>
      </motion.div>

      {/* ── ACADEMY + RULE + SHIMMER — appear after name done ── */}
      <AnimatePresence>
        {nameDone && (
          <>
            {/* Academy label */}
            <motion.p className={s.introAcademy}
              key="academy"
              initial={{ opacity: 0, y: 10, letterSpacing: "2px" }}
              animate={{ opacity: 1, y: 0, letterSpacing: "12px" }}
              transition={{ duration: 0.55, ease: [0.22, 1, 0.36, 1] }} />

            {/* Gold rule */}
            <motion.div className={s.introRule}
              key="rule"
              initial={{ scaleX: 0, opacity: 0 }}
              animate={{ scaleX: 1, opacity: 1 }}
              transition={{ delay: 0.28, duration: 0.5, ease: [0.22, 1, 0.36, 1] }} />

            {/* Shimmer sweep */}
            <motion.div className={s.introSweep}
              key="sweep"
              initial={{ x: "-115%" }}
              animate={{ x: "225%" }}
              transition={{ delay: 0.55, duration: 0.75, ease: "easeInOut" }} />
          </>
        )}
      </AnimatePresence>

      {/* ── SKIP ── */}
      <motion.button className={s.introSkip} onClick={onDone}
        initial={{ opacity: 0 }} animate={{ opacity: 1 }}
        transition={{ delay: 0.8 }}>
        Skip ›
      </motion.button>
    </motion.div>
  );
}

// ─────────────────────────────────────────
// CAT SVG
// ─────────────────────────────────────────
function CatSvg({ flipped }) {
  return (
    <svg viewBox="0 0 104 60" fill="none"
      style={{ width: 104, height: 60, transform: flipped ? "scaleX(-1)" : "none", display: "block" }}>
      <motion.path d="M14 36 Q1 21 7 8"
        stroke="rgba(255,255,255,0.22)" strokeWidth="5.5" strokeLinecap="round" fill="none"
        animate={{ d: ["M14 36 Q1 21 7 8", "M14 36 Q4 28 13 15", "M14 36 Q1 21 7 8"] }}
        transition={{ duration: 0.55, repeat: Infinity, ease: "easeInOut" }} />
      <ellipse cx="46" cy="31" rx="27" ry="14.5" fill="rgba(255,255,255,0.13)" />
      <circle cx="73" cy="23" r="14" fill="rgba(255,255,255,0.15)" />
      <polygon points="63,13 67,3 72,13" fill="rgba(255,255,255,0.22)" />
      <polygon points="72,13 77,3 81,13" fill="rgba(255,255,255,0.22)" />
      <polygon points="64,13 67,7 71,13" fill="rgba(255,160,175,0.3)" />
      <polygon points="73,13 77,7 80,13" fill="rgba(255,160,175,0.3)" />
      <motion.ellipse cx="68" cy="22" rx="2.7" ry="2.3"
        animate={{ ry: [2.3, 0.2, 2.3] }}
        transition={{ duration: 3.8, repeat: Infinity, repeatDelay: 1 }}
        fill="white" opacity="0.95" />
      <motion.ellipse cx="78" cy="22" rx="2.7" ry="2.3"
        animate={{ ry: [2.3, 0.2, 2.3] }}
        transition={{ duration: 3.8, repeat: Infinity, repeatDelay: 1, delay: 0.08 }}
        fill="white" opacity="0.95" />
      <ellipse cx="68" cy="22" rx="1.5" ry="1.7" fill="#0f0f20" />
      <ellipse cx="78" cy="22" rx="1.5" ry="1.7" fill="#0f0f20" />
      <ellipse cx="73" cy="26.5" rx="1.6" ry="1.05" fill="rgba(255,130,155,0.9)" />
      <line x1="58" y1="25" x2="70" y2="26" stroke="white" strokeWidth="0.7" opacity="0.38" />
      <line x1="58" y1="27" x2="70" y2="27" stroke="white" strokeWidth="0.7" opacity="0.38" />
      <line x1="76" y1="26" x2="88" y2="25" stroke="white" strokeWidth="0.7" opacity="0.38" />
      <line x1="76" y1="27" x2="88" y2="27" stroke="white" strokeWidth="0.7" opacity="0.38" />
      <motion.line x1="59" y1="43" x2="55" y2="56"
        stroke="rgba(255,255,255,0.2)" strokeWidth="5" strokeLinecap="round"
        animate={{ x2: [55, 62, 55], y2: [56, 51, 56] }}
        transition={{ duration: 0.34, repeat: Infinity }} />
      <motion.line x1="68" y1="45" x2="63" y2="56"
        stroke="rgba(255,255,255,0.2)" strokeWidth="5" strokeLinecap="round"
        animate={{ x2: [63, 57, 63], y2: [56, 51, 56] }}
        transition={{ duration: 0.34, repeat: Infinity, delay: 0.17 }} />
      <motion.line x1="33" y1="43" x2="29" y2="56"
        stroke="rgba(255,255,255,0.2)" strokeWidth="5" strokeLinecap="round"
        animate={{ x2: [29, 36, 29], y2: [56, 51, 56] }}
        transition={{ duration: 0.34, repeat: Infinity, delay: 0.17 }} />
      <motion.line x1="43" y1="45" x2="38" y2="56"
        stroke="rgba(255,255,255,0.2)" strokeWidth="5" strokeLinecap="round"
        animate={{ x2: [38, 32, 38], y2: [56, 51, 56] }}
        transition={{ duration: 0.34, repeat: Infinity }} />
    </svg>
  );
}

// ─────────────────────────────────────────
// CAT RUNNER
// ─────────────────────────────────────────
function CatRunner({ accentColor }) {
  const [dir, setDir] = useState("right");
  const [key, setKey] = useState(0);
  return (
    <div className={s.animalTrack}>
      <div className={s.trackLine}
        style={{ background: `linear-gradient(90deg,transparent,${accentColor}30,transparent)` }} />
      {Array.from({ length: 22 }).map((_, i) => (
        <div key={i} className={s.grassDot}
          style={{ left: `${(i / 21) * 93 + 3.5}%`, background: `${accentColor}22` }} />
      ))}
      <motion.div key={key} className={s.catWrap}
        initial={{ x: dir === "right" ? "-110px" : "calc(100% + 10px)" }}
        animate={{ x: dir === "right" ? "calc(100% + 10px)" : "-110px" }}
        transition={{ duration: 5.8, ease: "linear" }}
        onAnimationComplete={() => { setDir(d => d === "right" ? "left" : "right"); setKey(k => k + 1); }}>
        <motion.div
          animate={{ y: [0, -5, 0] }}
          transition={{ duration: 0.34, repeat: Infinity, ease: "easeInOut" }}>
          <CatSvg flipped={dir === "left"} />
        </motion.div>
        <motion.div className={s.catShadow}
          animate={{ scaleX: [1, 0.8, 1], opacity: [0.28, 0.13, 0.28] }}
          transition={{ duration: 0.34, repeat: Infinity }} />
      </motion.div>
    </div>
  );
}

// ─────────────────────────────────────────
// COLOR PICKER
// ─────────────────────────────────────────
function ColorPicker({ palette, current, onChange }) {
  const [open, setOpen] = useState(false);
  return (
    <div className={s.pickerWrap}>
      <motion.button className={s.pickerDot}
        style={{ background: current.dot, boxShadow: `0 0 16px ${current.glow},0 0 0 2px rgba(255,255,255,0.18)` }}
        onClick={() => setOpen(v => !v)}
        whileHover={{ scale: 1.22 }} whileTap={{ scale: 0.87 }} />
      <AnimatePresence>
        {open && (
          <motion.div className={s.pickerPanel}
            initial={{ opacity: 0, scale: 0.6, y: -4 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.6, y: -4 }}
            transition={{ duration: 0.2, ease: [0.22, 1, 0.36, 1] }}>
            {palette.map((c, i) => (
              <motion.button key={c.id}
                className={`${s.swatch} ${current.id === c.id ? s.swatchActive : ""}`}
                style={{ background: c.dot, boxShadow: current.id === c.id ? `0 0 12px ${c.glow}` : "none" }}
                onClick={() => { onChange(c); setOpen(false); }}
                initial={{ scale: 0, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ delay: i * 0.03, type: "spring", stiffness: 500, damping: 22 }}
                whileHover={{ scale: 1.38 }} whileTap={{ scale: 0.82 }} />
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}

// ─────────────────────────────────────────
// THEME SHAPES
// ─────────────────────────────────────────
function ThemeShapes({ themeId }) {
  const base = { position: "absolute", inset: 0, pointerEvents: "none", overflow: "hidden", zIndex: 0 };
  if (themeId === "ramadan") return (
    <div style={base}>
      {[8, 26, 44, 62, 80].map((x, i) => (
        <motion.div key={i} style={{ position: "absolute", left: `${x}%`, top: -8, transformOrigin: "top center" }}
          animate={{ rotate: [i % 2 ? -9 : 9, i % 2 ? 9 : -9] }}
          transition={{ duration: 2 + i * 0.3, repeat: Infinity, repeatType: "reverse", ease: "easeInOut" }}>
          <svg viewBox="0 0 28 52" fill="none" style={{ width: 14 + i * 3, opacity: 0.12 }}>
            <line x1="14" y1="0" x2="14" y2="7" stroke="white" strokeWidth="1.5" />
            <ellipse cx="14" cy="10" rx="4" ry="3" fill="white" />
            <path d="M4 15 Q1 27 4 37 Q14 43 24 37 Q27 27 24 15 Q14 9 4 15Z" fill="white" />
            <ellipse cx="14" cy="39" rx="4" ry="2.5" fill="white" />
            <line x1="14" y1="41" x2="14" y2="52" stroke="white" strokeWidth="1" />
          </svg>
        </motion.div>
      ))}
    </div>
  );
  if (themeId === "space") return (
    <div style={base}>
      {Array.from({ length: 22 }).map((_, i) => (
        <motion.div key={i} style={{
          position: "absolute", left: `${(i * 19 + 3) % 93}%`, top: `${(i * 27 + 5) % 87}%`,
          width: 3, height: 3, borderRadius: "50%", background: "rgba(255,255,255,0.6)"
        }}
          animate={{ opacity: [0.04, 0.3, 0.04], scale: [0.5, 1.6, 0.5] }}
          transition={{ duration: 1.7 + (i % 4) * 0.5, repeat: Infinity, delay: i * 0.14 }} />
      ))}
    </div>
  );
  if (themeId === "arctic" || themeId === "nordic") return (
    <div style={base}>
      {Array.from({ length: 9 }).map((_, i) => (
        <motion.div key={i} style={{
          position: "absolute", left: `${(i * 14 + 4) % 88}%`, top: "-3%",
          fontSize: 13 + (i % 3) * 8, color: "rgba(255,255,255,0.11)"
        }}
          animate={{ y: ["0vh", "110vh"], rotate: [0, 360], opacity: [0, 0.18, 0] }}
          transition={{ duration: 7 + (i % 3) * 2, repeat: Infinity, delay: i * 0.9, ease: "linear" }}>❄</motion.div>
      ))}
    </div>
  );
  if (themeId === "sakura") return (
    <div style={base}>
      {Array.from({ length: 11 }).map((_, i) => (
        <motion.div key={i} style={{ position: "absolute", left: `${(i * 12 + 5) % 88}%`, top: "-3%", fontSize: 16 }}
          animate={{ y: ["0vh", "110vh"], x: [0, i % 2 ? 38 : -38], rotate: [0, 360], opacity: [0, 0.18, 0.18, 0] }}
          transition={{ duration: 7 + (i % 3) * 2, repeat: Infinity, delay: i * 0.5, ease: "linear" }}>🌸</motion.div>
      ))}
    </div>
  );
  if (themeId === "forest") return (
    <div style={base}>
      <svg viewBox="0 0 400 200" fill="none" style={{ position: "absolute", bottom: 0, left: 0, width: "100%" }}>
        <polygon points="25,200 65,90 105,200" fill="rgba(255,255,255,0.044)" />
        <polygon points="85,200 135,68 185,200" fill="rgba(255,255,255,0.054)" />
        <polygon points="162,200 222,52 282,200" fill="rgba(255,255,255,0.044)" />
        <polygon points="245,200 305,78 365,200" fill="rgba(255,255,255,0.054)" />
      </svg>
    </div>
  );
  if (themeId === "pharaoh") return (
    <div style={base}>
      <svg viewBox="0 0 400 200" fill="none" style={{ position: "absolute", bottom: 0, left: 0, width: "100%" }}>
        <polygon points="45,200 115,52 185,200" fill="rgba(255,255,255,0.054)" />
        <polygon points="155,200 245,36 335,200" fill="rgba(255,255,255,0.064)" />
        <line x1="0" y1="180" x2="400" y2="180" stroke="rgba(255,255,255,0.045)" strokeWidth="1" />
      </svg>
    </div>
  );
  if (themeId === "ocean") return (
    <div style={base}>
      {[0, 1, 2].map(i => (
        <motion.div key={i}
          style={{
            position: "absolute", bottom: `${i * 20}%`, left: 0, right: 0, height: 38,
            background: "linear-gradient(90deg,transparent,rgba(255,255,255,0.04),transparent)", borderRadius: "50%"
          }}
          animate={{ x: [0, 50, 0, -50, 0] }}
          transition={{ duration: 5.5 + i * 1.5, repeat: Infinity, ease: "easeInOut", delay: i * 0.8 }} />
      ))}
    </div>
  );
  if (themeId === "saladin") return (
    <div style={base}>
      {[0, 1].map(i => (
        <motion.div key={i}
          style={{ position: "absolute", right: `${8 + i * 33}%`, top: `${7 + i * 28}%`, fontSize: 54, color: "rgba(255,255,255,0.038)" }}
          animate={{ rotate: [0, 360] }}
          transition={{ duration: 32 + i * 12, repeat: Infinity, ease: "linear" }}>✦</motion.div>
      ))}
    </div>
  );
  if (themeId === "fog") return (
    <div style={base}>
      {[0, 1, 2, 3].map(i => (
        <motion.div key={i}
          style={{
            position: "absolute", top: `${10 + i * 20}%`, left: "-20%", right: "-20%", height: 52,
            background: "linear-gradient(90deg,transparent,rgba(255,255,255,0.05),transparent)",
            borderRadius: "50%", filter: "blur(8px)"
          }}
          animate={{ x: [0, 65, 0, -65, 0], opacity: [0.04, 0.1, 0.04] }}
          transition={{ duration: 9 + i * 2, repeat: Infinity, ease: "easeInOut", delay: i * 1.3 }} />
      ))}
    </div>
  );
  return (
    <div style={base}>
      <svg viewBox="0 0 400 600" fill="none" style={{ position: "absolute", inset: 0, width: "100%", height: "100%" }}>
        {[{ cx: 60, cy: 100, r: 55 }, { cx: 340, cy: 260, r: 70 }, { cx: 75, cy: 440, r: 45 }, { cx: 320, cy: 510, r: 60 }].map((c, i) => (
          <circle key={i} cx={c.cx} cy={c.cy} r={c.r} stroke="rgba(255,255,255,0.028)" strokeWidth="1" />
        ))}
      </svg>
    </div>
  );
}

// ─────────────────────────────────────────
// ROLE CARD
// ─────────────────────────────────────────
function RoleCard({ role, selected, onSelect, accent }) {
  return (
    <motion.button type="button"
      className={`${s.roleCard} ${selected ? s.roleCardActive : ""}`}
      style={selected ? { borderColor: accent, boxShadow: `0 0 0 3px ${accent}2a` } : {}}
      onClick={() => onSelect(role.value)}
      whileHover={{ y: -3, scale: 1.04 }} whileTap={{ scale: 0.95 }}>
      <span className={s.roleEmoji}>{role.emoji}</span>
      <span className={s.roleLabel}>{role.label}</span>
      <AnimatePresence>
        {selected && (
          <motion.span className={s.roleCheck} style={{ background: accent }}
            initial={{ scale: 0, rotate: -90 }} animate={{ scale: 1, rotate: 0 }} exit={{ scale: 0 }}
            transition={{ type: "spring", stiffness: 430, damping: 20 }}>
            <svg viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="3">
              <polyline points="20 6 9 17 4 12" />
            </svg>
          </motion.span>
        )}
      </AnimatePresence>
    </motion.button>
  );
}

// ─────────────────────────────────────────
// MAIN PAGE
// ─────────────────────────────────────────
export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const { themeId } = useTheme();

  const [introDone, setIntroDone] = useState(false);
  const [color, setColor] = useState(
    () => PALETTE.find(c => c.id === (localStorage.getItem("login_color") || "amber")) || PALETTE[0]
  );
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState("student");
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => { localStorage.setItem("login_color", color.id); }, [color]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    if (!email.trim()) return setError("Please enter your email");
    if (!password) return setError("Please enter your password");
    setLoading(true);
    try {
      const data = await loginUser({ email: email.trim(), password, role });
      login(data.user, data.token);
      const displayName = data.user.name || data.user.displayName || "";
      toast.success("Welcome back, " + displayName + "! 🎓");
      const targetRole = (data.user.role || role).toLowerCase();
      const dashboards = {
        admin: "/admin/dashboard",
        instructor: "/instructor/dashboard",
        student: "/student/dashboard",
      };
      if (data.user.mustChangePassword && targetRole !== "admin") {
        navigate(`/${targetRole}/settings`, { replace: true });
      } else {
        navigate(dashboards[targetRole] || "/student/dashboard", { replace: true });
      }
    } catch (err) {
      setError(err.message || "Invalid credentials");
    } finally { setLoading(false); }
  };

  return (
    <div className={s.page}>

      {/* ── INTRO ── */}
      <AnimatePresence>
        {!introDone && (
          <IntroScreen key="intro" onDone={() => setIntroDone(true)} />
        )}
      </AnimatePresence>

      {/* ── MAIN LOGIN ── */}
      <AnimatePresence>
        {introDone && (
          <motion.div key="main" className={s.loginWrap}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.35 }}>

            <ColorPicker palette={PALETTE} current={color} onChange={setColor} />

            {/* ══════════════════════════════
                BRAND PANEL
                Starts at width 0, expands to 44%
                — creates the "panel split" reveal
            ══════════════════════════════ */}
            <motion.aside className={s.brandPanel}
              style={{
                background: `linear-gradient(155deg,${color.bg1} 0%,${color.bg2} 55%,${color.bg1} 100%)`,
                overflow: "hidden",
              }}
              initial={{ width: "0%" }}
              animate={{ width: "44%" }}
              transition={{ duration: 0.62, ease: [0.22, 1, 0.36, 1] }}>

              <div className={s.blobTop}
                style={{ background: `radial-gradient(circle,${color.accent}1c 0%,transparent 68%)` }} />
              <div className={s.blobBot}
                style={{ background: `radial-gradient(circle,${color.glow} 0%,transparent 68%)` }} />

              {Array.from({ length: 28 }).map((_, i) => (
                <motion.div key={i} className={s.star}
                  style={{ left: `${(i * 19 + 4) % 92}%`, top: `${(i * 27 + 6) % 88}%` }}
                  animate={{ opacity: [0.04, 0.32, 0.04], scale: [0.5, 1.5, 0.5] }}
                  transition={{ duration: 2.2 + (i % 5) * 0.7, repeat: Infinity, delay: i * 0.18 }} />
              ))}

              <div className={s.brandContent}>

                {/* Logo with dual arcs */}
                <motion.div className={s.logoWrap}
                  animate={{ y: [0, -10, 0] }}
                  transition={{ duration: 5, repeat: Infinity, ease: "easeInOut" }}
                  whileHover={{ scale: 1.07 }}>

                  <div className={s.logoGlow}
                    style={{ background: `radial-gradient(circle,${color.glow} 0%,transparent 65%)` }} />

                  {/* Arc A — slow CW */}
                  <motion.div className={s.logoArcWrap}
                    animate={{ rotate: 360 }}
                    transition={{ duration: 9, repeat: Infinity, ease: "linear" }}>
                    <svg viewBox="0 0 160 160" fill="none" style={{ width: "100%", height: "100%" }}>
                      <circle cx="80" cy="80" r="75"
                        stroke={color.accent} strokeWidth="1.8"
                        strokeDasharray="140 331" strokeLinecap="round" opacity="0.7" />
                    </svg>
                  </motion.div>

                  {/* Arc B — slower CCW */}
                  <motion.div className={s.logoArcWrap}
                    animate={{ rotate: -360 }}
                    transition={{ duration: 14, repeat: Infinity, ease: "linear" }}>
                    <svg viewBox="0 0 160 160" fill="none" style={{ width: "100%", height: "100%" }}>
                      <circle cx="80" cy="80" r="75"
                        stroke={color.accent} strokeWidth="0.9"
                        strokeDasharray="70 401" strokeLinecap="round" opacity="0.38" />
                    </svg>
                  </motion.div>

                  <div className={s.logoDisk}
                    style={{ border: `1px solid ${color.accent}30` }}>
                    <GradCap accent={color.accent} size={64} />
                  </div>
                </motion.div>

                {/* Name */}
                <motion.div className={s.nameBlock}
                  initial={{ opacity: 0, y: 22 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: 0.35, duration: 0.62, ease: [0.22, 1, 0.36, 1] }}>
                  <h1 className={s.brandName}>Akhbar Elyoum</h1>
                  <p className={s.brandSub} style={{ color: color.accent }}>
                    A &nbsp;C &nbsp;A &nbsp;D &nbsp;E &nbsp;M &nbsp;Y
                  </p>
                  <motion.div className={s.nameRule}
                    style={{ background: `linear-gradient(90deg,transparent,${color.accent},transparent)` }}
                    initial={{ scaleX: 0 }} animate={{ scaleX: 1 }}
                    transition={{ delay: 0.6, duration: 0.75 }} />
                  <p className={s.brandTagline}>Empowering minds — Building futures</p>
                </motion.div>

                {/* Feature cards */}
                <div className={s.featureList}>
                  {[
                    { icon: "📊", label: "Grades & GPA", d: 0.5 },
                    { icon: "🤖", label: "AI Study Tools", d: 0.6 },
                    { icon: "📅", label: "Timetable", d: 0.7 },
                    { icon: "📁", label: "Assignments", d: 0.8 },
                  ].map(f => (
                    <motion.div key={f.label} className={s.featureCard}
                      style={{ background: `${color.accent}0d`, border: `1px solid ${color.accent}20` }}
                      initial={{ x: -30, opacity: 0 }}
                      animate={{ x: 0, opacity: 1 }}
                      transition={{ delay: f.d, type: "spring", stiffness: 200, damping: 22 }}
                      whileHover={{
                        x: 8, background: `${color.accent}1a`, borderColor: `${color.accent}55`,
                        transition: { duration: 0.18 }
                      }}>
                      <span className={s.featureIcon}>{f.icon}</span>
                      <span className={s.featureLabel}>{f.label}</span>
                      <motion.span className={s.featureArrow} style={{ color: `${color.accent}80` }}
                        animate={{ x: [0, 5, 0] }}
                        transition={{ duration: 1.8, repeat: Infinity, delay: f.d }}>→</motion.span>
                    </motion.div>
                  ))}
                </div>
              </div>

              <CatRunner accentColor={color.accent} />
            </motion.aside>

            {/* ══════════════════════════════
                FORM PANEL
                Fades in while brand is opening
            ══════════════════════════════ */}
            <motion.main className={s.formPanel}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.12, duration: 0.45 }}>

              <ThemeShapes themeId={themeId} />

              <motion.div className={s.formCard}
                initial={{ y: 40, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ duration: 0.65, delay: 0.3, ease: [0.22, 1, 0.36, 1] }}>

                <div className={s.formHeader}>
                  <motion.span className={s.formEmoji}
                    animate={{ rotate: [0, 15, -15, 0] }}
                    transition={{ duration: 3.2, repeat: Infinity, repeatDelay: 2.8 }}>🎓</motion.span>
                  <h2 className={s.formTitle}>Welcome Back</h2>
                  <p className={s.formSub}>Sign in to your account</p>
                </div>

                <div className={s.fieldGroup}>
                  <label className={s.fieldLabel}>Your Role</label>
                  <div className={s.roleGrid}>
                    {ROLES.map(r => (
                      <RoleCard key={r.value} role={r}
                        selected={role === r.value}
                        onSelect={setRole}
                        accent={color.accent} />
                    ))}
                  </div>
                </div>

                <form onSubmit={handleSubmit} noValidate className={s.form}>
                  <div className={s.fieldGroup}>
                    <label htmlFor="email" className={s.fieldLabel}>Email Address</label>
                    <div className={s.inputWrap}>
                      <span className={s.inputIcon}>
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
                          <polyline points="22,6 12,13 2,6" />
                        </svg>
                      </span>
                      <input id="email" type="email" className={s.input}
                        style={{ "--fc": color.accent, "--fs": color.glow }}
                        placeholder="example@uni.edu" value={email}
                        onChange={e => { setEmail(e.target.value); setError(""); }}
                        autoComplete="email" />
                    </div>
                  </div>

                  <div className={s.fieldGroup}>
                    <label htmlFor="password" className={s.fieldLabel}>Password</label>
                    <div className={s.inputWrap}>
                      <span className={s.inputIcon}>
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <rect x="3" y="11" width="18" height="11" rx="2" />
                          <path d="M7 11V7a5 5 0 0110 0v4" />
                        </svg>
                      </span>
                      <input id="password" type={showPass ? "text" : "password"} className={s.input}
                        style={{ "--fc": color.accent, "--fs": color.glow }}
                        placeholder="••••••••" value={password}
                        onChange={e => { setPassword(e.target.value); setError(""); }}
                        autoComplete="current-password" />
                      <button type="button" className={s.eyeBtn} onClick={() => setShowPass(p => !p)}>
                        {showPass
                          ? <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94" />
                            <path d="M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19" />
                            <line x1="1" y1="1" x2="23" y2="23" />
                          </svg>
                          : <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                            <circle cx="12" cy="12" r="3" />
                          </svg>
                        }
                      </button>
                    </div>
                  </div>

                  <AnimatePresence>
                    {error && (
                      <motion.div className={s.errorBox}
                        initial={{ opacity: 0, y: -8, scale: 0.96 }}
                        animate={{ opacity: 1, y: 0, scale: 1 }}
                        exit={{ opacity: 0, y: -8 }} role="alert">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <circle cx="12" cy="12" r="10" />
                          <line x1="12" y1="8" x2="12" y2="12" />
                          <line x1="12" y1="16" x2="12.01" y2="16" />
                        </svg>
                        <span>{error}</span>
                      </motion.div>
                    )}
                  </AnimatePresence>

                  <motion.button type="submit" className={s.submitBtn}
                    style={{ background: `linear-gradient(135deg,${color.bg2},${color.accent})` }}
                    disabled={loading}
                    whileHover={{ scale: loading ? 1 : 1.025, boxShadow: `0 14px 44px ${color.glow}` }}
                    whileTap={{ scale: loading ? 1 : 0.975 }}>
                    {loading
                      ? <><span className={s.spinner} /><span>Signing in...</span></>
                      : <><span>Sign In</span>
                        <motion.svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5"
                          animate={{ x: [0, 5, 0] }} transition={{ duration: 1.5, repeat: Infinity }}>
                          <line x1="5" y1="12" x2="19" y2="12" />
                          <polyline points="12 5 19 12 12 19" />
                        </motion.svg>
                      </>
                    }
                  </motion.button>
                </form>

                <div className={s.devHint}>
                  <span className={s.devBadge}>DEV</span>
                  <span>
                    Use your registered account credentials
                  </span>
                </div>
                <p style={{ textAlign:"center", marginTop:"14px", fontSize:"0.8rem", color:"rgba(255,255,255,0.45)" }}>
                  Forgot your password?{" "}
                  <span
                    onClick={() => navigate("/forgot-password")}
                    style={{ color:"rgba(255,255,255,0.75)", fontWeight:600, cursor:"pointer", textDecoration:"underline" }}
                  >
                    Reset it here
                  </span>
                </p>
              </motion.div>
            </motion.main>

          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}