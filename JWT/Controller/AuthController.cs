using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JWT.Data;
using JWT.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;

namespace JWT.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthController(IConfiguration configuration, ApplicationDbContext _context, IDistributedCache _cache) : ControllerBase
    {
        public static User user = new();
       
        [HttpPost("register")]
        public  async Task<ActionResult<User>> Register(UserDTO request)
        {
            var usercheck = _context.User.Where(x => x.UserName == request.UserName).FirstOrDefault();
            if(usercheck != null)
            {
                return null;
            }

            var user = new User();
            var haspassword = new PasswordHasher<User>().HashPassword(user, request.HashPassword);
            user.UserName = request.UserName;
            user.HashPassword = haspassword;
            user.Role = request.Role;
            user.RefreshToken = request.RefreshToken;
           await  _context.AddAsync(user);
           await  _context.SaveChangesAsync();

            var userKey = $"user:{user.Id}";




            var cacheList = JsonSerializer.Serialize(new
            {
                user.Id,
                user.UserName,
                user.Role
            });
            await _cache.SetStringAsync(userKey, cacheList, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });


            return Ok(userKey);
            
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUser()
        {
            const string cachekey = "userListCache";

            var cacheData = await _cache.GetAsync(cachekey);

            if (cacheData != null)
            {

                return Ok(JsonSerializer.Deserialize<Object>(cacheData));

            }

            var userlist = _context.User.Select(u => new
            {
                Id = u.Id,
                Name = u.UserName,
                Role = u.Role,

            }).ToList();

            var serilizedData = JsonSerializer.Serialize(userlist);
            await _cache.SetStringAsync(cachekey, serilizedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)

            });
            return Ok(userlist);

        }

        [HttpPost("clear-cache")]
        public async Task<IActionResult> Remove(string key)
        {
            var Key = await _cache.GetAsync(key);
            if(Key != null)
            {
                await _cache.RemoveAsync(key);
                return Ok("Cache Cleared Successfully");
            }
            return BadRequest("Key Not Found");
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO request)
        {
            var user = await _context.User
                .Where(u => u.UserName == request.UserName)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            var result = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.HashPassword, request.HashPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest("Incorrect Password");
            }

            return Ok(CreateToken(user));
        }


        [Authorize]
        [HttpGet]
        public ActionResult AuthenticatedEndPoint()
        {
            return Ok("The user is Authenticated");
        }



        [Authorize(Roles="Admin")]
        [HttpGet("admin-only")]
        public ActionResult AuthenticatedEndPointAdminOnly()
        {
            return Ok("The user is Authenticated with With Admin Role");
        }





        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: cred
                );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }



        //private string GenerateRefreshToken()
        //{
        //    var randomNumber = new byte[32];
        //    using var rng = RandomNumberGenerator.Create();
        //    rng.GetBytes(randomNumber);

        //}

        //private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        //{
        //    var refreshToken = GenerateRefreshToken();
        //    user.RefreshToken = refreshToken;
        //    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        //    await _context.SaveChangesAsync();
        //    return refreshToken;

        //}

    }
    


    }

