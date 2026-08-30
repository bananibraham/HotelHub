using BLLayer1.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLLayer1.Interfaces
{
    public interface IBookingBL
    {
        Task<IEnumerable<BookingVM>> GetAllAsync();
        Task<BookingVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(BookingVM bookingVm);
        Task<bool> UpdateAsync(BookingVM bookingVm);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);
    }
}
