// src/components/common/FogAtmosphere.jsx
// 🌫️ Silent Fog — eerie mist, ghostly shapes, haunting silence
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ══════════════════════════════════════════════
   GHOST FIGURE  (wispy spirit shape)
══════════════════════════════════════════════ */
function GhostFigure({ size = 1, opacity = 0.08 }) {
  const s = size;
  return (
    <svg width={50 * s} height={72 * s} viewBox="0 0 50 72" fill="none">
      <defs>
        <radialGradient id="ghost-grad" cx="50%" cy="20%" r="70%">
          <stop offset="0%" stopColor="#b0c8c0" stopOpacity="0.9" />
          <stop offset="60%" stopColor="#8ab8b0" stopOpacity="0.6" />
          <stop offset="100%" stopColor="#607870" stopOpacity="0" />
        </radialGradient>
        <filter id="ghost-blur">
          <feGaussianBlur stdDeviation="2" result="b" />
          <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
      </defs>
      {/* Body */}
      <path d="M12,30 Q10,15 25,8 Q40,15 38,30 L40,58 Q35,68 32,60 Q29,50 25,58 Q21,66 18,58 Q15,50 12,60 Q8,68 10,58 Z"
        fill="url(#ghost-grad)" filter="url(#ghost-blur)" opacity={opacity / 0.08} />
      {/* Eyes */}
      <ellipse cx="19" cy="26" rx="3.5" ry="4" fill="#304040" opacity="0.7" />
      <ellipse cx="31" cy="26" rx="3.5" ry="4" fill="#304040" opacity="0.7" />
      {/* Eye glow */}
      <ellipse cx="19" cy="26" rx="2" ry="2.5" fill="#8ab8b0" opacity="0.6" />
      <ellipse cx="31" cy="26" rx="2" ry="2.5" fill="#8ab8b0" opacity="0.6" />
      {/* Mouth — silent O */}
      <ellipse cx="25" cy="34" rx="3" ry="2" fill="#304040" opacity="0.5" />
      {/* Arms wisp */}
      <path d="M12,38 Q4,34 2,38 Q4,42 12,42" fill="#8ab8b0" opacity="0.4" filter="url(#ghost-blur)" />
      <path d="M38,38 Q46,34 48,38 Q46,42 38,42" fill="#8ab8b0" opacity="0.4" filter="url(#ghost-blur)" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   HAUNTED TREE  (bare branches silhouette)
══════════════════════════════════════════════ */
function HauntedTree({ flip = false }) {
  const style = flip
    ? { transform: "scaleX(-1)", transformOrigin: "50% 100%" }
    : {};
  return (
    <svg width="160" height="280" viewBox="0 0 160 280" fill="none" style={style}>
      {/* Trunk */}
      <path d="M75,280 Q72,240 74,200 Q73,170 70,150" stroke="#243038" strokeWidth="8"
        strokeLinecap="round" fill="none" />
      {/* Main branches */}
      <path d="M70,150 Q50,120 30,110" stroke="#243038" strokeWidth="5" strokeLinecap="round" fill="none" />
      <path d="M70,155 Q90,125 120,115" stroke="#243038" strokeWidth="5" strokeLinecap="round" fill="none" />
      <path d="M70,165 Q55,140 40,130" stroke="#243038" strokeWidth="4" strokeLinecap="round" fill="none" />
      <path d="M70,160 Q85,140 105,132" stroke="#243038" strokeWidth="4" strokeLinecap="round" fill="none" />
      {/* Sub branches */}
      <path d="M30,110 Q18,95 10,85 M30,110 Q25,92 20,85 M30,110 Q38,94 42,84"
        stroke="#1e2c34" strokeWidth="2.5" strokeLinecap="round" fill="none" />
      <path d="M120,115 Q132,100 140,90 M120,115 Q125,96 128,86 M120,115 Q112,98 110,86"
        stroke="#1e2c34" strokeWidth="2.5" strokeLinecap="round" fill="none" />
      <path d="M40,130 Q28,115 22,105 M40,130 Q44,112 44,102"
        stroke="#1e2c34" strokeWidth="2" strokeLinecap="round" fill="none" />
      <path d="M105,132 Q116,116 122,106 M105,132 Q100,114 100,104"
        stroke="#1e2c34" strokeWidth="2" strokeLinecap="round" fill="none" />
      {/* Tiny twigs */}
      <path d="M10,85 Q4,76 2,70 M10,85 Q14,74 16,70"
        stroke="#182430" strokeWidth="1.5" strokeLinecap="round" fill="none" />
      <path d="M140,90 Q148,80 150,74 M140,90 Q136,79 134,74"
        stroke="#182430" strokeWidth="1.5" strokeLinecap="round" fill="none" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   FOG DRIP  (top icicle-like dripping fog)
══════════════════════════════════════════════ */
function FogDrip() {
  return (
    <svg style={{ position: "absolute", top: 0, left: 0, width: "100%", opacity: 0.1 }}
      viewBox="0 0 1440 60" preserveAspectRatio="none">
      <defs>
        <linearGradient id="drip-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#8ab8b0" stopOpacity="0.9" />
          <stop offset="100%" stopColor="#607870" stopOpacity="0" />
        </linearGradient>
      </defs>
      {/* Top fog layer */}
      <rect x="0" y="0" width="1440" height="20" fill="url(#drip-grad)" />
      {/* Drips */}
      {Array.from({ length: 28 }, (_, i) => {
        const x = i * 51 + 26;
        const h = 18 + (i % 7) * 7;
        const r = 2 + (i % 3);
        return (
          <g key={i}>
            <path d={`M${x - 4},18 Q${x},${h + 8} ${x},${h + 14} Q${x},${h + 20} ${x + 4},18`}
              fill="url(#drip-grad)" />
            <circle cx={x} cy={h + 16} r={r} fill="#8ab8b0" opacity="0.4" />
          </g>
        );
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   WILL-O'-WISPS  (floating orbs of light)
══════════════════════════════════════════════ */
const WISPS_CFG = [
  { x: "12%",  y: "35%", size: 90,  color: "#8ab8b0", dur: 8,  delay: 0,   opacity: 0.07  },
  { x: "74%",  y: "28%", size: 70,  color: "#607870", dur: 11, delay: 2.5, opacity: 0.06  },
  { x: "42%",  y: "62%", size: 80,  color: "#8ab8b0", dur: 9,  delay: 5,   opacity: 0.06  },
  { x: "88%",  y: "55%", size: 55,  color: "#b0c8c0", dur: 14, delay: 1.5, opacity: 0.05  },
  { x: "5%",   y: "72%", size: 62,  color: "#6a9090", dur: 10, delay: 3.5, opacity: 0.05  },
  { x: "55%",  y: "18%", size: 45,  color: "#8ab8b0", dur: 7,  delay: 4,   opacity: 0.04  },
];

/* ══════════════════════════════════════════════
   FOG BANDS  (horizontal drifting wisps)
══════════════════════════════════════════════ */
const FOG_BANDS = [
  { top: "22%", op: 0.045, dur: 28, delay: 0   },
  { top: "48%", op: 0.035, dur: 36, delay: 9   },
  { top: "70%", op: 0.055, dur: 22, delay: 16  },
  { top: "36%", op: 0.030, dur: 40, delay: 4   },
];

/* ══════════════════════════════════════════════
   GHOST CONFIGS
══════════════════════════════════════════════ */
const GHOSTS = [
  { left: "3%",  top: "28%", size: 1.1, delay: 0.8,  opacity: 0.10 },
  { left: "88%", top: "24%", size: 0.9, delay: 1.4,  opacity: 0.08 },
  { left: "46%", top: "10%", size: 0.75, delay: 2.2, opacity: 0.07 },
];

/* ══════════════════════════════════════════════
   DIM FIREFLIES  (tiny flickering lights)
══════════════════════════════════════════════ */
const FIREFLIES = Array.from({ length: 25 }, (_, i) => ({
  id: i,
  left: `${((i * 173.3) % 94).toFixed(1)}%`,
  top: `${((i * 111.7) % 78).toFixed(1)}%`,
  dur: 2.8 + (i % 6) * 1.1,
  delay: (i * 0.65) % 7,
  color: i % 3 === 0 ? "#8ab8b0" : i % 3 === 1 ? "#607870" : "#b0c8c0",
}));

/* ════════════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════════════ */
export default function FogAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "fog";

  return (
    <AnimatePresence>
      {active && (
        <motion.div
          style={{ position: "fixed", inset: 0, pointerEvents: "none", zIndex: 9998, overflow: "hidden" }}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          transition={{ duration: 3 }}
        >
          {/* ── Eerie background glow ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 90% 65% at 50% 0%, rgba(80,130,120,0.05), transparent 70%)",
          }} />

          {/* ── Fog drip from ceiling ── */}
          <FogDrip />

          {/* ── Haunted trees (sides) ── */}
          <motion.div style={{ position: "absolute", left: -10, bottom: 0, opacity: 0.12 }}
            animate={{ x: [0, -4, 0], skewX: [0, 1, 0] }}
            transition={{ duration: 12, repeat: Infinity, ease: "easeInOut" }}
          >
            <HauntedTree />
          </motion.div>
          <motion.div style={{ position: "absolute", right: -10, bottom: 0, opacity: 0.10 }}
            animate={{ x: [0, 4, 0], skewX: [0, -1, 0] }}
            transition={{ duration: 14, repeat: Infinity, ease: "easeInOut", delay: 2 }}
          >
            <HauntedTree flip />
          </motion.div>

          {/* ── Ghost figures ── */}
          {GHOSTS.map((g, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: g.left, top: g.top,
              opacity: g.opacity,
              filter: "blur(0.5px)",
            }}
              initial={{ opacity: 0, y: 20 }}
              animate={{
                opacity: [g.opacity * 0.3, g.opacity, g.opacity * 0.5, g.opacity, g.opacity * 0.3],
                y: [0, -18, 0, -12, 0],
                x: [0, 8, -5, 4, 0],
              }}
              transition={{ duration: 9, delay: g.delay, repeat: Infinity, ease: "easeInOut" }}
            >
              <GhostFigure size={g.size} opacity={g.opacity} />
            </motion.div>
          ))}

          {/* ── Horizontal fog bands ── */}
          {FOG_BANDS.map((b, i) => (
            <motion.div key={i} style={{
              position: "absolute", top: b.top,
              left: "-130%", width: "160%", height: 90,
              background: `linear-gradient(90deg, transparent 0%, rgba(138,184,176,${b.op}) 25%, rgba(176,200,192,${b.op * 1.3}) 50%, rgba(138,184,176,${b.op}) 75%, transparent 100%)`,
              filter: "blur(20px)",
            }}
              animate={{ x: ["0%", "170%"] }}
              transition={{ duration: b.dur, delay: b.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Will-o'-wisps ── */}
          {WISPS_CFG.map((w, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: w.x, top: w.y,
              width: w.size, height: w.size, borderRadius: "50%",
              background: `radial-gradient(circle, ${w.color}22, transparent 70%)`,
              filter: "blur(14px)",
            }}
              animate={{
                x: [-18, 18, -10, 14, -18],
                y: [-12, 10, -16, 6, -12],
                opacity: [w.opacity * 0.4, w.opacity, w.opacity * 0.5, w.opacity * 0.9, w.opacity * 0.4],
                scale: [0.85, 1.15, 0.9, 1.1, 0.85],
              }}
              transition={{ duration: w.dur, delay: w.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Dim fireflies / ghost lights ── */}
          {FIREFLIES.map((f) => (
            <motion.div key={f.id} style={{
              position: "absolute", left: f.left, top: f.top,
              width: 2.5, height: 2.5, borderRadius: "50%",
              background: f.color,
              filter: "blur(0.5px)",
            }}
              animate={{
                opacity: [0, 0.5, 0.1, 0.7, 0],
                x: [0, 8, -5, 6, 0],
                y: [0, -6, 3, -4, 0],
                scale: [0.8, 1.4, 0.9, 1.2, 0.8],
              }}
              transition={{ duration: f.dur, delay: f.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Vertical mist columns ── */}
          {[0, 1, 2, 3, 4].map((i) => (
            <motion.div key={i} style={{
              position: "absolute",
              left: `${8 + i * 20}%`, top: 0, bottom: 0, width: 90,
              background: "linear-gradient(180deg, transparent 0%, rgba(138,184,176,0.03) 50%, transparent 100%)",
              filter: "blur(22px)",
            }}
              animate={{ opacity: [0, 0.9, 0.2, 0.7, 0], scaleY: [0.8, 1, 0.85, 1.05, 0.8] }}
              transition={{ duration: 14, delay: i * 2.5, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Heavy fog top ── */}
          <div style={{
            position: "absolute", top: 0, left: 0, right: 0, height: 180,
            background: "linear-gradient(to bottom, rgba(8,14,16,0.4), transparent)",
          }} />

          {/* ── Heavy fog bottom ── */}
          <div style={{
            position: "absolute", bottom: 0, left: 0, right: 0, height: 200,
            background: "linear-gradient(to top, rgba(10,14,16,0.55), transparent)",
          }} />

          {/* ── Dark vignette edges ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 100% 100% at 50% 50%, transparent 45%, rgba(10,14,16,0.35) 100%)",
          }} />

          {/* ── "Silent Fog" label ── */}
          <motion.div style={{
            position: "absolute", right: 16, bottom: 90,
            fontSize: 10, fontWeight: 800, letterSpacing: 3,
            color: "rgba(138,184,176,0.22)", textTransform: "uppercase",
            writingMode: "vertical-rl", fontFamily: "'Sora', sans-serif",
          }}
            animate={{ opacity: [0.14, 0.28, 0.14] }}
            transition={{ duration: 6, repeat: Infinity }}
          >
            Silent Fog
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}