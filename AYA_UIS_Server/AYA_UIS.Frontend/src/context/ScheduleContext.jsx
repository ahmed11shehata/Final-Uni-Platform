// src/context/ScheduleContext.jsx
import { createContext, useContext, useState } from 'react';

/* ── Seed data – Year 4 already published so students see something ── */
const SEED_Y4 = [
  { id:'y4s1',  year:4,group:'A',day:'Saturday', start:8,   end:9.5,  code:'CS421',name:'Artificial Intelligence',      type:'Lecture',room:'Hall A', instructor:'Dr. Mohamed Farouk',color:'#e8a838',duration:1.5},
  { id:'y4s2',  year:4,group:'B',day:'Saturday', start:9.5, end:11,   code:'CS421',name:'Artificial Intelligence',      type:'Lecture',room:'Hall C', instructor:'Dr. Mohamed Farouk',color:'#e8a838',duration:1.5},
  { id:'y4s3',  year:4,group:'A',day:'Saturday', start:11,  end:12.5, code:'CS411',name:'Theory of Operating Systems',  type:'Lecture',room:'Hall B', instructor:'Dr. Rania Hassan',  color:'#3d8fe0',duration:1.5},
  { id:'y4s4',  year:4,group:'B',day:'Saturday', start:12.5,end:14,   code:'CS411',name:'Theory of Operating Systems',  type:'Lecture',room:'Hall D', instructor:'Dr. Rania Hassan',  color:'#3d8fe0',duration:1.5},
  { id:'y4s5',  year:4,group:'A',day:'Sunday',   start:8,   end:9.5,  code:'CS420',name:'Digital Image Processing',     type:'Lecture',room:'Lab 1',  instructor:'Dr. Mohamed Ali',   color:'#78909c',duration:1.5},
  { id:'y4s6',  year:4,group:'B',day:'Sunday',   start:9.5, end:11,   code:'CS420',name:'Digital Image Processing',     type:'Lecture',room:'Lab 2',  instructor:'Dr. Mohamed Ali',   color:'#78909c',duration:1.5},
  { id:'y4s7',  year:4,group:'A',day:'Sunday',   start:12,  end:13,   code:'CS421',name:'Artificial Intelligence',      type:'Section',room:'Room 12',instructor:'Eng. Ahmed Tarek',  color:'#e8a838',duration:1},
  { id:'y4s8',  year:4,group:'B',day:'Sunday',   start:13,  end:14,   code:'CS421',name:'Artificial Intelligence',      type:'Section',room:'Room 14',instructor:'Eng. Ahmed Tarek',  color:'#e8a838',duration:1},
  { id:'y4s9',  year:4,group:'A',day:'Monday',   start:9,   end:10.5, code:'CS422',name:'Neural Networks',              type:'Lecture',room:'Hall C', instructor:'Dr. Sara Khalil',   color:'#6366f1',duration:1.5},
  { id:'y4s10', year:4,group:'B',day:'Monday',   start:10.5,end:12,   code:'CS422',name:'Neural Networks',              type:'Lecture',room:'Hall C', instructor:'Dr. Sara Khalil',   color:'#6366f1',duration:1.5},
  { id:'y4s11', year:4,group:'A',day:'Monday',   start:13,  end:14,   code:'CS411',name:'Theory of Operating Systems',  type:'Section',room:'Lab 2',  instructor:'Eng. Youssef Ali',  color:'#3d8fe0',duration:1},
  { id:'y4s12', year:4,group:'B',day:'Monday',   start:14,  end:15,   code:'CS411',name:'Theory of Operating Systems',  type:'Section',room:'Lab 3',  instructor:'Eng. Youssef Ali',  color:'#3d8fe0',duration:1},
  { id:'y4s13', year:4,group:'A',day:'Tuesday',  start:8,   end:9.5,  code:'CS451',name:'Machine Learning',             type:'Lecture',room:'Hall A', instructor:'Dr. Aya Mostafa',   color:'#8b5cf6',duration:1.5},
  { id:'y4s14', year:4,group:'B',day:'Tuesday',  start:9.5, end:11,   code:'CS451',name:'Machine Learning',             type:'Lecture',room:'Hall A', instructor:'Dr. Aya Mostafa',   color:'#8b5cf6',duration:1.5},
  { id:'y4s15', year:4,group:'A',day:'Wednesday',start:8,   end:9,    code:'CS420',name:'Digital Image Processing',     type:'Section',room:'Lab 3',  instructor:'Eng. Nour Hamed',   color:'#78909c',duration:1},
  { id:'y4s16', year:4,group:'B',day:'Wednesday',start:9,   end:10,   code:'CS420',name:'Digital Image Processing',     type:'Section',room:'Lab 4',  instructor:'Eng. Nour Hamed',   color:'#78909c',duration:1},
  { id:'y4s17', year:4,group:'A',day:'Thursday', start:9,   end:10,   code:'CS422',name:'Neural Networks',              type:'Section',room:'Lab 1',  instructor:'Eng. Kareem Nabil', color:'#6366f1',duration:1},
  { id:'y4s18', year:4,group:'B',day:'Thursday', start:10,  end:11,   code:'CS422',name:'Neural Networks',              type:'Section',room:'Lab 1',  instructor:'Eng. Kareem Nabil', color:'#6366f1',duration:1},
];

const SEED_EXAMS = {
  '4_midterm': [
    {id:'me1',year:4,type:'midterm',code:'CS421',name:'Artificial Intelligence',    date:'2026-04-05',time:'10:00 AM',hall:'Hall A — 2nd Floor',  duration:2,  color:'#e8a838'},
    {id:'me2',year:4,type:'midterm',code:'CS411',name:'Theory of Operating Systems',date:'2026-04-07',time:'12:00 PM',hall:'Hall B — 1st Floor',  duration:2,  color:'#3d8fe0'},
    {id:'me3',year:4,type:'midterm',code:'CS420',name:'Digital Image Processing',  date:'2026-04-08',time:'10:00 AM',hall:'Lab Hall — 3rd Floor',duration:2.5,color:'#78909c'},
    {id:'me4',year:4,type:'midterm',code:'CS422',name:'Neural Networks',           date:'2026-04-09',time:'08:00 AM',hall:'Hall C — 2nd Floor',  duration:2,  color:'#6366f1'},
    {id:'me5',year:4,type:'midterm',code:'CS451',name:'Machine Learning',          date:'2026-04-10',time:'10:00 AM',hall:'Hall A — 2nd Floor',  duration:2,  color:'#8b5cf6'},
  ],
  '4_final': [
    {id:'fe1',year:4,type:'final',code:'CS421',name:'Artificial Intelligence',    date:'2026-06-14',time:'09:00 AM',hall:'Main Hall A',duration:3,  color:'#e8a838'},
    {id:'fe2',year:4,type:'final',code:'CS411',name:'Theory of Operating Systems',date:'2026-06-16',time:'11:00 AM',hall:'Main Hall B',duration:3,  color:'#3d8fe0'},
    {id:'fe3',year:4,type:'final',code:'CS420',name:'Digital Image Processing',  date:'2026-06-17',time:'09:00 AM',hall:'Lab Hall 1', duration:3,  color:'#78909c'},
    {id:'fe4',year:4,type:'final',code:'CS422',name:'Neural Networks',           date:'2026-06-18',time:'11:00 AM',hall:'Main Hall C',duration:3,  color:'#6366f1'},
    {id:'fe5',year:4,type:'final',code:'CS451',name:'Machine Learning',          date:'2026-06-19',time:'09:00 AM',hall:'Main Hall A',duration:3,  color:'#8b5cf6'},
  ],
};

/* ── Initial sessions state: keyed by year (number) ── */
const INITIAL_SESSIONS = { 1: [], 2: [], 3: [], 4: SEED_Y4 };

/* ══════════════════════════════════ */
const ScheduleContext = createContext(null);

export function ScheduleProvider({ children }) {
  const [sessions, setSessions] = useState(INITIAL_SESSIONS);
  const [exams,    setExams]    = useState(SEED_EXAMS);

  /* ── sessions ── */
  const addSession = (year, data) =>
    setSessions(p => ({ ...p, [year]: [...(p[year] || []), { id: Date.now() + Math.random(), year, ...data }] }));

  const removeSession = (year, id) =>
    setSessions(p => ({ ...p, [year]: (p[year] || []).filter(s => s.id !== id) }));

  const getSessionsForYear = (year, group = null) => {
    const list = sessions[year] || [];
    return group ? list.filter(s => s.group === group) : list;
  };

  /* ── exams ── */
  const addExam = (year, examType, data) => {
    const key = `${year}_${examType}`;
    setExams(p => ({ ...p, [key]: [...(p[key] || []), { id: Date.now() + Math.random(), year, type: examType, ...data }] }));
  };

  const removeExam = (year, examType, id) => {
    const key = `${year}_${examType}`;
    setExams(p => ({ ...p, [key]: (p[key] || []).filter(e => e.id !== id) }));
  };

  const getExamsForYear = (year, examType) => exams[`${year}_${examType}`] || [];

  const hasSchedule = (year) => (sessions[year] || []).length > 0;

  return (
    <ScheduleContext.Provider value={{
      sessions, exams,
      addSession, removeSession, getSessionsForYear,
      addExam,   removeExam,   getExamsForYear,
      hasSchedule,
    }}>
      {children}
    </ScheduleContext.Provider>
  );
}

export function useSchedule() {
  const ctx = useContext(ScheduleContext);
  if (!ctx) throw new Error('useSchedule must be inside <ScheduleProvider>');
  return ctx;
}
