using Abstraction.Contracts;
using AYA_UIS.Application.Commands.Assignment;
using AYA_UIS.Core.Domain.Entities.Models;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GradeSubmissionCommandHandler
        : IRequestHandler<GradeSubmissionCommand, Response<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public GradeSubmissionCommandHandler(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Response<int>> Handle(
            GradeSubmissionCommand request,
            CancellationToken cancellationToken)
        {
            var submission = await _unitOfWork.Assignments
                .GetSubmissionByIdAsync(request.SubmissionId);

            if (submission == null)
                return Response<int>.ErrorResponse("Submission not found");

            // Rejected submissions cannot be re-graded
            if (submission.Status == "Rejected")
                return Response<int>.ErrorResponse("Rejected submissions cannot be re-graded.");

            // GetByIdAsync now includes Course and CreatedBy
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(submission.AssignmentId);

            if (request.IsAccepted == true)
            {
                submission.Grade    = assignment?.Points ?? request.Grade;
                submission.Status   = "Accepted";
                submission.Feedback = request.Feedback;
            }
            else if (request.IsAccepted == false)
            {
                submission.Grade           = 0;
                submission.Status          = "Rejected";
                submission.RejectionReason = request.RejectionReason;
                submission.Feedback        = request.Feedback;
            }
            else
            {
                // Legacy path — instructor sends Grade directly
                submission.Grade    = request.Grade;
                submission.Feedback = request.Feedback;
                submission.Status   = request.Grade > 0 ? "Accepted" : "Rejected";
            }

            // Persist the graded submission before pushing the notification
            await _unitOfWork.SaveChangesAsync();

            // Build and push the notification via INotificationService (persists + SignalR push)
            var isAccepted = submission.Status == "Accepted";

            var notification = new Notification
            {
                UserId          = submission.StudentId,
                Type            = isAccepted ? "grade_approved" : "grade_rejected",
                Title           = isAccepted ? "Assignment Accepted ✅" : "Assignment Returned ❌",
                Body            = isAccepted
                    ? $"Your submission for '{assignment?.Title}' has been accepted. Grade: {submission.Grade}/{assignment?.Points}"
                    : $"Your submission for '{assignment?.Title}' was returned. Reason: {submission.RejectionReason ?? "No reason provided"}",
                AssignmentId    = assignment?.Id,
                AssignmentTitle = assignment?.Title,
                CourseId        = assignment?.CourseId,
                CourseName      = assignment?.Course?.Name,
                Grade           = submission.Grade,
                Max             = assignment?.Points,
                RejectionReason = submission.RejectionReason,
                InstructorName  = assignment?.CreatedBy?.DisplayName,
                IsRead          = false,
            };

            await _notificationService.SendAsync(notification, cancellationToken);

            return Response<int>.SuccessResponse(submission.Id);
        }
    }
}
