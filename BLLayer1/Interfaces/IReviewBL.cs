using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLLayer1.Interfaces
{
    public interface IReviewBL
    {
        // CRUD Operations
        Task<IEnumerable<ReviewVM>> GetAllAsync();
        Task<ReviewVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(ReviewVM reviewVm);
        Task<bool> UpdateAsync(ReviewVM reviewVm);
        Task<bool> DeleteAsync(int id);
        
        // Validation Methods
        Task<bool> BookingBelongsToCustomerAsync(int bookingId, int customerId);
        
        // Helper Methods
        Task<IEnumerable<SelectListItem>> GetBookingsByCustomerAsSelectListAsync(int customerId, int? excludeBookingId = null);
    }
}