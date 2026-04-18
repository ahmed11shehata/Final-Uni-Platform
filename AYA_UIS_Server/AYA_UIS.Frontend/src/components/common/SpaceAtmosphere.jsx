// src/components/common/SpaceAtmosphere.jsx
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ── Star field data ── */
const STARS = Array.from({ length: 120 }, (_, i) => ({
  left:  `${((i * 1.618 * 100) % 100).toFixed(1)}%`,
  top:   `${((i * 2.39 * 80)  % 85).toFixed(1)}%`,
  size:  0.8 + (i % 6) * 0.45,
  delay: (i * 0.18) % 6,
  dur:   1.8 + (i % 8) * 0.5,
  bright: 0.2 + (i % 7) * 0.11,
  color: i % 10 === 0 ? "#a0b8ff" : i % 7 === 0 ? "#ffe0a0" : "#c8d4f8",
}));

/* ── Shooting star ── */
function ShootingStar({ delay, top, color = "#818cf8" }) {
  return (
    <motion.div
      style={{
        position: "absolute",
        top, left: "-5%",
        width: 80, height: 1.5,
        background: `linear-gradient(90deg, transparent, ${color}dd, transparent)`,
        borderRadius: 2,
        transformOrigin: "left center",
        rotate: 18,
      }}
      animate={{ x: ["0%", "110vw"], opacity: [0, 0.95, 0.95, 0] }}
      transition={{
        duration: 1.5,
        delay,
        repeat: Infinity,
        repeatDelay: 12 + delay * 4,
        ease: "easeIn",
      }}
    />
  );
}

/* ── Nebula cloud ── */
function Nebula({ x, y, color, size, opacity }) {
  return (
    <motion.div
      style={{
        position: "absolute",
        left: x, top: y,
        width: size, height: size * 0.6,
        borderRadius: "50%",
        background: `radial-gradient(ellipse, ${color}22, transparent 70%)`,
        filter: "blur(18px)",
        pointerEvents: "none",
      }}
      animate={{ opacity: [opacity * 0.6, opacity, opacity * 0.6], scale: [0.96, 1.04, 0.96] }}
      transition={{ duration: 8 + Math.random() * 6, repeat: Infinity, ease: "easeInOut" }}
    />
  );
}

/* ── Planet ── */
function Planet({ right, top, size, color, ringColor }) {
  return (
    <motion.div
      style={{ position: "absolute", right, top, width: size, height: size, flexShrink: 0 }}
      animate={{ y: [0, -8, 0] }}
      transition={{ duration: 10, repeat: Infinity, ease: "easeInOut" }}
    >
      <svg width={size} height={size} viewBox="0 0 100 100">
        <defs>
          <radialGradient id="planetGrad" cx="35%" cy="30%" r="70%">
            <stop offset="0%" stopColor={color} stopOpacity="0.9"/>
            <stop offset="100%" stopColor={color} stopOpacity="0.5"/>
          </radialGradient>
          <filter id="glow">
            <feGaussianBlur stdDeviation="3" result="b"/>
            <feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge>
          </filter>
        </defs>
        {/* Ring */}
        <ellipse cx="50" cy="58" rx="46" ry="12"
          fill="none" stroke={ringColor} strokeWidth="3" opacity="0.4"/>
        {/* Planet */}
        <circle cx="50" cy="50" r="32" fill="url(#planetGrad)" filter="url(#glow)"/>
        {/* Highlight */}
        <ellipse cx="38" cy="38" rx="9" ry="6" fill="white" opacity="0.14"
          transform="rotate(-20 38 38)"/>
        {/* Surface bands */}
        <path d="M22 52 Q50 48 78 52" stroke={ringColor} strokeWidth="1.5"
          fill="none" opacity="0.2"/>
        <path d="M25 42 Q50 38 75 42" stroke="white" strokeWidth="1"
          fill="none" opacity="0.1"/>
        {/* Ring (front) */}
        <path d="M18 62 Q50 55 82 62" stroke={ringColor} strokeWidth="3"
          fill="none" opacity="0.38"/>
      </svg>
    </motion.div>
  );
}

/* ════════════════════════════════════════════ */
export default function SpaceAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "space";

  return (
    <AnimatePresence>
      {active && (
        <motion.div
          style={{
            position: "fixed", inset: 0,
            pointerEvents: "none", zIndex: 9998,
            overflow: "hidden",
          }}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 1.5 }}
        >
          {/* Background vignette */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 80% 70% at 60% 20%, rgba(103,120,224,0.08), transparent 70%)",
          }}/>

          {/* Stars */}
          {STARS.map((s, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: s.left, top: s.top,
              width: s.size, height: s.size,
              borderRadius: "50%", background: s.color,
            }}
              animate={{ opacity: [s.bright * 0.2, s.bright, s.bright * 0.2], scale: [0.8, 1.5, 0.8] }}
              transition={{ duration: s.dur, delay: s.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* Nebula clouds */}
          <Nebula x="5%"  y="15%" color="#6878e0" size={320} opacity={0.5} />
          <Nebula x="60%" y="5%"  color="#9060c0" size={260} opacity={0.4} />
          <Nebula x="35%" y="60%" color="#4060a0" size={200} opacity={0.35}/>
          <Nebula x="75%" y="45%" color="#8040c0" size={180} opacity={0.3} />

          {/* Shooting stars */}
          {[
            { delay: 2,  top: "12%", color: "#818cf8" },
            { delay: 8,  top: "28%", color: "#a0b8ff" },
            { delay: 15, top: "8%",  color: "#c8d4f8" },
          ].map((s, i) => <ShootingStar key={i} {...s} />)}

          {/* Planet top-right */}
          <Planet right={60} top={8} size={110} color="#4a5a9a" ringColor="#818cf8" />

          {/* Small distant planet */}
          <motion.div
            style={{ position: "absolute", left: "12%", top: "20%", width: 32, height: 32 }}
            animate={{ y: [0, -6, 0] }}
            transition={{ duration: 14, repeat: Infinity, ease: "easeInOut", delay: 3 }}
          >
            <svg width="32" height="32" viewBox="0 0 32 32">
              <circle cx="16" cy="16" r="10" fill="#3a6080" opacity="0.7"/>
              <ellipse cx="16" cy="16" rx="16" ry="5" fill="none"
                stroke="#5cb8d8" strokeWidth="1.5" opacity="0.35"/>
              <ellipse cx="10" cy="11" rx="4" ry="2.5" fill="white" opacity="0.1" transform="rotate(-25 10 11)"/>
            </svg>
          </motion.div>

          {/* "Space Explorer" watermark */}
          <motion.div
            style={{
              position: "absolute", right: 16, bottom: 80,
              fontSize: 10, fontWeight: 800, letterSpacing: 3,
              color: "rgba(103,120,224,0.25)",
              textTransform: "uppercase",
              writingMode: "vertical-rl",
              fontFamily: "'Sora', sans-serif",
            }}
            animate={{ opacity: [0.15, 0.35, 0.15] }}
            transition={{ duration: 6, repeat: Infinity }}
          >
            Space Explorer
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}