using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace JWT.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class InMemoryCacheController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        public InMemoryCacheController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;

        }
        [HttpPost("in-memory-set")]
        public IActionResult SetInMemory()
        {
            _memoryCache.Set("InMemoryTestKey", "This is In-Memory Cache Created", new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });

            return Ok("In Memory Data Cache Successfully");
        }
        [HttpGet("in-memory-get")]
        public IActionResult GetInMemory(string key)
        {
            var data = _memoryCache.Get(key);
            if (data != null)
            {
                return Ok(data);
            }
            return BadRequest("Key is Invalid");
        }
    }
}
