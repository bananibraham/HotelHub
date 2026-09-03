using BLLayer1.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLLayer1.Interfaces
{
    public interface IBookingBL
    {
        Task<IEnumerable<BookingVM>> GetAllAsync();
        Task<IEnumerable<BookingVM>> GetByCustomerIdAsync(int customerId);
        Task<BookingVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(BookingVM bookingVm);
        Task<int> CreateAndReturnIdAsync(BookingVM bookingVm);
        Task<bool> UpdateAsync(BookingVM bookingVm);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);
        Task<(bool Success, string Message)> ConfirmAsync(int id);
        Task<(bool Success, string Message)> CheckInAsync(int id);
        Task<(bool Success, string Message)> CheckOutAsync(int id);
    }
}
