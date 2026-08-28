using HotelHub.Models.Entities;

namespace HotelHub.Repository.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task SoftDeleteAsync(int id);
        Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
    }
}