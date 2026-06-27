
namespace ECommerce1.Models
{
    public class User
    {
        public Guid Id { get; set; }

        // Identity
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Authorization
        public string Role { get; set; } // "Admin", "User", ...

        // Account state
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;

        // Security
        public int FailedLoginCount { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public int RewardPoints { get; set; } = 0;

        // Navigation
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
        public virtual ICollection<ECommerce.Models.Cart>? Carts { get; set; }
        public virtual ICollection<ECommerce.Models.Review>? Reviews { get; set; }
        public virtual ICollection<ECommerce.Models.Order>? Orders { get; set; }
        public virtual ICollection<ECommerce.Models.ShippingInfo>? ShippingInfos { get; set; }
        public virtual ICollection<ECommerce.Models.PromotionUsage>? PromotionUsages { get; set; }
    }
}
