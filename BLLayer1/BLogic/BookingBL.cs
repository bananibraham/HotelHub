using BLLayer1.Interfaces;
using BLLayer1.MockData;
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

        public BookingBL(
            IBasicOperation<Booking> bookingRepo,
            IBasicOperation<Customer> customerRepo)
        {
            _bookingRepo = bookingRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<BookingVM>> GetAllAsync()
        {
            var bookings = await _bookingRepo.GetAllAsync();
            var customers = (await _customerRepo.GetAllAsync()).ToDictionary(c => c.CustomerId, c => c);

            return bookings.Select(b =>
            {
                customers.TryGetValue(b.CustomerId, out var customer);
                var room = MockRoomData.GetMockRoomById(b.RoomId);

                return new BookingVM
                {
                    BookingId = b.BookingId,
                    CustomerId = b.CustomerId,
                    CustomerName = customer?.FullName ?? "Unknown Guest",
                    CustomerEmail = customer?.Email,
                    CustomerPhone = customer?.Phone,
                    RoomId = b.RoomId,
                    RoomNumber = room?.RoomNumber ?? b.RoomId.ToString(),
                    RoomType = room?.RoomType ?? "Standard",
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt,
                    IsActive = b.IsActive
                };
            }).OrderByDescending(b => b.CreatedAt);
        }

        public async Task<BookingVM?> GetByIdAsync(int id)
        {
            var b = await _bookingRepo.GetByIdAsync(id);
            if (b == null) return null;

            var customer = await _customerRepo.GetByIdAsync(b.CustomerId);
            var room = MockRoomData.GetMockRoomById(b.RoomId);

            return new BookingVM
            {
                BookingId = b.BookingId,
                CustomerId = b.CustomerId,
                CustomerName = customer?.FullName ?? "Unknown Guest",
                CustomerEmail = customer?.Email,
                CustomerPhone = customer?.Phone,
                RoomId = b.RoomId,
                RoomNumber = room?.RoomNumber ?? b.RoomId.ToString(),
                RoomType = room?.RoomType ?? "Standard",
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Status = b.Status,
                TotalPrice = b.TotalPrice,
                CreatedAt = b.CreatedAt,
                IsActive = b.IsActive
            };
        }

        public async Task<bool> CreateAsync(BookingVM vm)
        {
            var room = MockRoomData.GetMockRoomById(vm.RoomId);
            int nights = Math.Max(1, (vm.CheckOutDate.Date - vm.CheckInDate.Date).Days);
            decimal rate = room?.PricePerNight ?? 100m;
            decimal calculatedPrice = nights * rate;

            var booking = new Booking
            {
                CustomerId = vm.CustomerId,
                RoomId = vm.RoomId,
                CheckInDate = vm.CheckInDate,
                CheckOutDate = vm.CheckOutDate,
                Status = string.IsNullOrWhiteSpace(vm.Status) ? "Pending" : vm.Status,
                TotalPrice = vm.TotalPrice > 0 ? vm.TotalPrice : calculatedPrice,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();
            vm.BookingId = booking.BookingId;
            return true;
        }

        public async Task<bool> UpdateAsync(BookingVM vm)
        {
            var booking = await _bookingRepo.GetByIdAsync(vm.BookingId);
            if (booking == null) return false;

            var room = MockRoomData.GetMockRoomById(vm.RoomId);
            int nights = Math.Max(1, (vm.CheckOutDate.Date - vm.CheckInDate.Date).Days);
            decimal rate = room?.PricePerNight ?? 100m;

            booking.CustomerId = vm.CustomerId;
            booking.RoomId = vm.RoomId;
            booking.CheckInDate = vm.CheckInDate;
            booking.CheckOutDate = vm.CheckOutDate;
            booking.Status = vm.Status;
            booking.TotalPrice = nights * rate;

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

            booking.Status = "Cancelled";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();
            return true;
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

            booking.Status = "CheckedIn";
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

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

            return (true, $"Booking #{id} successfully checked out.");
        }
    }
}
