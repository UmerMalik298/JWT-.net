using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace JWT.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        public CacheController(IDistributedCache distributedCache)
        {
            _cache = distributedCache;
            
        }
        [HttpPost("set")]
        public async Task<IActionResult> SetCache(string key, string value)
        {
            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
            return Ok("Cache Set Successfully");
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCache(string key)
        {
            var value = await _cache.GetStringAsync(key);
            if (value == null)
                return NotFound("Key not found in cache.");
            return Ok(value);
        }
    }
}
