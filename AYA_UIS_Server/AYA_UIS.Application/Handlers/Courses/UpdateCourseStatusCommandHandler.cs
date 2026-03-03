using MediatR;
using Domain.Contracts;
using AYA_UIS.Application.Commands.Courses;

public class UpdateCourseStatusCommandHandler
    : IRequestHandler<UpdateCourseStatusCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        UpdateCourseStatusCommand request,
        CancellationToken cancellationToken)
    {
        var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId);

        if (course is null)
            throw new Exception("Course not found");

        course.Status = request.Status;

        await _unitOfWork.SaveChangesAsync();

        return Unit.Value;
    }
}