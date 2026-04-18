// src/components/common/RamadanAtmosphere.jsx
import { useEffect, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "../../context/ThemeContext";

/* ── SVG Lantern ─────────────────────────────────────────────── */
function Lantern({ size = 1, color = "#d4af37", dark = "#a86c00" }) {
  const w = Math.round(42 * size), h = Math.round(78 * size);
  return (
    <svg width={w} height={h} viewBox="0 0 42 78" fill="none">
      <defs>
        <radialGradient id={`lg_${color.replace("#","")}`} cx="40%" cy="35%" r="65%">
          <stop offset="0%" stopColor={color} stopOpacity="0.9"/>
          <stop offset="100%" stopColor={dark} stopOpacity="1"/>
        </radialGradient>
        <filter id="lf_glow">
          <feGaussianBlur stdDeviation="1.5" result="blur"/>
          <feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge>
        </filter>
      </defs>
      {/* string */}
      <line x1="21" y1="0" x2="21" y2="9" stroke={color} strokeWidth="1.5" strokeLinecap="round"/>
      {/* top cap */}
      <path d="M12 9 Q21 5.5 30 9 L28 16 H14 Z" fill={color}/>
      {/* body */}
      <path d="M14 16 Q7 32 9 50 Q21 60 33 50 Q35 32 28 16 Z"
            fill={`url(#lg_${color.replace("#","")})`}/>
      {/* inner warm light */}
      <path d="M16 20 Q10 34 12 48 Q21 56 30 48 Q32 34 26 20 Z"
            fill={color} opacity="0.35"/>
      {/* ribs */}
      {[0.42, 0.62, 0.78].map((y, i) => {
        const yv = Math.round(y * 60);
        const w0 = Math.round(14 + i * 2), x0 = Math.round(21 - w0/2);
        return <path key={i} d={`M${x0} ${yv} Q21 ${yv-2} ${x0+w0} ${yv}`}
          stroke={color} strokeWidth="0.6" opacity="0.4" fill="none"/>;
      })}
      {/* side lines */}
      <path d="M14 16 Q10 33 12 48" stroke={color} strokeWidth="0.7" opacity="0.35" fill="none"/>
      <path d="M28 16 Q32 33 30 48" stroke={color} strokeWidth="0.7" opacity="0.35" fill="none"/>
      {/* bottom cap */}
      <path d="M13 48 L14.5 54 Q21 59 27.5 54 L29 48 Q21 53 13 48 Z" fill={color}/>
      {/* tassel */}
      {[-2, 0, 2].map((dx, i) => (
        <motion.line key={i} x1={21+dx} y1={54} x2={21+dx*1.5} y2={68+i*2}
          stroke={color} strokeWidth="1.1" strokeLinecap="round"
          animate={{ x2: [21+dx*1.3, 21+dx*1.8, 21+dx*1.3] }}
          transition={{ duration: 2.5, repeat: Infinity, ease: "easeInOut", delay: i*0.3 }}/>
      ))}
      {[-2, 0, 2].map((dx, i) => (
        <circle key={i} cx={21+dx*1.5} cy={68+i*2} r="1.8" fill={color}/>
      ))}
      {/* highlight */}
      <ellipse cx="15.5" cy="24" rx="2.2" ry="5" fill="white" opacity="0.16"
               transform="rotate(-14 15.5 24)"/>
    </svg>
  );
}

/* ── Crescent Moon + Star ─────────────────────────────────────── */
function CrescentMoon({ size = 1 }) {
  const s = size;
  return (
    <motion.div
      style={{ position: "relative", display: "inline-block" }}
      animate={{ y: [0, -10, 0] }}
      transition={{ duration: 7, repeat: Infinity, ease: "easeInOut" }}
    >
      <svg width={90*s} height={90*s} viewBox="0 0 90 90" fill="none">
        <defs>
          <filter id="moonGlow">
            <feGaussianBlur stdDeviation="3" result="blur"/>
            <feMerge><feMergeNode in="blur"/><feMergeNode in="SourceGraphic"/></feMerge>
          </filter>
        </defs>
        <path d="M56 15 A32 32 0 1 0 56 75 A24 24 0 1 1 56 15 Z"
              fill="#d4af37" filter="url(#moonGlow)"/>
        {/* Stars near moon */}
        <motion.polygon
          points="74,8 76.5,16 85,16 78,21 80.5,29 74,23.5 67.5,29 70,21 63,16 71.5,16"
          fill="#ffd700"
          animate={{ opacity: [0.7, 1, 0.7], scale: [0.95, 1.05, 0.95] }}
          transition={{ duration: 2.5, repeat: Infinity }}
        />
        <circle cx="74" cy="18.5" r="3" fill="white" opacity="0.35"/>
      </svg>
    </motion.div>
  );
}

/* ── Rope that connects lanterns ──────────────────────────────── */
function Rope({ x1, x2, y = 0, sag = 18 }) {
  const mx = (x1 + x2) / 2;
  return (
    <svg
      style={{ position: "absolute", top: y, left: 0, pointerEvents: "none", overflow: "visible" }}
      width="100%" height={sag + 20}
    >
      <path
        d={`M ${x1} 0 Q ${mx} ${sag} ${x2} 0`}
        stroke="#d4af3766"
        strokeWidth="1.5"
        fill="none"
        strokeDasharray="3 4"
      />
    </svg>
  );
}

/* ── Lantern config — groups with shared ropes ────────────────── */
const GROUPS = [
  { left: "1%",   lanterns: [
    { size: 1.2,  color: "#d4af37", dark: "#b07800", delay: 0,   swingDur: 5.5, swingAmp: 8  },
    { size: 0.88, color: "#f0c040", dark: "#a05800", delay: 0.5, swingDur: 6.8, swingAmp: -9 },
  ]},
  { left: "18%",  lanterns: [
    { size: 1.35, color: "#ffd700", dark: "#c06000", delay: 0.2,  swingDur: 5.0, swingAmp: -7  },
    { size: 1.0,  color: "#d4af37", dark: "#b07000", delay: 0.7,  swingDur: 6.2, swingAmp: 9   },
    { size: 0.82, color: "#e8c060", dark: "#906000", delay: 1.1,  swingDur: 5.7, swingAmp: -8  },
  ]},
  { left: "42%",  lanterns: [
    { size: 0.95, color: "#e8b840", dark: "#a06800", delay: 0.3, swingDur: 6.5, swingAmp: 10 },
    { size: 1.25, color: "#d4af37", dark: "#c07800", delay: 0.9, swingDur: 5.3, swingAmp: -6 },
  ]},
  { left: "65%",  lanterns: [
    { size: 1.1,  color: "#f5c030", dark: "#b06000", delay: 0.15, swingDur: 6.0, swingAmp: 8  },
    { size: 0.9,  color: "#d4af37", dark: "#a07000", delay: 0.6,  swingDur: 5.8, swingAmp: -9 },
    { size: 1.15, color: "#ffd700", dark: "#c05800", delay: 1.0,  swingDur: 5.4, swingAmp: 7  },
  ]},
  { left: "85%",  lanterns: [
    { size: 1.05, color: "#e8b840", dark: "#a06000", delay: 0.35, swingDur: 6.3, swingAmp: -8 },
    { size: 0.85, color: "#d4af37", dark: "#b07800", delay: 0.8,  swingDur: 5.6, swingAmp: 9  },
  ]},
];

/* ── Stars ────────────────────────────────────────────────────── */
const STARS = Array.from({ length: 55 }, (_, i) => ({
  left: `${((i * 1.618 * 100) % 100).toFixed(1)}%`,
  top:  `${((i * 2.39 * 60)  % 55).toFixed(1)}%`,
  size: 1 + (i % 5) * 0.55,
  delay: (i * 0.22) % 5,
  dur:   1.6 + (i % 7) * 0.45,
  bright: 0.25 + (i % 6) * 0.12,
}));

/* ── Shooting star ─────────────────────────────────────────────── */
function ShootingStar({ delay }) {
  return (
    <motion.div
      style={{
        position: "absolute",
        top: `${10 + Math.random() * 25}%`,
        left: "-5%",
        width: 60, height: 1.5,
        background: "linear-gradient(90deg, transparent, #ffd700cc, transparent)",
        borderRadius: 2,
        transformOrigin: "left center",
        rotate: 15,
      }}
      animate={{
        x: ["0%", "120vw"],
        opacity: [0, 0.9, 0.9, 0],
      }}
      transition={{
        duration: 1.8,
        delay,
        repeat: Infinity,
        repeatDelay: 14 + delay * 3,
        ease: "easeIn",
      }}
    />
  );
}

/* ══════════════════════════════════════════════════════════════ */
export default function RamadanAtmosphere() {
  const { themeId } = useTheme();
  const active = themeId === "ramadan";

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
          {/* ── Background gradient vignette ── */}
          <div style={{
            position: "absolute", inset: 0,
            background: "radial-gradient(ellipse 90% 60% at 50% 0%, rgba(212,175,55,0.06), transparent 70%)",
          }}/>

          {/* ── Stars ── */}
          {STARS.map((s, i) => (
            <motion.div key={i} style={{
              position: "absolute", left: s.left, top: s.top,
              width: s.size, height: s.size,
              borderRadius: "50%", background: "#d4af37",
            }}
              animate={{ opacity: [s.bright * 0.2, s.bright, s.bright * 0.2], scale: [0.8, 1.4, 0.8] }}
              transition={{ duration: s.dur, delay: s.delay, repeat: Infinity, ease: "easeInOut" }}
            />
          ))}

          {/* ── Shooting stars ── */}
          {[3, 9, 17].map((d, i) => <ShootingStar key={i} delay={d} />)}

          {/* ── Crescent Moon ── */}
          <motion.div
            style={{ position: "absolute", top: 12, right: 80,
              filter: "drop-shadow(0 0 22px rgba(212,175,55,0.5))" }}
            initial={{ x: 30, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            transition={{ delay: 0.6, duration: 1.5 }}
          >
            <CrescentMoon size={1} />
          </motion.div>

          {/* ── Horizontal rope across top ── */}
          <div style={{ position: "absolute", top: 4, left: 0, right: 0 }}>
            <svg width="100%" height="12" viewBox="0 0 1440 12" preserveAspectRatio="none">
              <path
                d="M0,6 Q60,2 120,6 Q180,10 240,6 Q300,2 360,6 Q420,10 480,6 Q540,2 600,6 Q660,10 720,6 Q780,2 840,6 Q900,10 960,6 Q1020,2 1080,6 Q1140,10 1200,6 Q1260,2 1320,6 Q1380,10 1440,6"
                stroke="#d4af3780" strokeWidth="1.8" fill="none"
              />
              {/* Rope decorative beads */}
              {[120, 240, 360, 480, 600, 720, 840, 960, 1080, 1200, 1320].map((x, i) => (
                <circle key={i} cx={x} cy={6} r="2.5" fill="#d4af37" opacity="0.7"/>
              ))}
            </svg>
          </div>

          {/* ── Second decorative rope lower ── */}
          <div style={{ position: "absolute", top: 55, left: 0, right: 0, opacity: 0.35 }}>
            <svg width="100%" height="8" viewBox="0 0 1440 8" preserveAspectRatio="none">
              <path
                d="M0,4 Q90,1 180,4 Q270,7 360,4 Q450,1 540,4 Q630,7 720,4 Q810,1 900,4 Q990,7 1080,4 Q1170,1 1260,4 Q1350,7 1440,4"
                stroke="#d4af37" strokeWidth="1" fill="none"
              />
            </svg>
          </div>

          {/* ── Lantern groups ── */}
          {GROUPS.map((group, gi) => (
            <div key={gi} style={{
              position: "absolute", top: 8, left: group.left,
              display: "flex", gap: 8, alignItems: "flex-start",
            }}>
              {group.lanterns.map((l, li) => (
                <motion.div key={li}
                  style={{
                    transformOrigin: "top center",
                    filter: `drop-shadow(0 0 16px ${l.color}80)`,
                  }}
                  initial={{ y: -120, opacity: 0 }}
                  animate={{ y: 0, opacity: 0.95 }}
                  transition={{
                    y: { delay: l.delay * 0.4, duration: 1.4, ease: [0.22, 1, 0.36, 1] },
                    opacity: { delay: l.delay * 0.4, duration: 1.0 },
                  }}
                >
                  <motion.div
                    animate={{ rotate: [0, l.swingAmp, 0, -l.swingAmp * 0.5, 0] }}
                    transition={{
                      delay: l.delay * 0.4 + 1.8,
                      duration: l.swingDur,
                      repeat: Infinity,
                      ease: "easeInOut",
                    }}
                    style={{ transformOrigin: "top center" }}
                  >
                    <Lantern size={l.size} color={l.color} dark={l.dark} />
                  </motion.div>
                </motion.div>
              ))}
            </div>
          ))}

          {/* ── Islamic pattern border at very top (subtle) ── */}
          <svg
            style={{ position: "absolute", top: 0, left: 0, right: 0, width: "100%", opacity: 0.08 }}
            viewBox="0 0 1440 30" preserveAspectRatio="none"
          >
            {Array.from({ length: 48 }, (_, i) => {
              const x = i * 30;
              return (
                <g key={i}>
                  <path d={`M${x} 0 Q${x+15} 28 ${x+30} 0`} fill="#d4af37"/>
                </g>
              );
            })}
          </svg>

          {/* ── Bottom fade ── */}
          <div style={{
            position: "absolute", bottom: 0, left: 0, right: 0, height: "22%",
            background: "linear-gradient(to top, #0c0618 0%, transparent 100%)",
          }}/>

          {/* ── Ramadan Kareem text (subtle, right side) ── */}
          <motion.div
            style={{
              position: "absolute", right: 16, bottom: 80,
              fontSize: 11, fontWeight: 800, letterSpacing: 3,
              color: "rgba(212,175,55,0.3)",
              textTransform: "uppercase",
              writingMode: "vertical-rl",
              fontFamily: "'Sora', sans-serif",
            }}
            animate={{ opacity: [0.2, 0.4, 0.2] }}
            transition={{ duration: 5, repeat: Infinity }}
          >
            Ramadan Kareem
          </motion.div>

        </motion.div>
      )}
    </AnimatePresence>
  );
}