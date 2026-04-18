using MediatR;
using Shared.Dtos.Info_Module.RegistrationSettingsDtos;

namespace AYA_UIS.Application.Commands.RegistrationSettings
{
    public record OpenRegistrationCommand(OpenRegistrationDto Dto) : IRequest<RegistrationStatusDto>;
}
