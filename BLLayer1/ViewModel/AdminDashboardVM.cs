using System;
using System.Collections.Generic;

namespace BLLayer1.ViewModel
{
    public class AdminDashboardVM
    {
        // Rooms Overview
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int ReservedRooms { get; set; }
        public int OccupiedRooms { get; set; }

        // Totals
        public int TotalBookings { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalReviews { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }

        // Today's Operations
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }

        // Recent Activity
        public List<RecentBookingItemVM> RecentBookings { get; set; } = new();
        public List<RecentPaymentItemVM> RecentPayments { get; set; } = new();
    }

    public class RecentBookingItemVM
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RecentPaymentItemVM
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }
}
