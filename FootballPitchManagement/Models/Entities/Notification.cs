using System.ComponentModel.DataAnnotations;

namespace FootballPitchManagement.Models.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "SYSTEM";

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

