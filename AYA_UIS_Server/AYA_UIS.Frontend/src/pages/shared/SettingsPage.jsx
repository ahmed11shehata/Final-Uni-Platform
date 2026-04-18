import { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";
import { changePassword } from "../../services/api/authApi";
import s from "./SettingsPage.module.css";

const extractError = (err) => {
  const d = err?.response?.data;
  if (d?.error?.message) return d.error.message;
  if (d?.message) return d.message;
  return err?.message ?? "Something went wrong.";
};

const getStrength = (pw) => {
  if (!pw) return { level: 0, label: "", color: "transparent" };
  let score = 0;
  if (pw.length >= 8) score++;
  if (pw.length >= 12) score++;
  if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) score++;
  if (/\d/.test(pw)) score++;
  if (/[^A-Za-z0-9]/.test(pw)) score++;
  if (score <= 1) return { level: 20, label: "Weak", color: "#ef4444" };
  if (score <= 2) return { level: 40, label: "Fair", color: "#f97316" };
  if (score <= 3) return { level: 65, label: "Good", color: "#f59e0b" };
  if (score <= 4) return { level: 85, label: "Strong", color: "#22c55e" };
  return { level: 100, label: "Excellent", color: "#10b981" };
};

export default function SettingsPage() {
  const { user, logout } = useAuth();
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
  const [countdown, setCountdown]             = useState(0);

  const strength = useMemo(() => getStrength(newPassword), [newPassword]);

  const accent = user?.role === "instructor" ? "#5ba4cf" : "#8b7cf8";
  const accentGlow = user?.role === "instructor" ? "rgba(91,164,207,.15)" : "rgba(139,124,248,.15)";

  useEffect(() => {
    if (countdown <= 0) return;
    const t = setTimeout(() => setCountdown((c) => c - 1), 1000);
    return () => clearTimeout(t);
  }, [countdown]);

  useEffect(() => {
    if (!success) return;
    setCountdown(3);
    const t = setTimeout(async () => {
      await logout();
      navigate("/login", { replace: true });
    }, 3000);
    return () => clearTimeout(t);
  }, [success]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (newPassword !== confirmPassword) {
      setError("New password and confirmation do not match.");
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
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={s.page}>
      <div className={s.wrapper}>
        {/* Header */}
        <motion.div
          className={s.header}
          initial={{ opacity: 0, y: -16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4, ease: "easeOut" }}
        >
          <div className={s.headerRow}>
            <div className={s.headerIcon} style={{ background: accentGlow, color: accent }}>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 00.33 1.82l.06.06a2 2 0 01-2.83 2.83l-.06-.06a1.65 1.65 0 00-1.82-.33 1.65 1.65 0 00-1 1.51V21a2 2 0 01-4 0v-.09A1.65 1.65 0 009 19.4a1.65 1.65 0 00-1.82.33l-.06.06a2 2 0 01-2.83-2.83l.06-.06A1.65 1.65 0 004.68 15a1.65 1.65 0 00-1.51-1H3a2 2 0 010-4h.09A1.65 1.65 0 004.6 9a1.65 1.65 0 00-.33-1.82l-.06-.06a2 2 0 012.83-2.83l.06.06A1.65 1.65 0 009 4.68a1.65 1.65 0 001-1.51V3a2 2 0 014 0v.09a1.65 1.65 0 001 1.51 1.65 1.65 0 001.82-.33l.06-.06a2 2 0 012.83 2.83l-.06.06A1.65 1.65 0 0019.4 9a1.65 1.65 0 001.51 1H21a2 2 0 010 4h-.09a1.65 1.65 0 00-1.51 1z"/>
              </svg>
            </div>
            <h1 className={s.title}>Settings</h1>
          </div>
          <p className={s.subtitle}>Manage your account security and preferences.</p>
        </motion.div>

        {/* Change Password Card */}
        <motion.div
          className={s.card}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.45, delay: 0.08, ease: "easeOut" }}
        >
          <div className={s.cardStripe} style={{ background: `linear-gradient(90deg, ${accent}, ${accent}88, ${accent}44)` }} />
          <div className={s.cardBody}>
            <h2 className={s.cardTitle}>
              <span className={s.cardTitleIcon}>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
                </svg>
              </span>
              Change Password
            </h2>
            <p className={s.cardDesc}>
              Enter your current password and choose a new one. You will be signed out after changing.
            </p>

            {/* Info box */}
            <div
              className={s.infoBox}
              style={{ background: `${accent}0d`, border: `1.5px solid ${accent}25`, color: "var(--text-secondary)" }}
            >
              <span className={s.infoIcon}>
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke={accent} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>
                </svg>
              </span>
              After a successful change you will be logged out and must sign in again with your new password.
            </div>

            {/* Success */}
            <AnimatePresence>
              {success && (
                <motion.div
                  className={s.successBox}
                  initial={{ opacity: 0, height: 0, marginBottom: 0 }}
                  animate={{ opacity: 1, height: "auto", marginBottom: 20 }}
                  exit={{ opacity: 0 }}
                >
                  <span className={s.successIcon}>
                    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#16a34a" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                    </svg>
                  </span>
                  Password changed successfully!
                </motion.div>
              )}
            </AnimatePresence>

            {success && countdown > 0 && (
              <div className={s.countdown}>
                Signing you out in <span className={s.countdownNum}>{countdown}</span> second{countdown !== 1 ? "s" : ""}...
              </div>
            )}

            {/* Error */}
            <AnimatePresence>
              {error && !success && (
                <motion.div
                  className={s.errorBox}
                  initial={{ opacity: 0, y: -8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -8 }}
                >
                  <span className={s.errorIcon}>
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="#ef4444" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                      <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
                    </svg>
                  </span>
                  {error}
                </motion.div>
              )}
            </AnimatePresence>

            {!success && (
              <form onSubmit={handleSubmit}>
                {/* Current Password */}
                <div className={s.fieldGroup}>
                  <label className={s.fieldLabel}>
                    <span className={s.fieldLabelIcon}>
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 11-7.78 7.78 5.5 5.5 0 017.78-7.78zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3m-3.5 3.5L19 4"/>
                      </svg>
                    </span>
                    Current Password
                  </label>
                  <div className={s.inputWrap}>
                    <input
                      className={s.input}
                      type={showCurrent ? "text" : "password"}
                      value={currentPassword}
                      onChange={(e) => setCurrentPassword(e.target.value)}
                      placeholder="Enter current password"
                      required
                      autoComplete="current-password"
                    />
                    <button
                      type="button"
                      className={s.toggleBtn}
                      onClick={() => setShowCurrent((v) => !v)}
                    >
                      {showCurrent ? "Hide" : "Show"}
                    </button>
                  </div>
                </div>

                {/* New Password */}
                <div className={s.fieldGroup}>
                  <label className={s.fieldLabel}>
                    <span className={s.fieldLabelIcon}>
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/>
                      </svg>
                    </span>
                    New Password
                  </label>
                  <div className={s.inputWrap}>
                    <input
                      className={s.input}
                      type={showNew ? "text" : "password"}
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      placeholder="At least 8 characters"
                      required
                      autoComplete="new-password"
                    />
                    <button
                      type="button"
                      className={s.toggleBtn}
                      onClick={() => setShowNew((v) => !v)}
                    >
                      {showNew ? "Hide" : "Show"}
                    </button>
                  </div>
                  {newPassword && (
                    <div className={s.strengthRow}>
                      <div className={s.strengthBar}>
                        <div
                          className={s.strengthFill}
                          style={{ width: `${strength.level}%`, background: strength.color }}
                        />
                      </div>
                      <span className={s.strengthLabel} style={{ color: strength.color }}>
                        {strength.label}
                      </span>
                    </div>
                  )}
                </div>

                {/* Confirm Password */}
                <div className={s.fieldGroup}>
                  <label className={s.fieldLabel}>
                    <span className={s.fieldLabelIcon}>
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                      </svg>
                    </span>
                    Confirm New Password
                  </label>
                  <div className={s.inputWrap}>
                    <input
                      className={s.input}
                      type={showConfirm ? "text" : "password"}
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      placeholder="Re-enter new password"
                      required
                      autoComplete="new-password"
                    />
                    <button
                      type="button"
                      className={s.toggleBtn}
                      onClick={() => setShowConfirm((v) => !v)}
                    >
                      {showConfirm ? "Hide" : "Show"}
                    </button>
                  </div>
                  {confirmPassword && newPassword && confirmPassword !== newPassword && (
                    <p style={{ margin: "6px 0 0", fontSize: "12px", color: "#ef4444", fontWeight: 600 }}>
                      Passwords do not match
                    </p>
                  )}
                  {confirmPassword && newPassword && confirmPassword === newPassword && confirmPassword.length >= 8 && (
                    <p style={{ margin: "6px 0 0", fontSize: "12px", color: "#22c55e", fontWeight: 600 }}>
                      Passwords match
                    </p>
                  )}
                </div>

                <motion.button
                  type="submit"
                  className={s.submitBtn}
                  disabled={loading || success}
                  style={{ background: accent }}
                  whileHover={{ opacity: 0.9 }}
                  whileTap={{ scale: 0.985 }}
                >
                  {loading ? "Updating..." : "Update Password"}
                </motion.button>
              </form>
            )}

            {/* Forgot Password link — always visible when form is showing, prominent on error */}
            {!success && (
              <div className={s.forgotRow}>
                <span className={s.forgotLinkIcon}>
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="#6366f1" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 015.83 1c0 2-3 3-3 3"/><line x1="12" y1="17" x2="12.01" y2="17"/>
                  </svg>
                </span>
                <button
                  type="button"
                  className={s.forgotLink}
                  onClick={() => navigate("/forgot-password", { state: { from: "settings" } })}
                >
                  Forgot your password?
                </button>
              </div>
            )}
          </div>
        </motion.div>
      </div>
    </div>
  );
}
