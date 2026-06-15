using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourPlannerAPI.Models
{
    public class TourLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        [Required]
        public int Difficulty { get; set; } // e.g., 1-5 scale

        [Required]
        public double TotalDistance { get; set; }

        [Required]
        public TimeSpan TotalTime { get; set; }

        [Required]
        public int Rating { get; set; } // e.g., 1-5 stars

        [Required]
        public int TourId { get; set; }

        [ForeignKey("TourId")]
        [JsonIgnore]
        public Tour? Tour { get; set; }
    }
}
