using System.Security.Cryptography;
using JWT.Data;
using JWT.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserProfileController> _logger;
        public UserProfileController(ApplicationDbContext dbContext, ILogger<UserProfileController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserResgistrationRequest request)
        {
            _logger.LogInformation("User Profile Creation");
            if (_dbContext.UserProfile.Any(x => x.Email == request.Email))
            {
                return BadRequest("User ALready Exist");
            }
            CreatePasswordHash(request.Password, 
                out byte [] passwordHash, 
                out byte[] passwordSalt);

            var user = new UserProfile
            {
                Email = request.Email,
                HashPassword = passwordHash,
                PasswordSalt = passwordSalt,
                PasswordResetToken = CreateRandomToken()

            };

            _dbContext.UserProfile.Add(user);
            await _dbContext.SaveChangesAsync();
            return Ok("User Registered Successfully");


        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {

                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));  
            }
        }
        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var user = _dbContext.UserProfile.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user == null)
            {
                return BadRequest("User Not Found");

            }
            if(user.VerifiedAt == null)
            {
                return BadRequest("Not Verified");
            }
            if (!VerifyPasswordHash(request.Password, user.HashPassword, user.PasswordSalt))
            {
                return BadRequest("Password is Incorrect");
            }
            return Ok($"Welcom back, {user.Email}");


        }


        [HttpPost("Verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = _dbContext.UserProfile.Where(x => x.VerificationToken == token).FirstOrDefault();
            if (user == null)
            {
                return BadRequest("Invalid Token");

            }
            user.VerifiedAt = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok("User Verified");


        }




        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = _dbContext.UserProfile.Where(x => x.Email == email).FirstOrDefault();
            if (user == null)
            {
                return BadRequest("User Not Found");

            }
            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpired = DateTime.Now.AddDays(1);
            _dbContext.SaveChanges();
            return Ok("You may now reset your password");


        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {

                var ComputedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return ComputedHash.SequenceEqual(passwordHash);
            }
        }
        
        //[HttpPost("Verify")]
        //public async Task<IActionResult> Verify(UserLogin request)
        //{
        //    var user = _dbContext.UserProfile.Where(x => x.Email == request.Email).FirstOrDefault();
        //    if (user == null)
        //    {
        //        return BadRequest("User Not Found");

        //    }
        //    if (user.VerifiedAt == null)
        //    {
        //        return BadRequest("Not Verified");
        //    }
        //    if (!VerifyPasswordHash(request.Password, user.HashPassword, user.PasswordSalt))
        //    {
        //        return BadRequest("Password is Incorrect");
        //    }
        //    return Ok($"Welcom back, {user.Email}");


        //}
    }
}
