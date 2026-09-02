using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLLayer1.ViewModel
{
    public class ReviewVM : IValidatableObject
    {
        public int ReviewId { get; set; }

        [Required(ErrorMessage = "Please select a Customer")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Booking (Optional)")]
        public int? BookingId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        [Display(Name = "Rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write your review")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 500 characters")]
        [Display(Name = "Your Review")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Display Properties (for Index/Details views)
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Booking Details")]
        public string? BookingDetails { get; set; }

        // For Dropdown Lists (populated by Controller)
        public IEnumerable<SelectListItem>? Customers { get; set; }
        public IEnumerable<SelectListItem>? Bookings { get; set; }

        // Custom Validation: Ensure data integrity
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Basic validation: if BookingId is provided, CustomerId must also be provided
            if (BookingId.HasValue && CustomerId <= 0)
            {
                results.Add(new ValidationResult(
                    "Customer must be selected when a Booking is specified.",
                    new[] { nameof(BookingId) }));
            }

            // Rating validation (though [Range] attribute handles this, adding as backup)
            if (Rating < 1 || Rating > 5)
            {
                results.Add(new ValidationResult(
                    "Rating must be between 1 and 5 stars.",
                    new[] { nameof(Rating) }));
            }

            return results;
        }
    }
}