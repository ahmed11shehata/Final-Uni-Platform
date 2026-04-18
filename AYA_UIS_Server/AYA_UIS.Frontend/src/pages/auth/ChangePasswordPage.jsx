import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { motion } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";
import { changePassword } from "../../services/api/authApi";

const extractError = (err) => {
  const d = err?.response?.data;
  if (d?.error?.message) return d.error.message;
  if (d?.message) return d.message;
  return err?.message ?? "Something went wrong.";
};

export default function ChangePasswordPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword]         = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showCurrent, setShowCurrent]         = useState(false);
  const [showNew, setShowNew]                 = useState(false);
  const [showConfirm, setShowConfirm]         = useState(false);
  const [error, setError]                     = useState("");
  const [success, setSuccess]                 = useState(false);
  const [loading, setLoading]                 = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (newPassword !== confirmPassword) {
      setError("New passwords do not match.");
      return;
    }
    if (newPassword.length < 8) {
      setError("New password must be at least 8 characters.");
      return;
    }

    setLoading(true);
    try {
      await changePassword({ currentPassword, newPassword, confirmPassword });
      setSuccess(true);
      setTimeout(async () => {
        await logout();
        navigate("/login", { replace: true });
      }, 1800);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

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
          maxWidth: "440px",
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
          {/* Header */}
          <h1 style={{
            margin: "0 0 6px",
            fontSize: "22px",
            fontWeight: 800,
            color: "var(--text-primary)",
          }}>
            Change Password
          </h1>
          <p style={{
            margin: "0 0 20px",
            fontSize: "14px",
            color: "var(--text-muted)",
            lineHeight: 1.6,
          }}>
            Enter your current password and choose a new one.
          </p>

          {/* Info box */}
          <div style={{
            background: "rgba(99,102,241,0.07)",
            border: "1px solid rgba(99,102,241,0.2)",
            borderRadius: "10px",
            padding: "12px 14px",
            marginBottom: "24px",
            fontSize: "13px",
            color: "var(--text-secondary)",
            lineHeight: 1.6,
          }}>
            After changing your password you will be logged out and must sign in again with your new password.
          </div>

          {/* Success message */}
          {success && (
            <div style={{
              background: "rgba(34,197,94,0.1)",
              border: "1px solid rgba(34,197,94,0.35)",
              borderRadius: "10px",
              padding: "12px 14px",
              marginBottom: "20px",
              fontSize: "14px",
              color: "#16a34a",
              textAlign: "center",
            }}>
              Password changed successfully. Logging you out…
            </div>
          )}

          <form onSubmit={handleSubmit}>
            <InputField
              label="Current Password"
              value={currentPassword}
              onChange={setCurrentPassword}
              show={showCurrent}
              onToggle={() => setShowCurrent(v => !v)}
            />
            <InputField
              label="New Password"
              value={newPassword}
              onChange={setNewPassword}
              show={showNew}
              onToggle={() => setShowNew(v => !v)}
            />
            <InputField
              label="Confirm New Password"
              value={confirmPassword}
              onChange={setConfirmPassword}
              show={showConfirm}
              onToggle={() => setShowConfirm(v => !v)}
            />

            {error && (
              <p style={{ margin: "0 0 16px", fontSize: "13px", color: "#ef4444" }}>
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={loading || success}
              style={{
                width: "100%",
                padding: "12px",
                background: "var(--accent, #818cf8)",
                color: "#fff",
                border: "none",
                borderRadius: "10px",
                fontSize: "15px",
                fontWeight: 800,
                fontFamily: "'Sora', sans-serif",
                cursor: loading || success ? "not-allowed" : "pointer",
                opacity: loading || success ? 0.7 : 1,
                transition: "opacity 0.2s",
              }}
            >
              {loading ? "Updating…" : "Update Password"}
            </button>
          </form>

          <button
            type="button"
            onClick={() => navigate("/password-help")}
            style={{
              marginTop: "16px",
              background: "none",
              border: "none",
              color: "#6366f1",
              fontSize: "13px",
              cursor: "pointer",
              fontFamily: "'Sora', sans-serif",
              padding: "0",
              textDecoration: "underline",
              display: "block",
            }}
          >
            ← Password Help
          </button>
        </div>
      </motion.div>
    </div>
  );
}

function InputField({ label, value, onChange, show, onToggle }) {
  return (
    <div style={{ marginBottom: "18px" }}>
      <label style={{
        display: "block",
        marginBottom: "6px",
        fontSize: "13px",
        fontWeight: 600,
        color: "var(--text-primary)",
      }}>
        {label}
      </label>
      <div style={{ position: "relative" }}>
        <input
          type={show ? "text" : "password"}
          value={value}
          onChange={e => onChange(e.target.value)}
          required
          style={{
            width: "100%",
            padding: "10px 44px 10px 12px",
            background: "transparent",
            border: "1.5px solid var(--border)",
            borderRadius: "10px",
            fontSize: "14px",
            color: "var(--text-primary)",
            fontFamily: "'Sora', sans-serif",
            outline: "none",
            boxSizing: "border-box",
            transition: "border-color 0.15s",
          }}
          onFocus={e => e.target.style.borderColor = "#6366f1"}
          onBlur={e => e.target.style.borderColor = ""}
        />
        <button
          type="button"
          onClick={onToggle}
          style={{
            position: "absolute",
            right: "12px",
            top: "50%",
            transform: "translateY(-50%)",
            background: "none",
            border: "none",
            cursor: "pointer",
            color: "var(--text-muted)",
            fontSize: "13px",
            padding: "0",
            fontFamily: "'Sora', sans-serif",
          }}
        >
          {show ? "Hide" : "Show"}
        </button>
      </div>
    </div>
  );
}
