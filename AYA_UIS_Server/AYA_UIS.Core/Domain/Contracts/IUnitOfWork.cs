using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;

namespace Domain.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        IDepartmentRepository              Departments              { get; }
        ICourseRepository                  Courses                  { get; }
        IAcademicScheduleRepository        AcademicSchedules        { get; }
        IFeeRepository                     Fees                     { get; }
        IStudyYearRepository               StudyYears               { get; }
        IRegistrationRepository            Registrations            { get; }
        ICourseUploadsRepository           CourseUploads            { get; }
        ISemesterRepository                Semesters                { get; }
        IUserStudyYearRepository           UserStudyYears           { get; }
        ICourseResultRepository            CourseResults            { get; }
        ICourseOfferingRepository          CourseOfferings          { get; }
        IStudentCourseExceptionRepository  StudentCourseExceptions  { get; }
        IAssignmentRepository              Assignments              { get; }
        IQuizRepository                    Quizzes                  { get; }
        IRegistrationSettingsRepository    RegistrationSettings     { get; }
        IAdminCourseLockRepository         AdminCourseLocks         { get; }

        // NEW — Admin Schedule Manager
        IScheduleSessionRepository                  ScheduleSessions            { get; }
        IExamScheduleRepository                     ExamSchedules               { get; }

        // NEW — Instructor Control
        IRegistrationCourseInstructorRepository     RegistrationCourseInstructors { get; }

        Task<int> SaveChangesAsync();
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntities<TKey>;
    }
}
