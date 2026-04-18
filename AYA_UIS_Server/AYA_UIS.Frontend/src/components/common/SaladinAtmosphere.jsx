// src/components/common/SaladinAtmosphere.jsx
// ⚔️ Saladin's Legacy Atmosphere
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ══════════════════════════════════════════════
   ISLAMIC 8-POINTED STAR  (نجمة إسلامية)
══════════════════════════════════════════════ */
function IslamicStar({ size = 50, color = "#e8a040", opacity = 0.3 }) {
  const points8 = Array.from({ length: 8 }, (_, i) => {
    const a = (i * 45 * Math.PI) / 180;
    const r = i % 2 === 0 ? size / 2 * 0.95 : size / 2 * 0.5;
    return `${size / 2 + r * Math.cos(a - Math.PI / 2)},${size / 2 + r * Math.sin(a - Math.PI / 2)}`;
  }).join(" ");

  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} fill="none">
      <defs>
        <filter id={`star-glow-${size}`}>
          <feGaussianBlur stdDeviation="1.5" result="b" />
          <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
        <radialGradient id={`star-fill-${size}`} cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor={color} stopOpacity="0.8" />
          <stop offset="100%" stopColor={color} stopOpacity="0.3" />
        </radialGradient>
      </defs>
      {/* Outer 8-point star */}
      <polygon points={points8} fill={`url(#star-fill-${size})`}
        stroke={color} strokeWidth="1" opacity={opacity}
        filter={`url(#star-glow-${size})`} />
      {/* Inner square rotated */}
      <rect x={size * 0.32} y={size * 0.32} width={size * 0.36} height={size * 0.36}
        transform={`rotate(45 ${size / 2} ${size / 2})`}
        fill="none" stroke={color} strokeWidth="0.8" opacity={opacity * 0.7} />
      {/* Center circle */}
      <circle cx={size / 2} cy={size / 2} r={size * 0.1}
        fill={color} opacity={opacity * 0.9} />
      {/* Tiny detail lines */}
      {Array.from({ length: 8 }, (_, i) => {
        const a = (i * 45 * Math.PI) / 180 - Math.PI / 2;
        const r1 = size * 0.14, r2 = size * 0.22;
        return (
          <line key={i}
            x1={size / 2 + r1 * Math.cos(a)} y1={size / 2 + r1 * Math.sin(a)}
            x2={size / 2 + r2 * Math.cos(a)} y2={size / 2 + r2 * Math.sin(a)}
            stroke={color} strokeWidth="0.7" opacity={opacity * 0.6} strokeLinecap="round"
          />
        );
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   CRESCENT + STAR  (هلال ونجمة)
══════════════════════════════════════════════ */
function CrescentAndStar({ size = 1 }) {
  const s = size;
  return (
    <motion.div
      style={{ position: "relative", display: "inline-block" }}
      animate={{ y: [0, -9, 0] }}
      transition={{ duration: 8, repeat: Infinity, ease: "easeInOut" }}
    >
      <svg width={80 * s} height={80 * s} viewBox="0 0 80 80" fill="none">
        <defs>
          <filter id="crescent-glow">
            <feGaussianBlur stdDeviation="3" result="b" />
            <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
          </filter>
          <radialGradient id="crescent-grad" cx="30%" cy="30%" r="70%">
            <stop offset="0%" stopColor="#ffd080" />
            <stop offset="100%" stopColor="#c07820" />
          </radialGradient>
        </defs>
        {/* Crescent */}
        <path d="M48 14 A28 28 0 1 0 48 66 A20 20 0 1 1 48 14 Z"
          fill="url(#crescent-grad)" filter="url(#crescent-glow)" />
        {/* Star */}
        <motion.polygon
          points="65,8 67.5,16 76,16 69,21 71.5,29 65,23.5 58.5,29 61,21 54,16 62.5,16"
          fill="#ffd700"
          animate={{ opacity: [0.7, 1, 0.7], scale: [0.9, 1.08, 0.9] }}
          transition={{ duration: 2.5, repeat: Infinity }}
        />
        <circle cx="65" cy="18.5" r="2.5" fill="white" opacity="0.4" />
      </svg>
    </motion.div>
  );
}

/* ══════════════════════════════════════════════
   MINARET SKYLINE  (bottom silhouette)
══════════════════════════════════════════════ */
function MinaretSkyline() {
  return (
    <svg style={{ position: "absolute", bottom: 0, left: 0, width: "100%", opacity: 0.1 }}
      viewBox="0 0 1440 270" preserveAspectRatio="none">
      <defs>
        <linearGradient id="min-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#c07820" stopOpacity="0.9" />
          <stop offset="100%" stopColor="#8a4810" stopOpacity="1" />
        </linearGradient>
      </defs>

      {/* Left minaret */}
      <rect x="65" y="70" width="36" height="200" fill="url(#min-grad)" />
      <path d="M65,70 Q83,28 101,70Z" fill="#c07820" />
      <ellipse cx="83" cy="70" rx="20" ry="8" fill="#d4a030" opacity="0.8" />
      <rect x="74" y="54" width="18" height="20" rx="9" fill="#c07820" />
      {/* Arch windows */}
      <path d="M70,110 Q83,96 96,110 L96,135 L70,135Z" fill="#7a4010" opacity="0.8" />
      <path d="M70,155 Q83,141 96,155 L96,180 L70,180Z" fill="#7a4010" opacity="0.8" />
      {/* Crescent on tip */}
      <path d="M83,40 A10,10 0 1,1 83,60 A7,7 0 1,0 83,40Z" fill="#e8a040" opacity="0.9" />

      {/* Main mosque dome */}
      <rect x="480" y="148" width="320" height="122" fill="url(#min-grad)" />
      {/* Main dome */}
      <path d="M480,148 Q640,55 800,148Z" fill="#c07820" />
      {/* Side domes */}
      <path d="M480,148 Q520,112 560,148Z" fill="#b06018" />
      <path d="M720,148 Q760,112 800,148Z" fill="#b06018" />
      {/* Windows */}
      <path d="M592,148 Q614,125 636,148 L636,175 L592,175Z" fill="#7a4010" opacity="0.8" />
      <path d="M656,148 Q678,125 700,148 L700,175 L656,175Z" fill="#7a4010" opacity="0.8" />
      {/* Crescent finial */}
      <path d="M640,58 A14,14 0 1,1 640,86 A10,10 0 1,0 640,58Z" fill="#e8a040" opacity="0.9" />
      <polygon points="656,65 659,74 668,74 661,80 664,89 656,83 648,89 651,80 644,74 653,74"
        fill="#ffd700" opacity="0.85" />

      {/* Right minaret */}
      <rect x="1340" y="50" width="36" height="220" fill="url(#min-grad)" />
      <path d="M1340,50 Q1358,8 1376,50Z" fill="#c07820" />
      <ellipse cx="1358" cy="50" rx="20" ry="8" fill="#d4a030" opacity="0.8" />
      <rect x="1348" y="34" width="18" height="20" rx="9" fill="#c07820" />
      <path d="M1345,95 Q1358,81 1371,95 L1371,120 L1345,120Z" fill="#7a4010" opacity="0.8" />
      <path d="M1345,140 Q1358,126 1371,140 L1371,165 L1345,165Z" fill="#7a4010" opacity="0.8" />
      <path d="M1358,20 A10,10 0 1,1 1358,40 A7,7 0 1,0 1358,20Z" fill="#e8a040" opacity="0.9" />

      {/* City walls */}
      {Array.from({ length: 36 }, (_, i) => {
        const x = i * 40;
        const h = 10 + (i % 3) * 6;
        return <rect key={i} x={x} y={270 - h} width="32" height={h} fill="#8a4810" opacity="0.7" />;
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   ARABESQUE ARCH TOP BORDER
══════════════════════════════════════════════ */
function ArabesqueBorder() {
  return (
    <svg style={{ position: "absolute", top: 0, left: 0, width: "100%", opacity: 0.15 }}
      viewBox="0 0 1440 38" preserveAspectRatio="none">
      <defs>
        <linearGradient id="arabesque-grad" x1="0" y1="0" x2="1" y2="0">
          <stop offset="0%" stopColor="#e8a040" stopOpacity="0" />
          <stop offset="8%" stopColor="#e8a040" stopOpacity="1" />
          <stop offset="92%" stopColor="#e8a040" stopOpacity="1" />
          <stop offset="100%" stopColor="#e8a040" stopOpacity="0" />
        </linearGradient>
      </defs>
      {/* Top & bottom lines */}
      <rect x="0" y="0" width="1440" height="2" fill="url(#arabesque-grad)" />
      <rect x="0" y="36" width="1440" height="2" fill="url(#arabesque-grad)" />
      {/* Pointed arches — khaliji style */}
      {Array.from({ length: 30 }, (_, i) => {
        const x = i * 48 + 24;
        return (
          <g key={i}>
            <path d={`M${x - 18},38 L${x - 18},22 Q${x - 8},4 ${x},2 Q${x + 8},4 ${x + 18},22 L${x + 18},38`}
              fill="none" stroke="#e8a040" strokeWidth="1.4" opacity="0.8" />
            {/* Diamond ornament */}
            <polygon points={`${x},3 ${x + 4},10 ${x},17 ${x - 4},10`}
              fill="#e8a040" opacity="0.65" />
            {/* 8-pointed mini star between arches */}
            {i < 29 && (
              <polygon
                points={`${x + 24},19 ${x + 25.5},23 ${x + 29.5},23 ${x + 26.5},26 ${x + 27.5},30 ${x + 24},27.5 ${x + 20.5},30 ${x + 21.5},26 ${x + 18.5},23 ${x + 22.5},23`}
                fill="#e8a040" opacity="0.45" />
            )}
          </g>
        );
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   BRASS DUST PARTICLES
══════════════════════════════════════════════ */
const DUST = Array.from({ length: 50 }, (_, i) => ({
  id: i,
  left: `${((i * 162.8) % 100).toFixed(1)}%`,
  size: 1.4 + (i % 5) * 1.5,
  dur: 9 + (i % 7) * 4,
  delay: (i * 0.38) % 11,
  color: [("#e8a040"), ("#c07820"), ("#f0c060"), ("#d48030"), ("#ffd070")][i % 5],
  opacity: 0.12 + (i % 6) * 0.08,
}));

/* ══════════════════════════════════════════════
   STARS  (55 twinkling night stars)
══════════════════════════════════════════════ */
const STARS = Array.from({ length: 55 }, (_, i) => ({
  left: `${((i * 1.618 * 100) % 100).toFixed(1)}%`,
  top: `${((i * 2.39 * 60) % 55).toFixed(1)}%`,
  size: 0.9 + (i % 5) * 0.55,
  delay: (i * 0.21) % 5,
  dur: 1.6 + (i % 7) * 0.4,
  bright: 0.2 + (i % 6) * 0.1,
}));

/* ══════════════════════════════════════════════
   STAR CONFIGS  (large Islamic stars)
══════════════════════════════════════════════ */
const ISLAMIC_STARS = [
  { left: "1%",  top: "14%", size: 56, opacity: 0.22, delay: 0   },
  { left: "89%", top: "11%", size: 48, opacity: 0.18, delay: 0.5 },
  { left: "46%", top: "4%",  size: 40, opacity: 0.15, delay: 0.8 },
  { left: "22%", top: "7%",  size: 30, opacity: 0.12, delay: 1.2 },
  { left: "70%", top: "7%",  size: 28, opacity: 0.11, delay: 0.4 },
  { left: "62%", top: "16%", size: 22, opacity: 0.09, delay: 1.6 },
];

/* ════════════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════════════ */
export default function SaladinAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "saladin";

  return (
    <AnimatePresence>
      {active && (
        <motion.div
          style={{ position: "fixed", inset: 0, pointerEvents: "none", zIndex: 9998, overflow: "hidden" }}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          transition={{ duration: 2 }}
        >
          {/* ── Night desert sky glow ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 80% 55% at 50% 0%, rgba(232,160,64,0.07), transparent 65%)",
          }} />

          {/* ── Night stars ── */}
          {STARS.map((s, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: s.left, top: s.top,
              width: s.size, height: s.size, borderRadius: "50%", background: "#f0c060",
            }}
              animate={{ opacity: [s.bright * 0.2, s.bright, s.bright * 0.2], scale: [0.8, 1.4, 0.8] }}
              transition={{ duration: s.dur, delay: s.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Islamic 8-pointed stars ── */}
          {ISLAMIC_STARS.map((st, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: st.left, top: st.top,
              filter: `drop-shadow(0 0 ${st.size / 4}px rgba(232,160,64,0.4))`,
            }}
              initial={{ opacity: 0, scale: 0 }}
              animate={{ opacity: st.opacity, scale: 1 }}
              transition={{ delay: st.delay, duration: 1.8, ease: [0.22, 1, 0.36, 1] }}
            >
              <motion.div
                animate={{ rotate: [0, 360] }}
                transition={{ duration: 45 + i * 8, repeat: Infinity, ease: "linear" }}
              >
                <IslamicStar size={st.size} opacity={st.opacity} />
              </motion.div>
            </motion.div>
          ))}

          {/* ── Arabesque top border ── */}
          <ArabesqueBorder />

          {/* ── Crescent + Star ── */}
          <motion.div
            style={{
              position: "absolute", top: 14, right: 88,
              filter: "drop-shadow(0 0 20px rgba(232,160,64,0.5))",
            }}
            initial={{ x: 30, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.8, duration: 1.5 }}
          >
            <CrescentAndStar size={1} />
          </motion.div>

          {/* ── Brass dust motes ── */}
          {DUST.map((d) => (
            <motion.div key={d.id} style={{
              position: "absolute", left: d.left, top: "-8px",
              width: d.size, height: d.size * 0.6, borderRadius: "50%",
              background: d.color, opacity: d.opacity,
            }}
              animate={{
                y: ["0vh", "112vh"],
                x: [0, 50, -35, 25, 0],
                opacity: [0, d.opacity, d.opacity, 0],
              }}
              transition={{
                duration: d.dur, delay: d.delay, repeat: Infinity,
                ease: "linear", times: [0, 0.1, 0.9, 1],
              }}
            />
          ))}

          {/* ── Oil lamp torches on sides ── */}
          {[{ l: "1%", t: "58%" }, { l: "97%", t: "55%" }].map((lm, i) => (
            <div key={i} style={{ position: "absolute", left: lm.l, top: lm.t }}>
              <motion.div style={{
                width: 12, height: 24, marginLeft: 1,
                background: "radial-gradient(ellipse at 50% 85%, #f59e0b, #ef4444 45%, transparent)",
                borderRadius: "50% 50% 30% 30%", filter: "blur(3px)",
              }}
                animate={{ scaleX: [1, 1.4, 0.75, 1.25, 1], scaleY: [1, 0.85, 1.25, 0.92, 1], opacity: [0.7, 1, 0.8, 1, 0.7] }}
                transition={{ duration: 2.2, repeat: Infinity, ease: "easeInOut" }}
              />
              <div style={{
                position: "absolute", top: -10, left: -7, width: 32, height: 32, borderRadius: "50%",
                background: "radial-gradient(circle, rgba(245,158,11,0.3), transparent)", filter: "blur(6px)",
              }} />
              <div style={{ width: 5, height: 60, background: "linear-gradient(to bottom, #8a4810, #5a2808)", borderRadius: 2, margin: "0 auto" }} />
              <div style={{ width: 16, height: 8, background: "#6a3808", borderRadius: 3, margin: "0 auto" }} />
            </div>
          ))}

          {/* ── Minaret skyline ── */}
          <MinaretSkyline />

          {/* ── Side warm glows ── */}
          <div style={{
            position: "absolute", left: 0, top: 0, bottom: 0, width: 110,
            background: "linear-gradient(90deg, rgba(200,120,32,0.06), transparent)",
          }} />
          <div style={{
            position: "absolute", right: 0, top: 0, bottom: 0, width: 110,
            background: "linear-gradient(-90deg, rgba(200,120,32,0.06), transparent)",
          }} />

          {/* ── Bottom fade ── */}
          <div style={{
            position: "absolute", bottom: 0, left: 0, right: 0, height: "20%",
            background: "linear-gradient(to top, #0e0c04 0%, transparent 100%)",
          }} />

          {/* ── "Saladin's Legacy" label ── */}
          <motion.div style={{
            position: "absolute", right: 16, bottom: 90,
            fontSize: 10, fontWeight: 800, letterSpacing: 3,
            color: "rgba(232,160,64,0.28)", textTransform: "uppercase",
            writingMode: "vertical-rl", fontFamily: "'Sora', sans-serif",
          }}
            animate={{ opacity: [0.18, 0.38, 0.18] }}
            transition={{ duration: 5.5, repeat: Infinity }}
          >
            Saladin&apos;s Legacy
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}