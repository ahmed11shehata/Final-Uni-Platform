// src/pages/student/ThemePage.jsx
// ✅ Shared across student / instructor / admin roles
import { motion } from "framer-motion";
import { useTheme, THEMES } from "../../context/ThemeContext";
import styles from "./ThemePage.module.css";

/* ── Mini-UI Preview data per theme ── */
const PREV = {
  default:  { bg: "#f0f4f8", sbar: "#1c2035", card: "#ffffff", top: "#ffffff", accent: "#8b7cf8" },
  light:    { bg: "#f0f5f2", sbar: "#1a3328", card: "#ffffff", top: "#ffffff", accent: "#34a86a" },
  dark:     { bg: "#111827", sbar: "#0b0f1a", card: "#1e2535", top: "#161e2e", accent: "#6878d8" },
  ramadan:  { bg: "#0e0820", sbar: "#08041a", card: "#1a0e38", top: "#140a28", accent: "#d4af37" },
  ocean:    { bg: "#071828", sbar: "#040d18", card: "#0e2540", top: "#0b2035", accent: "#38b8d8" },
  sakura:   { bg: "#fff4f7", sbar: "#420816", card: "#ffffff", top: "#ffffff", accent: "#ec4899" },
  forest:   { bg: "#0b1c10", sbar: "#060e08", card: "#132818", top: "#0e2014", accent: "#4ade80" },
  sunset:   { bg: "#fef7ec", sbar: "#3a1005", card: "#ffffff", top: "#ffffff", accent: "#f97316" },
  space:    { bg: "#060810", sbar: "#030508", card: "#0e1028", top: "#0c0e1c", accent: "#818cf8" },
  nordic:   { bg: "#f2f5fc", sbar: "#1c2e42", card: "#ffffff", top: "#ffffff", accent: "#4a90d4" },
  caramel:  { bg: "#fdf5ec", sbar: "#2a1408", card: "#fffaf5", top: "#fffaf5", accent: "#c07838" },
  /* ── 4 New Themes ── */
  arctic:   { bg: "#040c18", sbar: "#020609", card: "#081428", top: "#06101e", accent: "#7ad8f8" },
  pharaoh:  { bg: "#150900", sbar: "#0a0500", card: "#241408", top: "#1e0e02", accent: "#d4a817" },
  saladin:  { bg: "#0e0c04", sbar: "#080600", card: "#201808", top: "#16120a", accent: "#e8a040" },
  fog:      { bg: "#0a0e10", sbar: "#050708", card: "#121c20", top: "#0e1418", accent: "#8ab8b0" },
};

function MiniPreview({ id }) {
  const p = PREV[id] || PREV.default;
  return (
    <div className={styles.mini} style={{ background: p.bg }}>
      {/* Sidebar */}
      <div className={styles.miniSbar} style={{ background: p.sbar }}>
        <div className={styles.miniLogo}
          style={{ background: p.accent + "55", borderRadius: 7, marginBottom: 7 }}/>
        {[80, 65, 52, 40].map((w, i) => (
          <div key={i} className={styles.miniNav}
            style={{ width: `${w}%`,
              background: i === 1 ? p.accent + "66" : "rgba(255,255,255,0.1)" }}/>
        ))}
      </div>

      {/* Main area */}
      <div className={styles.miniMain}>
        {/* Topbar */}
        <div className={styles.miniTopbar}
          style={{ background: p.top, borderBottom: `1.5px solid ${p.accent}28` }}>
          <div style={{ width: 8, height: 8, borderRadius: "50%", background: p.accent }}/>
          <div style={{ width: 28, height: 5, borderRadius: 3,
            background: p.accent + "55", marginLeft: "auto" }}/>
        </div>
        {/* Cards */}
        <div className={styles.miniCards}>
          {[75, 55, 65].map((w, i) => (
            <div key={i} className={styles.miniCard}
              style={{ background: p.card, border: `1px solid ${p.accent}22` }}>
              <div style={{ width: `${w}%`, height: 4, borderRadius: 2,
                background: p.accent + "70", marginBottom: 3 }}/>
              <div style={{ width: "45%", height: 3, borderRadius: 2,
                background: p.accent + "38" }}/>
            </div>
          ))}
          {/* Progress bar */}
          <div style={{ height: 5, borderRadius: 3,
            background: p.accent + "20", overflow: "hidden", marginTop: 1 }}>
            <div style={{ width: "65%", height: "100%", borderRadius: 3,
              background: `linear-gradient(90deg, ${p.accent}, ${p.accent}80)` }}/>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function ThemePage() {
  const { themeId, setTheme } = useTheme();

  return (
    <div className={styles.page}>

      {/* ── Header ── */}
      <motion.div className={styles.header}
        initial={{ opacity: 0, y: -16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4 }}>
        <div className={styles.headLeft}>
          <div className={styles.headIcon}>🎨</div>
          <div>
            <h1 className={styles.title}>Theme Colors</h1>
            <p className={styles.subtitle}>
              Your preference is saved and synced across all sessions
            </p>
          </div>
        </div>
        <div className={styles.currentBadge}>
          <span>{THEMES[themeId]?.emoji}</span>
          <span>Active: <strong>{THEMES[themeId]?.name}</strong></span>
        </div>
      </motion.div>

      {/* ── Theme grid ── */}
      <div className={styles.grid}>
        {Object.values(THEMES).map((t, i) => {
          const isActive = themeId === t.id;
          const accent   = PREV[t.id]?.accent || "#8b7cf8";
          return (
            <motion.button
              key={t.id}
              className={`${styles.card} ${isActive ? styles.cardActive : ""}`}
              style={isActive ? {
                borderColor: accent,
                boxShadow: `0 0 0 3px ${accent}30, 0 10px 36px ${accent}22`,
              } : {}}
              onClick={() => setTheme(t.id)}
              initial={{ opacity: 0, y: 24, scale: 0.92 }}
              animate={{ opacity: 1, y: 0, scale: 1 }}
              transition={{ delay: i * 0.055, duration: 0.4, ease: [0.22, 1, 0.36, 1] }}
              whileHover={{ y: -6, transition: { duration: 0.18 } }}
              whileTap={{ scale: 0.97 }}
            >
              {/* Active badge */}
              {isActive && (
                <motion.div
                  className={styles.activePill}
                  style={{ background: accent }}
                  initial={{ scale: 0 }} animate={{ scale: 1 }}
                  transition={{ type: "spring", stiffness: 500, damping: 22 }}
                >
                  ✓ Active
                </motion.div>
              )}

              {/* Preview */}
              <div className={styles.previewWrap}>
                <MiniPreview id={t.id} />
              </div>

              {/* Info section */}
              <div className={styles.info}>
                <div className={styles.infoTop}>
                  <span className={styles.emoji}>{t.emoji}</span>
                  <div className={styles.infoText}>
                    <span className={styles.name}>{t.name}</span>
                    <span className={styles.desc}>{t.desc}</span>
                  </div>
                  {t.dark && <span className={styles.darkTag}>🌙</span>}
                </div>
                {/* Color swatches */}
                <div className={styles.swatches}>
                  {[PREV[t.id]?.bg, PREV[t.id]?.sbar, PREV[t.id]?.card, accent].map((clr, j) => (
                    <div key={j} className={styles.swatch} style={{ background: clr }} />
                  ))}
                </div>
              </div>
            </motion.button>
          );
        })}
      </div>

      <motion.p className={styles.tip}
        initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 0.6 }}>
        💡 Select any theme — changes apply instantly across the entire portal
      </motion.p>
    </div>
  );
}