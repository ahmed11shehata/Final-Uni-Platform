using AYA_UIS.Core.Domain.Entities.Identity;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presistence;
using Shared.Dtos.Auth_Module;
using AYA_UIS.Core.Domain.Enums;

public class DataSeeding : IDataSeeding
{
    private readonly UniversityDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public DataSeeding(
        UniversityDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedDataInfoAsync()
    {
        try
        {
            var pendingMigration = await _dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigration.Any())
                await _dbContext.Database.MigrateAsync();


            // ================= Departments =================
            if (!_dbContext.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new() { Name = "Computer Science", Code = "CS" },
                    new() { Name = "Business English", Code = "BE" },
                    new() { Name = "Business Arabic", Code = "BA" },
                    new() { Name = "Journalism", Code = "JR" },
                    new() { Name = "Engineering", Code = "ENG", HasPreparatoryYear = true }
                };

                await _dbContext.Departments.AddRangeAsync(departments);
                await _dbContext.SaveChangesAsync();
            }

            // ================= Study Years (global, not per-department) =================
            if (!_dbContext.StudyYears.Any())
            {
                var studyYears = new List<StudyYear>();

                // Seed global calendar study years from 2018-2019 up to 2025-2026
                for (int year = 2018; year <= 2025; year++)
                {
                    studyYears.Add(new StudyYear
                    {
                        StartYear = year,
                        EndYear = year + 1,
                        IsCurrent = year == 2025 // mark 2025-2026 as current
                    });
                }

                await _dbContext.StudyYears.AddRangeAsync(studyYears);
                await _dbContext.SaveChangesAsync();
            }

            // ================= Semesters =================
            if (!_dbContext.Semesters.Any())
            {
                var allStudyYears = await _dbContext.StudyYears.ToListAsync();
                var semesters = new List<Semester>();

                foreach (var studyYear in allStudyYears)
                {
                    // Semester1 (Fall) — Sep to Dec of StartYear
                    semesters.Add(new Semester
                    {
                        Title = SemesterEnum.First_Semester,
                        StartDate = new DateTime(studyYear.StartYear, 9, 1),
                        EndDate = new DateTime(studyYear.StartYear, 12, 31),
                        StudyYearId = studyYear.Id
                    });

                    // Semester2 (Spring) — Jan to May of EndYear
                    semesters.Add(new Semester
                    {
                        Title = SemesterEnum.Second_Semester,
                        StartDate = new DateTime(studyYear.EndYear, 1, 1),
                        EndDate = new DateTime(studyYear.EndYear, 5, 31),
                        StudyYearId = studyYear.Id
                    });
                }

                await _dbContext.Semesters.AddRangeAsync(semesters);
                await _dbContext.SaveChangesAsync();
            }

            // ================= Courses =================

            #region Test Course 
            //if (!_dbContext.Courses.Any())
            //{
            //    var csDepartment = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "CS");
            //    if (csDepartment != null)
            //    {
            //        var courses = new List<Course>
            //        {
            //            // Year 1
            //            new Course { Code = "CS101", Name = "Introduction to Computer Science", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS102", Name = "Programming Fundamentals I", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS103", Name = "Programming Fundamentals II", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS104", Name = "Computer Organization", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "MATH101", Name = "Discrete Mathematics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "MATH102", Name = "Calculus I", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "ENG101", Name = "English Communication", Credits = 2, DepartmentId = csDepartment.Id },
            //            new Course { Code = "PHY101", Name = "Physics I", Credits = 3, DepartmentId = csDepartment.Id },

            //            // Year 2
            //            new Course { Code = "CS201", Name = "Data Structures", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS202", Name = "Algorithms", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS203", Name = "Object-Oriented Programming", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS204", Name = "Database Systems", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS205", Name = "Web Development", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "MATH201", Name = "Linear Algebra", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "STAT201", Name = "Statistics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "ENG201", Name = "Technical Writing", Credits = 2, DepartmentId = csDepartment.Id },

            //            // Year 3
            //            new Course { Code = "CS301", Name = "Software Engineering", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS302", Name = "Operating Systems", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS303", Name = "Computer Networks", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS304", Name = "Artificial Intelligence", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS305", Name = "Machine Learning", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS306", Name = "Cybersecurity", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS307", Name = "Mobile App Development", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "MATH301", Name = "Numerical Methods", Credits = 3, DepartmentId = csDepartment.Id },

            //            // Year 4
            //            new Course { Code = "CS401", Name = "Advanced Algorithms", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS402", Name = "Distributed Systems", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS403", Name = "Computer Graphics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS404", Name = "Big Data Analytics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS405", Name = "Cloud Computing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS406", Name = "Blockchain Technology", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS407", Name = "IoT and Embedded Systems", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS408", Name = "Capstone Project I", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS409", Name = "Capstone Project II", Credits = 4, DepartmentId = csDepartment.Id },
            //            new Course { Code = "BUS401", Name = "Entrepreneurship", Credits = 2, DepartmentId = csDepartment.Id },

            //            // Elective Courses
            //            new Course { Code = "CS501", Name = "Advanced Machine Learning", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS502", Name = "Deep Learning", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS503", Name = "Natural Language Processing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS504", Name = "Computer Vision", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS505", Name = "Quantum Computing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS506", Name = "Game Development", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS507", Name = "DevOps", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS508", Name = "Ethical Hacking", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS509", Name = "Data Mining", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS510", Name = "Parallel Computing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS511", Name = "Compiler Design", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS512", Name = "Human-Computer Interaction", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS513", Name = "Software Testing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS514", Name = "Cryptography", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS515", Name = "Augmented Reality", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS516", Name = "Virtual Reality", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS517", Name = "Bioinformatics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS518", Name = "Robotics", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS519", Name = "Digital Signal Processing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS520", Name = "Information Retrieval", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS521", Name = "Network Security", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS522", Name = "Advanced Databases", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS523", Name = "Microservices Architecture", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS524", Name = "Serverless Computing", Credits = 3, DepartmentId = csDepartment.Id },
            //            new Course { Code = "CS525", Name = "Edge Computing", Credits = 3, DepartmentId = csDepartment.Id }
            //        };

            //        await _dbContext.Courses.AddRangeAsync(courses);
            //        await _dbContext.SaveChangesAsync();
            //    }
            //}


            #endregion

            if (!_dbContext.Courses.Any())
            {
                var csDepartment = await _dbContext.Departments
                    .FirstOrDefaultAsync(d => d.Code == "CS");

                if (csDepartment != null)
                {
                    var courses = new List<Course>
        {
            // ====== Mandatory ======
            new() { Code="BS101", Name="Calculus", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="G101", Name="Intro to Ecology", Credits=2, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS101", Name="Intro to Computer Science", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS102", Name="Intro to Information Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS103", Name="Computer Programming", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS103", Name="Discrete Mathematics", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS104", Name="Intro to Databases", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS102", Name="Linear Algebra", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="H105", Name="English Language", Credits=2, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS110", Name="Statistics and Probabilities", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS105", Name="Object-Oriented Programming", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="H203", Name="Human Rights", Credits=2, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="H204", Name="Business Administration", Credits=2, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },

            new() { Code="CS201", Name="Data Structures", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS203", Name="Differential Equations", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS205", Name="Operations Research", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS202", Name="Systems Analysis and Design", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS203", Name="File Processing", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="H211", Name="Quality Assurance & Control", Credits=2, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="BS221", Name="Electronics", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS204", Name="Fundamentals of Multimedia", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS205", Name="Assembly Language", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS206", Name="Web Programming", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS207", Name="Data Communications", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },

            new() { Code="CS301", Name="Information Retrieval Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS302", Name="Analysis of Algorithms", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS303", Name="Software Engineering", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS304", Name="Computer Graphics", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS310", Name="Computer Architecture", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS311", Name="Computer Networks", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS312", Name="Natural Language Databases", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS313", Name="Compiler Design & Theory", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },

            new() { Code="CS411", Name="Theory of Operating Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS420", Name="Digital Image Processing", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS421", Name="Artificial Intelligence", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS422", Name="Neural Networks", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS498", Name="Senior Project 1", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS499", Name="Senior Project 2", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },

            // ====== Electives ======
            new() { Code="CS305", Name="Library Information Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS306", Name="Mobile App Development", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS307", Name="Decision Support Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS308", Name="Information Visualization", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS309", Name="Advanced Programming Languages", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS314", Name="Network Management", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS315", Name="Network Security", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS316", Name="Embedded Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS317", Name="Information Security", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS318", Name="Time-Series Databases", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS321", Name="Logic Programming", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS322", Name="Knowledge Representation & Reasoning", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS323", Name="Human Computer Interaction", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS330", Name="Object-Oriented Database", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS331", Name="Electronic Business", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS332", Name="Digital Signal Processing", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS333", Name="Modeling and Simulation", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS334", Name="Geographical Information Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS335", Name="Medical Information Systems", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS336", Name="Speech Processing", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
            new() { Code="CS337", Name="Data Warehouses", Credits=3, DepartmentId=csDepartment.Id, Status=CourseStatus.Closed },
        };

                    await _dbContext.Courses.AddRangeAsync(courses);
                    await _dbContext.SaveChangesAsync();
                }
            }

            // ================= Course Prerequisites =================
            #region Old Prerequisites
            //if (!_dbContext.CoursePrerequisites.Any())
            //{
            //    var csDepartment = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "CS");
            //    if (csDepartment != null)
            //    {
            //        var courses = await _dbContext.Courses.Where(c => c.DepartmentId == csDepartment.Id).ToDictionaryAsync(c => c.Code, c => c.Id);

            //        var prerequisites = new List<CoursePrerequisite>();

            //        // Only add prerequisites for courses that exist in the database
            //        var prerequisiteDefinitions = new[]
            //        {
            //            new { CourseCode = "CS103", PrereqCode = "CS102" },
            //            new { CourseCode = "CS201", PrereqCode = "CS103" },
            //            new { CourseCode = "CS202", PrereqCode = "CS201" },
            //            new { CourseCode = "CS203", PrereqCode = "CS102" },
            //            new { CourseCode = "CS204", PrereqCode = "CS103" },
            //            new { CourseCode = "CS401", PrereqCode = "CS202" },
            //            new { CourseCode = "CS301", PrereqCode = "CS201" },
            //            new { CourseCode = "CS302", PrereqCode = "CS201" },
            //            new { CourseCode = "CS303", PrereqCode = "CS201" },
            //            new { CourseCode = "CS304", PrereqCode = "CS201" },
            //            new { CourseCode = "CS305", PrereqCode = "CS304" },
            //            new { CourseCode = "CS306", PrereqCode = "CS201" },
            //            new { CourseCode = "CS408", PrereqCode = "CS301" },
            //            new { CourseCode = "CS408", PrereqCode = "CS302" },
            //            new { CourseCode = "CS409", PrereqCode = "CS408" },
            //            new { CourseCode = "CS501", PrereqCode = "CS305" },
            //            new { CourseCode = "CS502", PrereqCode = "CS501" },
            //            new { CourseCode = "CS503", PrereqCode = "CS305" },
            //            new { CourseCode = "CS504", PrereqCode = "CS305" },
            //            new { CourseCode = "CS508", PrereqCode = "CS306" },
            //            new { CourseCode = "CS521", PrereqCode = "CS306" },
            //            new { CourseCode = "CS522", PrereqCode = "CS204" }
            //        };

            //        foreach (var prereq in prerequisiteDefinitions)
            //        {
            //            if (courses.ContainsKey(prereq.CourseCode) && courses.ContainsKey(prereq.PrereqCode))
            //            {
            //                prerequisites.Add(new CoursePrerequisite 
            //                { 
            //                    CourseId = courses[prereq.CourseCode], 
            //                    PrerequisiteCourseId = courses[prereq.PrereqCode] 
            //                });
            //            }
            //        }

            //        if (prerequisites.Any())
            //        {
            //            await _dbContext.CoursePrerequisites.AddRangeAsync(prerequisites);
            //            await _dbContext.SaveChangesAsync();
            //        }
            //    }
            //} 
            #endregion

            if (!_dbContext.CoursePrerequisites.Any())
            {
                var courses = await _dbContext.Courses
                    .ToDictionaryAsync(c => c.Code, c => c.Id);

                var prereqDefs = new[]
                {
                       new { Course="CS103", Prereq="CS101" },
                       new { Course="CS105", Prereq="CS103" },
                       new { Course="CS201", Prereq="CS105" },
                       new { Course="CS202", Prereq="CS104" },
                       new { Course="CS203", Prereq="CS103" },
                       new { Course="CS205", Prereq="CS102" },
                       new { Course="CS206", Prereq="CS103" },
                       new { Course="CS207", Prereq="CS101" },
                       new { Course="CS302", Prereq="CS201" },
                       new { Course="CS303", Prereq="CS202" },
                       new { Course="CS304", Prereq="CS204" },
                       new { Course="CS311", Prereq="CS207" },
                       new { Course="CS313", Prereq="CS310" },
                       new { Course="CS422", Prereq="CS302" },
                       new { Course="CS499", Prereq="CS498" }
                };

                var list = new List<CoursePrerequisite>();

                foreach (var p in prereqDefs)
                {
                    if (courses.ContainsKey(p.Course) &&
                        courses.ContainsKey(p.Prereq))
                    {
                        list.Add(new CoursePrerequisite
                        {
                            CourseId = courses[p.Course],
                            PrerequisiteCourseId = courses[p.Prereq]
                        });
                    }
                }

                await _dbContext.CoursePrerequisites.AddRangeAsync(list);
                await _dbContext.SaveChangesAsync();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Seeder Error: " + ex.Message);
            throw; // optional: عشان يظهر full exception
        }
    }

    public async Task SeedIdentityDataAsync()
    {
        // Ensure database is migrated
        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
            await _dbContext.Database.MigrateAsync();

        // Seed roles
        string[] roleNames = { "Admin", "Instructor", "Student" };
        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole { Name = roleName };
                await _roleManager.CreateAsync(role);
            }
        }

        // Seed admin user if no users exist
        if (!_userManager.Users.Any())
        {
            var adminUser = new User()
            {
                DisplayName = "Moustafa Ezzat",
                Email = "MoustafaEzzat@gmail.com",
                UserName = "Moustafa02",
                PhoneNumber = "01557703382",
                Academic_Code = "2203071",
                Level = null,
                Specialization = null,
                TotalCredits = null,
                DepartmentId = null,
                AllowedCredits = null
            };
            
            var result = await _userManager.CreateAsync(adminUser, "Moustafa@123");
            
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
