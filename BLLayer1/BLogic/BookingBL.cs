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
    public class BookingBL : IBookingBL
    {
        private readonly IBasicOperation<Booking> _bookingRepo;
        private readonly IBasicOperation<Customer> _customerRepo;
        private readonly IBasicOperation<Room> _roomRepo;
        private readonly IBasicOperation<Payment> _paymentRepo;

        public BookingBL(
            IBasicOperation<Booking> bookingRepo,
            IBasicOperation<Customer> customerRepo,
            IBasicOperation<Room> roomRepo,
            IBasicOperation<Payment> paymentRepo)
        {
            _bookingRepo = bookingRepo;
            _customerRepo = customerRepo;
            _roomRepo = roomRepo;
            _paymentRepo = paymentRepo;
        }

        public async Task<IEnumerable<BookingVM>> GetAllAsync()
        {
            var bookings = await _bookingRepo.GetAllWithIncludesAsync(b => b.Customer!, b => b.Room!);
            var rooms = (await _roomRepo.GetAllWithIncludesAsync(r => r.RoomType!)).ToDictionary(r => r.Id, r => r);
            var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.CustomerId, c => c);
            var payments = await _paymentRepo.GetAllAsync();
            var paymentsDict = payments.GroupBy(p => p.BookingId).ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            return bookings.Select(b => MapToVM(b, customers, rooms, paymentsDict)).OrderByDescending(b => b.CreatedAt);
        }

        public async Task<IEnumerable<BookingVM>> GetByCustomerIdAsync(int customerId)
        {
            var all = await GetAllAsync();
            return all.Where(b => b.CustomerId == customerId);
        }

        public async Task<BookingVM?> GetByIdAsync(int id)
        {
            var b = await _bookingRepo.GetByIdWithIncludesAsync(x => x.BookingId == id, x => x.Customer!, x => x.Room!);
            if (b == null) return null;

            var customer = await _customerRepo.GetByIdAsync(b.CustomerId);
            var room = await _roomRepo.GetByIdWithIncludesAsync(r => r.Id == b.RoomId, r => r.RoomType!);
            var payments = await _paymentRepo.GetAllAsync();
            var paidAmount = payments.Where(p => p.BookingId == b.BookingId).Sum(p => p.Amount);

            return new BookingVM
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
                CustomerName = customer?.FullName ?? "Guest",
                CustomerEmail = customer?.Email,
                CustomerPhone = customer?.Phone,
                RoomId = b.RoomId,
                RoomNumber = room != null ? room.RoomNumber.ToString() : b.RoomId.ToString(),
                RoomType = room?.RoomType?.Name ?? "Standard Room",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Status = b.Status,
                TotalPrice = b.TotalPrice,
                PaidAmount = paidAmount,
                CreatedAt = b.CreatedAt,
                IsActive = b.IsActive
            };
        }

        public async Task<bool> CreateAsync(BookingVM vm)
        {
            var id = await CreateAndReturnIdAsync(vm);
            return id > 0;
        }

        public async Task<int> CreateAndReturnIdAsync(BookingVM vm)
        {
            var allBookings = await _bookingRepo.GetAllAsync();
            bool hasOverlap = allBookings.Any(b => b.IsActive 
                                                   && b.RoomId == vm.RoomId 
                                                   && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                                                   && b.CheckInDate < vm.CheckOutDate 
                                                   && b.CheckOutDate > vm.CheckInDate);
            if (hasOverlap)
            {
                return 0;
            }

            var room = await _roomRepo.GetByIdWithIncludesAsync(r => r.Id == vm.RoomId, r => r.RoomType!);
            int nights = Math.Max(1, (vm.CheckOutDate.Date - vm.CheckInDate.Date).Days);
            decimal rate = room?.RoomType?.PricePerNight ?? 1500m;
            decimal calculatedPrice = nights * rate;

            var booking = new Booking
            {
                CustomerId = vm.CustomerId,
                RoomId = vm.RoomId,
                CheckInDate = vm.CheckInDate,
                CheckOutDate = vm.CheckOutDate,
                Status = string.IsNullOrWhiteSpace(vm.Status) ? "Confirmed" : vm.Status,
                TotalPrice = vm.TotalPrice > 0 ? vm.TotalPrice : calculatedPrice,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();
            vm.BookingId = booking.BookingId;
            return booking.BookingId;
        }

        public async Task<bool> UpdateAsync(BookingVM vm)
        {
            var booking = await _bookingRepo.GetByIdAsync(vm.BookingId);
            if (booking == null) return false;

            var allBookings = await _bookingRepo.GetAllAsync();
            bool hasOverlap = allBookings.Any(b => b.IsActive 
                                                   && b.RoomId == vm.RoomId 
                                                   && b.BookingId != vm.BookingId
                                                   && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                                                   && b.CheckInDate < vm.CheckOutDate 
                                                   && b.CheckOutDate > vm.CheckInDate);
            if (hasOverlap)
            {
                return false;
            }

            var room = await _roomRepo.GetByIdWithIncludesAsync(r => r.Id == vm.RoomId, r => r.RoomType!);
            int nights = Math.Max(1, (vm.CheckOutDate.Date - vm.CheckInDate.Date).Days);
            decimal rate = room?.RoomType?.PricePerNight ?? 1500m;

            booking.CustomerId = vm.CustomerId;
            booking.RoomId = vm.RoomId;
            booking.CheckInDate = vm.CheckInDate;
            booking.CheckOutDate = vm.CheckOutDate;
            booking.Status = vm.Status;
            booking.TotalPrice = vm.TotalPrice > 0 ? vm.TotalPrice : nights * rate;

            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            booking.IsActive = false;
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            if (booking.Status == "CheckedIn" || booking.Status == "CheckedOut")
            {
                return false; // Cannot cancel an already commenced stay
            }

            booking.Status = "Cancelled";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Release room status if needed
            var room = await _roomRepo.GetByIdAsync(booking.RoomId);
            if (room != null && room.Status == "Occupied")
            {
                room.Status = "Available";
                _roomRepo.Update(room);
                await _roomRepo.SaveChangesAsync();
            }

            return true;
        }

        public async Task<(bool Success, string Message)> ConfirmAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null || !booking.IsActive)
            {
                return (false, "Booking not found or inactive.");
            }

            if (booking.Status == "Cancelled")
            {
                return (false, "Cannot confirm a cancelled booking.");
            }

            if (booking.Status == "Confirmed")
            {
                return (false, "This booking is already confirmed.");
            }

            if (booking.Status == "CheckedIn" || booking.Status == "CheckedOut")
            {
                return (false, "This booking has already commenced or concluded.");
            }

            booking.Status = "Confirmed";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            return (true, $"Booking #{id} successfully confirmed.");
        }

        public async Task<(bool Success, string Message)> CheckInAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null || !booking.IsActive)
            {
                return (false, "Booking not found or inactive.");
            }

            if (booking.Status == "Cancelled")
            {
                return (false, "Cannot check in a cancelled booking.");
            }

            if (booking.Status == "CheckedIn")
            {
                return (false, "This booking is already checked in.");
            }

            if (booking.Status == "CheckedOut")
            {
                return (false, "This booking has already been checked out.");
            }

            if (booking.CheckInDate.Date > DateTime.Today)
            {
                return (false, $"Check-in date is {booking.CheckInDate:yyyy-MM-dd}. Guest cannot be checked in before the scheduled arrival date.");
            }

            booking.Status = "CheckedIn";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Mark room occupied
            var room = await _roomRepo.GetByIdAsync(booking.RoomId);
            if (room != null)
            {
                room.Status = "Occupied";
                _roomRepo.Update(room);
                await _roomRepo.SaveChangesAsync();
            }

            return (true, $"Booking #{id} successfully checked in.");
        }

        public async Task<(bool Success, string Message)> CheckOutAsync(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null || !booking.IsActive)
            {
                return (false, "Booking not found or inactive.");
            }

            if (booking.Status == "Cancelled")
            {
                return (false, "Cannot check out a cancelled booking.");
            }

            if (booking.Status == "CheckedOut")
            {
                return (false, "This booking is already checked out.");
            }

            if (booking.Status != "CheckedIn")
            {
                return (false, "Guest must be checked in before performing check-out.");
            }

            booking.Status = "CheckedOut";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Mark room available
            var room = await _roomRepo.GetByIdAsync(booking.RoomId);
            if (room != null)
            {
                room.Status = "Available";
                _roomRepo.Update(room);
                await _roomRepo.SaveChangesAsync();
            }

            return (true, $"Booking #{id} successfully checked out.");
        }

        private static BookingVM MapToVM(Booking b, Dictionary<int, Customer> customers, Dictionary<int, Room> rooms, Dictionary<int, decimal>? paymentsDict = null)
        {
            customers.TryGetValue(b.CustomerId, out var customer);
            rooms.TryGetValue(b.RoomId, out var room);
            decimal paidAmount = 0;
            paymentsDict?.TryGetValue(b.BookingId, out paidAmount);

            return new BookingVM
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
                CustomerName = customer?.FullName ?? "Guest",
                CustomerEmail = customer?.Email,
                CustomerPhone = customer?.Phone,
                RoomId = b.RoomId,
                RoomNumber = room != null ? room.RoomNumber.ToString() : b.RoomId.ToString(),
                RoomType = room?.RoomType?.Name ?? "Standard Room",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Status = b.Status,
                TotalPrice = b.TotalPrice,
                PaidAmount = paidAmount,
                CreatedAt = b.CreatedAt,
                IsActive = b.IsActive
            };
        }
    }
}
