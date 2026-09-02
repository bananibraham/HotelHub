using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; } // References Room entity (mocked for now, ready for Tasneem's Room entity)
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Customer? Customer { get; set; }
        public Room? Room { get; set; }
    }
}
