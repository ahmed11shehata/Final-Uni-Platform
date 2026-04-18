using AYA_UIS.Application.Queries.UserStudyYears;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.UserStudyYearDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.UserStudyYears
{
    public class GetCurrentUserStudyYearQueryHandler
        : IRequestHandler<GetCurrentUserStudyYearQuery, Response<UserStudyYearDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrentUserStudyYearQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<UserStudyYearDto>> Handle(
            GetCurrentUserStudyYearQuery request,
            CancellationToken            cancellationToken)
        {
            var current = await _unitOfWork.UserStudyYears.GetCurrentByUserIdAsync(request.UserId);
            if (current is null)
                return Response<UserStudyYearDto>.ErrorResponse(
                    "No current study year found for this user.");

            // Find the active semester for this study year
            int? currentSemesterId = null;
            try
            {
                var semesters = await _unitOfWork.Semesters
                    .GetByStudyYearIdAsync(current.StudyYearId);

                var activeSem = semesters
                    .FirstOrDefault(s =>
                        s.StartDate <= DateTime.UtcNow &&
                        s.EndDate   >= DateTime.UtcNow)
                    ?? semesters
                        .OrderByDescending(s => s.StartDate)
                        .FirstOrDefault();

                currentSemesterId = activeSem?.Id;
            }
            catch { /* non-critical */ }

            return Response<UserStudyYearDto>.SuccessResponse(new UserStudyYearDto
            {
                Id                = current.Id,
                UserId            = current.UserId,
                StudyYearId       = current.StudyYearId,
                StartYear         = current.StudyYear?.StartYear ?? 0,
                EndYear           = current.StudyYear?.EndYear   ?? 0,
                Level             = current.Level,
                LevelName         = current.Level.ToString().Replace("_", " "),
                IsCurrent         = current.StudyYear?.IsCurrent ?? false,
                EnrolledAt        = current.EnrolledAt,
                CurrentSemesterId = currentSemesterId
            });
        }
    }
}
