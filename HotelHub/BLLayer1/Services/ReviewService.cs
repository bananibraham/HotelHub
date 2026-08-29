using HotelHub.Models.Entities;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace BLLayer1.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                .ToListAsync();
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        public async Task AddAsync(Review review)
        {
            review.ReviewDate = DateTime.Now;
            review.IsApproved = false;
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsBookingReviewedAsync(int bookingId)
        {
            return await _context.Reviews.AnyAsync(r => r.BookingId == bookingId);
        }
    }
}