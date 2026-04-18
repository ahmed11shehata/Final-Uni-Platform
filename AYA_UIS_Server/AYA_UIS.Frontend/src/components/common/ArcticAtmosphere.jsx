// src/components/common/ArcticAtmosphere.jsx
// ❄️ Arctic Blizzard Atmosphere
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ══════════════════════════════════════════════
   ICE CRYSTAL SVG  (dendrite snowflake)
══════════════════════════════════════════════ */
function IceCrystal({ size = 46, color = "#90d8f8", glow = "#a8ecff" }) {
  const arms = [0, 60, 120, 180, 240, 300];
  return (
    <svg width={size} height={size} viewBox="0 0 80 80" fill="none">
      <defs>
        <filter id="crystal-glow">
          <feGaussianBlur stdDeviation="2" result="b" />
          <feMerge><feMergeNode in="b" /><feMergeNode in="SourceGraphic" /></feMerge>
        </filter>
        <radialGradient id="crystal-fill" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="white" stopOpacity="0.9" />
          <stop offset="100%" stopColor={color} stopOpacity="0.6" />
        </radialGradient>
      </defs>
      {arms.map((a) => {
        const r = (a * Math.PI) / 180;
        const ex = 40 + 34 * Math.cos(r), ey = 40 + 34 * Math.sin(r);
        const m1x = 40 + 16 * Math.cos(r), m1y = 40 + 16 * Math.sin(r);
        const m2x = 40 + 26 * Math.cos(r), m2y = 40 + 26 * Math.sin(r);
        const br = ((a + 90) * Math.PI) / 180;
        return (
          <g key={a} filter="url(#crystal-glow)">
            <line x1="40" y1="40" x2={ex} y2={ey}
              stroke={color} strokeWidth="2" strokeLinecap="round" />
            <line x1={m1x - 9 * Math.cos(br)} y1={m1y - 9 * Math.sin(br)}
              x2={m1x + 9 * Math.cos(br)} y2={m1y + 9 * Math.sin(br)}
              stroke={glow} strokeWidth="1.4" strokeLinecap="round" />
            <line x1={m2x - 6 * Math.cos(br)} y1={m2y - 6 * Math.sin(br)}
              x2={m2x + 6 * Math.cos(br)} y2={m2y + 6 * Math.sin(br)}
              stroke={glow} strokeWidth="1.1" strokeLinecap="round" />
            <circle cx={ex} cy={ey} r="2.8" fill={glow} opacity="0.85" />
          </g>
        );
      })}
      <circle cx="40" cy="40" r="5.5" fill="url(#crystal-fill)" />
      <circle cx="40" cy="40" r="2.5" fill="white" opacity="0.9" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   POLAR BEAR  (detailed SVG silhouette)
══════════════════════════════════════════════ */
function PolarBear() {
  return (
    <motion.div
      style={{ position: "absolute", bottom: 4, right: "3%", opacity: 0.09 }}
      animate={{ x: [0, -7, 0, -4, 0], y: [0, -3, 0, -2, 0] }}
      transition={{ duration: 7, repeat: Infinity, ease: "easeInOut" }}
    >
      <svg width="120" height="82" viewBox="0 0 120 82" fill="#b8e0f0">
        {/* Body */}
        <ellipse cx="65" cy="60" rx="36" ry="20" />
        {/* Head */}
        <circle cx="26" cy="46" r="18" />
        {/* Ears */}
        <circle cx="13" cy="29" r="8" />
        <circle cx="13" cy="29" r="5" fill="#d0ecf8" />
        <circle cx="35" cy="27" r="8" />
        <circle cx="35" cy="27" r="5" fill="#d0ecf8" />
        {/* Snout */}
        <ellipse cx="11" cy="48" rx="9" ry="6" />
        <ellipse cx="11" cy="48" rx="6" ry="4" fill="#cce4f0" />
        <circle cx="9" cy="46" r="1.5" fill="#607080" />
        <circle cx="13" cy="46" r="1.5" fill="#607080" />
        {/* Eye */}
        <circle cx="20" cy="40" r="3" fill="#304050" />
        <circle cx="19" cy="39" r="1" fill="white" />
        {/* Front legs */}
        <ellipse cx="36" cy="76" rx="9" ry="7" />
        <ellipse cx="54" cy="78" rx="9" ry="7" />
        {/* Back legs */}
        <ellipse cx="74" cy="78" rx="9" ry="7" />
        <ellipse cx="92" cy="76" rx="9" ry="7" />
        {/* Tail */}
        <ellipse cx="101" cy="54" rx="7" ry="6" />
        {/* Belly highlight */}
        <ellipse cx="60" cy="56" rx="22" ry="12" fill="#d8eef8" opacity="0.5" />
      </svg>
    </motion.div>
  );
}

/* ══════════════════════════════════════════════
   AURORA BOREALIS  (5 layered glowing bands)
══════════════════════════════════════════════ */
const AURORA_BANDS = [
  { color: "#00d4aa", top: "5%",  w: "65%", left: "6%",  blur: 26, dur: 15, delay: 0   },
  { color: "#00b4d8", top: "11%", w: "50%", left: "33%", blur: 32, dur: 20, delay: 3.5 },
  { color: "#4ecdc4", top: "16%", w: "36%", left: "10%", blur: 20, dur: 12, delay: 7   },
  { color: "#7de8c8", top: "8%",  w: "30%", left: "60%", blur: 24, dur: 17, delay: 2   },
  { color: "#38b8d8", top: "20%", w: "44%", left: "22%", blur: 28, dur: 23, delay: 10  },
];

/* ══════════════════════════════════════════════
   ICICLES  (top border)
══════════════════════════════════════════════ */
function Icicles() {
  return (
    <svg style={{ position: "absolute", top: 0, left: 0, width: "100%", opacity: 0.16 }}
      viewBox="0 0 1440 58" preserveAspectRatio="none">
      <defs>
        <linearGradient id="icicle-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#c0e8ff" stopOpacity="1" />
          <stop offset="80%" stopColor="#90ccee" stopOpacity="0.5" />
          <stop offset="100%" stopColor="#80c0e8" stopOpacity="0" />
        </linearGradient>
      </defs>
      {Array.from({ length: 54 }, (_, i) => {
        const x = i * 26.6 + 13;
        const h = 12 + (i % 7) * 6.5;
        const w = 5 + (i % 4) * 2;
        return (
          <g key={i}>
            <polygon points={`${x - w},0 ${x + w},0 ${x},${h}`}
              fill="url(#icicle-grad)" />
            {/* Ice highlight */}
            <line x1={x - w * 0.25} y1={1.5} x2={x - w * 0.08} y2={h * 0.55}
              stroke="white" strokeWidth="0.8" opacity="0.5" strokeLinecap="round" />
          </g>
        );
      })}
    </svg>
  );
}

/* ══════════════════════════════════════════════
   SNOW PILE  (bottom terrain)
══════════════════════════════════════════════ */
function SnowPile() {
  return (
    <svg style={{ position: "absolute", bottom: 0, left: 0, width: "100%", opacity: 0.13 }}
      viewBox="0 0 1440 40" preserveAspectRatio="none">
      <defs>
        <linearGradient id="snow-grad" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#d0ecff" />
          <stop offset="100%" stopColor="#a8d8f0" />
        </linearGradient>
      </defs>
      <path d="M0,40 L0,28 Q72,10 144,22 Q216,34 288,16 Q360,2 432,14 Q504,28 576,12 Q648,0 720,14 Q792,26 864,12 Q936,0 1008,14 Q1080,30 1152,18 Q1224,6 1296,20 Q1368,34 1440,22 L1440,40Z"
        fill="url(#snow-grad)" />
    </svg>
  );
}

/* ══════════════════════════════════════════════
   SNOWFLAKES  (70 animated particles)
══════════════════════════════════════════════ */
const FLAKES = Array.from({ length: 70 }, (_, i) => ({
  id: i,
  left: `${((i * 137.508) % 100).toFixed(2)}%`,
  size: 2 + (i % 6) * 1.7,
  dur: 6 + (i % 8) * 2.3,
  delay: (i * 0.23) % 9.5,
  sway: 16 + (i % 5) * 18,
  opacity: 0.18 + (i % 7) * 0.11,
  blur: i % 5 === 0 ? 1 : 0,
}));

/* ══════════════════════════════════════════════
   WIND STREAKS
══════════════════════════════════════════════ */
const STREAKS = Array.from({ length: 14 }, (_, i) => ({
  id: i,
  top: `${4 + i * 7}%`,
  w: `${20 + (i % 6) * 11}%`,
  dur: 1.4 + i * 0.32,
  delay: i * 1.15,
  rDelay: 4 + i * 1.9,
  opacity: 0.04 + (i % 5) * 0.028,
}));

/* ══════════════════════════════════════════════
   CRYSTAL CONFIGS  (6 large fixed crystals)
══════════════════════════════════════════════ */
const CRYSTALS = [
  { left: "1%",  top: "7%",  size: 56, spin: 14,  delay: 0.2, opacity: 0.38 },
  { left: "87%", top: "4%",  size: 48, spin: -11, delay: 0.6, opacity: 0.30 },
  { left: "43%", top: "1%",  size: 38, spin: 9,   delay: 0.9, opacity: 0.24 },
  { left: "68%", top: "9%",  size: 30, spin: -16, delay: 1.2, opacity: 0.20 },
  { left: "17%", top: "13%", size: 26, spin: 11,  delay: 1.5, opacity: 0.16 },
  { left: "78%", top: "17%", size: 22, spin: -8,  delay: 0.7, opacity: 0.14 },
];

/* ════════════════════════════════════════════
   MAIN COMPONENT
════════════════════════════════════════════ */
export default function ArcticAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "arctic";

  return (
    <AnimatePresence>
      {active && (
        <motion.div
          style={{ position: "fixed", inset: 0, pointerEvents: "none", zIndex: 9998, overflow: "hidden" }}
          initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
          transition={{ duration: 2.5 }}
        >
          {/* ── Cold sky vignette ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 90% 55% at 50% 0%, rgba(0,180,220,0.08), transparent 65%)",
          }} />

          {/* ── Aurora Borealis ── */}
          {AURORA_BANDS.map((b, i) => (
            <motion.div key={i} style={{
              position: "absolute", top: b.top, left: b.left, width: b.w,
              height: 5, borderRadius: 5,
              background: `linear-gradient(90deg, transparent, ${b.color}88, ${b.color}aa, ${b.color}88, transparent)`,
              filter: `blur(${b.blur}px)`,
            }}
              animate={{
                opacity: [0, 0.85, 0.35, 0.95, 0.2, 0.75, 0],
                scaleX: [0.6, 1.4, 0.85, 1.3, 0.75, 1.1, 0.6],
                x: [0, 55, -28, 40, -15, 25, 0],
              }}
              transition={{ duration: b.dur, delay: b.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Ice crystals (fixed, large) ── */}
          {CRYSTALS.map((c, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: c.left, top: c.top,
              opacity: c.opacity, filter: `drop-shadow(0 0 ${c.size / 4}px rgba(100,210,255,0.5))`,
            }}
              initial={{ opacity: 0, scale: 0 }}
              animate={{ opacity: c.opacity, scale: 1 }}
              transition={{ delay: c.delay, duration: 2, ease: [0.22, 1, 0.36, 1] }}
            >
              <motion.div
                animate={{ rotate: [0, c.spin, 0, -c.spin * 0.6, 0] }}
                transition={{ duration: 18, repeat: Infinity, ease: "easeInOut" }}
              >
                <IceCrystal size={c.size} />
              </motion.div>
            </motion.div>
          ))}

          {/* ── Falling snowflakes ── */}
          {FLAKES.map((f) => (
            <motion.div key={f.id} style={{
              position: "absolute", left: f.left, top: "-24px",
              width: f.size, height: f.size, borderRadius: "50%",
              background: "radial-gradient(circle, rgba(255,255,255,0.95) 20%, rgba(190,235,255,0.3) 100%)",
              filter: f.blur ? `blur(${f.blur}px)` : "none",
            }}
              animate={{
                y: ["0vh", "112vh"],
                x: [0, f.sway, -f.sway * 0.6, f.sway * 0.35, 0],
                opacity: [0, f.opacity, f.opacity, f.opacity * 0.5, 0],
                rotate: [0, 180, 360],
              }}
              transition={{
                duration: f.dur, delay: f.delay, repeat: Infinity,
                ease: "linear", times: [0, 0.1, 0.65, 0.92, 1],
              }}
            />
          ))}

          {/* ── Blizzard wind streaks ── */}
          {STREAKS.map((s) => (
            <motion.div key={s.id} style={{
              position: "absolute", top: s.top, left: "-10%",
              width: s.w, height: 1.5,
              background: "linear-gradient(90deg, transparent, rgba(190,238,255,0.6), transparent)",
              borderRadius: 1,
            }}
              animate={{ x: ["-10%", "130vw"], opacity: [0, s.opacity, s.opacity, 0] }}
              transition={{
                duration: s.dur, delay: s.delay, repeat: Infinity,
                repeatDelay: s.rDelay, ease: "easeIn",
              }}
            />
          ))}

          {/* ── Icicles top border ── */}
          <Icicles />

          {/* ── Polar bear ── */}
          <PolarBear />

          {/* ── Snow pile bottom ── */}
          <SnowPile />

          {/* ── Frosted side panels ── */}
          <div style={{
            position: "absolute", left: 0, top: 0, bottom: 0, width: 70,
            background: "linear-gradient(90deg, rgba(0,90,160,0.07), transparent)",
          }} />
          <div style={{
            position: "absolute", right: 0, top: 0, bottom: 0, width: 70,
            background: "linear-gradient(-90deg, rgba(0,90,160,0.07), transparent)",
          }} />

          {/* ── Bottom fade ── */}
          <div style={{
            position: "absolute", bottom: 0, left: 0, right: 0, height: "14%",
            background: "linear-gradient(to top, rgba(4,12,24,0.5), transparent)",
          }} />

          {/* ── "Arctic Blizzard" label ── */}
          <motion.div style={{
            position: "absolute", right: 16, bottom: 90,
            fontSize: 10, fontWeight: 800, letterSpacing: 3,
            color: "rgba(130,210,255,0.25)", textTransform: "uppercase",
            writingMode: "vertical-rl", fontFamily: "'Sora', sans-serif",
          }}
            animate={{ opacity: [0.18, 0.35, 0.18] }}
            transition={{ duration: 5, repeat: Infinity }}
          >
            Arctic Blizzard
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}