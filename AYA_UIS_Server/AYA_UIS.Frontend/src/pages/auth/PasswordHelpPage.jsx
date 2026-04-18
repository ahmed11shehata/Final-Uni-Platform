import { useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";

export default function PasswordHelpPage() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const dashboardPath = user
    ? ({ admin: "/admin/dashboard", instructor: "/instructor/dashboard", student: "/student/dashboard" }[user.role?.toLowerCase()] || "/student/dashboard")
    : null;

  return (
    <div style={{
      minHeight: "100vh",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      background: "var(--page-bg)",
      fontFamily: "'Sora', sans-serif",
      padding: "24px",
    }}>
      <motion.div
        initial={{ opacity: 0, y: 28 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.38, ease: "easeOut" }}
        style={{
          background: "var(--card-bg)",
          border: "1px solid var(--border)",
          borderRadius: "20px",
          overflow: "hidden",
          width: "100%",
          maxWidth: "460px",
          boxSizing: "border-box",
          boxShadow: "0 8px 32px rgba(0,0,0,0.10)",
        }}
      >
        {/* Top accent bar */}
        <div style={{
          height: "5px",
          background: "linear-gradient(90deg, #4338ca, #818cf8, #6366f1)",
        }} />

        <div style={{ padding: "36px 36px 32px" }}>
          <h1 style={{ margin: "0 0 8px", fontSize: "22px", fontWeight: 800, color: "var(--text-primary)" }}>
            Password Help
          </h1>
          <p style={{ margin: "0 0 28px", fontSize: "14px", color: "var(--text-muted)", lineHeight: 1.6 }}>
            Choose an option below.
          </p>

          <OptionCard
            icon="🔑"
            title="Forgot my password"
            description="Reset your password using a verification code sent to your recovery email."
            onClick={() => navigate("/forgot-password")}
          />

          <OptionCard
            icon="🔒"
            title="Change my password"
            description="Update your current password. You must be logged in."
            onClick={() => navigate("/change-password")}
          />

          <button
            onClick={() => navigate(dashboardPath || "/login")}
            style={{
              marginTop: "8px",
              background: "none",
              border: "none",
              color: "#6366f1",
              fontSize: "13px",
              cursor: "pointer",
              fontFamily: "'Sora', sans-serif",
              padding: "0",
              textDecoration: "underline",
            }}
          >
            ← Back
          </button>
        </div>
      </motion.div>
    </div>
  );
}

function OptionCard({ icon, title, description, onClick }) {
  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.015 }}
      whileTap={{ scale: 0.985 }}
      style={{
        display: "flex",
        alignItems: "flex-start",
        gap: "14px",
        width: "100%",
        background: "transparent",
        border: "1.5px solid var(--border)",
        borderRadius: "12px",
        padding: "18px",
        marginBottom: "12px",
        cursor: "pointer",
        textAlign: "left",
        fontFamily: "'Sora', sans-serif",
        transition: "border-color 0.15s",
      }}
      onHoverStart={e => { e.target.style && (e.target.style.borderColor = "#6366f1"); }}
      onHoverEnd={e => { e.target.style && (e.target.style.borderColor = ""); }}
    >
      <span style={{ fontSize: "22px", lineHeight: 1 }}>{icon}</span>
      <div>
        <p style={{ margin: "0 0 4px", fontSize: "14px", fontWeight: 600, color: "var(--text-primary)" }}>
          {title}
        </p>
        <p style={{ margin: 0, fontSize: "13px", color: "var(--text-muted)", lineHeight: 1.5 }}>
          {description}
        </p>
      </div>
    </motion.button>
  );
}
