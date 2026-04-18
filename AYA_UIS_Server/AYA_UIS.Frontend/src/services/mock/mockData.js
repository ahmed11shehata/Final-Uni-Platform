// src/services/mock/mockData.js
// ═══════════════════════════════════════════════════════════════════
//  UNI-LEARN  —  Complete Mock Data
//  Courses from: CS Bachelor Program (133 credit hours)
// ═══════════════════════════════════════════════════════════════════

/* ─── Users ─────────────────────────────────────────────────── */
export const MOCK_USERS = {
  "admin@uni.edu": {
    password: "admin123", role: "admin",
    name: "Ahmed Hassan",   id: "ADM-001",
    token: "mock-token-admin-xyz",
    email: "admin@uni.edu", gender: "male",
    department: "Administration", year: null,
    entryYear: "2020", phone: "+20 100 000 0001",
    address: "Cairo, Egypt", dob: "1985-03-15", avatar: null,
  },
  "instructor@uni.edu": {
    password: "inst123", role: "instructor",
    name: "Dr. Sara Mahmoud", id: "INS-001",
    token: "mock-token-instructor-xyz",
    email: "instructor@uni.edu", gender: "female",
    department: "Computer Science", year: null,
    entryYear: "2018", phone: "+20 100 000 0002",
    address: "Alexandria, Egypt", dob: "1982-07-22", avatar: null,
  },
  "student@uni.edu": {
    password: "std123", role: "student",
    name: "Omar Khaled",  id: "STD-2024-001",
    token: "mock-token-student-xyz",
    email: "student@uni.edu", gender: "male",
    department: "Computer Science", year: "3rd Year",
    entryYear: "2022", phone: "+20 100 000 0003",
    address: "Giza, Egypt", dob: "2003-11-05", avatar: null,
    gpa: 3.2,
  },
};

export const MOCK_DELAY = 1000;

/* ═══════════════════════════════════════════════════════════════
   ACADEMIC STANDING — GPA-Based Credit Hour System
   Egyptian University Standard (Cairo University / Ain Shams style)
═══════════════════════════════════════════════════════════════ */
export const ACADEMIC_STANDING = [
  {
    id: "excellent",
    label: "Excellent",
    labelAr: "ممتاز",
    minGPA: 3.5,
    maxCredits: 21,
    color: "#22c55e",
    bg: "rgba(34,197,94,0.1)",
    border: "rgba(34,197,94,0.25)",
    rule: "Full course load — no restrictions",
    canOnlyRetake: false,
    mustRetakeFirst: false,
  },
  {
    id: "vgood",
    label: "Very Good",
    labelAr: "جيد جداً",
    minGPA: 3.0,
    maxCredits: 18,
    color: "#4ade80",
    bg: "rgba(74,222,128,0.1)",
    border: "rgba(74,222,128,0.25)",
    rule: "Standard course load — no restrictions",
    canOnlyRetake: false,
    mustRetakeFirst: false,
  },
  {
    id: "good",
    label: "Good",
    labelAr: "جيد",
    minGPA: 2.5,
    maxCredits: 18,
    color: "#86efac",
    bg: "rgba(134,239,172,0.1)",
    border: "rgba(134,239,172,0.25)",
    rule: "Standard course load — no restrictions",
    canOnlyRetake: false,
    mustRetakeFirst: false,
  },
  {
    id: "pass",
    label: "Pass",
    labelAr: "مقبول",
    minGPA: 2.0,
    maxCredits: 15,
    color: "#f59e0b",
    bg: "rgba(245,158,11,0.1)",
    border: "rgba(245,158,11,0.25)",
    rule: "Reduced course load — can register normally",
    canOnlyRetake: false,
    mustRetakeFirst: false,
  },
  {
    id: "warning",
    label: "Academic Warning",
    labelAr: "إنذار أكاديمي",
    minGPA: 1.5,
    maxCredits: 12,
    color: "#ef4444",
    bg: "rgba(239,68,68,0.1)",
    border: "rgba(239,68,68,0.25)",
    rule: "Must include all failed courses in registration first",
    canOnlyRetake: false,
    mustRetakeFirst: true,
  },
  {
    id: "probation",
    label: "Academic Probation",
    labelAr: "فصل أكاديمي مشروط",
    minGPA: 0,
    maxCredits: 9,
    color: "#991b1b",
    bg: "rgba(153,27,27,0.1)",
    border: "rgba(153,27,27,0.25)",
    rule: "Can ONLY register previously failed courses",
    canOnlyRetake: true,
    mustRetakeFirst: true,
  },
];

/** Get academic standing from GPA */
export function getAcademicStanding(gpa) {
  if (gpa === null || gpa === undefined) return ACADEMIC_STANDING[1]; // default Very Good for new students
  for (const s of ACADEMIC_STANDING) {
    if (gpa >= s.minGPA) return s;
  }
  return ACADEMIC_STANDING[ACADEMIC_STANDING.length - 1];
}

/* ═══════════════════════════════════════════════════════════════
   FULL COURSE DATABASE
   Source: Computer Science Bachelor Program — 133 credit hours
   Mandatory (109 hrs) + Elective (24 hrs)
═══════════════════════════════════════════════════════════════ */
export const COURSE_DB = [

  /* ════════ YEAR 1 — SEMESTER 1 ════════ */
  {
    code:"BS101", name:"Calculus",                    nameAr:"التفاضل والتكامل",
    credits:3, year:1, semester:1, type:"mandatory",
    dept:"Mathematics", prereqs:[], color:"#6366f1", pattern:"mosaic",
    instructor:"Dr. Mona Adel",
    description:"Differential and integral calculus, limits, derivatives and integrals.",
  },
  {
    code:"G101",  name:"Intro to Ecology",             nameAr:"مقدمة في علم البيئة",
    credits:2, year:1, semester:1, type:"mandatory",
    dept:"General", prereqs:[], color:"#22c55e", pattern:"waves",
    instructor:"Dr. Heba Nasser",
    description:"Basic concepts of ecology, ecosystems and environmental science.",
  },
  {
    code:"CS101", name:"Intro to Computer Science",    nameAr:"مقدمة في علوم الحاسب",
    credits:3, year:1, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:[], color:"#3b82f6", pattern:"circles",
    instructor:"Dr. Ahmed Mostafa",
    description:"Introduction to computing concepts, algorithms and problem solving.",
  },
  {
    code:"CS102", name:"Intro to Information Systems", nameAr:"مقدمة في نظم المعلومات",
    credits:3, year:1, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:[], color:"#0ea5e9", pattern:"squares",
    instructor:"Dr. Khaled Ibrahim",
    description:"Fundamentals of information systems, data management and analysis.",
  },
  {
    code:"CS103", name:"Computer Programming",         nameAr:"برمجة الحاسبات",
    credits:3, year:1, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS101"], color:"#8b5cf6", pattern:"diamonds",
    instructor:"Dr. Sara Khalil",
    description:"Programming fundamentals using C/C++, control structures, functions.",
  },
  {
    code:"BS103", name:"Discrete Mathematics",         nameAr:"الرياضيات غير المتصلة",
    credits:3, year:1, semester:1, type:"mandatory",
    dept:"Mathematics", prereqs:["BS101"], color:"#7c3aed", pattern:"dots",
    instructor:"Dr. Omar Farouk",
    description:"Logic, sets, relations, graphs and combinatorics.",
  },

  /* ════════ YEAR 1 — SEMESTER 2 ════════ */
  {
    code:"CS104", name:"Intro to Databases",           nameAr:"مقدمة في قواعد البيانات",
    credits:3, year:1, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS102"], color:"#ec4899", pattern:"mosaic",
    instructor:"Dr. Dina Mahmoud",
    description:"Database concepts, relational model, SQL and database design.",
  },
  {
    code:"BS102", name:"Linear Algebra",               nameAr:"الجبر الخطي",
    credits:3, year:1, semester:2, type:"mandatory",
    dept:"Mathematics", prereqs:["BS101"], color:"#f59e0b", pattern:"circles",
    instructor:"Dr. Nadia Fouad",
    description:"Vectors, matrices, linear transformations, eigenvalues.",
  },
  {
    code:"H105",  name:"English Language",             nameAr:"اللغة الإنجليزية",
    credits:2, year:1, semester:2, type:"mandatory",
    dept:"Humanities", prereqs:[], color:"#14b8a6", pattern:"waves",
    instructor:"Dr. Mervat Samir",
    description:"Academic English writing, reading and communication skills.",
  },
  {
    code:"BS110", name:"Statistics & Probabilities",   nameAr:"إحصاء واحتمالات",
    credits:3, year:1, semester:2, type:"mandatory",
    dept:"Mathematics", prereqs:["BS101"], color:"#f97316", pattern:"squares",
    instructor:"Dr. Tarek Hassan",
    description:"Probability theory, distributions, statistical inference.",
  },
  {
    code:"CS105", name:"Object-Oriented Programming",  nameAr:"البرمجة الشيئية",
    credits:3, year:1, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS103"], color:"#6366f1", pattern:"diamonds",
    instructor:"Dr. Hana Mostafa",
    description:"OOP concepts: classes, inheritance, polymorphism using Java.",
  },
  {
    code:"H203",  name:"Human Rights",                 nameAr:"حقوق الإنسان",
    credits:2, year:1, semester:2, type:"mandatory",
    dept:"Humanities", prereqs:[], color:"#84cc16", pattern:"dots",
    instructor:"Dr. Laila Ahmed",
    description:"Introduction to human rights, international law and ethics.",
  },
  {
    code:"H204",  name:"Business Administration",      nameAr:"إدارة الأعمال",
    credits:2, year:1, semester:2, type:"mandatory",
    dept:"Humanities", prereqs:[], color:"#06b6d4", pattern:"mosaic",
    instructor:"Dr. Rania Adel",
    description:"Basic principles of business management and administration.",
  },

  /* ════════ YEAR 2 — SEMESTER 1 ════════ */
  {
    code:"CS201", name:"Data Structures",              nameAr:"هياكل البيانات",
    credits:3, year:2, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS105"], color:"#e05c8a", pattern:"circles",
    instructor:"Dr. Khaled Nour",
    description:"Arrays, linked lists, stacks, queues, trees, graphs and sorting.",
  },
  {
    code:"BS203", name:"Differential Equations",       nameAr:"المعادلات التفاضلية",
    credits:3, year:2, semester:1, type:"mandatory",
    dept:"Mathematics", prereqs:["BS101"], color:"#7c6fc4", pattern:"squares",
    instructor:"Dr. Ahmed Zaki",
    description:"Ordinary differential equations and their applications.",
  },
  {
    code:"BS205", name:"Operations Research",          nameAr:"بحوث العمليات",
    credits:3, year:2, semester:1, type:"mandatory",
    dept:"Mathematics", prereqs:["BS103"], color:"#78909c", pattern:"diamonds",
    instructor:"Dr. Samira Youssef",
    description:"Linear programming, transportation, network analysis.",
  },
  {
    code:"CS202", name:"Systems Analysis & Design",    nameAr:"تحليل وتصميم النظم",
    credits:3, year:2, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS104"], color:"#3d8fe0", pattern:"dots",
    instructor:"Dr. Mohamed Farouk",
    description:"System development lifecycle, UML, requirements engineering.",
  },
  {
    code:"CS203", name:"File Processing",              nameAr:"معالجة الملفات",
    credits:3, year:2, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS103"], color:"#e8a838", pattern:"mosaic",
    instructor:"Dr. Heba Rashid",
    description:"File organization, sequential and random access methods.",
  },
  {
    code:"H211",  name:"Quality Assurance & Control",  nameAr:"ضبط وتوكيد الجودة",
    credits:2, year:2, semester:1, type:"mandatory",
    dept:"Humanities", prereqs:[], color:"#10b981", pattern:"waves",
    instructor:"Dr. Iman Khalil",
    description:"Quality management systems, ISO standards and TQM.",
  },

  /* ════════ YEAR 2 — SEMESTER 2 ════════ */
  {
    code:"BS221", name:"Electronics",                  nameAr:"الإلكترونيات",
    credits:3, year:2, semester:2, type:"mandatory",
    dept:"Engineering", prereqs:[], color:"#f59e0b", pattern:"circles",
    instructor:"Dr. Tarek Samir",
    description:"Electronic circuits, semiconductors, digital logic circuits.",
  },
  {
    code:"CS204", name:"Fundamentals of Multimedia",   nameAr:"أساسيات الوسائط المتعددة",
    credits:3, year:2, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS105"], color:"#ec4899", pattern:"squares",
    instructor:"Dr. Nermeen Salah",
    description:"Multimedia components, compression, audio/video processing.",
  },
  {
    code:"CS205", name:"Assembly Language",            nameAr:"لغة التجميع",
    credits:3, year:2, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS102"], color:"#78909c", pattern:"diamonds",
    instructor:"Dr. Amr Sayed",
    description:"Assembly language programming, CPU architecture, addressing modes.",
  },
  {
    code:"CS206", name:"Web Programming",              nameAr:"برمجة الويب",
    credits:3, year:2, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS103"], color:"#6366f1", pattern:"dots",
    instructor:"Dr. Sara Mahmoud",
    description:"HTML5, CSS3, JavaScript, client-server architecture.",
  },
  {
    code:"CS207", name:"Data Communications",          nameAr:"اتصالات البيانات",
    credits:3, year:2, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS101"], color:"#0ea5e9", pattern:"mosaic",
    instructor:"Dr. Youssef Khalid",
    description:"Data transmission, protocols, OSI model, error detection.",
  },

  /* ════════ YEAR 3 — SEMESTER 1 ════════ */
  {
    code:"CS301", name:"Information Retrieval Systems",nameAr:"نظم استرجاع المعلومات",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS203"], color:"#8b5cf6", pattern:"circles",
    instructor:"Dr. Mariam Selim",
    description:"Search engines, indexing, ranking algorithms, text mining.",
  },
  {
    code:"CS302", name:"Analysis of Algorithms",       nameAr:"تحليل الخوارزميات",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS201"], color:"#3b82f6", pattern:"squares",
    instructor:"Dr. Ibrahim Nour",
    description:"Algorithm design, complexity analysis, dynamic programming.",
  },
  {
    code:"CS303", name:"Software Engineering",         nameAr:"هندسة البرمجيات",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS202"], color:"#22c55e", pattern:"diamonds",
    instructor:"Dr. Dalia Hassan",
    description:"SDLC methodologies, Agile, testing, software quality.",
  },
  {
    code:"CS304", name:"Computer Graphics",            nameAr:"الرسم بالحاسب",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS204"], color:"#e05c8a", pattern:"dots",
    instructor:"Dr. Amr Saleh",
    description:"2D/3D graphics, transformations, rendering, OpenGL.",
  },
  {
    code:"CS310", name:"Computer Architecture",        nameAr:"بناء الحاسب",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS205"], color:"#f97316", pattern:"mosaic",
    instructor:"Dr. Karim Farid",
    description:"CPU design, memory hierarchy, pipelining, RISC/CISC.",
  },
  {
    code:"CS311", name:"Computer Networks",            nameAr:"مقدمة في شبكات الحاسب",
    credits:3, year:3, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS207"], color:"#14b8a6", pattern:"waves",
    instructor:"Dr. Sherif Moussa",
    description:"Network protocols, TCP/IP, routing, security basics.",
  },

  /* ════════ YEAR 3 — SEMESTER 2 ════════ */
  {
    code:"CS312", name:"Natural Language Databases",   nameAr:"قواعد بيانات اللغات الحية",
    credits:3, year:3, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS302"], color:"#7c6fc4", pattern:"circles",
    instructor:"Dr. Khaled Ibrahim",
    description:"NLP databases, linguistic data, morphological analysis.",
  },
  {
    code:"CS313", name:"Compiler Design & Theory",     nameAr:"نظرية وتصميم المترجمات",
    credits:3, year:3, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS310"], color:"#e8a838", pattern:"squares",
    instructor:"Dr. Heba Nasser",
    description:"Lexical analysis, parsing, code generation, optimization.",
  },

  /* ════════ YEAR 4 — SEMESTER 1 ════════ */
  {
    code:"CS411", name:"Theory of Operating Systems",  nameAr:"نظريات نظم التشغيل",
    credits:3, year:4, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS310"], color:"#3d8fe0", pattern:"diamonds",
    instructor:"Dr. Rania Hassan",
    description:"Process scheduling, memory management, file systems, IPC.",
  },
  {
    code:"CS420", name:"Digital Image Processing",     nameAr:"معالجة الصور الرقمية",
    credits:3, year:4, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS304"], color:"#78909c", pattern:"dots",
    instructor:"Dr. Mohamed Ali",
    description:"Image enhancement, segmentation, feature extraction, ML.",
  },
  {
    code:"CS421", name:"Artificial Intelligence",      nameAr:"الذكاء الاصطناعي",
    credits:3, year:4, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS302"], color:"#e8a838", pattern:"mosaic",
    instructor:"Dr. Mohamed Farouk",
    description:"Search, knowledge representation, reasoning, planning.",
  },
  {
    code:"CS422", name:"Neural Networks",              nameAr:"الشبكات العصبية",
    credits:3, year:4, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS302"], color:"#6366f1", pattern:"waves",
    instructor:"Dr. Sara Khalil",
    description:"Perceptrons, backpropagation, deep learning architectures.",
  },
  {
    code:"CS498", name:"Senior Project 1",             nameAr:"مشروع التخرج ١",
    credits:3, year:4, semester:1, type:"mandatory",
    dept:"Computer Science", prereqs:["CS313"], color:"#22c55e", pattern:"circles",
    instructor:"Dr. Department Committee",
    description:"First phase of graduation project: proposal, design, implementation.",
  },

  /* ════════ YEAR 4 — SEMESTER 2 ════════ */
  {
    code:"CS499", name:"Senior Project 2",             nameAr:"مشروع التخرج ٢",
    credits:3, year:4, semester:2, type:"mandatory",
    dept:"Computer Science", prereqs:["CS498"], color:"#f59e0b", pattern:"squares",
    instructor:"Dr. Department Committee",
    description:"Final phase: testing, documentation, presentation and defense.",
  },

  /* ════════ ELECTIVE COURSES (24 hrs) ════════ */
  {
    code:"CS451", name:"Machine Learning",             nameAr:"التعلم الآلي",
    credits:3, year:4, semester:1, type:"elective",
    dept:"Computer Science", prereqs:["CS421"], color:"#8b5cf6", pattern:"diamonds",
    instructor:"Dr. Aya Mostafa",
    description:"Supervised/unsupervised learning, regression, classification.",
  },
  {
    code:"CS452", name:"Cloud Computing",              nameAr:"الحوسبة السحابية",
    credits:3, year:4, semester:1, type:"elective",
    dept:"Computer Science", prereqs:["CS311"], color:"#0ea5e9", pattern:"dots",
    instructor:"Dr. Amr Kamel",
    description:"Cloud services, virtualization, AWS/Azure fundamentals.",
  },
  {
    code:"CS453", name:"Mobile App Development",       nameAr:"تطوير تطبيقات الجوال",
    credits:3, year:4, semester:1, type:"elective",
    dept:"Computer Science", prereqs:["CS206"], color:"#ec4899", pattern:"mosaic",
    instructor:"Dr. Nour El-Din",
    description:"Android/iOS development, React Native, cross-platform.",
  },
  {
    code:"CS454", name:"Cybersecurity Fundamentals",   nameAr:"أساسيات الأمن السيبراني",
    credits:3, year:4, semester:1, type:"elective",
    dept:"Computer Science", prereqs:["CS311"], color:"#ef4444", pattern:"circles",
    instructor:"Dr. Sherif Saad",
    description:"Network security, cryptography, ethical hacking basics.",
  },
  {
    code:"CS455", name:"Big Data Analytics",           nameAr:"تحليل البيانات الضخمة",
    credits:3, year:4, semester:2, type:"elective",
    dept:"Computer Science", prereqs:["CS302"], color:"#f97316", pattern:"squares",
    instructor:"Dr. Mariam Youssef",
    description:"Hadoop, Spark, data warehousing, MapReduce.",
  },
  {
    code:"CS456", name:"Internet of Things",           nameAr:"إنترنت الأشياء",
    credits:3, year:4, semester:2, type:"elective",
    dept:"Computer Science", prereqs:["CS207"], color:"#10b981", pattern:"waves",
    instructor:"Dr. Karim Nabil",
    description:"IoT architecture, sensors, embedded systems, protocols.",
  },
  {
    code:"CS457", name:"Human-Computer Interaction",   nameAr:"تفاعل الإنسان والحاسب",
    credits:3, year:3, semester:2, type:"elective",
    dept:"Computer Science", prereqs:["CS202"], color:"#7c6fc4", pattern:"dots",
    instructor:"Dr. Dalia Ragab",
    description:"UX design principles, usability testing, accessibility.",
  },
  {
    code:"CS458", name:"Distributed Systems",          nameAr:"الأنظمة الموزعة",
    credits:3, year:4, semester:2, type:"elective",
    dept:"Computer Science", prereqs:["CS311"], color:"#3d8fe0", pattern:"diamonds",
    instructor:"Dr. Islam Samir",
    description:"Distributed computing, consensus algorithms, CAP theorem.",
  },
];

/** Get courses by year and semester */
export function getCoursesByYear(year, semester = null) {
  return COURSE_DB.filter(c =>
    c.year === year && (semester === null || c.semester === semester)
  );
}

/** Get a course by code */
export function getCourseByCode(code) {
  return COURSE_DB.find(c => c.code === code);
}

/* ═══════════════════════════════════════════════════════════════
   MOCK STUDENTS (for ManageUsers page)
   Fields: registeredCourses, completedCourses (with grades), failedCourses
═══════════════════════════════════════════════════════════════ */
export const MOCK_STUDENTS = {

  /* ── Student 1: Good standing, Year 3 ── */
  "CS2024001": {
    id: "CS2024001", code: "CS2024001",
    name: "Ahmed Mohamed Ali",
    email: "ahmed@university.edu",
    phone: "+20 100 111 2233",
    avatar: null, gender: "male",
    year: 3, semester: 1,
    dept: "Computer Science",
    gpa: 3.2,
    totalCreditsEarned: 90,
    entryYear: "2022",
    address: "Cairo, Egypt",
    dob: "2002-05-14",

    // Currently registered this semester
    registeredCourses: ["CS301","CS302","CS303","CS310","CS311","CS304"],

    // Completed courses with grades (pass = total >= 60)
    completedCourses: [
      // Year 1 S1
      { code:"BS101", total:88, year:1, semester:1 },
      { code:"G101",  total:75, year:1, semester:1 },
      { code:"CS101", total:92, year:1, semester:1 },
      { code:"CS102", total:85, year:1, semester:1 },
      { code:"CS103", total:90, year:1, semester:1 },
      { code:"BS103", total:82, year:1, semester:1 },
      // Year 1 S2
      { code:"CS104", total:88, year:1, semester:2 },
      { code:"BS102", total:79, year:1, semester:2 },
      { code:"H105",  total:91, year:1, semester:2 },
      { code:"BS110", total:84, year:1, semester:2 },
      { code:"CS105", total:94, year:1, semester:2 },
      { code:"H203",  total:87, year:1, semester:2 },
      { code:"H204",  total:82, year:1, semester:2 },
      // Year 2 S1
      { code:"CS201", total:86, year:2, semester:1 },
      { code:"BS203", total:72, year:2, semester:1 },
      { code:"BS205", total:68, year:2, semester:1 },
      { code:"CS202", total:91, year:2, semester:1 },
      { code:"CS203", total:88, year:2, semester:1 },
      { code:"H211",  total:85, year:2, semester:1 },
      // Year 2 S2
      { code:"BS221", total:76, year:2, semester:2 },
      { code:"CS204", total:89, year:2, semester:2 },
      { code:"CS205", total:83, year:2, semester:2 },
      { code:"CS206", total:95, year:2, semester:2 },
      { code:"CS207", total:87, year:2, semester:2 },
    ],

    // Failed = completed with total < 60 OR not yet taken
    failedCourses: [], // this student passed everything

    // Admin overrides (admin can manually unlock/lock courses)
    adminUnlocked: [],
    adminLocked:   [],

    // Admin override for credit hours (null = use GPA auto)
    adminMaxCredits: null,
  },

  /* ── Student 2: Academic Warning, Year 2 ── */
  "CS2024002": {
    id: "CS2024002", code: "CS2024002",
    name: "Sara Khaled Ibrahim",
    email: "sara@university.edu",
    phone: "+20 100 222 3344",
    avatar: null, gender: "female",
    year: 2, semester: 1,
    dept: "Computer Science",
    gpa: 1.6,
    totalCreditsEarned: 31,
    entryYear: "2023",
    address: "Alexandria, Egypt",
    dob: "2003-09-20",

    registeredCourses: ["CS201","BS203"],

    completedCourses: [
      // Year 1 S1 — some failures
      { code:"BS101", total:55, year:1, semester:1 }, // FAILED
      { code:"G101",  total:72, year:1, semester:1 },
      { code:"CS101", total:78, year:1, semester:1 },
      { code:"CS102", total:65, year:1, semester:1 },
      { code:"CS103", total:48, year:1, semester:1 }, // FAILED
      { code:"BS103", total:52, year:1, semester:1 }, // FAILED (needs BS101)
      // Year 1 S2
      { code:"H105",  total:80, year:1, semester:2 },
      { code:"H203",  total:75, year:1, semester:2 },
      { code:"H204",  total:70, year:1, semester:2 },
      // CS105 locked because CS103 failed
      // Year 2 S1 — retaking
      { code:"BS101", total:71, year:2, semester:1 }, // Retake — passed
    ],

    failedCourses: ["CS103","BS103"], // still need to retake these
    adminUnlocked: [],
    adminLocked:   [],
    adminMaxCredits: null,
  },

  /* ── Student 3: Pass standing, Year 2 ── */
  "CS2024003": {
    id: "CS2024003", code: "CS2024003",
    name: "Omar Hassan Farouk",
    email: "omar@university.edu",
    phone: "+20 100 333 4455",
    avatar: null, gender: "male",
    year: 2, semester: 2,
    dept: "Computer Science",
    gpa: 2.4,
    totalCreditsEarned: 60,
    entryYear: "2023",
    address: "Giza, Egypt",
    dob: "2003-03-08",

    registeredCourses: ["BS221","CS204","CS205","CS206","CS207"],

    completedCourses: [
      // Year 1 S1
      { code:"BS101", total:70, year:1, semester:1 },
      { code:"G101",  total:68, year:1, semester:1 },
      { code:"CS101", total:74, year:1, semester:1 },
      { code:"CS102", total:72, year:1, semester:1 },
      { code:"CS103", total:76, year:1, semester:1 },
      { code:"BS103", total:65, year:1, semester:1 },
      // Year 1 S2
      { code:"CS104", total:71, year:1, semester:2 },
      { code:"BS102", total:68, year:1, semester:2 },
      { code:"H105",  total:82, year:1, semester:2 },
      { code:"BS110", total:66, year:1, semester:2 },
      { code:"CS105", total:73, year:1, semester:2 },
      { code:"H203",  total:79, year:1, semester:2 },
      { code:"H204",  total:75, year:1, semester:2 },
      // Year 2 S1
      { code:"CS201", total:69, year:2, semester:1 },
      { code:"BS203", total:62, year:2, semester:1 },
      { code:"BS205", total:67, year:2, semester:1 },
      { code:"CS202", total:71, year:2, semester:1 },
      { code:"CS203", total:74, year:2, semester:1 },
      { code:"H211",  total:80, year:2, semester:1 },
    ],

    failedCourses: [],
    adminUnlocked: [],
    adminLocked:   [],
    adminMaxCredits: null,
  },
};

/* ═══════════════════════════════════════════════════════════════
   REGISTRATION ENGINE
   Core logic: calculate what a student CAN register
═══════════════════════════════════════════════════════════════ */

/** Get grade letter from total score */
export function getGradeFromTotal(total) {
  if (total >= 97) return "A+";
  if (total >= 93) return "A";
  if (total >= 90) return "A-";
  if (total >= 87) return "B+";
  if (total >= 83) return "B";
  if (total >= 80) return "B-";
  if (total >= 77) return "C+";
  if (total >= 73) return "C";
  if (total >= 70) return "C-";
  if (total >= 67) return "D+";
  if (total >= 60) return "D";
  return "F";
}

/** Get GPA points from total score */
export function getGPAPoints(total) {
  if (total >= 97) return 4.0;
  if (total >= 93) return 4.0;
  if (total >= 90) return 3.7;
  if (total >= 87) return 3.3;
  if (total >= 83) return 3.0;
  if (total >= 80) return 2.7;
  if (total >= 77) return 2.3;
  if (total >= 73) return 2.0;
  if (total >= 70) return 1.7;
  if (total >= 67) return 1.3;
  if (total >= 60) return 1.0;
  return 0.0;
}

/**
 * Calculate course status for a student
 * Returns: "registered" | "completed" | "failed" | "locked" | "available" | "unavailable"
 *
 * Logic:
 * 1. If currently registered → "registered"
 * 2. If completed with pass (>= 60) → "completed"
 * 3. If completed with fail (< 60) and is most recent attempt → "failed" (needs retake)
 * 4. If admin locked → "locked"
 * 5. If prerequisite not met → "locked"
 * 6. If student is on probation and course is not failed → "unavailable"
 * 7. If admin unlocked → "available" (override)
 * 8. If course not in open year/semester → "unavailable"
 * 9. Otherwise → "available"
 */
export function getCourseStatus(course, student, openYears = null) {
  const { registeredCourses, completedCourses, failedCourses, adminUnlocked, adminLocked } = student;
  const standing = getAcademicStanding(student.gpa);

  // Admin hard lock
  if (adminLocked?.includes(course.code)) return "locked";

  // Currently registered
  if (registeredCourses?.includes(course.code)) return "registered";

  // Check completions (take most recent)
  const attempts = completedCourses?.filter(c => c.code === course.code) || [];
  if (attempts.length > 0) {
    const latest = attempts[attempts.length - 1];
    if (latest.total >= 60) return "completed"; // passed
    // Latest attempt was a fail → show as "failed" (needs retake)
    return "failed";
  }

  // Admin unlock override — bypasses ALL restrictions
  if (adminUnlocked?.includes(course.code)) return "available";

  // Probation: can ONLY register failed courses
  if (standing.canOnlyRetake && !failedCourses?.includes(course.code)) return "unavailable";

  // Check prerequisites
  const prereqsMet = course.prereqs.every(prereqCode => {
    const prereqAttempts = completedCourses?.filter(c => c.code === prereqCode) || [];
    // Check if any attempt passed
    return prereqAttempts.some(a => a.total >= 60) || adminUnlocked?.includes(prereqCode);
  });
  if (!prereqsMet) return "locked";

  // Check if course is in an open year (if admin has set open years)
  if (openYears && !openYears.includes(course.year)) return "unavailable";

  return "available";
}
