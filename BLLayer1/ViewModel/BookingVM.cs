using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BLLayer1.ViewModel
{
    public class BookingVM : IValidatableObject
    {
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid customer.")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        [Required(ErrorMessage = "Room is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid room.")]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Display(Name = "Room Type")]
        public string? RoomType { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-in Date")]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Check-out date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Check-out Date")]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Booking Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Total Price ($)")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Paid Amount (EGP)")]
        [DataType(DataType.Currency)]
        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount => Math.Max(0, TotalPrice - PaidAmount);

        public bool IsPaid => TotalPrice > 0 && PaidAmount >= TotalPrice;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOutDate.Date < CheckInDate.Date)
            {
                yield return new ValidationResult(
                    "Check-out date must be equal to or later than Check-in date.",
                    new[] { nameof(CheckOutDate) });
            }
        }
    }
}
