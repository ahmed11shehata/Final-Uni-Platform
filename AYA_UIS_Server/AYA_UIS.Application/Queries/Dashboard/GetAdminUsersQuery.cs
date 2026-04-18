using MediatR;
using Shared.Dtos.Info_Module.DashboardDtos;

namespace AYA_UIS.Application.Queries.Dashboard
{
    public record GetAdminUsersQuery(string? Search = null, string? Role = null)
        : IRequest<IEnumerable<AdminUserDto>>;

    public record GetAdminUserByCodeQuery(string AcademicCode)
        : IRequest<AdminUserDto?>;
}
