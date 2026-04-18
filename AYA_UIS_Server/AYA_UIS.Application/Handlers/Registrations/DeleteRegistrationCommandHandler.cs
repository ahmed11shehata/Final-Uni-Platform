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
    public class DeleteRegistrationCommandHandler : IRequestHandler<DeleteRegistrationCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteRegistrationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Unit> Handle(DeleteRegistrationCommand request, CancellationToken cancellationToken)
        {
            var registration = await _unitOfWork.Registrations.GetByIdAsync(request.RegistrationId);
            if (registration == null)
           
                throw new NotFoundException("Registration not found");
            

            await _unitOfWork.Registrations.Delete(registration);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}