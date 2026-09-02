using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLLayer1.Interfaces
{
    public interface ICustomerBL
    {
        // CRUD Operations
        Task<IEnumerable<CustomerVM>> GetAllAsync();
        Task<CustomerVM?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CustomerVM customerVm);
        Task<bool> UpdateAsync(CustomerVM customerVm);
        Task<bool> DeleteAsync(int id);
        
        // Validation Methods
        Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null);
        Task<bool> NationalIdExistsAsync(string nationalId, int? excludeCustomerId = null);
        
        // Helper Methods
        Task<IEnumerable<SelectListItem>> GetActiveCustomersAsSelectListAsync();
    }
}