using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelHub.Models.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Booking ID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }

        [ForeignKey(nameof(BookingId))]
        public virtual Booking Booking { get; set; }
    }
}