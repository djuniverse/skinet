using System.Text.Json;
using Core.Entities;
using Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Data;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;
    private const string keyNamespace = "ttotest";
    
    public BasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
        
    }

    public async Task<CustomerBasket> GetBasketAsync(string basketId)
    {
        var data = await _database.StringGetAsync(getKey(basketId));
        
        

        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CustomerBasket>(data);
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        
        var created = await _database.StringSetAsync(getKey(basket.Id), JsonSerializer.Serialize(basket), TimeSpan.FromDays(30));
        
        if (!created) return null;
        return await GetBasketAsync(getKey(basket.Id));
    }

    public async Task<bool> DeleteBasketAsync(string basketId)
    {
        return await _database.KeyDeleteAsync(getKey(basketId));
    }

    private String getKey(string key)
    {
        return keyNamespace + ":" + key;
    }
}