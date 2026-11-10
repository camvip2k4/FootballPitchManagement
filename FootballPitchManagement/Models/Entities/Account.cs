using System.ComponentModel.DataAnnotations;
using FootballPitchManagement.Models.Enums;

namespace FootballPitchManagement.Models.Entities
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public RoleType Role { get; set; } = RoleType.CUSTOMER;

        public AccountStatus Status { get; set; } = AccountStatus.ACTIVE;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

