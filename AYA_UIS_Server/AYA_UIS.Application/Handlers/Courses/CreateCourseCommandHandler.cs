using AutoMapper;
using AYA_UIS.Application.Commands.Courses;
using AYA_UIS.Core.Domain.Entities.Models;
using AYA_UIS.Core.Domain.Enums;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.CourseDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class CreateCourseCommandHandler
        : IRequestHandler<CreateCourseCommand, Response<CourseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<CourseDto>> Handle(
            CreateCourseCommand request,
            CancellationToken cancellationToken)
        {
            // تحقق من القسم
            var department = await _unitOfWork.Departments
                .GetByIdAsync(request.Course.DepartmentId);

            if (department is null)
                throw new Exception(
                    $"Department with ID {request.Course.DepartmentId} not found.");

   
            var course = new Course
            {
                Code = request.Course.Code,
                Name = request.Course.Name,
                Credits = request.Course.Credits,
                DepartmentId = request.Course.DepartmentId,
                Status = CourseStatus.Closed 
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            
            if (request.Course.PrerequisiteCourseCodes.Any())
            {
                var prerequisiteCourses =
                    await _unitOfWork.Courses
                        .GetByCodesAsync(
                            request.Course.PrerequisiteCourseCodes);

                foreach (var prereq in prerequisiteCourses)
                {
                    var relation = new CoursePrerequisite
                    {
                        CourseId = course.Id,
                        PrerequisiteCourseId = prereq.Id
                    };

                    await _unitOfWork.Courses
                        .AddPrerequisiteAsync(relation);
                }

                await _unitOfWork.SaveChangesAsync();
            }


            var result = _mapper.Map<CourseDto>(course);
            return Response<CourseDto>.SuccessResponse(result);
        }
    }
}