using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLLayer1.BLogic
{
    public class DashboardBL : IDashboardBL
    {
        private readonly IBasicOperation<Room> _roomRepo;
        private readonly IBasicOperation<Booking> _bookingRepo;
        private readonly IBasicOperation<Customer> _customerRepo;
        private readonly IBasicOperation<Payment> _paymentRepo;
        private readonly IBasicOperation<Review> _reviewRepo;

        public DashboardBL(
            IBasicOperation<Room> roomRepo,
            IBasicOperation<Booking> bookingRepo,
            IBasicOperation<Customer> customerRepo,
            IBasicOperation<Payment> paymentRepo,
            IBasicOperation<Review> reviewRepo)
        {
            _roomRepo = roomRepo;
            _bookingRepo = bookingRepo;
            _customerRepo = customerRepo;
            _paymentRepo = paymentRepo;
            _reviewRepo = reviewRepo;
        }

        public async Task<AdminDashboardVM> GetDashboardDataAsync()
        {
            var rooms = (await _roomRepo.GetAllWithIncludesAsync(r => r.RoomType!)).ToList();
            var bookings = (await _bookingRepo.GetAllWithIncludesAsync(b => b.Customer!, b => b.Room!)).ToList();
            var customers = (await _customerRepo.GetAllAsync()).ToList();
            var payments = (await _paymentRepo.GetAllAsync()).ToList();
            var reviews = (await _reviewRepo.GetAllAsync()).ToList();

            var today = DateTime.Today;

            var occupiedCount = rooms.Count(r => string.Equals(r.Status, "Occupied", StringComparison.OrdinalIgnoreCase));
            var reservedCount = bookings.Count(b => string.Equals(b.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) 
                                                   && b.CheckInDate.Date <= today && b.CheckOutDate.Date >= today);
            var availableCount = rooms.Count(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase));

            // Customer dictionary for fast lookups
            var customerDict = customers.ToDictionary(c => c.CustomerId, c => c.FullName);
            var bookingDict = bookings.ToDictionary(b => b.BookingId, b => b);

            var recentBookings = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBookingItemVM
                {
                    BookingId = b.BookingId,
                    CustomerName = b.Customer?.FullName ?? (customerDict.TryGetValue(b.CustomerId, out var name) ? name : "Guest"),
                    RoomNumber = b.Room != null ? b.Room.RoomNumber.ToString() : b.RoomId.ToString(),
                    RoomType = b.Room?.RoomType?.Name ?? "Standard",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .ToList();

            var recentPayments = payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p =>
                {
                    bookingDict.TryGetValue(p.BookingId, out var b);
                    var custName = b?.Customer?.FullName ?? (b != null && customerDict.TryGetValue(b.CustomerId, out var cName) ? cName : "Guest");
                    return new RecentPaymentItemVM
                    {
                        PaymentId = p.PaymentId,
                        BookingId = p.BookingId,
                        CustomerName = custName,
                        Amount = p.Amount,
                        PaymentMethod = p.PaymentMethod,
                        PaymentDate = p.PaymentDate
                    };
                })
                .ToList();

            return new AdminDashboardVM
            {
                TotalRooms = rooms.Count,
                AvailableRooms = availableCount,
                ReservedRooms = reservedCount,
                OccupiedRooms = occupiedCount,

                TotalBookings = bookings.Count,
                TotalCustomers = customers.Count,
                TotalReviews = reviews.Count,
                TotalPayments = payments.Count,
                TotalRevenue = payments.Sum(p => p.Amount),

                TodayCheckIns = bookings.Count(b => b.CheckInDate.Date == today && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)),
                TodayCheckOuts = bookings.Count(b => b.CheckOutDate.Date == today && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)),

                RecentBookings = recentBookings,
                RecentPayments = recentPayments
            };
        }
    }
}
