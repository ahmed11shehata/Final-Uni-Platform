// src/components/common/PharaohAtmosphere.jsx
// 🏺 Pharaoh's Egypt Atmosphere
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ══════════════════════════════════════════════
   EYE OF RA  (detailed SVG — العين الفرعونية)
══════════════════════════════════════════════ */
function EyeOfRa({ size = 1 }) {
  const s = size;
  return (
    <motion.svg width={90 * s} height={54 * s} viewBox="0 0 90 54" fill="none">
      <defs>
        <filter id="ra-glow">
          <feGaussianBlur stdDeviation="2.5" result="b" />
          <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
        <radialGradient id="ra-eye" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="#ffd700" stopOpacity="0.9" />
          <stop offset="100%" stopColor="#c88820" stopOpacity="1" />
        </radialGradient>
      </defs>
      {/* Eye outline */}
      <path d="M8,27 Q20,8 45,8 Q70,8 82,27 Q70,46 45,46 Q20,46 8,27 Z"
        stroke="#d4a817" strokeWidth="2" fill="none" filter="url(#ra-glow)" />
      {/* Iris */}
      <circle cx="45" cy="27" r="11" fill="url(#ra-eye)" filter="url(#ra-glow)" />
      {/* Pupil */}
      <circle cx="45" cy="27" r="5" fill="#1a0800" />
      <circle cx="43" cy="25" r="2" fill="white" opacity="0.5" />
      {/* Inner shine */}
      <circle cx="45" cy="27" r="8" fill="none" stroke="#ffd700" strokeWidth="0.8" opacity="0.4" />
      {/* Kohl line left */}
      <path d="M8,27 L2,27 M8,27 L4,18 M8,27 L4,36"
        stroke="#d4a817" strokeWidth="1.5" strokeLinecap="round" />
      {/* Kohl line right + tail of Ra */}
      <path d="M82,27 Q88,22 88,18 Q85,14 80,16 Q78,20 80,22"
        stroke="#d4a817" strokeWidth="1.8" fill="none" strokeLinecap="round" />
      {/* Tears/marks below eye */}
      <path d="M38,45 Q36,50 34,52" stroke="#c88820" strokeWidth="1.2" strokeLinecap="round" />
      <path d="M45,46 Q45,51 43,53" stroke="#c88820" strokeWidth="1.2" strokeLinecap="round" />
      {/* Eyelashes top */}
      {[20, 28, 36, 45, 54, 62, 70].map((x, i) => (
        <line key={i} x1={x} y1={15 - (i === 3 ? 4 : 2)} x2={x} y2={10 - (i === 3 ? 4 : 2)}
          stroke="#d4a817" strokeWidth="1" strokeLinecap="round" opacity="0.7" />
      ))}
    </motion.svg>
  );
}

/* ══════════════════════════════════════════════
   SCARAB  (Sacred beetle — الخنفساء المقدسة)
══════════════════════════════════════════════ */
function Scarab({ size = 1 }) {
  const s = size;
  return (
    <svg width={44 * s} height={52 * s} viewBox="0 0 44 52" fill="none">
      <defs>
        <radialGradient id="scarab-body" cx="40%" cy="30%" r="65%">
          <stop offset="0%" stopColor="#4a9060" />
          <stop offset="60%" stopColor="#2a6040" />
          <stop offset="100%" stopColor="#1a4028" />
        </radialGradient>
        <filter id="scarab-glow">
          <feGaussianBlur stdDeviation="1" result="b" />
          <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
      </defs>
      {/* Wings open */}
      <path d="M22,16 Q8,6 4,12 Q2,22 8,28 Q14,32 22,30 Z"
        fill="url(#scarab-body)" stroke="#3a7050" strokeWidth="0.8" filter="url(#scarab-glow)" />
      <path d="M22,16 Q36,6 40,12 Q42,22 36,28 Q30,32 22,30 Z"
        fill="url(#scarab-body)" stroke="#3a7050" strokeWidth="0.8" filter="url(#scarab-glow)" />
      {/* Wing veins */}
      <path d="M22,18 Q12,12 8,18" stroke="#4aaa70" strokeWidth="0.6" fill="none" opacity="0.6" />
      <path d="M22,18 Q32,12 36,18" stroke="#4aaa70" strokeWidth="0.6" fill="none" opacity="0.6" />
      <path d="M22,24 Q14,20 10,24" stroke="#4aaa70" strokeWidth="0.5" fill="none" opacity="0.5" />
      <path d="M22,24 Q30,20 34,24" stroke="#4aaa70" strokeWidth="0.5" fill="none" opacity="0.5" />
      {/* Body/thorax */}
      <ellipse cx="22" cy="32" rx="9" ry="12" fill="url(#scarab-body)"
        stroke="#3a7050" strokeWidth="0.8" />
      {/* Head */}
      <ellipse cx="22" cy="18" rx="7" ry="5" fill="#2a6040" stroke="#3a7050" strokeWidth="0.8" />
      {/* Solar disc on head */}
      <circle cx="22" cy="17" r="3" fill="#d4a817" opacity="0.7" />
      {/* Legs */}
      {[-1, 0, 1].map((side, i) => (
        <g key={i}>
          <line x1={22 - 9} y1={25 + i * 5} x2={22 - 18} y2={22 + i * 5}
            stroke="#3a7050" strokeWidth="1.2" strokeLinecap="round" />
          <line x1={22 + 9} y1={25 + i * 5} x2={22 + 18} y2={22 + i * 5}
            stroke="#3a7050" strokeWidth="1.2" strokeLinecap="round" />
        </g>
      ))}
      {/* Antennae */}
      <path d="M18,14 Q12,8 10,5" stroke="#3a7050" strokeWidth="1" fill="none" strokeLinecap="round" />
      <path d="M26,14 Q32,8 34,5" stroke="#3a7050" strokeWidth="1" fill="none" strokeLinecap="round" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   PYRAMID SILHOUETTE  (bottom skyline)
══════════════════════════════════════════════ */
function Pyramids() {
  return (
    <svg style={{ position: "absolute", bottom: 0, left: 0, width: "100%", opacity: 0.1 }}
      viewBox="0 0 1440 220" preserveAspectRatio="none">
      <defs>
        <linearGradient id="pyr-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#c88820" stopOpacity="0.8" />
          <stop offset="100%" stopColor="#8a5010" stopOpacity="1" />
        </linearGradient>
      </defs>
      {/* Great Pyramid */}
      <polygon points="260,0 520,220 0,220" fill="url(#pyr-grad)" />
      {/* Shadow face */}
      <polygon points="260,0 520,220 390,220" fill="#6a3808" opacity="0.5" />
      {/* Medium pyramid */}
      <polygon points="680,55 860,220 500,220" fill="url(#pyr-grad)" />
      <polygon points="680,55 860,220 770,220" fill="#6a3808" opacity="0.4" />
      {/* Small pyramid */}
      <polygon points="990,90 1110,220 870,220" fill="url(#pyr-grad)" />
      <polygon points="990,90 1110,220 1050,220" fill="#6a3808" opacity="0.35" />
      {/* Sphinx silhouette */}
      <path d="M1200,220 L1200,155 Q1220,130 1255,140 L1290,148 L1330,148 Q1360,130 1375,148 L1390,165 L1390,220Z"
        fill="#a86010" opacity="0.7" />
      <circle cx="1252" cy="135" r="18" fill="#a86010" opacity="0.7" />
      {/* Nile hint */}
      <path d="M0,220 Q360,215 720,218 Q1080,221 1440,216 L1440,220 Z"
        fill="#1a3860" opacity="0.3" />
      {/* Capstone glow */}
      <circle cx="260" cy="4" r="8" fill="#ffd700" opacity="0.3" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   HIEROGLYPH BORDER  (top decorative band)
══════════════════════════════════════════════ */
function HieroglyphBorder() {
  return (
    <svg style={{ position: "absolute", top: 0, left: 0, width: "100%", opacity: 0.13 }}
      viewBox="0 0 1440 36" preserveAspectRatio="none">
      <defs>
        <linearGradient id="hiero-grad" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0%" stopColor="#d4a817" stopOpacity="0" />
          <stop offset="10%" stopColor="#d4a817" stopOpacity="1" />
          <stop offset="90%" stopColor="#d4a817" stopOpacity="1" />
          <stop offset="100%" stopColor="#d4a817" stopOpacity="0" />
        </linearGradient>
      </defs>
      {/* Horizontal frame lines */}
      <rect x="0" y="0" width="1440" height="2" fill="url(#hiero-grad)" opacity="0.8" />
      <rect x="0" y="34" width="1440" height="2" fill="url(#hiero-grad)" opacity="0.8" />
      {/* Repeating cartouche-like cells */}
      {Array.from({ length: 36 }, (_, i) => {
        const x = i * 40 + 5;
        return (
          <g key={i}>
            {/* Cartouche oval */}
            <rect x={x} y="5" width="28" height="26" rx="10" ry="6"
              fill="none" stroke="#d4a817" strokeWidth="0.8" opacity="0.7" />
            {/* Inner glyph shapes — alternating */}
            {i % 4 === 0 && <circle cx={x + 14} cy={18} r="4" fill="#d4a817" opacity="0.5" />}
            {i % 4 === 1 && <path d={`M${x + 7},22 L${x + 14},10 L${x + 21},22`}
              fill="#d4a817" opacity="0.5" />}
            {i % 4 === 2 && <rect x={x + 9} y={12} width="10" height="12" rx="1"
              fill="#d4a817" opacity="0.4" />}
            {i % 4 === 3 && <path d={`M${x + 14},10 Q${x + 22},14 ${x + 22},20 Q${x + 14},26 ${x + 6},20 Q${x + 6},14 ${x + 14},10Z`}
              fill="#d4a817" opacity="0.35" />}
          </g>
        );
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   RA SUN  (animated sun disc)
══════════════════════════════════════════════ */
function RaSun() {
  return (
    <motion.div
      style={{
        position: "absolute", top: 18, left: "50%", transform: "translateX(-50%)",
        filter: "drop-shadow(0 0 28px rgba(212,168,23,0.5))",
      }}
      animate={{ y: [0, -8, 0] }}
      transition={{ duration: 8, repeat: Infinity, ease: "easeInOut" }}
    >
      <svg width="88" height="88" viewBox="0 0 88 88" fill="none">
        <defs>
          <radialGradient id="sun-grad" cx="50%" cy="50%" r="50%">
            <stop offset="0%" stopColor="#fff0a0" />
            <stop offset="50%" stopColor="#f0c030" />
            <stop offset="100%" stopColor="#c88020" />
          </radialGradient>
        </defs>
        {/* Sun rays */}
        {Array.from({ length: 16 }, (_, i) => {
          const a = (i * 22.5 * Math.PI) / 180;
          const r1 = 28, r2 = i % 2 === 0 ? 40 : 35;
          return (
            <line key={i}
              x1={44 + r1 * Math.cos(a)} y1={44 + r1 * Math.sin(a)}
              x2={44 + r2 * Math.cos(a)} y2={44 + r2 * Math.sin(a)}
              stroke="#d4a817" strokeWidth={i % 2 === 0 ? 2 : 1.2} strokeLinecap="round"
            />
          );
        })}
        {/* Main disc */}
        <circle cx="44" cy="44" r="22" fill="url(#sun-grad)" />
        <circle cx="44" cy="44" r="18" fill="none" stroke="#ffd700" strokeWidth="1.2" opacity="0.6" />
        {/* Eye of Ra within sun */}
        <ellipse cx="44" cy="44" rx="8" ry="5" fill="none" stroke="#8a4800" strokeWidth="1" />
        <circle cx="44" cy="44" r="3.5" fill="#6a3000" />
        <circle cx="43" cy="43" r="1.2" fill="white" opacity="0.6" />
        {/* Uraeus serpent coil hint */}
        <path d="M44,22 Q50,18 52,22 Q54,26 50,28" stroke="#d4a817" strokeWidth="1.5"
          fill="none" strokeLinecap="round" opacity="0.6" />
      </svg>
    </motion.div>
  );
}

/* ══════════════════════════════════════════════
   SAND PARTICLES
══════════════════════════════════════════════ */
const SAND = Array.from({ length: 55 }, (_, i) => ({
  id: i,
  left: `${((i * 172.5) % 100).toFixed(1)}%`,
  size: 1.5 + (i % 5) * 1.3,
  dur: 9 + (i % 7) * 3.5,
  delay: (i * 0.36) % 10,
  color: [("#d4a817"), ("#c07820"), ("#e8c060"), ("#a86010"), ("#f0d070")][i % 5],
  opacity: 0.15 + (i % 6) * 0.09,
}));

/* ══════════════════════════════════════════════
   SCARAB CONFIGS
══════════════════════════════════════════════ */
const SCARABS = [
  { left: "4%",  top: "25%", size: 0.9, delay: 1.0, spin:  12 },
  { left: "91%", top: "22%", size: 0.8, delay: 1.5, spin: -10 },
  { left: "48%", top: "8%",  size: 0.7, delay: 2.0, spin:   8 },
];

/* ════════════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════════════ */
export default function PharaohAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "pharaoh";

  return (
    <AnimatePresence>
      {active && (
        <motion.div
          style={{ position: "fixed", inset: 0, pointerEvents: "none", zIndex: 9998, overflow: "hidden" }}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          transition={{ duration: 2 }}
        >
          {/* ── Desert glow sky ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 80% 50% at 50% 0%, rgba(212,168,23,0.07), transparent 65%)",
          }} />

          {/* ── Hieroglyph border ── */}
          <HieroglyphBorder />

          {/* ── Ra Sun ── */}
          <RaSun />

          {/* ── Eye of Ra (left + right) ── */}
          {[
            { left: "2%", top: "12%", size: 1.1, delay: 0.8 },
            { left: "84%", top: "10%", size: 0.9, delay: 1.2 },
          ].map((e, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: e.left, top: e.top,
              filter: "drop-shadow(0 0 14px rgba(212,168,23,0.5))",
            }}
              initial={{ opacity: 0 }}
              animate={{ opacity: [0.4, 0.85, 0.4] }}
              transition={{ duration: 4, delay: e.delay, repeat: Infinity, ease: "easeInOut" }}
            >
              <EyeOfRa size={e.size} />
            </motion.div>
          ))}

          {/* ── Flying scarabs ── */}
          {SCARABS.map((sc, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: sc.left, top: sc.top,
              filter: "drop-shadow(0 0 8px rgba(74,144,96,0.6))",
            }}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: [0.5, 0.9, 0.5], y: [0, -12, 0] }}
              transition={{ duration: 6, delay: sc.delay, repeat: Infinity, ease: "easeInOut" }}
            >
              <motion.div
                animate={{ rotate: [0, sc.spin, 0, -sc.spin * 0.5, 0] }}
                transition={{ duration: 5, repeat: Infinity, ease: "easeInOut" }}
              >
                <Scarab size={sc.size} />
              </motion.div>
            </motion.div>
          ))}

          {/* ── Sand particles ── */}
          {SAND.map((s) => (
            <motion.div key={s.id} style={{
              position: "absolute", left: s.left, top: "-8px",
              width: s.size, height: s.size * 0.55, borderRadius: "50%",
              background: s.color, opacity: s.opacity,
            }}
              animate={{
                y: ["0vh", "112vh"],
                x: [0, 65, -40, 30, 0],
                opacity: [0, s.opacity, s.opacity, 0],
              }}
              transition={{
                duration: s.dur, delay: s.delay, repeat: Infinity,
                ease: "easeIn", times: [0, 0.12, 0.88, 1],
              }}
            />
          ))}

          {/* ── Torches on sides ── */}
          {[{ left: "1%", top: "55%" }, { left: "97%", top: "52%" }].map((t, i) => (
            <div key={i} style={{ position: "absolute", left: t.left, top: t.top }}>
              {/* Flame */}
              <motion.div style={{
                width: 10, height: 22, marginLeft: 2,
                background: "radial-gradient(ellipse at 50% 85%, #f59e0b, #ef4444 50%, transparent)",
                borderRadius: "50% 50% 30% 30%",
                filter: "blur(2.5px)",
              }}
                animate={{ scaleX: [1, 1.35, 0.8, 1.2, 1], scaleY: [1, 0.88, 1.2, 0.95, 1], opacity: [0.7, 1, 0.8, 1, 0.7] }}
                transition={{ duration: 2, repeat: Infinity, ease: "easeInOut" }}
              />
              {/* Glow */}
              <div style={{
                position: "absolute", top: -8, left: -6, width: 28, height: 28, borderRadius: "50%",
                background: "radial-gradient(circle, rgba(245,158,11,0.35), transparent)",
                filter: "blur(5px)",
              }} />
              {/* Pole */}
              <div style={{ width: 4, height: 50, background: "linear-gradient(to bottom, #7a4010, #5a2808)", borderRadius: 2, margin: "0 auto" }} />
              {/* Base */}
              <div style={{ width: 14, height: 6, background: "#6a3808", borderRadius: 2, margin: "0 auto" }} />
            </div>
          ))}

          {/* ── Pyramid silhouette ── */}
          <Pyramids />

          {/* ── Side desert warmth ── */}
          <div style={{
            position: "absolute", left: 0, top: 0, bottom: 0, width: 120,
            background: "linear-gradient(90deg, rgba(200,120,20,0.06), transparent)",
          }} />
          <div style={{
            position: "absolute", right: 0, top: 0, bottom: 0, width: 120,
            background: "linear-gradient(-90deg, rgba(200,120,20,0.06), transparent)",
          }} />

          {/* ── Bottom fade ── */}
          <div style={{
            position: "absolute", bottom: 0, left: 0, right: 0, height: "20%",
            background: "linear-gradient(to top, #150900 0%, transparent 100%)",
          }} />

          {/* ── "Pharaoh's Egypt" label ── */}
          <motion.div style={{
            position: "absolute", right: 16, bottom: 90,
            fontSize: 10, fontWeight: 800, letterSpacing: 3,
            color: "rgba(212,168,23,0.28)", textTransform: "uppercase",
            writingMode: "vertical-rl", fontFamily: "'Sora', sans-serif",
          }}
            animate={{ opacity: [0.18, 0.38, 0.18] }}
            transition={{ duration: 5.5, repeat: Infinity }}
          >
            Pharaoh&apos;s Egypt
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}