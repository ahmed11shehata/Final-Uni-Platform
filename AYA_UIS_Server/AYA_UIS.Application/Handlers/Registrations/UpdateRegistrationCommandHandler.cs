using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AYA_UIS.Application.Commands.Registrations;
using AYA_UIS.Shared.Exceptions;
using Domain.Contracts;
using MediatR;
using Shared.Respones;

namespace AYA_UIS.Application.Handlers.Registrations
{
    public class UpdateRegistrationCommandHandler : IRequestHandler<UpdateRegistrationCommand,  Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateRegistrationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Unit> Handle(UpdateRegistrationCommand request, CancellationToken cancellationToken)
        {
            var registration = await _unitOfWork.Registrations.GetByIdAsync(request.RegistrationId);
            if (registration == null)
                throw new NotFoundException("Registration not found");

            registration.Status = request.UpdateDto.Status;
            registration.Reason = request.UpdateDto.Reason;

            await _unitOfWork.Registrations.Update(registration);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}