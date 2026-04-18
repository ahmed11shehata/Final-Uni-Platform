using AYA_UIS.Core.Domain.Entities.Models;

namespace AYA_UIS.Core.Domain.Contracts
{
    public interface IRegistrationSettingsRepository
    {
        Task<RegistrationSettings?> GetCurrentAsync();
        Task AddAsync(RegistrationSettings settings);
        Task UpdateAsync(RegistrationSettings settings);
    }
}
