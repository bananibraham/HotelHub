using HotelHub.Data;
using HotelHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.Repository.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.Now;
            customer.IsActive = true;
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                customer.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email && c.CustomerId != excludeId);
        }
    }
}