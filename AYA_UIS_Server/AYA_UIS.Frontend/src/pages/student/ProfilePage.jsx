// ProfilePage — Luxury card design + avatar click menu + drag crop + lightbox
import { useState, useRef, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { useAuth } from "../../hooks/useAuth";
import { getUserProfile, updateUserProfile } from "../../services/api/authApi";
import s from "./ProfilePage.module.css";

const ROLE_COLOR={student:"#6366f1",instructor:"#3d8fe0",admin:"#e05c8a"};
const ROLE_LABEL={student:"Student",instructor:"Instructor",admin:"Administrator"};
const ROLE_ICON ={student:"🎓",instructor:"👨‍🏫",admin:"⚙️"};

/* ─── Drag-to-Crop Modal ─── */
function CropModal({ src, onSave, onCancel }){
  const canvasRef=useRef(),previewRef=useRef();
  const[drag,setDrag]=useState(false),[start,setStart]=useState(null);
  const[box,setBox]=useState({x:40,y:40,size:200});
  const imgRef=useRef(new Image());
  const[loaded,setLoaded]=useState(false);

  useEffect(()=>{
    const img=imgRef.current;
    img.onload=()=>setLoaded(true);
    img.src=src;
  },[src]);

  useEffect(()=>{
    if(!loaded)return;
    const canvas=canvasRef.current;if(!canvas)return;
    const ctx=canvas.getContext("2d");
    const img=imgRef.current;
    canvas.width=img.naturalWidth;canvas.height=img.naturalHeight;
    ctx.drawImage(img,0,0);
    // darken outside crop
    ctx.fillStyle="rgba(0,0,0,.55)";ctx.fillRect(0,0,canvas.width,canvas.height);
    // clear crop circle
    ctx.save();ctx.globalCompositeOperation="destination-out";
    ctx.beginPath();ctx.arc(box.x+box.size/2,box.y+box.size/2,box.size/2,0,Math.PI*2);
    ctx.fill();ctx.restore();
    // circle border
    ctx.strokeStyle="white";ctx.lineWidth=3;
    ctx.beginPath();ctx.arc(box.x+box.size/2,box.y+box.size/2,box.size/2,0,Math.PI*2);ctx.stroke();
    // preview
    const pc=previewRef.current;if(!pc)return;
    const pr=pc.getContext("2d");pc.width=120;pc.height=120;
    pr.beginPath();pr.arc(60,60,60,0,Math.PI*2);pr.clip();
    pr.drawImage(img,box.x,box.y,box.size,box.size,0,0,120,120);
  },[loaded,box]);

  const save=()=>{
    const pc=previewRef.current;
    onSave(pc.toDataURL("image/png"));
  };

  const onMD=e=>{
    const r=canvasRef.current.getBoundingClientRect();
    const sx=canvasRef.current.width/r.width;
    const x=(e.clientX-r.left)*sx,y=(e.clientY-r.top)*sx;
    const cx=box.x+box.size/2,cy=box.y+box.size/2;
    if(Math.hypot(x-cx,y-cy)<box.size/2){setDrag(true);setStart({x:x-box.x,y:y-box.y});}
  };
  const onMM=e=>{
    if(!drag)return;
    const r=canvasRef.current.getBoundingClientRect();
    const sx=canvasRef.current.width/r.width;
    const x=(e.clientX-r.left)*sx,y=(e.clientY-r.top)*sx;
    const img=imgRef.current;
    const nx=Math.max(0,Math.min(img.naturalWidth-box.size,x-start.x));
    const ny=Math.max(0,Math.min(img.naturalHeight-box.size,y-start.y));
    setBox(b=>({...b,x:nx,y:ny}));
  };

  return(
    <motion.div className={s.cropOverlay} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}}
      onMouseMove={onMM} onMouseUp={()=>setDrag(false)}>
      <motion.div className={s.cropModal} initial={{scale:.9,opacity:0}} animate={{scale:1,opacity:1}} exit={{scale:.9,opacity:0}}>
        <h3 className={s.cropTitle}>Crop Photo</h3>
        <p className={s.cropSub}>Drag the circle to select the visible area</p>
        <div className={s.cropCanvasWrap}>
          {loaded?<canvas ref={canvasRef} className={s.cropCanvas} onMouseDown={onMD} style={{cursor:drag?"grabbing":"grab"}}/>
            :<div className={s.cropLoad}><span className={s.spin} style={{"--c":"#6366f1"}}/>Loading...</div>}
        </div>
        <div className={s.cropPreviewRow}>
          <div className={s.cropPreviewLabel}>Preview</div>
          <canvas ref={previewRef} className={s.cropPreview}/>
        </div>
        <div className={s.cropSizeRow}>
          <span className={s.cropSizeLabel}>Size</span>
          <input type="range" min={80} max={Math.min(imgRef.current?.naturalWidth||400,imgRef.current?.naturalHeight||400)} value={box.size}
            onChange={e=>setBox(b=>({...b,size:+e.target.value}))} className={s.cropSlider}/>
          <span className={s.cropSizeVal}>{Math.round(box.size)}px</span>
        </div>
        <div className={s.cropBtns}>
          <motion.button className={s.cropCancel} onClick={onCancel} whileHover={{scale:1.03}}>Cancel</motion.button>
          <motion.button className={s.cropSave} onClick={save} whileHover={{scale:1.04,boxShadow:"0 8px 24px rgba(99,102,241,.4)"}} whileTap={{scale:.96}}>
            ✓ Save Photo
          </motion.button>
        </div>
      </motion.div>
    </motion.div>
  );
}

/* ─── Lightbox ─── */
function Lightbox({ src, name, onClose }){
  return(
    <motion.div className={s.lbOverlay} initial={{opacity:0}} animate={{opacity:1}} exit={{opacity:0}} onClick={onClose}>
      <motion.div className={s.lbContent} initial={{scale:.7,opacity:0}} animate={{scale:1,opacity:1}} exit={{scale:.7,opacity:0}}
        transition={{type:"spring",stiffness:300,damping:26}} onClick={e=>e.stopPropagation()}>
        <img src={src} alt={name} className={s.lbImg}/>
        <p className={s.lbName}>{name}</p>
      </motion.div>
      <motion.button className={s.lbClose} onClick={onClose} whileHover={{scale:1.1}} whileTap={{scale:.9}}>✕</motion.button>
    </motion.div>
  );
}

/* ─── Avatar context menu ─── */
function AvatarMenu({ avatar, gender, initials, grad, onChange, onView, onDelete }){
  const[menu,setMenu]=useState(false),[cropSrc,setCrop]=useState(null),[lightbox,setLb]=useState(false);
  const fileRef=useRef();
  const menuRef=useRef();
  useEffect(()=>{
    const h=e=>{if(menuRef.current&&!menuRef.current.contains(e.target))setMenu(false);};
    document.addEventListener("mousedown",h);return()=>document.removeEventListener("mousedown",h);
  },[]);

  const handleFile=e=>{
    const f=e.target.files[0];if(!f)return;
    const reader=new FileReader();
    reader.onload=ev=>setCrop(ev.target.result);
    reader.readAsDataURL(f);
    e.target.value="";
  };

  const genderIcon=gender==="female"?"👩‍🎓":"👨‍🎓";
  return(
    <>
      <div className={s.avatarContainer} ref={menuRef}>
        <motion.div className={s.avatarRing} style={{borderColor:grad.split(",")[1]?.trim().slice(0,7)||"#6366f1"}}
          whileHover={{scale:1.04}} onClick={()=>setMenu(v=>!v)}>
          {avatar?(
            <img src={avatar} alt="avatar" className={s.avatarImg}/>
          ):(
            <div className={s.avatarFB} style={{background:grad}}>
              <span className={s.avatarInit}>{initials}</span>
              <span className={s.avatarGI}>{genderIcon}</span>
            </div>
          )}
          <div className={s.avatarEdit}>✏️</div>
        </motion.div>

        <AnimatePresence>
          {menu&&(
            <motion.div className={s.avatarMenu}
              initial={{opacity:0,scale:.9,y:-8}} animate={{opacity:1,scale:1,y:0}} exit={{opacity:0,scale:.9,y:-8}}
              transition={{type:"spring",stiffness:400,damping:28}}>
              {avatar&&(
                <motion.button className={s.menuItem} whileHover={{x:4}} onClick={()=>{setLb(true);setMenu(false);}}>
                  <span className={s.menuIco}>🔍</span><span>View Photo</span>
                </motion.button>
              )}
              <motion.button className={s.menuItem} whileHover={{x:4}} onClick={()=>{setMenu(false);fileRef.current?.click();}}>
                <span className={s.menuIco}>📷</span><span>Change Photo</span>
              </motion.button>
              {avatar&&(
                <motion.button className={`${s.menuItem} ${s.menuDel}`} whileHover={{x:4}} onClick={()=>{onDelete();setMenu(false);}}>
                  <span className={s.menuIco}>🗑️</span><span>Delete Photo</span>
                </motion.button>
              )}
            </motion.div>
          )}
        </AnimatePresence>
        <input ref={fileRef} type="file" accept="image/*" style={{display:"none"}} onChange={handleFile}/>
      </div>

      <AnimatePresence>
        {cropSrc&&<CropModal src={cropSrc} onSave={url=>{onChange(url);setCrop(null);}} onCancel={()=>setCrop(null)}/>}
        {lightbox&&avatar&&<Lightbox src={avatar} name="Profile Photo" onClose={()=>setLb(false)}/>}
      </AnimatePresence>
    </>
  );
}

/* ─── Field ─── */
function Field({icon,label,value,full}){
  return(
    <motion.div className={`${s.field} ${full?s.fieldFull:""}`}
      whileHover={{y:-2,boxShadow:"0 6px 20px rgba(0,0,0,.08)"}} transition={{duration:.15}}>
      <div className={s.fieldIcon}>{icon}</div>
      <div className={s.fieldContent}>
        <span className={s.fieldLabel}>{label}</span>
        <span className={s.fieldValue}>{value||"—"}</span>
      </div>
    </motion.div>
  );
}

/* ─── Main ─── */
export default function ProfilePage(){
  const{user: authUser, login}=useAuth();
  const [profile, setProfile] = useState(null);

  // Fetch fresh profile from API; re-fetch when logged-in user changes
  useEffect(() => {
    let cancelled = false;
    setProfile(null);
    getUserProfile()
      .then((data) => { if (!cancelled) setProfile(data); })
      .catch(() => { if (!cancelled) setProfile(null); });
    return () => { cancelled = true; };
  }, [authUser?.id]);

  // Merge: API profile wins, auth context is fallback
  const user = profile ? { ...authUser, ...profile } : authUser;
  const accent=ROLE_COLOR[user?.role]||"#6366f1";
  const grad=`linear-gradient(135deg,${accent}dd,${accent}88)`;
  const initials=user?.name?.split(" ").map(w=>w[0]).slice(0,2).join("").toUpperCase()||"?";
  const age=user?.dob?Math.floor((new Date()-new Date(user.dob))/(1000*60*60*24*365.25)):null;
  const dobFmt=user?.dob?new Date(user.dob).toLocaleDateString("en-GB",{day:"numeric",month:"long",year:"numeric"}):null;

  const updateAvatar=async(url)=>{
    try{await updateUserProfile({avatar:url});}catch(_){}
    login({...user,avatar:url},localStorage.getItem("token"));
  };
  const deleteAvatar=async()=>{
    try{await updateUserProfile({avatar:null});}catch(_){}
    login({...user,avatar:null},localStorage.getItem("token"));
  };

  const stagger={hidden:{},show:{transition:{staggerChildren:.07}}};
  const item={hidden:{opacity:0,y:20},show:{opacity:1,y:0,transition:{duration:.4,ease:[.22,1,.36,1]}}};

  return(
    <div className={s.page}>
      {/* Background decoration */}
      <div className={s.heroBg} style={{background:`linear-gradient(160deg,${accent}18 0%,transparent 60%)`}}/>

      <div className={s.wrapper}>
        {/* HERO CARD */}
        <motion.div className={s.heroCard} initial={{opacity:0,y:28}} animate={{opacity:1,y:0}} transition={{duration:.5,ease:[.22,1,.36,1]}}>
          {/* gradient stripe */}
          <div className={s.heroStripe} style={{background:grad}}/>

          <div className={s.heroBody}>
            {/* Avatar */}
            <AvatarMenu
              avatar={user?.avatar} gender={user?.gender} initials={initials} grad={grad}
              onChange={updateAvatar} onDelete={deleteAvatar}
            />

            {/* Info */}
            <div className={s.heroInfo}>
              <motion.h1 className={s.heroName} initial={{opacity:0,x:-20}} animate={{opacity:1,x:0}} transition={{delay:.1}}>{user?.name}</motion.h1>
              <motion.div className={s.heroRoleBadge} style={{background:`${accent}18`,color:accent,borderColor:`${accent}30`}}
                initial={{opacity:0,scale:.8}} animate={{opacity:1,scale:1}} transition={{delay:.2,type:"spring",stiffness:300}}>
                {ROLE_ICON[user?.role]} {ROLE_LABEL[user?.role]}
              </motion.div>
              <motion.p className={s.heroId} initial={{opacity:0}} animate={{opacity:1}} transition={{delay:.25}}>🆔 {user?.academicCode || user?.id}</motion.p>
            </div>

            {/* Quick stats */}
            <motion.div className={s.heroStats} initial={{opacity:0,x:20}} animate={{opacity:1,x:0}} transition={{delay:.15}}>
              {[{icon:"🎂",label:"Age",val:age?`${age} yrs`:"—"},{icon:"✉️",label:"Email",val:user?.email||"—"}].map((st,i)=>(
                <motion.div key={i} className={s.statBox} whileHover={{scale:1.04}} transition={{type:"spring",stiffness:400}}>
                  <span className={s.statIcon}>{st.icon}</span>
                  <div><p className={s.statVal}>{st.val}</p><p className={s.statLabel}>{st.label}</p></div>
                </motion.div>
              ))}
            </motion.div>
          </div>
        </motion.div>

        {/* FIELDS GRID */}
        <motion.div className={s.grid} variants={stagger} initial="hidden" animate="show">
          <motion.div className={s.section} variants={item}>
            <div className={s.secHead}><div className={s.secDot} style={{background:accent}}/><h2 className={s.secTitle}>Personal Information</h2></div>
            <div className={s.fields}>
              <Field icon="👤" label="Full Name"      value={user?.name}/>
              <Field icon="✉️"  label="Email"          value={user?.email}/>
              <Field icon="📱" label="Phone"           value={user?.phone}/>
              <Field icon={user?.gender==="female"?"👩":"👨"} label="Gender" value={user?.gender==="female"?"Female":"Male"}/>
              <Field icon="🎂" label="Date of Birth"  value={dobFmt}/>
              <Field icon="🗺️" label="Address"        value={user?.address}/>
            </div>
          </motion.div>

          <motion.div className={s.section} variants={item}>
            <div className={s.secHead}><div className={s.secDot} style={{background:accent}}/><h2 className={s.secTitle}>Account</h2></div>
            <div className={s.fields}>
              <Field icon="🆔" label="Academic Code" value={user?.academicCode || user?.id}/>
              <Field icon="🏷️" label="Role" value={ROLE_LABEL[user?.role]}/>
            </div>
          </motion.div>
        </motion.div>
      </div>
    </div>
  );
}