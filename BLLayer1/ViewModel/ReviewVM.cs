using System.ComponentModel.DataAnnotations;

namespace BLLayer1.ViewModel
{
    public class ReviewVM
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Please choose a Customer")]
        [Display(Name = "Customer Name")]
        public int CustomerId { get; set; }

        [Display(Name = "Booking Number (optional)")]
        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Review is required")]
        [Range(1, 5, ErrorMessage = "Review must be between 1 and 5")]
        [Display(Name = "Review Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "comment required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "comment must be between 5 and 500 characters")]
        [Display(Name = "Comment")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display Properties
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Booking Number")]
        public string? BookingDetails { get; set; }
    }
}