using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelHub.Models.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;

        // Navigation properties
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; }
    }
}