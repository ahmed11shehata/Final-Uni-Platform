using MediatR;

namespace AYA_UIS.Application.Commands.RegistrationSettings
{
    public record CloseRegistrationCommand : IRequest<bool>;
}
