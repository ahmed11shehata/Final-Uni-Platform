using AYA_UIS.Application.Commands.RegistrationSettings;
using Domain.Contracts;
using MediatR;

namespace AYA_UIS.Application.Handlers.RegistrationSettings
{
    public class CloseRegistrationCommandHandler
        : IRequestHandler<CloseRegistrationCommand, bool>
    {
        private readonly IUnitOfWork _uow;

        public CloseRegistrationCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<bool> Handle(
            CloseRegistrationCommand request,
            CancellationToken        ct)
        {
            var existing = await _uow.RegistrationSettings.GetCurrentAsync();
            if (existing is null) return false;

            existing.IsOpen   = false;
            existing.ClosedAt = DateTime.UtcNow;
            await _uow.RegistrationSettings.UpdateAsync(existing);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
