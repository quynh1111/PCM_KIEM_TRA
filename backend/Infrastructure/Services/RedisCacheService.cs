using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PCM.Domain.Interfaces;

namespace PCM.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (RedisConnectionException)
            {
                return default;
            }
            catch (RedisTimeoutException)
            {
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expiration);
            }
            catch (RedisConnectionException)
            {
            }
            catch (RedisTimeoutException)
            {
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (RedisConnectionException)
            {
            }
            catch (RedisTimeoutException)
            {
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (RedisConnectionException)
            {
                return false;
            }
            catch (RedisTimeoutException)
            {
                return false;
            }
        }

        public async Task AddToSortedSetAsync(string key, string member, double score)
        {
            try
            {
                await _database.SortedSetAddAsync(key, member, score);
            }
            catch (RedisConnectionException)
            {
            }
            catch (RedisTimeoutException)
            {
            }
        }

        public async Task<List<(string Member, double Score)>> GetSortedSetRangeAsync(string key, int start, int stop)
        {
            try
            {
                var entries = await _database.SortedSetRangeByRankWithScoresAsync(
                    key,
                    start,
                    stop,
                    Order.Descending);

                return entries
                    .Select(e => (Member: e.Element.ToString(), Score: e.Score))
                    .ToList();
            }
            catch (RedisConnectionException)
            {
                return new List<(string Member, double Score)>();
            }
            catch (RedisTimeoutException)
            {
                return new List<(string Member, double Score)>();
            }
        }
    }
}
