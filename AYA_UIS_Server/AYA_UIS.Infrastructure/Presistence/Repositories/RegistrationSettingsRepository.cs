using AYA_UIS.Core.Domain.Contracts;
using AYA_UIS.Core.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Presistence;

namespace Presistence.Repositories
{
    public class RegistrationSettingsRepository : IRegistrationSettingsRepository
    {
        private readonly UniversityDbContext _ctx;

        public RegistrationSettingsRepository(UniversityDbContext ctx)
            => _ctx = ctx;

        public async Task<RegistrationSettings?> GetCurrentAsync()
            => await _ctx.RegistrationSettings
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

        public async Task AddAsync(RegistrationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            await _ctx.RegistrationSettings.AddAsync(settings);
        }

        public Task UpdateAsync(RegistrationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _ctx.RegistrationSettings.Update(settings);
            return Task.CompletedTask;
        }
    }
}
