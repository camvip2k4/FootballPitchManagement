using System.ComponentModel.DataAnnotations;

namespace FootballPitchManagement.Models.Entities
{
    public class ScheduleBlock
    {
        [Key]
        public int BlockId { get; set; }

        public int FieldId { get; set; }
        public FootballField? Field { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        public bool IsRecurring { get; set; } = false;
    }
}

