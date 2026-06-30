using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourPlannerAPI.Models
{
    public class Tour
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string From { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string To { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TransportType { get; set; } = string.Empty;

        public double Distance { get; set; }

        public TimeSpan EstimatedTime { get; set; }

        public string? RouteInformation { get; set; } // Could store GeoJSON or similar

        public ICollection<TourLog> Logs { get; set; } = new List<TourLog>();

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
