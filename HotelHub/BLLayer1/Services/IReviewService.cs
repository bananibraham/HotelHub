using HotelHub.Models.Entities;

namespace BLLayer1.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review> GetByIdAsync(int id);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task<bool> IsBookingReviewedAsync(int bookingId);
    }
}