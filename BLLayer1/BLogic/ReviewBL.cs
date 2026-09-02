using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLLayer1.BLogic
{
    public class ReviewBL : IReviewBL
    {
        private readonly IBasicOperation<Review> _reviewRepo;
        private readonly IBasicOperation<Booking> _bookingRepo;
        private readonly IBasicOperation<Customer> _customerRepo;

        public ReviewBL(
            IBasicOperation<Review> reviewRepo,
            IBasicOperation<Booking> bookingRepo,
            IBasicOperation<Customer> customerRepo)
        {
            _reviewRepo = reviewRepo;
            _bookingRepo = bookingRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<ReviewVM>> GetAllAsync()
        {
            var reviews = await _reviewRepo.GetAllWithIncludesAsync(r => r.Customer!, r => r.Booking!);
            return reviews.Select(r => new ReviewVM
            {
                ReviewId = r.ReviewId,
                CustomerId = r.CustomerId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                CustomerName = r.Customer != null ? r.Customer.FullName : "Unknown",
                BookingDetails = r.BookingId.HasValue 
                    ? $"#{r.BookingId} - {r.Booking?.CheckInDate:yyyy-MM-dd} to {r.Booking?.CheckOutDate:yyyy-MM-dd}" 
                    : "No Booking"
            });
        }

        public async Task<ReviewVM?> GetByIdAsync(int id)
        {
            var r = await _reviewRepo.GetByIdWithIncludesAsync(x => x.ReviewId == id, r => r.Customer!, r => r.Booking!);
            if (r == null) return null;

            return new ReviewVM
            {
                ReviewId = r.ReviewId,
                CustomerId = r.CustomerId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                CustomerName = r.Customer != null ? r.Customer.FullName : "Unknown",
                BookingDetails = r.BookingId.HasValue 
                    ? $"#{r.BookingId} - {r.Booking?.CheckInDate:yyyy-MM-dd} to {r.Booking?.CheckOutDate:yyyy-MM-dd}" 
                    : "No Booking"
            };
        }

        public async Task<bool> CreateAsync(ReviewVM vm)
        {
            // Validate that booking belongs to customer (if booking is specified)
            if (vm.BookingId.HasValue)
            {
                if (!await BookingBelongsToCustomerAsync(vm.BookingId.Value, vm.CustomerId))
                {
                    return false;
                }
            }

            // Validate that customer exists and is active
            var customer = await _customerRepo.GetByIdAsync(vm.CustomerId);
            if (customer == null || !customer.IsActive)
            {
                return false;
            }

            var review = new Review
            {
                CustomerId = vm.CustomerId,
                BookingId = vm.BookingId,
                Rating = vm.Rating,
                Comment = vm.Comment,
                CreatedAt = DateTime.Now
            };

            await _reviewRepo.AddAsync(review);
            await _reviewRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(ReviewVM vm)
        {
            var review = await _reviewRepo.GetByIdAsync(vm.ReviewId);
            if (review == null) return false;

            // Validate that booking belongs to customer (if booking is specified)
            if (vm.BookingId.HasValue)
            {
                if (!await BookingBelongsToCustomerAsync(vm.BookingId.Value, vm.CustomerId))
                {
                    return false;
                }
            }

            // Validate that customer exists and is active
            var customer = await _customerRepo.GetByIdAsync(vm.CustomerId);
            if (customer == null || !customer.IsActive)
            {
                return false;
            }

            review.CustomerId = vm.CustomerId;
            review.BookingId = vm.BookingId;
            review.Rating = vm.Rating;
            review.Comment = vm.Comment;

            _reviewRepo.Update(review);
            await _reviewRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _reviewRepo.GetByIdAsync(id);
            if (review == null) return false;

            await _reviewRepo.DeleteAsync(id);
            await _reviewRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BookingBelongsToCustomerAsync(int bookingId, int customerId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return false;
            
            return booking.CustomerId == customerId;
        }

        public async Task<IEnumerable<SelectListItem>> GetBookingsByCustomerAsSelectListAsync(int customerId, int? excludeBookingId = null)
        {
            var bookings = await _bookingRepo.GetAllAsync();
            var query = bookings.Where(b => b.CustomerId == customerId && b.IsActive);
            
            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.BookingId != excludeBookingId.Value);
            }
            
            return query.Select(b => new SelectListItem
            {
                Value = b.BookingId.ToString(),
                Text = $"#{b.BookingId} - {b.CheckInDate:yyyy-MM-dd} to {b.CheckOutDate:yyyy-MM-dd}"
            })
            .OrderBy(s => s.Text);
        }
    }
}