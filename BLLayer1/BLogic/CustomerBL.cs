using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

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

        public async Task CreateAsync(CustomerVM vm)
        {
            var customer = new Customer
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                NationalId = vm.NationalId ?? string.Empty,
                Address = vm.Address ?? string.Empty,
                City = vm.City ?? string.Empty,
                Country = vm.Country ?? string.Empty,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _customerRepo.AddAsync(customer);
            await _customerRepo.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerVM vm)
        {
            var customer = await _customerRepo.GetByIdAsync(vm.CustomerId);
            if (customer != null)
            {
                customer.FullName = vm.FullName;
                customer.Email = vm.Email;
                customer.Phone = vm.Phone;
                customer.NationalId = vm.NationalId ?? customer.NationalId;
                customer.Address = vm.Address ?? customer.Address;
                customer.City = vm.City ?? customer.City;
                customer.Country = vm.Country ?? customer.Country;
                customer.IsActive = vm.IsActive;

                _customerRepo.Update(customer);
                await _customerRepo.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            if (customer != null)
            {
                customer.IsActive = false;
                _customerRepo.Update(customer);
                await _customerRepo.SaveChangesAsync();
            }
        }
    }
}