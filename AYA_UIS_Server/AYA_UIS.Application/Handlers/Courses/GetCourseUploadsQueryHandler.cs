using AYA_UIS.Application.Queries.Courses;
using AYA_UIS.Core.Domain.Entities.Identity;
using Domain.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos.Info_Module.CourseUploadDtos;

namespace AYA_UIS.Application.Handlers.Courses
{
    public class GetCourseUploadsQueryHandler
        : IRequestHandler<GetCourseUploadsQuery, IEnumerable<CourseUploadDto>>
    {
        private readonly IUnitOfWork       _unitOfWork;
        private readonly UserManager<User> _userManager;

        public GetCourseUploadsQueryHandler(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager)
        {
            _unitOfWork  = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IEnumerable<CourseUploadDto>> Handle(
            GetCourseUploadsQuery request,
            CancellationToken     cancellationToken)
        {
            var uploads = await _unitOfWork.CourseUploads.GetByCourseIdAsync(request.CourseId);

            // Build uploader display-name cache
            var userIds = uploads.Select(u => u.UploadedByUserId).Distinct().ToList();
            var nameMap = new Dictionary<string, string>();
            foreach (var uid in userIds)
            {
                var u = await _userManager.FindByIdAsync(uid);
                nameMap[uid] = u?.DisplayName ?? "Unknown";
            }

            return uploads.Select(u => new CourseUploadDto
            {
                Id          = u.Id,
                Title       = u.Title,
                Description = u.Description,
                Type        = u.Type,
                Url         = u.Url,
                UploadedAt  = u.UploadedAt,
                UploadedBy  = nameMap.GetValueOrDefault(u.UploadedByUserId, "Unknown")
            });
        }
    }
}
