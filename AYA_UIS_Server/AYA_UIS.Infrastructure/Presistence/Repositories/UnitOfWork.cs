using System.Collections.Concurrent;
using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Presistence;
using Presistence.Repositories;

namespace Presistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UniversityDbContext _dbContext;
        private readonly ConcurrentDictionary<string, object> _repositories = new();

        private IDepartmentRepository?             _departments;
        private ICourseRepository?                 _courses;
        private IAcademicScheduleRepository?       _academicSchedules;
        private IFeeRepository?                    _fees;
        private IStudyYearRepository?              _studyYears;
        private IRegistrationRepository?           _registrations;
        private ICourseUploadsRepository?          _courseUploads;
        private ISemesterRepository?               _semesters;
        private IUserStudyYearRepository?          _userStudyYears;
        private ICourseResultRepository?           _courseResults;
        private ICourseOfferingRepository?         _courseOfferings;
        private IStudentCourseExceptionRepository? _studentCourseExceptions;
        private IAssignmentRepository?             _assignments;
        private IQuizRepository?                   _quizzes;
        private IRegistrationSettingsRepository?   _registrationSettings;
        private IAdminCourseLockRepository?        _adminCourseLocks;
        private IScheduleSessionRepository?                  _scheduleSessions;
        private IExamScheduleRepository?                     _examSchedules;
        private IRegistrationCourseInstructorRepository?     _registrationCourseInstructors;
        private INotificationRepository?                     _notifications;
        private IMidtermGradeRepository?                     _midtermGrades;
        private IFinalGradeRepository?                       _finalGrades;
        private IFinalGradeReviewRepository?                 _finalGradeReviews;

        public UnitOfWork(UniversityDbContext dbContext) => _dbContext = dbContext;

        public IDepartmentRepository            Departments             => _departments            ??= new DepartmentRepository(_dbContext);
        public ICourseRepository                Courses                 => _courses                ??= new CourseRepository(_dbContext);
        public IAcademicScheduleRepository      AcademicSchedules       => _academicSchedules      ??= new AcademicScheduleRepository(_dbContext);
        public IFeeRepository                   Fees                    => _fees                   ??= new FeeRepository(_dbContext);
        public IStudyYearRepository             StudyYears              => _studyYears             ??= new StudyYearRepository(_dbContext);
        public IRegistrationRepository          Registrations           => _registrations          ??= new RegistrationRepository(_dbContext);
        public ICourseUploadsRepository         CourseUploads           => _courseUploads          ??= new CourseUploadsRepository(_dbContext);
        public ISemesterRepository              Semesters               => _semesters              ??= new SemesterRepository(_dbContext);
        public IUserStudyYearRepository         UserStudyYears          => _userStudyYears         ??= new UserStudyYearRepository(_dbContext);
        public ICourseResultRepository          CourseResults           => _courseResults          ??= new CourseResultRepository(_dbContext);
        public ICourseOfferingRepository        CourseOfferings         => _courseOfferings        ??= new CourseOfferingRepository(_dbContext);
        public IStudentCourseExceptionRepository StudentCourseExceptions => _studentCourseExceptions ??= new StudentCourseExceptionRepository(_dbContext);
        public IAssignmentRepository            Assignments             => _assignments            ??= new AssignmentRepository(_dbContext);
        public IQuizRepository                  Quizzes                 => _quizzes               ??= new QuizRepository(_dbContext);
        public IRegistrationSettingsRepository  RegistrationSettings    => _registrationSettings   ??= new RegistrationSettingsRepository(_dbContext);
        public IAdminCourseLockRepository       AdminCourseLocks        => _adminCourseLocks       ??= new AdminCourseLockRepository(_dbContext);
        public IScheduleSessionRepository       ScheduleSessions        => _scheduleSessions       ??= new ScheduleSessionRepository(_dbContext);
        public IExamScheduleRepository                    ExamSchedules                   => _examSchedules                   ??= new ExamScheduleRepository(_dbContext);
        public IRegistrationCourseInstructorRepository    RegistrationCourseInstructors   => _registrationCourseInstructors   ??= new RegistrationCourseInstructorRepository(_dbContext);
        public INotificationRepository                    Notifications                   => _notifications                   ??= new NotificationRepository(_dbContext);
        public IMidtermGradeRepository                    MidtermGrades                   => _midtermGrades                   ??= new MidtermGradeRepository(_dbContext);
        public IFinalGradeRepository                      FinalGrades                     => _finalGrades                     ??= new FinalGradeRepository(_dbContext);
        public IFinalGradeReviewRepository                FinalGradeReviews               => _finalGradeReviews               ??= new FinalGradeReviewRepository(_dbContext);

        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntities<TKey>
            => (IGenericRepository<TEntity, TKey>)_repositories.GetOrAdd(
                typeof(TEntity).Name, _ => new GenericRepository<TEntity, TKey>(_dbContext));

        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();
        public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }
    }
}
