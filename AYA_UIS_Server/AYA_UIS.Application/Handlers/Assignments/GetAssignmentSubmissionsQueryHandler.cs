using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AYA_UIS.Application.Queries.Assignments;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.AssignmentDto;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Assignments
{
    public class GetAssignmentSubmissionsQueryHandler
: IRequestHandler<GetAssignmentSubmissionsQuery,
    Response<IEnumerable<AssignmentSubmissionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAssignmentSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AssignmentSubmissionDto>>> Handle(
            GetAssignmentSubmissionsQuery request,
            CancellationToken cancellationToken)
        {
            var submissions = await _unitOfWork.Assignments
                .GetSubmissions(request.AssignmentId);

            var result = _mapper.Map<IEnumerable<AssignmentSubmissionDto>>(submissions);

            return Response<IEnumerable<AssignmentSubmissionDto>>
                .SuccessResponse(result);
        }
    }
}
