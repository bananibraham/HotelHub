using BLLayer1.ViewModel;

namespace BLLayer1.Interfaces
{
    public interface ICustomerBL
    {
        Task<IEnumerable<CustomerVM>> GetAllAsync();
        Task<CustomerVM?> GetByIdAsync(int id);
        Task CreateAsync(CustomerVM customerVm);
        Task UpdateAsync(CustomerVM customerVm);
        Task DeleteAsync(int id);
    }
}