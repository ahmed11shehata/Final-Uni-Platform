using System;
using AYA_UIS.Application.Contracts;
using AYA_UIS.Core.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AYA_UIS.Core.Services.Implementations; // Fix: Change from Services.Implementatios
using Shared.Common;
using Domain.Contracts;
using AYA_UIS.Core.Abstractions.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Services.Implementations
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authService;
        private readonly Lazy<IRoleService> _roleService;
        private readonly Lazy<IUserService> _userService;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceManager(
            UserManager<User> userManager,
            IOptions<JwtOptions> options,
            RoleManager<IdentityRole> roleManager,
            IUserService userService, // Fix: Use the interface instead of concrete class
            IUnitOfWork unitOfWork 
            
            ) // Fix: Add IUnitOfWork to the constructor
        {
            _authService = new Lazy<IAuthenticationService>(
                () => new AuthenticationService(userManager, options, roleManager, unitOfWork));
            _roleService = new Lazy<IRoleService>(
                () => new RoleService(roleManager, userManager));
            _userService = new Lazy<IUserService>(
                () => userService);

            
        }

        public IAuthenticationService AuthenticationService => _authService.Value;
        public IRoleService RoleService => _roleService.Value;
        public IUserService UserService => _userService.Value;

    }
}