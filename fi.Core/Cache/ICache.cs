using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace fi.Core
{
    public interface ICache
    {
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        bool Add<T>(string key, T value, TimeSpan? expireTime = null, bool @override = true);
        Task<bool> AddAsync<T>(string key, T value, TimeSpan? expireTime = null, bool @override = true);
        bool Delete(string key);
        Task<bool> DeleteAsync(string key);
        IEnumerable<T> Get<T>(string key, long start = 0, long stop = -1);
        Task<IEnumerable<T>> GetAsync<T>(string key, long start = 0, long stop = -1);
        T GetItem<T>(string key, long index);
        Task<T> GetItemAsync<T>(string key, long index);
        void Add<T>(string key, T value);
        Task AddAsync<T>(string key, T value);
        void AddRange<T>(string key, T values);
        Task AddRangeAsync<T>(string key, IEnumerable<T> values);
        void DeleteItem<T>(string key, T value);
        Task DeleteItemAsync<T>(string key, T value);
        void IncrementString(string key, int value = 1);
        Task IncrementStringAsync(string key, int value = 1);
        void DecrementString(string key, int value = 1);
        Task DecrementStringAsync(string key, int value = 1);
        bool LockTake(string key, string value, TimeSpan expiryTime);
        Task<bool> LockTakeAsync(string key, string value, TimeSpan expiryTime);
        bool LockRelease(string key, string value);
        Task<bool> LockReleaseAsync(string key, string value);
    }
}
