namespace JWT.Entities
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string HashPassword { get; set; }

        public string Role { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
