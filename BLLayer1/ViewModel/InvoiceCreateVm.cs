using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BLLayer1.ViewModel
{
    public class InvoiceCreateVm
    {
        [Required(ErrorMessage = "Booking is required.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid booking.")]
        public int BookingId { get; set; }


        [Required(ErrorMessage = "Invoice number is required.")]
        [StringLength(50,
            ErrorMessage = "Invoice number cannot exceed 50 characters.")]
        public string InvoiceNumber { get; set; } = string.Empty;


        [Required(ErrorMessage = "Issue date is required.")]
        public DateTime IssueDate { get; set; }
    }
}
