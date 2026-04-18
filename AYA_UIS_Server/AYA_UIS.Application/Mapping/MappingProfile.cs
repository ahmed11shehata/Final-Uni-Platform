using AutoMapper;
using AYA_UIS.Core.Domain.Entities.Models;
using Shared.Dtos.Info_Module.AcademicSheduleDtos;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Dtos.Info_Module.CourseUploadDtos;
using Shared.Dtos.Info_Module.DepartmentDtos;
using Shared.Dtos.Info_Module.FeeDtos;
using Shared.Dtos.Info_Module.RegistrationDtos;
using Shared.Dtos.Info_Module.StudyYearDtos;
using Shared.Dtos.Info_Module.UserStudyYearDtos;

namespace AYA_UIS.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Department mappings
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<Department, DepartmentDetailsDto>().ReverseMap();
            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<UpdateDepartmentDto, Department>();

            // AcademicSchedule mappings
            CreateMap<AcademicSchedule, AcademicSchedulesDto>().ReverseMap();
            CreateMap<CreateSemesterAcademicScheduleDto, AcademicSchedule>();

            // DepartmentFee mappings

            // Fee mappings
            CreateMap<Fee, FeeDto>().ReverseMap();

            //Course mappings
            CreateMap<Course, CourseDto>().ReverseMap();
            CreateMap<CreateCourseDto, Course>();
            //CreateMap<UpdateCourseDto, Course>();

            //CourseUpload mappings
            CreateMap<CourseUpload, CourseUploadDto>().ReverseMap();
            CreateMap<CreateCourseUploadDto, CourseUpload>();

            //Registration mappings
            CreateMap<Registration, RegistrationCourseDto>()
                .ForMember(dest => dest.CourseId,   opt => opt.MapFrom(src => src.Course != null ? src.Course.Id   : src.CourseId))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course != null ? src.Course.Code : string.Empty))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty));

            // Fee mappings
            CreateMap<Fee, FeeDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
                .ReverseMap();
            CreateMap<CreateFeeDto, Fee>();

            CreateMap<Assignment, AssignmentDto>()
                .ForMember(dest => dest.InstructorName,
                    opt => opt.MapFrom(src => src.CreatedBy != null
                        ? (src.CreatedBy.DisplayName ?? src.CreatedBy.UserName)
                        : string.Empty))
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : string.Empty));


            CreateMap<AssignmentSubmission, AssignmentSubmissionDto>()
.ForMember(dest => dest.StudentName,
opt => opt.MapFrom(src => src.Student.UserName));
            // map study year to study year dto
            CreateMap<StudyYear, StudyYearDto>().ReverseMap();



            //user study year to user study year dto
            CreateMap<UserStudyYear, UserStudyYearDetailsDto>();
            
        }
    }
}


