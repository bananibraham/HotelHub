using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BLLayer1.BLogic
{
    public class ReviewBL : IReviewBL
    {
        private readonly IBasicOperation<Review> _reviewRepo;

        public ReviewBL(IBasicOperation<Review> reviewRepo)
        {
            _reviewRepo = reviewRepo;
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
                CustomerName = r.Customer != null ? r.Customer.FullName : "Guest",
                BookingDetails = r.BookingId.HasValue ? $"Booking #{r.BookingId}" : "General Guest Review"
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
                CustomerName = r.Customer != null ? r.Customer.FullName : "Guest",
                BookingDetails = r.BookingId.HasValue ? $"Booking #{r.BookingId}" : "General Guest Review"
            };
        }

        public async Task CreateAsync(ReviewVM vm)
        {
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
        }

        public async Task UpdateAsync(ReviewVM vm)
        {
            var review = await _reviewRepo.GetByIdAsync(vm.ReviewId);
            if (review != null)
            {
                review.CustomerId = vm.CustomerId;
                review.BookingId = vm.BookingId;
                review.Rating = vm.Rating;
                review.Comment = vm.Comment;

                _reviewRepo.Update(review);
                await _reviewRepo.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _reviewRepo.DeleteAsync(id);
            await _reviewRepo.SaveChangesAsync();
        }
    }
}