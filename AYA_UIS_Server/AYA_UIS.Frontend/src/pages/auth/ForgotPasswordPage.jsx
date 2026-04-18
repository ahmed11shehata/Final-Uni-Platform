import { useState, useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import { forgotPasswordRequest, forgotPasswordVerify } from "../../services/api/authApi";
import s from "./ForgotPasswordPage.module.css";

const extractError = (err) => {
  const d = err?.response?.data;
  if (d?.error?.message) return d.error.message;
  if (d?.message) return d.message;
  return err?.message ?? "Something went wrong.";
};

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const fromSettings = location.state?.from === "settings";

  const [step, setStep]                       = useState(1);
  const [email, setEmail]                     = useState("");
  const [otp, setOtp]                         = useState(["", "", "", "", "", ""]);
  const [newPassword, setNewPassword]         = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showNew, setShowNew]                 = useState(false);
  const [showConfirm, setShowConfirm]         = useState(false);
  const [loading, setLoading]                 = useState(false);
  const [error, setError]                     = useState("");
  const [success, setSuccess]                 = useState(false);
  const [cooldown, setCooldown]               = useState(0);

  const otpRefs = useRef([]);

  useEffect(() => {
    if (cooldown <= 0) return;
    const t = setTimeout(() => setCooldown((c) => c - 1), 1000);
    return () => clearTimeout(t);
  }, [cooldown]);

  const startCooldown = () => setCooldown(60);

  // OTP input handler
  const handleOtpChange = (index, value) => {
    if (value && !/^\d$/.test(value)) return;
    const next = [...otp];
    next[index] = value;
    setOtp(next);
    if (value && index < 5) {
      otpRefs.current[index + 1]?.focus();
    }
  };

  const handleOtpKeyDown = (index, e) => {
    if (e.key === "Backspace" && !otp[index] && index > 0) {
      otpRefs.current[index - 1]?.focus();
    }
  };

  const handleOtpPaste = (e) => {
    e.preventDefault();
    const pasted = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
    if (!pasted) return;
    const next = [...otp];
    for (let i = 0; i < 6; i++) next[i] = pasted[i] || "";
    setOtp(next);
    const focusIdx = Math.min(pasted.length, 5);
    otpRefs.current[focusIdx]?.focus();
  };

  const handleRequestCode = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await forgotPasswordRequest(email);
      setStep(2);
      startCooldown();
      setTimeout(() => otpRefs.current[0]?.focus(), 100);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (cooldown > 0) return;
    setError("");
    setLoading(true);
    try {
      await forgotPasswordRequest(email);
      setOtp(["", "", "", "", "", ""]);
      startCooldown();
      setTimeout(() => otpRefs.current[0]?.focus(), 100);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  const handleVerify = async (e) => {
    e.preventDefault();
    setError("");
    const code = otp.join("");
    if (code.length < 6) { setError("Please enter the full 6-digit code."); return; }
    if (newPassword !== confirmPassword) { setError("Passwords do not match."); return; }
    if (newPassword.length < 8) { setError("Password must be at least 8 characters."); return; }
    setLoading(true);
    try {
      await forgotPasswordVerify({ email, code, newPassword, confirmPassword });
      setSuccess(true);
      setTimeout(() => navigate("/login", { replace: true }), 2500);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={s.page} style={{ background: "var(--page-bg)" }}>
      {/* Ambient background */}
      <div className={s.pageBg}>
        <div className={`${s.bgOrb} ${s.bgOrb1}`} />
        <div className={`${s.bgOrb} ${s.bgOrb2}`} />
        <div className={s.bgNoise} />
      </div>

      <motion.div
        className={s.card}
        initial={{ opacity: 0, y: 28, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.45, ease: "easeOut" }}
      >
        <div className={s.stripe} />
        <div className={s.body}>

          {/* Step indicator */}
          <div className={s.steps}>
            <div>
              <div className={`${s.stepDot} ${step >= 1 ? (step > 1 ? s.stepDotDone : s.stepDotActive) : ""}`}>
                {step > 1 ? (
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                ) : "1"}
              </div>
              <span className={s.stepLabel}>Email</span>
            </div>
            <div className={`${s.stepLine} ${step >= 2 ? s.stepLineActive : ""}`} />
            <div>
              <div className={`${s.stepDot} ${step >= 2 ? (success ? s.stepDotDone : s.stepDotActive) : ""}`}>
                {success ? (
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                ) : "2"}
              </div>
              <span className={s.stepLabel}>Verify</span>
            </div>
            <div className={`${s.stepLine} ${success ? s.stepLineActive : ""}`} />
            <div>
              <div className={`${s.stepDot} ${success ? s.stepDotDone : ""}`}>
                {success ? (
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                ) : "3"}
              </div>
              <span className={s.stepLabel}>Done</span>
            </div>
          </div>

          <AnimatePresence mode="wait">
            {success ? (
              /* ── SUCCESS ── */
              <motion.div
                key="success"
                className={s.successBox}
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.35 }}
              >
                <div className={s.successIcon}>
                  <svg width="42" height="42" viewBox="0 0 24 24" fill="none" stroke="#22c55e" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                  </svg>
                </div>
                <p className={s.successTitle}>Password Reset Successfully!</p>
                <p className={s.successDesc}>Redirecting you to login...</p>
              </motion.div>
            ) : step === 1 ? (
              /* ── STEP 1: Email ── */
              <motion.div key="step1" initial={{ opacity: 0, x: -16 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: 16 }} transition={{ duration: 0.25 }}>
                <h1 className={s.heading}>Forgot Password</h1>
                <p className={s.desc}>
                  Enter your university email address. A 6-digit verification code will be sent to your linked recovery email.
                </p>
                <form onSubmit={handleRequestCode}>
                  <div className={s.fieldGroup}>
                    <label className={s.fieldLabel}>University Email</label>
                    <input
                      className={s.input}
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="your@akhbaracademy.edu.eg"
                      required
                      autoFocus
                    />
                  </div>
                  {error && (
                    <div className={s.errorBox}>
                      <span className={s.errorIcon}>
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="#ef4444" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                          <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
                        </svg>
                      </span>
                      {error}
                    </div>
                  )}
                  <button type="submit" className={s.submitBtn} disabled={loading}>
                    {loading ? "Sending..." : "Send Verification Code"}
                  </button>
                </form>
                <button type="button" className={s.backBtn} onClick={() => fromSettings ? navigate(-1) : navigate("/login")}>
                  {fromSettings ? "← Back to Settings" : "← Back to Login"}
                </button>
              </motion.div>
            ) : (
              /* ── STEP 2: OTP + New Password ── */
              <motion.div key="step2" initial={{ opacity: 0, x: 16 }} animate={{ opacity: 1, x: 0 }} exit={{ opacity: 0, x: -16 }} transition={{ duration: 0.25 }}>
                <h1 className={s.heading}>Enter Verification Code</h1>
                <p className={s.desc}>
                  A code was sent to the recovery email linked to <span className={s.emailHighlight}>{email}</span>
                </p>

                <form onSubmit={handleVerify}>
                  {/* OTP boxes */}
                  <div className={s.otpRow} onPaste={handleOtpPaste}>
                    {otp.map((digit, i) => (
                      <input
                        key={i}
                        ref={(el) => (otpRefs.current[i] = el)}
                        className={s.otpDigit}
                        type="text"
                        inputMode="numeric"
                        maxLength={1}
                        value={digit}
                        onChange={(e) => handleOtpChange(i, e.target.value)}
                        onKeyDown={(e) => handleOtpKeyDown(i, e)}
                        autoFocus={i === 0}
                      />
                    ))}
                  </div>

                  <div className={s.fieldGroup}>
                    <label className={s.fieldLabel}>New Password</label>
                    <div className={s.inputWrap}>
                      <input
                        className={`${s.input} ${s.inputPassword}`}
                        type={showNew ? "text" : "password"}
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        placeholder="At least 8 characters"
                        required
                      />
                      <button type="button" className={s.toggleBtn} onClick={() => setShowNew((v) => !v)}>
                        {showNew ? "Hide" : "Show"}
                      </button>
                    </div>
                  </div>

                  <div className={s.fieldGroup}>
                    <label className={s.fieldLabel}>Confirm New Password</label>
                    <div className={s.inputWrap}>
                      <input
                        className={`${s.input} ${s.inputPassword}`}
                        type={showConfirm ? "text" : "password"}
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        placeholder="Re-enter new password"
                        required
                      />
                      <button type="button" className={s.toggleBtn} onClick={() => setShowConfirm((v) => !v)}>
                        {showConfirm ? "Hide" : "Show"}
                      </button>
                    </div>
                  </div>

                  {error && (
                    <div className={s.errorBox}>
                      <span className={s.errorIcon}>
                        <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="#ef4444" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                          <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
                        </svg>
                      </span>
                      {error}
                    </div>
                  )}

                  <button type="submit" className={s.submitBtn} disabled={loading}>
                    {loading ? "Verifying..." : "Reset Password"}
                  </button>
                </form>

                {/* Resend */}
                <p className={s.resendRow}>
                  Didn't receive it?{" "}
                  <button
                    type="button"
                    className={`${s.resendBtn} ${cooldown > 0 ? s.resendDisabled : ""}`}
                    onClick={handleResend}
                    disabled={cooldown > 0}
                  >
                    {cooldown > 0 ? `Resend in ${cooldown}s` : "Resend code"}
                  </button>
                </p>

                <button type="button" className={s.backBtn} onClick={() => { setStep(1); setError(""); setOtp(["","","","","",""]); }}>
                  ← Change email
                </button>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </motion.div>
    </div>
  );
}
