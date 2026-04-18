// src/pages/student/AIToolsPage.jsx
import { useState, useEffect } from "react";
import { motion } from "framer-motion";
import { useNavigate } from "react-router-dom";
import { aiApi } from "../../services/aiApi";
import styles from "./AIToolsPage.module.css";

const TOOLS = [
  {
    id: "summary",
    icon: "📄",
    name: "Smart Summary",
    nameAr: "ملخص ذكي",
    desc: "Turn any document into a structured, exam-ready summary with key concepts and definitions.",
    color: "#6366f1",
    shade: "#4338ca",
    gradient: "linear-gradient(135deg, #6366f1, #4338ca)",
    features: ["PDF, Word, PPT, Images", "EN & AR languages", "Structured headings"],
    badge: null,
  },
  {
    id: "quiz",
    icon: "✏️",
    name: "Quiz Generator",
    nameAr: "مولّد الكويزات",
    desc: "Generate interactive MCQ quizzes with explanations to test your understanding.",
    color: "#e8a838",
    shade: "#b07820",
    gradient: "linear-gradient(135deg, #e8a838, #c07818)",
    features: ["Up to 30 questions", "4-option MCQ", "Answer explanations"],
    badge: null,
  },
  {
    id: "mindmap",
    icon: "🗺️",
    name: "Mind Map",
    nameAr: "خريطة ذهنية",
    desc: "Visualize complex topics as hierarchical mind maps for better retention.",
    color: "#22c55e",
    shade: "#16a34a",
    gradient: "linear-gradient(135deg, #22c55e, #16a34a)",
    features: ["4-6 main branches", "Visual hierarchy", "Quick export"],
    badge: null,
  },
  {
    id: "question-bank",
    icon: "📚",
    name: "Question Bank",
    nameAr: "بنك الأسئلة",
    desc: "Build a comprehensive question bank with T/F, MCQ, and short answer questions.",
    color: "#e05c8a",
    shade: "#be185d",
    gradient: "linear-gradient(135deg, #e05c8a, #be185d)",
    features: ["Up to 100 questions", "3 question types", "Exam-ready format"],
    badge: null,
  },
  {
    id: "chat",
    icon: "💬",
    name: "Study Chat",
    nameAr: "المساعد الذكي",
    desc: "Chat with your document — ask questions and get instant AI-powered explanations.",
    color: "#3d8fe0",
    shade: "#1d6ab0",
    gradient: "linear-gradient(135deg, #3d8fe0, #1d6ab0)",
    features: ["Streaming responses", "Context-aware", "EN & AR support"],
    badge: "HOT",
  },
  {
    id: "generate-all",
    icon: "⚡",
    name: "Generate All",
    nameAr: "توليد شامل",
    desc: "Upload once, get everything: Summary + Quiz + Mind Map + Question Bank in one go.",
    color: "#a855f7",
    shade: "#7c3aed",
    gradient: "linear-gradient(135deg, #a855f7, #7c3aed)",
    features: ["All 4 features", "Single upload", "Save time 10×"],
    badge: "PRO",
  },
];

const stagger = {
  hidden: {},
  show: { transition: { staggerChildren: 0.07 } },
};
const cardAnim = {
  hidden: { opacity: 0, y: 32, scale: 0.94 },
  show:   { opacity: 1, y: 0,  scale: 1,   transition: { duration: 0.42, ease: [0.22, 1, 0.36, 1] } },
};

export default function AIToolsPage() {
  const navigate = useNavigate();
  const [apiOnline, setApiOnline] = useState(null);

  useEffect(() => {
    aiApi.health().then(r => setApiOnline(r !== null));
  }, []);

  return (
    <div className={styles.page}>

      {/* ── Decorative background ── */}
      <div className={styles.bgOrb1} />
      <div className={styles.bgOrb2} />
      <div className={styles.bgOrb3} />

      {/* ── Hero Header ── */}
      <motion.div className={styles.hero}
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}>

        <div className={styles.heroLeft}>
          <div className={styles.heroBadge}>
            <span className={styles.heroBadgeDot} />
            AI-Powered Learning Suite
          </div>
          <h1 className={styles.heroTitle}>
            Study Smarter<br/>
            <span className={styles.heroGradText}>with AI Tools</span>
          </h1>
          <p className={styles.heroDesc}>
            Upload your documents and let AI generate summaries, quizzes, mind maps,
            and question banks — in seconds, in any language.
          </p>

          {/* API status */}
          <div className={styles.apiStatus}>
            <div className={`${styles.apiDot} ${apiOnline === true ? styles.apiOnline : apiOnline === false ? styles.apiOffline : styles.apiChecking}`} />
            <span className={styles.apiLabel}>
              {apiOnline === null ? "Checking API..." : apiOnline ? "API Online — Ready to use" : "API Offline — Start server on port 8000"}
            </span>
          </div>
        </div>

        {/* Stats row */}
        <div className={styles.heroStats}>
          {[
            { icon: "📁", val: "5+",  label: "File types" },
            { icon: "🌍", val: "2",   label: "Languages" },
            { icon: "⚡", val: "10×", label: "Faster study" },
            { icon: "🎯", val: "6",   label: "AI tools" },
          ].map((s, i) => (
            <motion.div key={i} className={styles.statCard}
              initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 + i * 0.07 }}>
              <span className={styles.statIcon}>{s.icon}</span>
              <span className={styles.statVal}>{s.val}</span>
              <span className={styles.statLabel}>{s.label}</span>
            </motion.div>
          ))}
        </div>
      </motion.div>

      {/* ── Tool Grid ── */}
      <motion.div className={styles.grid}
        variants={stagger} initial="hidden" animate="show">
        {TOOLS.map(tool => (
          <motion.div key={tool.id} className={styles.card} variants={cardAnim}
            whileHover={{ y: -8, transition: { duration: 0.2 } }}>

            {/* Top gradient bar */}
            <div className={styles.cardBar} style={{ background: tool.gradient }} />

            {/* Badge */}
            {tool.badge && (
              <div className={styles.cardBadge}
                style={{ background: tool.color }}>
                {tool.badge}
              </div>
            )}

            {/* Icon */}
            <div className={styles.cardIcon}
              style={{ background: `${tool.color}18`, color: tool.color }}>
              {tool.icon}
            </div>

            {/* Info */}
            <div className={styles.cardBody}>
              <h3 className={styles.cardName}>{tool.name}</h3>
              <p className={styles.cardNameAr}>{tool.nameAr}</p>
              <p className={styles.cardDesc}>{tool.desc}</p>

              {/* Feature pills */}
              <div className={styles.cardFeatures}>
                {tool.features.map((f, i) => (
                  <span key={i} className={styles.featurePill}
                    style={{ color: tool.color, background: `${tool.color}12`, borderColor: `${tool.color}28` }}>
                    {f}
                  </span>
                ))}
              </div>
            </div>

            {/* CTA */}
            <motion.button
              className={styles.cardBtn}
              style={{ background: tool.gradient }}
              onClick={() => navigate(`/student/ai-tools/${tool.id}`)}
              whileHover={{ scale: 1.04, filter: "brightness(1.08)" }}
              whileTap={{ scale: 0.97 }}>
              Launch Tool →
            </motion.button>
          </motion.div>
        ))}
      </motion.div>

      {/* ── Footer tip ── */}
      <motion.div className={styles.footerTip}
        initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 0.8 }}>
        <span>💡</span>
        <span>All tools support PDF, Word (.docx), PowerPoint (.pptx), images, and text files up to 50MB</span>
      </motion.div>
    </div>
  );
}