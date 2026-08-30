using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLLayer1.ViewModel
{
    public class PaymentCreateVm
    {
        [Required(ErrorMessage = "Booking is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid booking.")]
        public int BookingId { get; set; }


        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue,
            ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }


        [Required(ErrorMessage = "Payment method is required.")]
        [StringLength(50,
            ErrorMessage = "Payment method cannot exceed 50 characters.")]
        public string PaymentMethod { get; set; } = string.Empty;


        [Required(ErrorMessage = "Payment date is required.")]
        public DateTime PaymentDate { get; set; }


        [StringLength(100,
            ErrorMessage = "Transaction ID cannot exceed 100 characters.")]
        public string? TransactionId { get; set; }


        [StringLength(500,
            ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }
    }

}
