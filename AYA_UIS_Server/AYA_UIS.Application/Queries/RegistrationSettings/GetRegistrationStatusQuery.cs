using MediatR;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace AYA_UIS.Application.Queries.RegistrationSettings
{
    public record GetRegistrationStatusQuery : IRequest<RegistrationStatusDto>;
}
