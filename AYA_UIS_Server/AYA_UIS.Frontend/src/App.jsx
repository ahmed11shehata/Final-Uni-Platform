// src/App.jsx
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import { ThemeProvider } from "./context/ThemeContext";
import { NotificationProvider } from "./context/NotificationContext";
import { RegistrationProvider } from "./context/RegistrationContext";
import ProtectedRoute from "./components/common/ProtectedRoute";
import MainLayout from "./components/layout/MainLayout";
import RamadanAtmosphere from "./components/common/RamadanAtmosphere";
import SpaceAtmosphere   from "./components/common/SpaceAtmosphere";
import ArcticAtmosphere  from "./components/common/ArcticAtmosphere";
import PharaohAtmosphere from "./components/common/PharaohAtmosphere";
import SaladinAtmosphere from "./components/common/SaladinAtmosphere";
import FogAtmosphere     from "./components/common/FogAtmosphere";
import { Toaster } from "react-hot-toast";

import LoginPage from "./pages/auth/LoginPage";
import ForgotPasswordPage from "./pages/auth/ForgotPasswordPage";
import SettingsPage from "./pages/shared/SettingsPage";

import StudentDashboard       from "./pages/student/StudentDashboard";
import GradesPage             from "./pages/student/GradesPage";
import TimetablePage          from "./pages/student/TimetablePage";
import QuizzesPage            from "./pages/student/QuizzesPage";
import ThemePage              from "./pages/student/ThemePage";
import AIToolsPage            from "./pages/student/AIToolsPage";
import ProfilePage            from "./pages/student/ProfilePage";
import SummaryTool      from "./pages/student/tools/SummaryTool";
import QuizTool         from "./pages/student/tools/QuizTool";
import MindMapTool      from "./pages/student/tools/MindMapTool";
import QuestionBankTool from "./pages/student/tools/QuestionBankTool";
import ChatTool         from "./pages/student/tools/ChatTool";
import GenerateAllTool  from "./pages/student/tools/GenerateAllTool";
import CourseRegistrationPage from "./pages/student/CourseRegistrationPage";
import CoursesPage            from "./pages/student/CoursesPage";
import CourseDetailPage       from "./pages/student/CourseDetailPage";
import QuizDetail             from "./pages/student/QuizDetail";

import InstructorDashboard from "./pages/instructor/InstructorDashboard";
import QuizBuilderPage     from "./pages/instructor/QuizBuilderPage";
import AssignmentMgmtPage  from "./pages/instructor/AssignmentMgmtPage";
import LectureUploadPage   from "./pages/instructor/LectureUploadPage";
import UploadMaterialPage  from "./pages/instructor/UploadMaterialPage";
import GradesMgmtPage      from "./pages/instructor/GradesMgmtPage";

import AdminDashboard   from "./pages/admin/AdminDashboard";
import RegisterUserPage from "./pages/admin/RegisterUserPage";
import ManageUsers      from "./pages/admin/ManageUsers";
import AdminCoursesPage from "./pages/admin/AdminCoursesPage";
import RegistrationManagerPage from "./pages/admin/RegistrationManagerPage";
import RegisterEmailPage      from "./pages/admin/RegisterEmailPage";
import AdminSchedulePage         from "./pages/admin/AdminSchedulePage";
import InstructorControlPage    from "./pages/admin/InstructorControlPage";
import SchedulePage              from "./pages/student/SchedulePage";

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <NotificationProvider>
          <RegistrationProvider>
          <BrowserRouter>
            <Toaster position="top-center" toastOptions={{ style: { fontFamily: "Sora" } }} />
            <RamadanAtmosphere />
            <SpaceAtmosphere />
            <ArcticAtmosphere />
            <PharaohAtmosphere />
            <SaladinAtmosphere />
            <FogAtmosphere />
            <Routes>

              <Route path="/login" element={<LoginPage />} />
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />
              <Route path="/" element={<Navigate to="/login" replace />} />

              {/* ── Student ── */}
              <Route element={<ProtectedRoute allowedRoles={["student"]}><MainLayout /></ProtectedRoute>}>
                <Route path="/student/dashboard"         element={<StudentDashboard />} />
                <Route path="/student/grades"            element={<GradesPage />} />
                <Route path="/student/timetable"         element={<TimetablePage />} />
                <Route path="/student/quizzes"           element={<QuizzesPage />} />
                <Route path="/student/themes"            element={<ThemePage />} />
                <Route path="/student/profile"            element={<ProfilePage />} />
                <Route path="/student/ai-tools"                element={<AIToolsPage />} />
                <Route path="/student/ai-tools/summary"       element={<SummaryTool />} />
                <Route path="/student/ai-tools/quiz"           element={<QuizTool />} />
                <Route path="/student/ai-tools/mindmap"        element={<MindMapTool />} />
                <Route path="/student/ai-tools/question-bank"  element={<QuestionBankTool />} />
                <Route path="/student/ai-tools/chat"           element={<ChatTool />} />
                <Route path="/student/ai-tools/generate-all"   element={<GenerateAllTool />} />
                <Route path="/student/register-courses"  element={<CourseRegistrationPage />} />
                <Route path="/student/schedule"          element={<SchedulePage />} />
                <Route path="/student/courses"           element={<CoursesPage />} />
                <Route path="/student/courses/:courseId" element={<CourseDetailPage />} />
                <Route path="/student/quiz/:courseId/:quizId" element={<QuizDetail />} />
                <Route path="/student/settings"         element={<SettingsPage />} />
              </Route>

              {/* ── Instructor ── */}
              <Route element={<ProtectedRoute allowedRoles={["instructor"]}><MainLayout /></ProtectedRoute>}>
                <Route path="/instructor/dashboard"    element={<InstructorDashboard />} />
                <Route path="/instructor/schedule"       element={<SchedulePage />} />
                <Route path="/instructor/quiz-builder" element={<QuizBuilderPage />} />
                <Route path="/instructor/assignments"  element={<AssignmentMgmtPage />} />
                <Route path="/instructor/lectures"     element={<LectureUploadPage />} />
                <Route path="/instructor/material"     element={<UploadMaterialPage />} />
                <Route path="/instructor/grades"       element={<GradesMgmtPage />} />
                <Route path="/instructor/themes"       element={<ThemePage />} />
                <Route path="/instructor/profile"       element={<ProfilePage />} />
                <Route path="/instructor/settings"     element={<SettingsPage />} />
              </Route>

              {/* ── Admin ── */}
              <Route element={<ProtectedRoute allowedRoles={["admin"]}><MainLayout /></ProtectedRoute>}>
                <Route path="/admin/dashboard"    element={<AdminDashboard />} />
                <Route path="/admin/register"     element={<RegisterUserPage />} />
                <Route path="/admin/manage-users" element={<ManageUsers />} />
                <Route path="/admin/courses"      element={<AdminCoursesPage />} />
                <Route path="/admin/registration"  element={<RegistrationManagerPage />} />
                <Route path="/admin/email-manager"  element={<RegisterEmailPage />} />
                <Route path="/admin/schedule"             element={<AdminSchedulePage />} />
                <Route path="/admin/instructor-control"  element={<InstructorControlPage />} />
                <Route path="/admin/themes"              element={<ThemePage />} />
                <Route path="/admin/profile"       element={<ProfilePage />} />
              </Route>

              <Route path="*" element={<Navigate to="/login" replace />} />

            </Routes>
          </BrowserRouter>
          </RegistrationProvider>
        </NotificationProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}