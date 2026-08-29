using System.ComponentModel.DataAnnotations;

namespace BLLayer1.ViewModel
{
    public class ReviewVM
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "customer name required")]
        [Display(Name = "customer name")]
        public int CustomerId { get; set; }

        [Display(Name = "booking number(optional)")]
        public int? BookingId { get; set; }

        [Required(ErrorMessage = "review required")]
        [Range(1, 5, ErrorMessage = "review must be between 1 and 5")]
        [Display(Name = "review")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "review comment required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "comment must be between 5 and 500 characters")]
        [Display(Name = "comment")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display properties for Views
        public string? CustomerName { get; set; }
        public string? BookingNumber { get; set; }
    }
}