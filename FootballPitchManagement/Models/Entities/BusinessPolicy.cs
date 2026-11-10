using System.ComponentModel.DataAnnotations;

namespace FootballPitchManagement.Models.Entities
{
    public class BusinessPolicy
    {
        [Key]
        public int PolicyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ParamKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ParamValue { get; set; } = string.Empty;

        [MaxLength(20)]
        public string DataType { get; set; } = "Integer";

        [MaxLength(20)]
        public string Unit { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

