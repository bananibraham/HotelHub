using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BLLayer1.BLogic
{
    public class CustomerBL : ICustomerBL
    {
        private readonly IBasicOperation<Customer> _customerRepo;

        public CustomerBL(IBasicOperation<Customer> customerRepo)
        {
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<CustomerVM>> GetAllAsync()
        {
            var customers = await _customerRepo.GetAllAsync();
            return customers.Select(c => new CustomerVM
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                NationalId = c.NationalId,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                IsActive = c.IsActive
            });
        }

        public async Task<CustomerVM?> GetByIdAsync(int id)
        {
            var c = await _customerRepo.GetByIdAsync(id);
            if (c == null) return null;

            return new CustomerVM
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                NationalId = c.NationalId,
                Address = c.Address,
                City = c.City,
                Country = c.Country,
                IsActive = c.IsActive
            };
        }

        public async Task<bool> CreateAsync(CustomerVM vm)
        {
            // ✅ Performance optimized validation
            if (await EmailExistsAsync(vm.Email))
            {
                return false;
            }
            
            if (await NationalIdExistsAsync(vm.NationalId))
            {
                return false;
            }

            var customer = new Customer
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                NationalId = vm.NationalId,
                Address = vm.Address,
                City = vm.City,
                Country = vm.Country,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.Now
            };

            await _customerRepo.AddAsync(customer);
            await _customerRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(CustomerVM vm)
        {
            var customer = await _customerRepo.GetByIdAsync(vm.CustomerId);
            if (customer == null) return false;

            // ✅ Performance optimized validation
            if (await EmailExistsAsync(vm.Email, vm.CustomerId))
            {
                return false;
            }
            
            if (await NationalIdExistsAsync(vm.NationalId, vm.CustomerId))
            {
                return false;
            }

            customer.FullName = vm.FullName;
            customer.Email = vm.Email;
            customer.Phone = vm.Phone;
            customer.NationalId = vm.NationalId;
            customer.Address = vm.Address;
            customer.City = vm.City;
            customer.Country = vm.Country;
            customer.IsActive = vm.IsActive;

            _customerRepo.Update(customer);
            await _customerRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            if (customer == null) return false;

            customer.IsActive = false;
            _customerRepo.Update(customer);
            await _customerRepo.SaveChangesAsync();
            return true;
        }

        // ✅ Performance optimized: Uses AnyAsync instead of GetAllAsync
        public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
        {
            if (excludeCustomerId.HasValue)
            {
                return await _customerRepo.AnyAsync(c => 
                    c.Email.ToLower() == email.ToLower() && 
                    c.CustomerId != excludeCustomerId.Value);
            }
            
            return await _customerRepo.AnyAsync(c => c.Email.ToLower() == email.ToLower());
        }

        // ✅ Performance optimized: Uses AnyAsync instead of GetAllAsync
        public async Task<bool> NationalIdExistsAsync(string nationalId, int? excludeCustomerId = null)
        {
            if (excludeCustomerId.HasValue)
            {
                return await _customerRepo.AnyAsync(c => 
                    c.NationalId == nationalId && 
                    c.CustomerId != excludeCustomerId.Value);
            }
            
            return await _customerRepo.AnyAsync(c => c.NationalId == nationalId);
        }

        public async Task<IEnumerable<SelectListItem>> GetActiveCustomersAsSelectListAsync()
        {
            var customers = await _customerRepo.GetAllAsync();
            return customers
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = $"{c.FullName} ({c.Email})"
                })
                .OrderBy(s => s.Text);
        }
    }
}