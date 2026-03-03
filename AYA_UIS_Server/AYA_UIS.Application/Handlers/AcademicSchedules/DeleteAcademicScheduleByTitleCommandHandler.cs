using AYA_UIS.Application.Commands.AcademicSchedules;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Shared.Exceptions;

using Domain.Contracts;
using MediatR;

namespace AYA_UIS.Application.Handlers.AcademicSchedules
{
    public class DeleteAcademicScheduleByTitleCommandHandler : IRequestHandler<DeleteAcademicScheduleByTitleCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public DeleteAcademicScheduleByTitleCommandHandler(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<bool> Handle(DeleteAcademicScheduleByTitleCommand request, CancellationToken cancellationToken)
        {
            var schedule = await _unitOfWork.AcademicSchedules.GetByTitleAsync(request.ScheduleTitle);

            if (schedule is null)
                throw new NotFoundException($"Academic schedule '{request.ScheduleTitle}' not found.");

            // Delete file from Cloudinary
            if (!string.IsNullOrEmpty(schedule.FileId))
                await _cloudinaryService.DeleteImageAsync(schedule.FileId, cancellationToken);

            await _unitOfWork.AcademicSchedules.Delete(schedule);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
