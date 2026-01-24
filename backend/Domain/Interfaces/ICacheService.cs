using System.Threading.Tasks;

namespace PCM.Domain.Interfaces
{
    /// <summary>
    /// Redis cache service interface
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        
        // For sorted sets (leaderboard)
        Task AddToSortedSetAsync(string key, string member, double score);
        Task<List<(string Member, double Score)>> GetSortedSetRangeAsync(string key, int start, int stop);
    }
}
