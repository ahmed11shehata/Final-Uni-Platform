// src/pages/admin/RegisterEmailPage.jsx — Full redesign: Split Panel Layout
import { useState, useRef, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import styles from "./RegisterEmailPage.module.css";
import {
  getEmails, createEmailAccount, toggleActive, resetPassword, deleteAccount, updateAccount,
  studentDeletePreview, studentDeleteExecute,
} from "../../services/api/adminApi";

const STUDENT_DELETE_PWD = "StudentDelete@123#";

const DOMAIN = "@akhbaracademy.edu.eg";

const EGYPT_GOVERNORATES = [
  "Alexandria","Assiut","Aswan","Beheira","Beni Suef","Cairo","Dakahlia",
  "Damietta","Fayoum","Gharbiya","Giza","Ismailia","Kafr el-Sheikh","Luxor",
  "Matrouh","Menoufiya","Minya","New Valley","North Sinai","Port Said","Qalyubia",
  "Qena","Red Sea","Sharqiya","Sohag","South Sinai","Suez",
];

function pwdRules(pw) {
  return [
    { ok: pw.length >= 8,                     label: "At least 8 characters" },
    { ok: /[A-Z]/.test(pw),                   label: "One uppercase letter" },
    { ok: /[a-z]/.test(pw),                   label: "One lowercase letter" },
    { ok: /\d/.test(pw),                       label: "One number" },
    { ok: /[^A-Za-z0-9]/.test(pw),            label: "One special character" },
  ];
}

const ROLES = [
  { key:"student",    label:"Student",    prefix:"cs",  icon:"🎓", color:"#818cf8", dark:"#4338ca", bg:"linear-gradient(135deg,#4338ca,#818cf8)" },
  { key:"instructor", label:"Instructor", prefix:"dr",  icon:"🏛",  color:"#22c55e", dark:"#15803d", bg:"linear-gradient(135deg,#15803d,#22c55e)" },
  { key:"admin",      label:"Admin",      prefix:"adm", icon:"⚡",  color:"#f59e0b", dark:"#b45309", bg:"linear-gradient(135deg,#b45309,#f59e0b)" },
];


function buildEmail(role, code, firstName, lastName) {
  const clean = s => s.trim().replace(/\s+/g,"").replace(/[^a-zA-Z0-9]/g,"");
  const r = ROLES.find(x=>x.key===role) || ROLES[0];
  const f=clean(firstName), l=clean(lastName), c=clean(code);
  if (!c||!f||!l) return "";
  return `${r.prefix}-${c}${f}${l}${DOMAIN}`;
}
function genPwd() {
  const chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMN23456789@#$!";
  return Array.from({length:14},()=>chars[Math.floor(Math.random()*chars.length)]).join("");
}

/* ── Icons ── */
const I = {
  search: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>,
  key:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><circle cx="7" cy="17" r="4"/><path d="M10.8 13.2L20 4"/><path d="M18 6l2 2"/><path d="M15 7l2 2"/></svg>,
  trash:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a1 1 0 011-1h4a1 1 0 011 1v2"/></svg>,
  pause:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/></svg>,
  play:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><polygon points="5 3 19 12 5 21 5 3"/></svg>,
  copy:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><rect x="9" y="9" width="13" height="13" rx="2"/><path d="M5 15H4a2 2 0 01-2-2V4a2 2 0 012-2h9a2 2 0 012 2v1"/></svg>,
  close:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>,
  eye:    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>,
  eyeoff: <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0112 20c-7 0-11-8-11-8a18.45 18.45 0 015.06-5.94M9.9 4.24A9.12 9.12 0 0112 4c7 0 11 8 11 8a18.5 18.5 0 01-2.16 3.19m-6.72-1.07a3 3 0 11-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>,
  plus:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>,
  back:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round"><polyline points="15 18 9 12 15 6"/></svg>,
  spark:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M12 2L15.09 8.26L22 9.27L17 14.14L18.18 21.02L12 17.77L5.82 21.02L7 14.14L2 9.27L8.91 8.26L12 2Z"/></svg>,
  check:  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.8" strokeLinecap="round"><polyline points="20 6 9 17 4 12"/></svg>,
  warn:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>,
  mail:   <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>,
};

const sp = { type:"spring", stiffness:400, damping:28 };

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^[+\d\s\-()\u0660-\u0669]{7,20}$/;

function getEditErrors(f) {
  const e = {};
  if (!f.firstName.trim())  e.firstName   = "Required";
  if (!f.lastName.trim())   e.lastName    = "Required";
  if (!f.subEmail.trim())   e.subEmail    = "Required";
  else if (!EMAIL_RE.test(f.subEmail.trim())) e.subEmail = "Invalid email format";
  if (!f.phone.trim())      e.phone       = "Required";
  else if (!PHONE_RE.test(f.phone.trim()))    e.phone    = "Invalid phone number";
  if (!f.gender)            e.gender      = "Required";
  if (!f.dob)               e.dob         = "Required";
  if (!f.governorate)       e.governorate = "Required";
  if (!f.location.trim())   e.location    = "Required";
  return e;
}

/* ═══════════════════════════════════════
   ENVELOPE SCENE
═══════════════════════════════════════ */
function EnvelopeScene({ roleColor }) {
  return (
    <motion.div className={styles.envScene}>
      <motion.div className={styles.envWrap}
        initial={{ scale:0.2, opacity:0, y:80, rotate:-8 }}
        animate={{
          scale:   [0.2, 1.08, 1, 1, 0.4],
          opacity: [0,   1,    1, 1, 0],
          y:       [80,  0,    0, -10, -90],
          x:       [0,   0,    0, 0,   420],
          rotate:  [-8,  0,    0, 0,   16],
        }}
        transition={{ duration:1.8, times:[0,.22,.32,.55,1], ease:"easeInOut" }}>
        <svg viewBox="0 0 200 136" fill="none" xmlns="http://www.w3.org/2000/svg" className={styles.envSvg}>
          <defs>
            <linearGradient id="eg" x1="0" y1="0" x2="200" y2="136" gradientUnits="userSpaceOnUse">
              <stop offset="0%" stopColor="#4338ca"/><stop offset="100%" stopColor="#6366f1"/>
            </linearGradient>
          </defs>
          <ellipse cx="100" cy="130" rx="60" ry="6" fill="rgba(0,0,0,0.2)"/>
          <rect x="8" y="24" width="184" height="100" rx="14" fill="url(#eg)"/>
          <rect x="8" y="24" width="184" height="44" rx="14" fill="rgba(255,255,255,0.08)"/>
          <path d="M8 36 L100 84 L192 36" stroke="rgba(255,255,255,0.25)" strokeWidth="2.5" fill="none" strokeLinejoin="round"/>
          <path d="M8 124 L68 80" stroke="rgba(255,255,255,0.18)" strokeWidth="2"/>
          <path d="M192 124 L132 80" stroke="rgba(255,255,255,0.18)" strokeWidth="2"/>
          <motion.g initial={{y:20,opacity:0}} animate={{y:[20,0],opacity:[0,1]}} transition={{delay:.3,duration:.3}}>
            <rect x="62" y="10" width="76" height="54" rx="6" fill="white" opacity="0.95"/>
            <rect x="70" y="20" width="60" height="4" rx="2" fill={roleColor} opacity="0.7"/>
            <rect x="70" y="30" width="44" height="3" rx="1.5" fill="rgba(0,0,0,0.15)"/>
            <rect x="70" y="38" width="50" height="3" rx="1.5" fill="rgba(0,0,0,0.1)"/>
          </motion.g>
          <circle cx="100" cy="68" r="22" fill={roleColor}/>
          <text x="100" y="75" textAnchor="middle" fill="white" fontSize="20" fontWeight="900" fontFamily="monospace">@</text>
          <motion.circle cx="34" cy="30" r="4" fill="#fbbf24" animate={{scale:[0,1.2,1],opacity:[0,1,.8]}} transition={{delay:.35,duration:.4}}/>
          <motion.circle cx="166" cy="34" r="3.5" fill="#34d399" animate={{scale:[0,1.2,1],opacity:[0,1,.8]}} transition={{delay:.45,duration:.4}}/>
          <motion.circle cx="172" cy="100" r="3" fill="#f87171" animate={{scale:[0,1.2,1],opacity:[0,1,.7]}} transition={{delay:.55,duration:.4}}/>
        </svg>
        <motion.div className={styles.envCheck}
          initial={{scale:0,opacity:0}} animate={{scale:1,opacity:1}}
          transition={{delay:.45,type:"spring",stiffness:520,damping:22}}>✓</motion.div>
      </motion.div>
      <motion.p className={styles.envText} initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} transition={{delay:.28}}>
        Creating account…
      </motion.p>
    </motion.div>
  );
}

/* ═══════════════════════════════════════
   MAIN PAGE
═══════════════════════════════════════ */
export default function RegisterEmailPage() {
  const navigate = useNavigate();
  const [db,         setDb]         = useState([]);
  const [search,     setSearch]     = useState("");
  const [result,     setResult]     = useState(null);
  const [mode,       setMode]       = useState("home"); // home|found|notfound|changepwd|confirmdelete|create|success
  const [activeTab,  setActiveTab]  = useState("overview"); // overview|password|danger
  const [activeRole, setActiveRole] = useState(null); // sidebar filter
  const [createStep, setCreateStep] = useState(1);
  const [createRole, setCreateRole] = useState("student");
  const [form,       setForm]       = useState({code:"",firstName:"",lastName:"",password:"",subEmail:"",phone:"",address:"",gender:"",dob:""});
  const [govSearch,  setGovSearch]  = useState("");
  const [govOpen,    setGovOpen]    = useState(false);
  const [newPwd,     setNewPwd]     = useState("");
  const [showPwd,    setShowPwd]    = useState(false);
  const [copied,     setCopied]     = useState(null);
  const [toast,      setToast]      = useState(null);
  const [emailAnim,  setEmailAnim]  = useState(false);
  const [createdAccount, setCreatedAccount] = useState(null);
  const [sideSearch, setSideSearch] = useState("");
  const [editForm,   setEditForm]   = useState({firstName:"",lastName:"",subEmail:"",phone:"",gender:"",dob:"",governorate:"",location:""});
  const [editBusy,   setEditBusy]   = useState(false);
  const [editGovSearch,  setEditGovSearch]  = useState("");
  const [editGovOpen,    setEditGovOpen]    = useState(false);
  const [editAttempted,  setEditAttempted]  = useState(false);
  // ── Permanent student delete (Danger Zone) ──
  const [dzPreview,      setDzPreview]      = useState(null);
  const [dzPreviewBusy,  setDzPreviewBusy]  = useState(false);
  const [dzPreviewErr,   setDzPreviewErr]   = useState(null);
  const [dzCodeInput,    setDzCodeInput]    = useState("");
  const [dzPwdInput,     setDzPwdInput]     = useState("");
  const [dzAck,          setDzAck]          = useState(false);
  const [dzShowFinal,    setDzShowFinal]    = useState(false);
  const [dzBusy,         setDzBusy]         = useState(false);
  const [dzResult,       setDzResult]       = useState(null);
  const [dzExecError,    setDzExecError]    = useState(null);
  const inputRef = useRef(null);
  const govWrapRef = useRef(null);
  const editGovWrapRef = useRef(null);

  const loadAccounts = () => {
    getEmails().then(data => {
      setDb((data.accounts||[]).map(a=>({
        id:a.id, code:a.code, firstName:a.firstName, lastName:a.lastName,
        role:a.role, active:a.active, createdAt:a.createdAt,
        password:"••••••••••••••", subEmail:a.subEmail??"",
        phone:a.phone??"", gender:a.gender??"",
        dob:a.dateOfBirth?(a.dateOfBirth.includes("T")?a.dateOfBirth.split("T")[0]:a.dateOfBirth):"",
        address:a.address??"",
      })));
    }).catch(()=>{});
  };

  useEffect(()=>{ loadAccounts(); },[]); // eslint-disable-line react-hooks/exhaustive-deps
  useEffect(()=>{ if(mode==="home") inputRef.current?.focus(); },[mode]);
  useEffect(()=>{
    if(!govOpen) return;
    const h=(e)=>{
      if(govWrapRef.current && !govWrapRef.current.contains(e.target)) setGovOpen(false);
    };
    document.addEventListener("mousedown",h);
    return()=>document.removeEventListener("mousedown",h);
  },[govOpen]);

  useEffect(()=>{
    if(!editGovOpen) return;
    const h=(e)=>{
      if(editGovWrapRef.current && !editGovWrapRef.current.contains(e.target)) setEditGovOpen(false);
    };
    document.addEventListener("mousedown",h);
    return()=>document.removeEventListener("mousedown",h);
  },[editGovOpen]);

  const showToast=(msg,type="ok")=>{ setToast({msg,type}); setTimeout(()=>setToast(null),2800); };
  const copyText=(text,id)=>{ navigator.clipboard?.writeText(text).catch(()=>{}); setCopied(id); setTimeout(()=>setCopied(null),1600); showToast("Copied ✓"); };

  const doSearch=()=>{
    const q=search.trim().toUpperCase(); if(!q) return;
    const found=db.find(e=>e.code.toUpperCase()===q);
    setResult(found||null); setMode(found?"found":"notfound"); setActiveTab("overview"); setActiveRole(null);
  };

  const toggle=async(entry)=>{
    try {
      await toggleActive(entry.id);
      const u={...entry,active:!entry.active};
      setDb(p=>p.map(e=>e.id===entry.id?u:e));
      if(result?.id===entry.id) setResult(u);
      showToast(u.active?"Account activated ✓":"Account suspended");
    } catch { showToast("Failed to toggle account","err"); }
  };

  const del=async()=>{
    try {
      await deleteAccount(result.id);
      setResult(null); setMode("home"); setSearch(""); setActiveRole(null);
      loadAccounts();
      showToast("Account deleted");
    } catch { showToast("Failed to delete account","err"); }
  };

  const savePwd=async()=>{
    if(!newPwd||newPwd.length<8){showToast("Password must be at least 8 characters","err");return;}
    try {
      await resetPassword(result.id, newPwd);
      showToast("Password reset ✓");
    } catch { showToast("Failed to reset password","err"); }
  };

  useEffect(()=>{
    if(!result) return;
    const raw = result.address||"";
    const matchedGov = EGYPT_GOVERNORATES.find(g=>raw.startsWith(g));
    const gov = matchedGov||"";
    const loc = matchedGov ? raw.slice(matchedGov.length).replace(/^[,\s]+/,"") : raw;
    setEditForm({
      firstName:   result.firstName||"",
      lastName:    result.lastName||"",
      subEmail:    result.subEmail||"",
      phone:       result.phone||"",
      gender:      (result.gender||"").toLowerCase(),
      dob:         result.dob||"",
      governorate: gov,
      location:    loc,
    });
    setEditAttempted(false);
    setEditGovSearch("");
    setEditGovOpen(false);
  },[result?.id]); // eslint-disable-line react-hooks/exhaustive-deps

  // Reset Danger Zone state whenever the loaded account changes or the active tab leaves Danger.
  useEffect(()=>{
    setDzPreview(null);
    setDzPreviewErr(null);
    setDzCodeInput("");
    setDzPwdInput("");
    setDzAck(false);
    setDzShowFinal(false);
    setDzResult(null);
    setDzExecError(null);
  },[result?.id, activeTab]);

  // Auto-fetch the student preview when the admin opens the Danger tab on a student account.
  useEffect(()=>{
    if (activeTab !== "danger") return;
    if (!result || result.role !== "student") return;
    if (!result.code) return;
    setDzPreviewBusy(true); setDzPreviewErr(null);
    studentDeletePreview(result.code)
      .then(setDzPreview)
      .catch(e => setDzPreviewErr(e?.response?.data?.error?.message || "Failed to load student preview"))
      .finally(() => setDzPreviewBusy(false));
  },[activeTab, result?.id, result?.role, result?.code]);

  const dzCanDelete = !!dzPreview
    && dzCodeInput === (result?.code || "")
    && dzPwdInput === STUDENT_DELETE_PWD
    && dzAck
    && !dzBusy;

  const dzExecute = async () => {
    if (!dzCanDelete) return;
    setDzBusy(true);
    setDzExecError(null);
    try {
      const data = await studentDeleteExecute({
        academicCode: result.code,
        confirmAcademicCode: dzCodeInput,
        password: dzPwdInput,
      });
      setDzResult(data);
      setDzShowFinal(false);
      // Refresh the accounts list — the deleted account vanishes.
      loadAccounts();
      showToast("Student permanently deleted ✓");
    } catch (e) {
      setDzExecError(e?.response?.data?.error?.message || "Permanent delete failed");
    } finally {
      setDzBusy(false);
    }
  };

  const dzReturnHome = () => {
    setDzResult(null);
    setResult(null);
    setMode("home");
    setSearch("");
    setActiveRole(null);
  };

  const saveEdit=async()=>{
    setEditAttempted(true);
    const errs = getEditErrors(editForm);
    if(Object.keys(errs).length>0){ showToast("Fix the errors before saving","err"); return; }
    const {firstName,lastName,subEmail,phone,gender,dob,governorate,location}=editForm;
    const combinedAddress = location.trim() ? `${governorate}, ${location.trim()}` : governorate;
    setEditBusy(true);
    try {
      const payload={
        firstName:   firstName.trim(),
        lastName:    lastName.trim(),
        subEmail:    subEmail.trim(),
        phone:       phone.trim(),
        gender,
        dateOfBirth: dob,
        address:     combinedAddress,
      };
      await updateAccount(result.id, payload);
      const updated={...result,...payload,dob,address:combinedAddress};
      setResult(updated);
      setDb(p=>p.map(e=>e.id===result.id?{...e,...payload,dob,address:combinedAddress}:e));
      showToast("Account updated ✓");
      setEditAttempted(false);
      setActiveTab("overview");
    } catch(err){
      showToast(err?.response?.data?.message||"Failed to update account","err");
    } finally { setEditBusy(false); }
  };

  const create=async()=>{
    const {code,firstName,lastName,password,subEmail,phone,address,gender,dob}=form;
    if(!code.trim()||!firstName.trim()||!lastName.trim()||!subEmail.trim()) return;
    if(!gender||!dob||!address) return;
    setEmailAnim(true);
    const safetyTimer = setTimeout(()=>setEmailAnim(false), 4000);
    try {
      const payload={code:code.trim(),firstName:firstName.trim(),lastName:lastName.trim(),role:createRole,subEmail:subEmail.trim()};
      if(password.trim()) payload.password=password.trim();
      if(phone.trim()) payload.phone=phone.trim();
      if(gender) payload.gender=gender;
      if(dob) payload.dateOfBirth=dob;
      if(address) payload.address=address;
      const created=await createEmailAccount(payload);
      loadAccounts();
      const resolvedCode = created?.code || code.trim();
      setTimeout(()=>{
        clearTimeout(safetyTimer);
        setEmailAnim(false);
        setCreatedAccount({
          id:        created?.id        || "",
          code:      resolvedCode,
          email:     created?.email     || "",
          role:      createRole,
          active:    true,
          createdAt: created?.createdAt || new Date().toISOString().slice(0,10),
          password:  created?.password  || "",
          firstName: firstName.trim(),
          lastName:  lastName.trim(),
          subEmail:  subEmail.trim(),
        });
        setForm({code:"",firstName:"",lastName:"",password:"",subEmail:"",phone:"",address:"",gender:"",dob:""});
        setGovSearch(""); setGovOpen(false);
        setCreateStep(1); setActiveRole(null);
        setMode("success");
      }, 2000);
    } catch(err) {
      clearTimeout(safetyTimer);
      setEmailAnim(false);
      showToast(err?.response?.data?.message || "Failed to create account","err");
    }
  };

  const sideList = useMemo(()=>{
    let list = [...db];
    if(activeRole) list = list.filter(e=>activeRole==="suspended"?!e.active:e.role===activeRole);
    if(sideSearch.trim()) {
      const q = sideSearch.trim().toLowerCase();
      list = list.filter(e=>
        e.firstName.toLowerCase().includes(q) ||
        e.lastName.toLowerCase().includes(q)  ||
        e.code.toLowerCase().includes(q)
      );
    }
    return list;
  },[db,activeRole,sideSearch]);

  const stats = [
    { key:"student",    label:"Students",    n:db.filter(e=>e.role==="student").length,    c:"#818cf8", bg:"linear-gradient(135deg,#4338ca,#818cf8)" },
    { key:"instructor", label:"Instructors", n:db.filter(e=>e.role==="instructor").length, c:"#22c55e", bg:"linear-gradient(135deg,#15803d,#22c55e)" },
    { key:"admin",      label:"Admins",      n:db.filter(e=>e.role==="admin").length,      c:"#f59e0b", bg:"linear-gradient(135deg,#b45309,#f59e0b)" },
    { key:"suspended",  label:"Suspended",   n:db.filter(e=>!e.active).length,             c:"#ef4444", bg:"linear-gradient(135deg,#991b1b,#ef4444)" },
  ];

  const email     = result ? buildEmail(result.role,result.code,result.firstName,result.lastName) : "";
  const roleData  = ROLES.find(r=>r.key===result?.role)||ROLES[0];
  const prevEmail = buildEmail(createRole,form.code,form.firstName,form.lastName);

  return (
    <div className={styles.page}>

      <AnimatePresence>
        {toast&&(
          <motion.div className={`${styles.toast} ${toast.type==="ok"?styles.toastOk:styles.toastErr}`}
            initial={{opacity:0,y:-28,x:"-50%",scale:.9}} animate={{opacity:1,y:0,x:"-50%",scale:1}}
            exit={{opacity:0,y:-16,x:"-50%"}} transition={sp}>
            <span className={styles.toastDot}/>{toast.msg}
          </motion.div>
        )}
      </AnimatePresence>

      <div className={styles.topBar}>
        <div className={styles.topBarLeft}>
          <div className={styles.atBadge}>
            <span>@</span>
            <div className={styles.atBadgeRing}/>
          </div>
          <div>
            <h1 className={styles.topBarTitle}>Email Manager</h1>
            <p className={styles.topBarDomain}>{DOMAIN.slice(1)}</p>
          </div>
        </div>

        <div className={styles.topBarCenter}>
          {stats.map((s)=>(
            <motion.button
              key={s.key}
              className={`${styles.topStatBtn} ${activeRole===s.key?styles.topStatBtnOn:""}`}
              style={activeRole===s.key ? { background:s.bg, borderColor:"transparent" } : {"--statColor":s.c}}
              onClick={()=>{ setSideSearch(""); setActiveRole(p=>p===s.key?null:s.key); }}
              whileHover={{ y:-2, scale:1.03 }}
              whileTap={{ scale:.97 }}
            >
              <span className={styles.topStatNum}>{s.n}</span>
              <span className={styles.topStatLabel}>{s.label}</span>
            </motion.button>
          ))}
        </div>

        <motion.button className={styles.newAccBtn}
          onClick={()=>{setMode("create");setCreateStep(1);setActiveRole(null);}}
          whileHover={{scale:1.04}} whileTap={{scale:.96}}>
          <span className={styles.newAccBtnIcon}>{I.plus}</span>
          New Account
        </motion.button>
      </div>

      <div className={styles.split}>
        <main className={styles.main}>
          <div className={styles.searchRow}>
            <div className={`${styles.searchBox} ${mode==="found"?styles.searchOk:mode==="notfound"?styles.searchErr:""}`}>
              <span className={styles.searchIco}>{I.search}</span>
              <input ref={inputRef} className={styles.searchInp}
                value={search} placeholder="Enter account code"
                onChange={e=>{setSearch(e.target.value);if(mode!=="home"){setMode("home");setResult(null);}}}
                onKeyDown={e=>e.key==="Enter"&&doSearch()} autoComplete="off" spellCheck={false}/>
              {search&&(
                <button className={styles.clearX} onClick={()=>{setSearch("");setMode("home");setResult(null);}}>
                  {I.close}
                </button>
              )}
            </div>
            <motion.button className={styles.searchGoBtn} onClick={doSearch} whileHover={{scale:1.04}} whileTap={{scale:.96}}>
              Search
            </motion.button>
          </div>

          <div className={styles.fmtHints}>
            {ROLES.map(r=>(
              <span key={r.key} className={styles.fmtHint}>
                <span style={{color:r.color,fontWeight:800}}>{r.prefix}-</span>
                <span>code·Name@domain</span>
              </span>
            ))}
          </div>

          <div className={styles.contentBox}>
            <AnimatePresence mode="wait">
              {mode==="home"&&(
                <motion.div key="home" className={styles.homePane}
                  initial={{opacity:0,scale:.96}} animate={{opacity:1,scale:1}} exit={{opacity:0}}>
                  <div className={styles.homeIllusWrap}>
                    <svg viewBox="0 0 120 88" fill="none" xmlns="http://www.w3.org/2000/svg" className={styles.homeIllusSvg}>
                      <ellipse cx="60" cy="84" rx="36" ry="4" fill="var(--border)" opacity="0.5"/>
                      <rect x="8" y="16" width="104" height="64" rx="10" fill="var(--card-bg)" stroke="var(--border)" strokeWidth="2"/>
                      <path d="M8 26 L60 54 L112 26" stroke="var(--border)" strokeWidth="2" fill="none" strokeLinejoin="round"/>
                      <path d="M8 80 L42 52" stroke="var(--border)" strokeWidth="1.5" opacity="0.5"/>
                      <path d="M112 80 L78 52" stroke="var(--border)" strokeWidth="1.5" opacity="0.5"/>
                      <circle cx="60" cy="38" r="14" fill="#818cf8"/>
                      <text x="60" y="43" textAnchor="middle" fill="white" fontSize="14" fontWeight="900" fontFamily="monospace">@</text>
                      <circle cx="28" cy="70" r="5" fill="#818cf8" opacity="0.7"/>
                      <circle cx="60" cy="74" r="4" fill="#22c55e" opacity="0.7"/>
                      <circle cx="92" cy="70" r="5" fill="#f59e0b" opacity="0.7"/>
                    </svg>
                  </div>
                  <div className={styles.homeSearchTitle}>Search for account</div>
                  <div className={styles.homeSamples}>
                    {["2203119","INS001","ADM001","2203122"].map(c=>(
                      <button key={c} className={styles.sampleBtn} onClick={()=>setSearch(c)}>{c}</button>
                    ))}
                  </div>
                </motion.div>
              )}

              {mode==="notfound"&&(
                <motion.div key="nf" className={styles.notFoundPane}
                  initial={{opacity:0,y:20}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
                  <motion.div className={styles.nfIconWrap}
                    animate={{rotate:[0,-10,10,-5,5,0]}} transition={{duration:.6,delay:.1}}>
                    {I.warn}
                  </motion.div>
                  <h3 className={styles.nfTitle}>No account found for <strong>"{search}"</strong></h3>
                  <p className={styles.nfSub}>Would you like to create a new account with this code?</p>
                  <motion.button className={styles.nfCreateBtn}
                    onClick={()=>{setForm(p=>({...p,code:search}));setMode("create");setCreateStep(2);setCreateRole("student");}}
                    whileHover={{scale:1.03,y:-2}} whileTap={{scale:.97}}>
                    {I.plus} Create account for "{search}"
                  </motion.button>
                </motion.div>
              )}

              {mode==="found"&&result&&(
                <motion.div key={`found-${result.id}`} className={styles.foundPane}
                  initial={{opacity:0,x:24,scale:.97}} animate={{opacity:1,x:0,scale:1}}
                  exit={{opacity:0,x:-16}} transition={{...sp}}>

                  <div className={styles.idStrip} style={{background:roleData.bg}}>
                    <div className={styles.idStripAvatar}>
                      {result.firstName[0]}{result.lastName[0]}
                      {!result.active&&<div className={styles.idStripBan}/>}
                    </div>
                    <div className={styles.idStripInfo}>
                      <span className={styles.idName}>{result.firstName} {result.lastName}</span>
                      <span className={styles.idMeta}>
                        {roleData.icon} {roleData.label} · #{result.code} · {result.createdAt}
                      </span>
                    </div>
                    <div className={styles.idStripRight}>
                      <button
                        className={`${styles.idStatusBtn} ${result.active?styles.idStatusActive:styles.idStatusSuspended}`}
                        onClick={()=>toggle(result)}>
                        <span className={styles.idStatusDot}/>
                        {result.active?"Active":"Suspended"}
                      </button>
                    </div>
                  </div>

                  <div className={styles.tabsRow}>
                    {[
                      {k:"overview", label:"📧 Overview"},
                      {k:"edit",     label:"✏️ Edit"},
                      {k:"password", label:"🔑 Password"},
                      {k:"danger",   label:"⚠️ Danger Zone"},
                    ].map(t=>(
                      <button key={t.k}
                        className={`${styles.tab} ${activeTab===t.k?styles.tabOn:""}`}
                        style={activeTab===t.k?{"--tc":t.k==="danger"?"#ef4444":roleData.color}:{}}
                        onClick={()=>setActiveTab(t.k)}>
                        {t.label}
                      </button>
                    ))}
                  </div>

                  <AnimatePresence mode="wait">
                    {activeTab==="overview"&&(
                      <motion.div key="ov" className={styles.tabContent}
                        initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} exit={{opacity:0}}>

                        <div className={styles.emailCard}>
                          <div className={styles.emailCardIcon} style={{background:roleData.bg}}>
                            {I.mail}
                          </div>
                          <div className={styles.emailCardBody}>
                            <div className={styles.emailCardLabel}>Email Address</div>
                            <div className={styles.emailCardAddr}>
                              <span style={{color:roleData.color,fontWeight:900}}>{email.split("@")[0]}</span>
                              <span className={styles.emailCardAt}>@</span>
                              <span className={styles.emailCardDomain}>akhbaracademy.edu.eg</span>
                            </div>
                          </div>
                          <motion.button
                            className={`${styles.copyEmailBtn} ${copied===result.id?styles.copyEmailBtnDone:""}`}
                            style={copied===result.id?{}:{background:roleData.bg}}
                            onClick={()=>copyText(email,result.id)}
                            whileHover={{scale:1.06}} whileTap={{scale:.92}}>
                            <AnimatePresence mode="wait">
                              {copied===result.id
                                ? <motion.span key="y" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.check}</motion.span>
                                : <motion.span key="n" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.copy}</motion.span>
                              }
                            </AnimatePresence>
                            <span>{copied===result.id?"Copied!":"Copy"}</span>
                          </motion.button>
                        </div>

                        <div className={styles.infoGrid}>
                          {[
                            {l:"First Name", v:result.firstName},
                            {l:"Last Name",  v:result.lastName},
                            {l:"Code",       v:"#"+result.code, mono:true},
                            {l:"Role",       v:roleData.label},
                            {l:"Status",     v:result.active?"Active":"Suspended", c:result.active?"#22c55e":"#ef4444"},
                            {l:"Created",    v:result.createdAt},
                          ].map(item=>(
                            <div key={item.l} className={styles.infoCell}>
                              <span className={styles.infoCellLabel}>{item.l}</span>
                              <span className={styles.infoCellVal} style={{color:item.c||"var(--text-primary)",fontFamily:item.mono?"monospace":undefined}}>
                                {item.v}
                              </span>
                            </div>
                          ))}
                        </div>

                        <div className={styles.quickActions}>
                          <motion.button className={styles.qaBtn} style={{background:"linear-gradient(135deg,#f59e0b,#fbbf24)"}}
                            onClick={()=>toggle(result)} whileHover={{scale:1.04,y:-2}} whileTap={{scale:.96}}>
                            <span>{result.active?I.pause:I.play}</span>
                            {result.active?"Suspend Account":"Activate Account"}
                          </motion.button>
                        </div>
                      </motion.div>
                    )}

                    {activeTab==="edit"&&(()=>{
                      const errs = getEditErrors(editForm);
                      const hasErr = k => editAttempted && !!errs[k];
                      const isOk  = k => !errs[k];
                      const fCls  = k => [
                        styles.formInput,
                        hasErr(k) ? styles.editInputErr : "",
                        editAttempted && isOk(k) ? styles.editInputOk : "",
                      ].filter(Boolean).join(" ");
                      const filteredEditGovs = EGYPT_GOVERNORATES.filter(g=>
                        g.toLowerCase().includes(editGovSearch.toLowerCase()));
                      const canSave = Object.keys(errs).length===0;

                      return (
                        <motion.div key="ed" className={styles.tabContent}
                          initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
                          <div className={styles.editSection}>
                            <div className={styles.editGrid}>

                              <div className={styles.editReadOnly}>
                                <div className={styles.editRoField}>
                                  <span className={styles.editRoLabel}>University Email</span>
                                  <span className={styles.editRoVal} style={{fontFamily:"monospace",color:roleData.color}}>{email||"—"}</span>
                                </div>
                                <div className={styles.editRoField}>
                                  <span className={styles.editRoLabel}>Academic Code</span>
                                  <span className={styles.editRoVal} style={{fontFamily:"monospace"}}>#{result.code}</span>
                                </div>
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>First Name <span className={styles.req}>*</span></label>
                                <input className={fCls("firstName")} value={editForm.firstName}
                                  onChange={e=>setEditForm(p=>({...p,firstName:e.target.value}))}
                                  placeholder="First name"/>
                                {hasErr("firstName")&&<span className={styles.editErr}>{errs.firstName}</span>}
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Last Name <span className={styles.req}>*</span></label>
                                <input className={fCls("lastName")} value={editForm.lastName}
                                  onChange={e=>setEditForm(p=>({...p,lastName:e.target.value}))}
                                  placeholder="Last name"/>
                                {hasErr("lastName")&&<span className={styles.editErr}>{errs.lastName}</span>}
                              </div>

                              <div className={`${styles.formField} ${styles.editFieldFull}`}>
                                <div className={styles.formLabelRow}>
                                  <label className={styles.formLabel}>Recovery Email <span className={styles.req}>*</span></label>
                                  <span className={styles.editSubHint}>OTP &amp; temp password destination</span>
                                </div>
                                <input className={fCls("subEmail")} type="email" value={editForm.subEmail}
                                  onChange={e=>setEditForm(p=>({...p,subEmail:e.target.value}))}
                                  placeholder="personal@gmail.com"/>
                                {hasErr("subEmail")&&<span className={styles.editErr}>{errs.subEmail}</span>}
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Phone <span className={styles.req}>*</span></label>
                                <input className={fCls("phone")} type="tel" value={editForm.phone}
                                  onChange={e=>setEditForm(p=>({...p,phone:e.target.value}))}
                                  placeholder="+20 1xxxxxxxxx"/>
                                {hasErr("phone")&&<span className={styles.editErr}>{errs.phone}</span>}
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Gender <span className={styles.req}>*</span></label>
                                <select className={fCls("gender")} value={editForm.gender}
                                  onChange={e=>setEditForm(p=>({...p,gender:e.target.value}))}>
                                  <option value="">Select gender...</option>
                                  <option value="male">Male</option>
                                  <option value="female">Female</option>
                                </select>
                                {hasErr("gender")&&<span className={styles.editErr}>{errs.gender}</span>}
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Date of Birth <span className={styles.req}>*</span></label>
                                <input className={fCls("dob")} type="date" value={editForm.dob}
                                  onChange={e=>setEditForm(p=>({...p,dob:e.target.value}))}
                                  max={new Date(new Date().setFullYear(new Date().getFullYear()-15)).toISOString().slice(0,10)}/>
                                {hasErr("dob")&&<span className={styles.editErr}>{errs.dob}</span>}
                              </div>

                              <div className={`${styles.formField} ${styles.editGovField}`} ref={editGovWrapRef}>
                                <label className={styles.formLabel}>Governorate <span className={styles.req}>*</span></label>
                                <button
                                  type="button"
                                  className={[styles.editGovBox, hasErr("governorate")?styles.editInputErr:"", editAttempted&&isOk("governorate")?styles.editInputOk:""].filter(Boolean).join(" ")}
                                  onClick={()=>setEditGovOpen(v=>!v)}
                                >
                                  <span className={styles.editGovMeta}>Governorate</span>
                                  <span className={editForm.governorate ? styles.editGovValue : styles.editGovPlaceholder}>
                                    {editForm.governorate || "Choose governorate..."}
                                  </span>
                                  <span className={`${styles.editGovArrow} ${editGovOpen?styles.editGovArrowOpen:""}`}>▾</span>
                                </button>

                                <AnimatePresence>
                                  {editGovOpen&&(
                                    <motion.div className={styles.editGovDropdown}
                                      initial={{opacity:0,y:8,scale:.98}} animate={{opacity:1,y:0,scale:1}} exit={{opacity:0,y:8,scale:.98}}
                                      transition={{duration:.16}}>
                                      <div className={styles.editGovList}>
                                        {filteredEditGovs.length===0&&<div className={styles.editGovEmpty}>No governorate found</div>}
                                        {filteredEditGovs.map(g=>{
                                          const active = editForm.governorate===g;
                                          return (
                                            <button key={g} type="button" className={`${styles.editGovItem} ${active?styles.editGovItemActive:""}`}
                                              onClick={()=>{setEditForm(p=>({...p,governorate:g}));setEditGovSearch("");setEditGovOpen(false);}}>
                                              <span>{g}</span>
                                              {active && <span className={styles.editGovCheck}>✓</span>}
                                            </button>
                                          );
                                        })}
                                      </div>
                                      <div className={styles.editGovSearchWrap}>
                                        <input
                                          autoFocus
                                          className={styles.editGovSearchInp}
                                          value={editGovSearch}
                                          onChange={e=>setEditGovSearch(e.target.value)}
                                          placeholder="Search governorate..."
                                        />
                                        {editForm.governorate && (
                                          <button
                                            type="button"
                                            className={styles.editGovReset}
                                            onClick={()=>{
                                              setEditForm(p=>({...p,governorate:""}));
                                              setEditGovSearch("");
                                            }}
                                          >
                                            Clear
                                          </button>
                                        )}
                                      </div>
                                    </motion.div>
                                  )}
                                </AnimatePresence>
                                {hasErr("governorate")&&<span className={styles.editErr}>{errs.governorate}</span>}
                              </div>

                              <div className={`${styles.formField} ${styles.editFieldFull}`}>
                                <label className={styles.formLabel}>Street / Area <span className={styles.req}>*</span></label>
                                <input className={fCls("location")} value={editForm.location}
                                  onChange={e=>setEditForm(p=>({...p,location:e.target.value}))}
                                  placeholder="e.g. Nasr City, Block 5"/>
                                {hasErr("location")&&<span className={styles.editErr}>{errs.location}</span>}
                              </div>
                            </div>

                            <AnimatePresence>
                              {editAttempted&&!canSave&&(
                                <motion.div className={styles.editBanner}
                                  initial={{opacity:0,height:0}} animate={{opacity:1,height:"auto"}} exit={{opacity:0,height:0}}>
                                  {Object.keys(errs).length} field{Object.keys(errs).length!==1?"s":""} need attention before saving
                                </motion.div>
                              )}
                            </AnimatePresence>

                            <div className={styles.editActionRow}>
                              <button className={styles.btnGhost}
                                onClick={()=>{setActiveTab("overview");setEditAttempted(false);}}>Cancel</button>
                              <motion.button
                                className={`${styles.btnSave} ${editAttempted&&!canSave?styles.btnSaveBlocked:""}`}
                                style={canSave||!editAttempted?{background:roleData.bg}:{}}
                                onClick={saveEdit} disabled={editBusy}
                                whileHover={{scale:canSave?1.02:1}} whileTap={{scale:.97}}>
                                {editBusy?"Saving…":"Save Changes"}
                              </motion.button>
                            </div>

                          </div>
                        </motion.div>
                      );
                    })()}

                    {activeTab==="password"&&(
                      <motion.div key="pw" className={styles.tabContent}
                        initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
                        <div className={styles.pwdSection}>
                          <h3 className={styles.pwdSectionTitle}>Change Password</h3>
                          <p className={styles.pwdSectionSub}>Set a new password for {result.firstName} {result.lastName}</p>

                          <div className={styles.pwdField} style={newPwd?{borderColor:`${roleData.color}60`}:{}}>
                            <input type={showPwd?"text":"password"} className={styles.pwdInput}
                              value={newPwd} onChange={e=>setNewPwd(e.target.value)}
                              placeholder="Enter new password" autoFocus/>
                            <button className={styles.pwdEye} onClick={()=>setShowPwd(p=>!p)}>
                              {showPwd?I.eyeoff:I.eye}
                            </button>
                          </div>

                          <motion.button className={styles.genBtn}
                            onClick={()=>setNewPwd(genPwd())} whileHover={{scale:1.02}} whileTap={{scale:.97}}>
                            {I.spark} Generate strong password
                          </motion.button>

                          <AnimatePresence>
                            {newPwd&&(
                              <motion.div className={styles.pwdPreview}
                                initial={{opacity:0,height:0}} animate={{opacity:1,height:"auto"}} exit={{opacity:0,height:0}}>
                                <code className={styles.pwdPreviewCode}>{newPwd}</code>
                                <button className={styles.pwdPreviewCopy}
                                  onClick={()=>copyText(newPwd,"preview")}>{copied==="preview"?I.check:I.copy}</button>
                              </motion.div>
                            )}
                          </AnimatePresence>

                          <div className={styles.pwdActions}>
                            <button className={styles.btnGhost} onClick={()=>setActiveTab("overview")}>Cancel</button>
                            <motion.button className={styles.btnSave}
                              style={{background:roleData.bg}}
                              onClick={savePwd} whileHover={{scale:1.02}} whileTap={{scale:.97}}>
                              Reset Password
                            </motion.button>
                          </div>
                        </div>
                      </motion.div>
                    )}

                    {activeTab==="danger"&&(
                      <motion.div key="dz" className={styles.tabContent}
                        initial={{opacity:0,y:12}} animate={{opacity:1,y:0}} exit={{opacity:0}}>
                        <div className={styles.dangerZone}>
                          <div className={styles.dangerHeader}>
                            <div className={styles.dangerIcon}>{I.warn}</div>
                            <div>
                              <h3 className={styles.dangerTitle}>Danger Zone</h3>
                              <p className={styles.dangerSub}>These actions cannot be undone</p>
                            </div>
                          </div>

                          {/* ── Non-student accounts: keep the original simple delete ── */}
                          {result.role !== "student" && (
                            <div className={styles.dangerCard}>
                              <div className={styles.dangerCardLeft}>
                                <div className={styles.dangerCardTitle}>Delete Account</div>
                                <div className={styles.dangerCardSub}>
                                  Permanently delete <strong>{buildEmail(result.role,result.code,result.firstName,result.lastName)}</strong> and all associated data.
                                </div>
                              </div>
                              <motion.button className={styles.dangerDeleteBtn}
                                onClick={del} whileHover={{scale:1.04}} whileTap={{scale:.96}}>
                                {I.trash} Delete Account
                              </motion.button>
                            </div>
                          )}

                          {/* ── Student accounts: strong-confirmation permanent delete ── */}
                          {result.role === "student" && !dzResult && (
                            <div className={styles.dangerCard} style={{flexDirection:"column",alignItems:"stretch",gap:14}}>
                              <div>
                                <div className={styles.dangerCardTitle}>Permanent Student Delete</div>
                                <div className={styles.dangerCardSub} style={{lineHeight:1.55}}>
                                  This will permanently remove the student account and every row tied to them
                                  (registrations, grades, submissions, quiz attempts, notifications, audit
                                  snapshots, profile data) plus their submission files from storage. Shared
                                  course/material rows (assignments, quizzes, lectures, courses) are kept.
                                </div>
                              </div>

                              {/* Student preview from backend */}
                              {dzPreviewBusy && (
                                <div style={{padding:14,fontSize:13,color:"var(--text-muted)"}}>Loading student preview…</div>
                              )}
                              {dzPreviewErr && (
                                <div style={{padding:"10px 12px",borderRadius:10,background:"rgba(239,68,68,.08)",
                                             border:"1px solid rgba(239,68,68,.30)",color:"#ef4444",fontSize:13,fontWeight:700}}>
                                  ⚠ {dzPreviewErr}
                                </div>
                              )}
                              {dzPreview && (
                                <div style={{
                                  padding:"12px 14px",borderRadius:12,
                                  background:"rgba(239,68,68,.05)",
                                  border:"1px solid rgba(239,68,68,.22)",
                                  display:"grid",gap:8,
                                }}>
                                  <div style={{display:"flex",alignItems:"center",gap:10}}>
                                    <div className={styles.idStripAvatar} style={{
                                      width:40,height:40,fontSize:".95rem",
                                      background:"linear-gradient(135deg,#991b1b,#ef4444)",
                                    }}>
                                      {(dzPreview.name||"S").split(" ").map(s=>s[0]).join("").slice(0,2)}
                                    </div>
                                    <div style={{flex:1,minWidth:0}}>
                                      <div style={{fontWeight:900,fontSize:".98rem"}}>{dzPreview.name}</div>
                                      <div style={{fontSize:".78rem",color:"var(--text-secondary)",fontFamily:"monospace"}}>#{dzPreview.academicCode}</div>
                                    </div>
                                  </div>
                                  <div style={{
                                    display:"grid",gap:6,
                                    gridTemplateColumns:"repeat(auto-fill, minmax(140px, 1fr))",
                                    fontSize:11.5,color:"var(--text-secondary)",
                                  }}>
                                    {dzPreview.email && <div><strong>Email:</strong> {dzPreview.email}</div>}
                                    {dzPreview.year && <div><strong>Year:</strong> {dzPreview.year}</div>}
                                    {dzPreview.semester && <div><strong>Term:</strong> {dzPreview.semester}</div>}
                                    <div><strong>Active courses:</strong> {dzPreview.registeredCoursesCount}</div>
                                    <div><strong>Submissions:</strong> {dzPreview.submissionsCount}</div>
                                    <div><strong>Quiz attempts:</strong> {dzPreview.quizAttemptsCount}</div>
                                  </div>
                                </div>
                              )}

                              {/* Warning list */}
                              <div style={{
                                padding:"10px 14px",borderRadius:10,
                                background:"rgba(239,68,68,.06)",
                                border:"1px solid rgba(239,68,68,.22)",
                                fontSize:12.5,lineHeight:1.6,color:"var(--text-secondary)",
                              }}>
                                <div style={{fontWeight:800,color:"#ef4444",marginBottom:4}}>The following will be deleted:</div>
                                Student account · all registrations · final &amp; midterm grades · final-grade reviews ·
                                assignment submissions and submission files · quiz attempts &amp; answers ·
                                course results · semester GPAs · user-study-year rows · course exceptions ·
                                admin per-student locks · academic-year reset snapshots · OTPs · notifications.
                                <div style={{marginTop:6,fontWeight:700,color:"#22c55e"}}>Kept:</div>
                                Courses, instructor-created assignments/quizzes/lectures, instructor assignments to courses, departments.
                              </div>

                              {/* Confirmation fields */}
                              <div style={{display:"flex",flexDirection:"column",gap:10}}>
                                <div>
                                  <label className={styles.formLabel}>Re-type academic code <span className={styles.req}>*</span></label>
                                  <input className={styles.formInput} placeholder={result.code}
                                    value={dzCodeInput} onChange={e=>setDzCodeInput(e.target.value)}
                                    autoComplete="off" spellCheck={false}
                                    style={dzCodeInput && dzCodeInput===result.code ? {borderColor:"#22c55e60"} : {}}/>
                                </div>
                                <div>
                                  <label className={styles.formLabel}>Reset password <span className={styles.req}>*</span></label>
                                  <input className={styles.formInput} type="password"
                                    placeholder="StudentDelete@123#"
                                    value={dzPwdInput} onChange={e=>setDzPwdInput(e.target.value)}
                                    autoComplete="off"
                                    style={dzPwdInput && dzPwdInput===STUDENT_DELETE_PWD ? {borderColor:"#22c55e60"} : {}}/>
                                </div>
                                <label style={{display:"flex",alignItems:"flex-start",gap:8,fontSize:13,fontWeight:600,color:"var(--text-secondary)",cursor:"pointer"}}>
                                  <input type="checkbox" checked={dzAck} onChange={e=>setDzAck(e.target.checked)}
                                    style={{marginTop:3,width:14,height:14}}/>
                                  <span>I understand this permanently deletes this student and cannot be undone.</span>
                                </label>
                              </div>

                              {dzExecError && (
                                <div style={{padding:"10px 12px",borderRadius:10,background:"rgba(239,68,68,.08)",
                                             border:"1px solid rgba(239,68,68,.30)",color:"#ef4444",fontSize:13,fontWeight:700}}>
                                  ⚠ {dzExecError}
                                </div>
                              )}

                              <div style={{display:"flex",justifyContent:"flex-end",gap:10}}>
                                <motion.button className={styles.dangerDeleteBtn}
                                  onClick={()=>setDzShowFinal(true)}
                                  disabled={!dzCanDelete}
                                  style={{opacity:dzCanDelete?1:0.45}}
                                  whileHover={dzCanDelete?{scale:1.03}:{}}
                                  whileTap={dzCanDelete?{scale:.96}:{}}>
                                  {I.trash} Delete student permanently
                                </motion.button>
                              </div>
                            </div>
                          )}

                          {/* Success state after deletion */}
                          {result.role === "student" && dzResult && (
                            <div className={styles.dangerCard} style={{
                              flexDirection:"column",alignItems:"stretch",gap:12,
                              borderColor:"rgba(34,197,94,0.4)",
                              background:"rgba(34,197,94,0.06)",
                            }}>
                              <div style={{display:"flex",alignItems:"center",gap:10}}>
                                <div style={{width:36,height:36,borderRadius:11,
                                  background:"linear-gradient(135deg,#15803d,#22c55e)",color:"#fff",
                                  display:"flex",alignItems:"center",justifyContent:"center",
                                  fontSize:".95rem",fontWeight:900}}>✓</div>
                                <div>
                                  <div style={{fontSize:".95rem",fontWeight:900,color:"#22c55e"}}>
                                    Student permanently deleted
                                  </div>
                                  <div style={{fontSize:11.5,color:"var(--text-secondary)"}}>
                                    Audit batch #{dzResult.auditId} · code <strong>#{dzResult.deletedStudentCode}</strong> · {dzResult.deletedStudentName}
                                  </div>
                                </div>
                              </div>
                              <div style={{
                                display:"grid",gap:6,
                                gridTemplateColumns:"repeat(auto-fill, minmax(160px, 1fr))",
                                fontSize:12,color:"var(--text-secondary)",
                              }}>
                                <div>Registrations: <strong>{dzResult.counts.registrationsRemoved}</strong></div>
                                <div>Final grades: <strong>{dzResult.counts.finalGradesRemoved}</strong></div>
                                <div>Midterm grades: <strong>{dzResult.counts.midtermGradesRemoved}</strong></div>
                                <div>Submissions: <strong>{dzResult.counts.assignmentSubmissionsRemoved}</strong></div>
                                <div>Submission files: <strong>{dzResult.counts.submissionFilesRemoved}</strong></div>
                                <div>Quiz attempts: <strong>{dzResult.counts.quizAttemptsRemoved}</strong></div>
                                <div>Quiz answers: <strong>{dzResult.counts.quizAnswersRemoved}</strong></div>
                                <div>Notifications: <strong>{dzResult.counts.notificationsRemoved}</strong></div>
                                <div>Course results: <strong>{dzResult.counts.courseResultsRemoved}</strong></div>
                                <div>Semester GPAs: <strong>{dzResult.counts.semesterGpasRemoved}</strong></div>
                                <div>Reset snapshots: <strong>{dzResult.counts.resetSnapshotsRemoved}</strong></div>
                              </div>
                              <div style={{display:"flex",justifyContent:"flex-end"}}>
                                <button className={styles.btnGhost} onClick={dzReturnHome}>Done</button>
                              </div>
                            </div>
                          )}
                        </div>

                        {/* Final confirmation modal */}
                        <AnimatePresence>
                          {dzShowFinal && (
                            <motion.div
                              onClick={()=>!dzBusy && setDzShowFinal(false)}
                              initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}
                              style={{
                                position:"fixed",inset:0,background:"rgba(0,0,0,0.55)",backdropFilter:"blur(8px)",
                                display:"flex",alignItems:"center",justifyContent:"center",zIndex:9999,padding:16,
                              }}>
                              <motion.div
                                onClick={e=>e.stopPropagation()}
                                initial={{scale:0.9,y:18}} animate={{scale:1,y:0}} exit={{scale:0.95}}
                                transition={sp}
                                style={{
                                  width:"min(440px, 100%)",background:"var(--card-bg)",
                                  borderRadius:18,overflow:"hidden",
                                  border:"1px solid var(--border)",boxShadow:"0 24px 60px rgba(0,0,0,0.32)",
                                }}>
                                <div style={{height:4,background:"linear-gradient(135deg,#b91c1c,#ef4444)"}}/>
                                <div style={{padding:22}}>
                                  <h3 style={{margin:"0 0 8px",fontSize:"1.1rem",fontWeight:800}}>
                                    Final confirmation
                                  </h3>
                                  <p style={{margin:0,fontSize:13.5,color:"var(--text-secondary)",lineHeight:1.55}}>
                                    Permanently delete student <strong>#{result.code}</strong> — {result.firstName} {result.lastName}?
                                    This action cannot be undone.
                                  </p>
                                  <div style={{display:"flex",justifyContent:"flex-end",gap:10,marginTop:18}}>
                                    <button onClick={()=>setDzShowFinal(false)} disabled={dzBusy}
                                      className={styles.btnGhost}>Cancel</button>
                                    <button onClick={dzExecute} disabled={dzBusy}
                                      className={styles.dangerDeleteBtn}>
                                      {dzBusy ? "Deleting…" : "Yes, delete permanently"}
                                    </button>
                                  </div>
                                </div>
                              </motion.div>
                            </motion.div>
                          )}
                        </AnimatePresence>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </motion.div>
              )}

              {mode==="create"&&(
                <motion.div key="create" className={styles.createPane}
                  initial={{opacity:0,x:40,scale:.97}} animate={{opacity:1,x:0,scale:1}}
                  exit={{opacity:0,x:-30,scale:.97}}
                  transition={{type:"spring",stiffness:340,damping:30}}>

                  <div className={styles.createWrap}>
                    <div className={styles.createSidebar}>
                      <button className={styles.sideBackBtn}
                        onClick={()=>createStep===2?setCreateStep(1):setMode("home")}>
                        {I.back} {createStep===2?"Role":"Back"}
                      </button>

                      <div className={styles.createSideTitle}>New Account</div>

                      <div className={styles.vStepper}>
                        {[{n:1,label:"Account Type",desc:"Student, Instructor or Admin"},{n:2,label:"Account Details",desc:"Code, name & password"}].map((s,i)=>{
                          const on=createStep===s.n, done=createStep>s.n;
                          const rc=ROLES.find(r=>r.key===createRole);
                          return (
                            <div key={s.n} className={styles.vStepItem}>
                              {i>0&&<div className={styles.vStepLine}><div className={styles.vStepLineFill} style={{background:done||on?rc?.color:"rgba(255,255,255,.12)",height:done?"100%":on?"50%":"0%"}}/></div>}
                              <div className={styles.vStepRow}>
                                <div className={styles.vStepNum} style={on||done?{background:rc?.color,borderColor:rc?.color,color:"#fff",boxShadow:`0 4px 14px ${rc?.color}44`}:{}}>
                                  {done?<span style={{display:"flex"}}>{I.check}</span>:s.n}
                                </div>
                                <div className={styles.vStepText}>
                                  <div className={styles.vStepLabel} style={on?{color:"rgba(255,255,255,.95)"}:{}}>{s.label}</div>
                                  <div className={styles.vStepDesc}>{s.desc}</div>
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      </div>

                      {createStep===2&&(()=>{
                        const rc=ROLES.find(r=>r.key===createRole);
                        return (
                          <div className={styles.createSideRole}>
                            <div className={styles.createSideRoleIcon} style={{background:rc?.bg}}>{rc?.icon}</div>
                            <div className={styles.createSideRoleName} style={{color:rc?.color}}>{rc?.label}</div>
                            <div className={styles.createSideRolePrefix}><code>{rc?.prefix}-</code>code·Name</div>
                          </div>
                        );
                      })()}
                    </div>

                    <div className={styles.createContent}>
                      <AnimatePresence mode="wait">
                        {createStep===1&&(
                          <motion.div key="s1"
                            initial={{opacity:0,x:24}} animate={{opacity:1,x:0}} exit={{opacity:0,x:-18}}
                            transition={{type:"spring",stiffness:380,damping:28}}>
                            <h2 className={styles.createTitle}>Choose Account Type</h2>
                            <p className={styles.createSub}>Select the role for the new account</p>
                            <div className={styles.roleGrid}>
                              {ROLES.map((r,ri)=>{
                                const on=createRole===r.key;
                                return (
                                  <motion.button key={r.key}
                                    className={`${styles.roleCard} ${on?styles.roleCardOn:""}`}
                                    style={on?{background:r.bg,borderColor:r.color,boxShadow:`0 12px 36px ${r.color}44`}:{"--rc":r.color}}
                                    onClick={()=>setCreateRole(r.key)}
                                    initial={{opacity:0,y:16}} animate={{opacity:1,y:0}}
                                    transition={{delay:ri*.07,...sp}}
                                    whileHover={{y:-6}} whileTap={{scale:.97}}>
                                    <div className={styles.roleCardIconWrap}
                                      style={on?{background:"rgba(255,255,255,.2)",border:"2px solid rgba(255,255,255,.35)"}:{background:`${r.color}16`,border:`2px solid ${r.color}28`}}>
                                      <span className={styles.roleCardIcon}>{r.icon}</span>
                                    </div>
                                    <div className={styles.roleCardName} style={on?{color:"#fff"}:{}}>{r.label}</div>
                                    <div className={styles.roleCardPrefix} style={on?{color:"rgba(255,255,255,.7)"}:{color:r.color}}>
                                      <code>{r.prefix}-</code>code·Name
                                    </div>
                                    {on&&(
                                      <motion.div className={styles.roleCheck}
                                        initial={{scale:0}} animate={{scale:1}}
                                        transition={{type:"spring",stiffness:500,damping:20}}>
                                        {I.check}
                                      </motion.div>
                                    )}
                                  </motion.button>
                                );
                              })}
                            </div>
                            <motion.button className={styles.nextBtn}
                              style={{background:ROLES.find(r=>r.key===createRole)?.bg}}
                              onClick={()=>setCreateStep(2)}
                              whileHover={{scale:1.02,y:-2}} whileTap={{scale:.97}}>
                              Continue → Select {ROLES.find(r=>r.key===createRole)?.label}
                            </motion.button>
                          </motion.div>
                        )}

                        {createStep===2&&(
                          <motion.div key="s2"
                            initial={{opacity:0,x:24}} animate={{opacity:1,x:0}} exit={{opacity:0,x:-18}}
                            transition={{type:"spring",stiffness:380,damping:28}}>
                            <h2 className={styles.createTitle}>Account Details</h2>
                            <p className={styles.createSub}>Fill in the information for the new account</p>

                            {prevEmail&&(
                              <AnimatePresence>
                                <motion.div className={styles.livePreview}
                                  style={{borderColor:`${ROLES.find(r=>r.key===createRole)?.color}35`,background:`${ROLES.find(r=>r.key===createRole)?.color}08`}}
                                  initial={{opacity:0,height:0}} animate={{opacity:1,height:"auto"}} exit={{opacity:0,height:0}}>
                                  <div className={styles.lpLeft}>
                                    <span className={styles.lpLabel}>Email Preview</span>
                                    <span className={styles.lpEmail} style={{color:ROLES.find(r=>r.key===createRole)?.color}}>{prevEmail}</span>
                                  </div>
                                  <span style={{color:ROLES.find(r=>r.key===createRole)?.color,display:"flex"}}>{I.check}</span>
                                </motion.div>
                              </AnimatePresence>
                            )}

                            {(()=>{
                              const rc=ROLES.find(r=>r.key===createRole);
                              const bc=v=>v?{borderColor:`${rc?.color}55`}:{};
                              const rules=pwdRules(form.password);
                              const filteredGovs=EGYPT_GOVERNORATES.filter(g=>g.toLowerCase().includes(govSearch.toLowerCase()));
                              const canCreate=form.code&&form.firstName&&form.lastName&&form.subEmail&&form.gender&&form.dob&&form.address;
                              return (<>
                            <div className={styles.formGrid}>
                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Code <span className={styles.req}>*</span></label>
                                <input className={styles.formInput} placeholder="e.g. 2203119" value={form.code} onChange={e=>setForm(p=>({...p,code:e.target.value}))} style={bc(form.code)}/>
                              </div>
                              <div/>
                              <div className={styles.formField}>
                                <label className={styles.formLabel}>First Name <span className={styles.req}>*</span></label>
                                <input className={styles.formInput} placeholder="Ahmed" value={form.firstName} onChange={e=>setForm(p=>({...p,firstName:e.target.value}))} style={bc(form.firstName)}/>
                              </div>
                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Last Name <span className={styles.req}>*</span></label>
                                <input className={styles.formInput} placeholder="Mohamed" value={form.lastName} onChange={e=>setForm(p=>({...p,lastName:e.target.value}))} style={bc(form.lastName)}/>
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Gender <span className={styles.req}>*</span></label>
                                <select className={styles.formInput} value={form.gender} onChange={e=>setForm(p=>({...p,gender:e.target.value}))} style={bc(form.gender)}>
                                  <option value="">Select gender…</option>
                                  <option value="male">Male</option>
                                  <option value="female">Female</option>
                                </select>
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Date of Birth <span className={styles.req}>*</span></label>
                                <input className={styles.formInput} type="date" value={form.dob}
                                  onChange={e=>setForm(p=>({...p,dob:e.target.value}))} style={bc(form.dob)}
                                  max={new Date(new Date().setFullYear(new Date().getFullYear()-15)).toISOString().slice(0,10)}/>
                              </div>

                              <div className={styles.formField}>
                                <label className={styles.formLabel}>Phone Number</label>
                                <input className={styles.formInput} type="tel" placeholder="+20 1xxxxxxxxx"
                                  value={form.phone} onChange={e=>setForm(p=>({...p,phone:e.target.value}))} style={bc(form.phone)}/>
                              </div>

                              <div className={`${styles.formField} ${styles.createGovField}`} ref={govWrapRef}>
                                <label className={styles.formLabel}>Governorate <span className={styles.req}>*</span></label>
                                <button
                                  type="button"
                                  className={`${styles.createGovBox} ${form.address ? styles.createGovBoxFilled : ""}`}
                                  style={bc(form.address)}
                                  onClick={()=>setGovOpen(v=>!v)}
                                >
                                  <span className={styles.createGovMeta}>Governorate</span>
                                  <span className={form.address ? styles.createGovValue : styles.createGovPlaceholder}>
                                    {form.address || "Choose governorate..."}
                                  </span>
                                  <span className={`${styles.createGovArrow} ${govOpen?styles.createGovArrowOpen:""}`}>▾</span>
                                </button>
                                <AnimatePresence>
                                  {govOpen&&(
                                    <motion.div className={styles.govDropdown}
                                      initial={{opacity:0,y:8,scale:.98}} animate={{opacity:1,y:0,scale:1}} exit={{opacity:0,y:8,scale:.98}}
                                      transition={{duration:.15}}>
                                      <div className={styles.govList}>
                                        {filteredGovs.length===0&&<div className={styles.govEmpty}>No governorate found</div>}
                                        {filteredGovs.map(g=>{
                                          const active = form.address===g;
                                          return (
                                            <button key={g} type="button" className={`${styles.govItem} ${active?styles.govItemActive:""}`}
                                              onClick={()=>{setForm(p=>({...p,address:g}));setGovSearch("");setGovOpen(false);}}>
                                              <span>{g}</span>
                                              {active && <span className={styles.govCheck}>✓</span>}
                                            </button>
                                          );
                                        })}
                                      </div>
                                      <div className={styles.govSearchWrap}>
                                        <input className={styles.govSearchInp} autoFocus
                                          value={govSearch}
                                          onChange={e=>setGovSearch(e.target.value)}
                                          placeholder="Search governorate..."/>
                                        {form.address&&(
                                          <button type="button" className={styles.govReset}
                                            onClick={()=>{setForm(p=>({...p,address:""}));setGovSearch("");}}>
                                            Clear
                                          </button>
                                        )}
                                      </div>
                                    </motion.div>
                                  )}
                                </AnimatePresence>
                              </div>

                              <div className={styles.formFieldFull}>
                                <div className={styles.formLabelRow}>
                                  <label className={styles.formLabel}>Password <span style={{color:"var(--text-muted)",fontWeight:400}}>(optional — auto-generated if empty)</span></label>
                                  <button className={styles.genSmall} onClick={()=>setForm(p=>({...p,password:genPwd()}))}>{I.spark} Auto-generate</button>
                                </div>
                                <div className={styles.pwdField} style={form.password?{borderColor:`${rc?.color}55`}:{}}>
                                  <input type={showPwd?"text":"password"} className={styles.pwdInput}
                                    placeholder="Leave empty to auto-generate" value={form.password}
                                    onChange={e=>setForm(p=>({...p,password:e.target.value}))}/>
                                  <button className={styles.pwdEye} onClick={()=>setShowPwd(p=>!p)}>{showPwd?I.eyeoff:I.eye}</button>
                                </div>
                                <div className={styles.pwdRules}>
                                  {rules.map((r,i)=>(
                                    <span key={i} className={`${styles.pwdRule} ${form.password?(r.ok?styles.pwdRuleOk:styles.pwdRuleFail):""}`}>
                                      <span className={styles.pwdRuleDot}>{r.ok&&form.password?"✓":"·"}</span>
                                      {r.label}
                                    </span>
                                  ))}
                                </div>
                              </div>

                              <div className={styles.formFieldFull}>
                                <label className={styles.formLabel}>Recovery Email (subEmail) <span className={styles.req}>*</span></label>
                                <input className={styles.formInput} type="email"
                                  placeholder="Personal email for credentials delivery"
                                  value={form.subEmail}
                                  onChange={e=>setForm(p=>({...p,subEmail:e.target.value}))}
                                  style={bc(form.subEmail)}/>
                              </div>
                            </div>

                            <div className={styles.createActions}>
                              <button className={styles.btnGhost} onClick={()=>setMode("home")}>Cancel</button>
                              <motion.button className={styles.btnCreate}
                                style={{background:rc?.bg,opacity:canCreate?1:.38}}
                                disabled={!canCreate}
                                onClick={create} whileHover={{scale:1.02}} whileTap={{scale:.97}}>
                                {I.plus} Create Account
                              </motion.button>
                            </div>
                            </>);})()}
                          </motion.div>
                        )}
                      </AnimatePresence>
                    </div>
                  </div>
                </motion.div>
              )}

              {mode==="success"&&createdAccount&&(
                <motion.div key="success" className={styles.successPane}
                  initial={{opacity:0,scale:.96,y:20}} animate={{opacity:1,scale:1,y:0}}
                  exit={{opacity:0,y:-14}} transition={{type:"spring",stiffness:360,damping:28}}>

                  <div className={styles.successHeader}>
                    <motion.div className={styles.successCheckBadge}
                      initial={{scale:0}} animate={{scale:1}}
                      transition={{delay:.08,type:"spring",stiffness:520,damping:22}}>
                      {I.check}
                    </motion.div>
                    <div>
                      <h2 className={styles.successTitle}>Account Created Successfully</h2>
                      <p className={styles.successSub}>The new {(ROLES.find(r=>r.key===createdAccount.role)||ROLES[0]).label.toLowerCase()} account is ready.</p>
                    </div>
                  </div>

                  <div className={styles.successCard}>
                    <div className={styles.successStrip} style={{background:(ROLES.find(r=>r.key===createdAccount.role)||ROLES[0]).bg}}>
                      <div className={styles.successAvatar}>
                        {createdAccount.firstName[0]}{createdAccount.lastName[0]}
                      </div>
                      <div className={styles.successNameBlock}>
                        <span className={styles.successName}>{createdAccount.firstName} {createdAccount.lastName}</span>
                        <span className={styles.successRoleBadge}>
                          {(ROLES.find(r=>r.key===createdAccount.role)||ROLES[0]).icon} {(ROLES.find(r=>r.key===createdAccount.role)||ROLES[0]).label}
                        </span>
                      </div>
                      <div className={styles.successActiveDot}><span/>Active</div>
                    </div>

                    <div className={styles.successFields}>
                      <div className={styles.successCodeRow}>
                        <span className={styles.successFieldLabel}>Academic Code</span>
                        <div className={styles.successCodeValue}>
                          <code className={styles.successCode}>{createdAccount.code}</code>
                          <motion.button
                            className={`${styles.successCopyBtn} ${copied==="sc_code"?styles.successCopyDone:""}`}
                            onClick={()=>copyText(createdAccount.code,"sc_code")}
                            whileHover={{scale:1.08}} whileTap={{scale:.92}}>
                            <AnimatePresence mode="wait">
                              {copied==="sc_code"
                                ?<motion.span key="y" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.check}</motion.span>
                                :<motion.span key="n" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.copy}</motion.span>}
                            </AnimatePresence>
                          </motion.button>
                        </div>
                      </div>

                      <div className={styles.successFieldRow}>
                        <span className={styles.successFieldLabel}>University Email</span>
                        <div className={styles.successFieldRight}>
                          <span className={styles.successFieldMono}>{createdAccount.email}</span>
                          <motion.button
                            className={`${styles.successCopyBtn} ${copied==="sc_email"?styles.successCopyDone:""}`}
                            onClick={()=>copyText(createdAccount.email,"sc_email")}
                            whileHover={{scale:1.08}} whileTap={{scale:.92}}>
                            <AnimatePresence mode="wait">
                              {copied==="sc_email"
                                ?<motion.span key="y" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.check}</motion.span>
                                :<motion.span key="n" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.copy}</motion.span>}
                            </AnimatePresence>
                          </motion.button>
                        </div>
                      </div>

                      {createdAccount.password&&(
                        <div className={styles.successFieldRow}>
                          <span className={styles.successFieldLabel}>
                            Password <span className={styles.successOnce}>(shown once)</span>
                          </span>
                          <div className={styles.successFieldRight}>
                            <code className={styles.successFieldMono}>{createdAccount.password}</code>
                            <motion.button
                              className={`${styles.successCopyBtn} ${copied==="sc_pwd"?styles.successCopyDone:""}`}
                              onClick={()=>copyText(createdAccount.password,"sc_pwd")}
                              whileHover={{scale:1.08}} whileTap={{scale:.92}}>
                              <AnimatePresence mode="wait">
                                {copied==="sc_pwd"
                                  ?<motion.span key="y" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.check}</motion.span>
                                  :<motion.span key="n" initial={{scale:0}} animate={{scale:1}} exit={{scale:0}}>{I.copy}</motion.span>}
                              </AnimatePresence>
                            </motion.button>
                          </div>
                        </div>
                      )}

                      <div className={styles.successFieldRow}>
                        <span className={styles.successFieldLabel}>Recovery Email</span>
                        <span className={styles.successFieldVal}>{createdAccount.subEmail}</span>
                      </div>

                      <div className={styles.successFieldRow}>
                        <span className={styles.successFieldLabel}>Created</span>
                        <span className={styles.successFieldVal}>{createdAccount.createdAt}</span>
                      </div>
                    </div>
                  </div>

                  <div className={styles.successActions}>
                    <motion.button className={styles.btnCreate}
                      style={{background:(ROLES.find(r=>r.key===createdAccount.role)||ROLES[0]).bg}}
                      onClick={()=>{setCreatedAccount(null);setMode("create");setCreateStep(1);setCreateRole("student");}}
                      whileHover={{scale:1.02,y:-2}} whileTap={{scale:.97}}>
                      {I.plus} Create Another Account
                    </motion.button>
                    <button className={styles.btnGhost}
                      onClick={()=>navigate('/admin/manage-users',{state:{autoSearchCode:createdAccount.code}})}>
                      Open in Manage Users →
                    </button>
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </main>
      </div>

      <AnimatePresence>
        {activeRole && (
          <motion.div
            className={styles.rolePopupOverlay}
            initial={{opacity:0}}
            animate={{opacity:1}}
            exit={{opacity:0}}
            onClick={()=>{setActiveRole(null);setSideSearch("");}}
          >
            <motion.div
              className={styles.rolePopup}
              initial={{opacity:0,y:24,scale:.97}}
              animate={{opacity:1,y:0,scale:1}}
              exit={{opacity:0,y:18,scale:.98}}
              transition={{duration:.22}}
              onClick={e=>e.stopPropagation()}
            >
              <div className={styles.rolePopupHeader}>
                <div>
                  <h3 className={styles.rolePopupTitle}>
                    {stats.find(s=>s.key===activeRole)?.label || "Accounts"}
                  </h3>
                  <p className={styles.rolePopupSub}>Select an account to open it in the main panel.</p>
                </div>

                <div className={styles.rolePopupHeadActions}>
                  <div className={styles.rolePopupSearch}>
                    <span className={styles.rolePopupSearchIcon}>{I.search}</span>
                    <input
                      className={styles.rolePopupSearchInp}
                      value={sideSearch}
                      onChange={e=>setSideSearch(e.target.value)}
                      placeholder="Search by name or code..."
                    />
                    {sideSearch && (
                      <button className={styles.rolePopupSearchClear} onClick={()=>setSideSearch("")}>✕</button>
                    )}
                  </div>
                  <button className={styles.rolePopupClose} onClick={()=>{setActiveRole(null);setSideSearch("");}}>
                    {I.close}
                  </button>
                </div>
              </div>

              <div className={styles.rolePopupMetaRow}>
                <span className={styles.rolePopupCount}>{sideList.length} accounts</span>
                <span className={styles.rolePopupHint}>Choose one and continue editing it below</span>
              </div>

              <div className={styles.rolePopupGrid}>
                {sideList.map((acc,i)=>{
                  const r=ROLES.find(x=>x.key===acc.role)||ROLES[0];
                  const selected = result?.id===acc.id;
                  return (
                    <motion.button key={acc.id}
                      className={`${styles.popupAccountCard} ${selected?styles.popupAccountCardOn:""}`}
                      style={selected?{borderColor:r.color,boxShadow:`0 0 0 3px ${r.color}18`}:{}}
                      onClick={()=>{ setResult(acc); setMode("found"); setSearch(acc.code); setActiveTab("overview"); setActiveRole(null); setSideSearch(""); }}
                      initial={{opacity:0,y:14}} animate={{opacity:1,y:0}}
                      transition={{delay:i*.02,duration:.18}}
                      whileHover={{y:-3,scale:1.01}} whileTap={{scale:.98}}>
                      <div className={styles.popupAccountTop}>
                        <div className={styles.popupAccountAvatar} style={{background:r.bg}}>
                          {acc.firstName[0]}{acc.lastName[0]}
                          {!acc.active&&<div className={styles.popupAccountBan}/>}
                        </div>
                        <div className={styles.popupAccountRole} style={{background:`${r.color}16`,color:r.color}}>
                          {r.icon}
                        </div>
                      </div>

                      <div className={styles.popupAccountName}>{acc.firstName} {acc.lastName}</div>
                      <div className={styles.popupAccountCode} style={{color:r.color}}>#{acc.code}</div>

                      <div className={styles.popupAccountFooter}>
                        <span className={styles.popupAccountStatus} style={acc.active ? {} : {color:"#ef4444",background:"rgba(239,68,68,.08)"}}>
                          {acc.active ? "Active" : "Suspended"}
                        </span>
                        <span className={styles.popupAccountOpen}>Open →</span>
                      </div>
                    </motion.button>
                  );
                })}

                {sideList.length===0 && (
                  <div className={styles.rolePopupEmpty}>No accounts match this filter.</div>
                )}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {emailAnim&&(
          <motion.div className={styles.envOverlay}
            initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}} transition={{duration:.25}}>
            <EnvelopeScene roleColor={ROLES.find(r=>r.key===createRole)?.color||"#818cf8"}/>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
