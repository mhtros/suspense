using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Suspense.Server.Entities;
using Suspense.Server.Exceptions;

namespace Suspense.Server.Extensions;

/// <summary>
/// Provides extension methods for working with a distributed cache in ASP.NET Core applications.
/// </summary>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// Stores an <see cref="CacheEntity"/> in the distributed cache with the specified key and cache options.
    /// </summary>
    /// <typeparam name="T">The type of the model to store in the cache, which must inherit from <see cref="CacheEntity"/>.</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> instance used for cache operations.</param>
    /// <param name="key">The key used to store and retrieve the cached model.</param>
    /// <param name="model">The model to be stored in the cache.</param>
    /// <param name="expirationTime">The absolute expiration time for the cached model. If null, the model will use the default 60 seconds expiration time.</param>
    /// <param name="slidingExpirationTime">The sliding expiration time for the cached model. If null, the model will not expire based on sliding time.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SetModelAsync<T>(this IDistributedCache cache, string key, T model,
        TimeSpan? expirationTime = null, TimeSpan? slidingExpirationTime = null) where T : CacheEntity
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromSeconds(60),
            SlidingExpiration = slidingExpirationTime
        };

        var jsonModel = JsonSerializer.Serialize(model);
        await cache.SetStringAsync(key, jsonModel, options);
    }

    /// <summary>
    /// Asynchronously retrieves an <see cref="CacheEntity"/> from the distributed cache by its unique key.
    /// </summary>
    /// <typeparam name="T">The type of entity to retrieve, which must inherit from <see cref="CacheEntity"/>.</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> instance used for caching.</param>
    /// <param name="key">The unique cache key associated with the entity.</param>
    /// <returns>
    /// A task that represents the asynchronous operation and returns the retrieved entity of type T.
    /// </returns>
    /// <exception cref="EntityException">Thrown when the entity was not found.</exception>
    public static async Task<T> GetModelAsync<T>(this IDistributedCache cache, string key) where T : CacheEntity
    {
        var jsonModel = await cache.GetStringAsync(key);
        var model = string.IsNullOrEmpty(jsonModel) ? default : JsonSerializer.Deserialize<T>(jsonModel);

        // To prevent repositories to load different classes entities.
        if (model is null || model.EntityType != typeof(T).Name)
            throw new EntityException($"Entity with ID: {key} was not found.");

        return model;
    }
}