using AYA_UIS.Application.Queries.Departments;
using AutoMapper;
using Domain.Contracts;
using MediatR;
using Shared.Dtos.Info_Module.DepartmentDtos;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Departments
{
    public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, Response<IEnumerable<DepartmentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllDepartmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<DepartmentDto>>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            var result = _mapper.Map<IEnumerable<DepartmentDto>>(departments);
            return Response<IEnumerable<DepartmentDto>>.SuccessResponse(result);
        }
    }
}
