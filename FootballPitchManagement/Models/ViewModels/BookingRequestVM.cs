using System.ComponentModel.DataAnnotations;

namespace FootballPitchManagement.Models.ViewModels
{
    public class BookingRequestVM
    {
        [Required]
        public int PitchId { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string? CustomerEmail { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        public string? Note { get; set; }
    }
}

