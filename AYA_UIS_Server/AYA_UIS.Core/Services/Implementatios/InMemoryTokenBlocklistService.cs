using System.Collections.Concurrent;
using AYA_UIS.Application.Contracts;

namespace AYA_UIS.Core.Services.Implementations
{
    /// <summary>
    /// In-memory token blocklist for JWT logout/revocation.
    /// In production, replace with Redis or a persistent store.
    /// Automatically cleans up expired tokens.
    /// </summary>
    public class InMemoryTokenBlocklistService : ITokenBlocklistService
    {
        // token -> expiry time
        private static readonly ConcurrentDictionary<string, DateTime> _blockedTokens = new();
        private static DateTime _lastCleanup = DateTime.UtcNow;
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(30);

        public Task BlockTokenAsync(string token, DateTime expiry)
        {
            _blockedTokens.TryAdd(token, expiry);
            CleanupExpiredTokens();
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlockedAsync(string token)
        {
            if (_blockedTokens.TryGetValue(token, out var expiry))
            {
                if (expiry > DateTime.UtcNow)
                    return Task.FromResult(true);
                
                // Token has expired naturally, remove from blocklist
                _blockedTokens.TryRemove(token, out _);
            }
            return Task.FromResult(false);
        }

        private void CleanupExpiredTokens()
        {
            if (DateTime.UtcNow - _lastCleanup < CleanupInterval)
                return;

            _lastCleanup = DateTime.UtcNow;
            var expiredTokens = _blockedTokens.Where(kvp => kvp.Value <= DateTime.UtcNow).ToList();
            foreach (var kvp in expiredTokens)
            {
                _blockedTokens.TryRemove(kvp.Key, out _);
            }
        }
    }
}
