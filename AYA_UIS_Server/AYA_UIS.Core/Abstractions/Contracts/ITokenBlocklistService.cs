namespace AYA_UIS.Application.Contracts
{
    /// <summary>
    /// Service to manage JWT token blocklist for logout/revocation.
    /// </summary>
    public interface ITokenBlocklistService
    {
        /// <summary>
        /// Adds a token to the blocklist so it can no longer be used.
        /// </summary>
        Task BlockTokenAsync(string token, DateTime expiry);

        /// <summary>
        /// Checks if a token has been blocked (logged out / revoked).
        /// </summary>
        Task<bool> IsTokenBlockedAsync(string token);
    }
}
