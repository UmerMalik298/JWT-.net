namespace JWT.Entities
{
    public class UserProfile
    {

        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public byte[] HashPassword { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpired { get; set; }
    }
}
