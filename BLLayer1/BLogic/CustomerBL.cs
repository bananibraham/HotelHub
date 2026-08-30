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
                Phone = c.Phone
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
                Phone = c.Phone
            };
        }

        public async Task CreateAsync(CustomerVM vm)
        {
            var customer = new Customer
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone
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

                _customerRepo.Update(customer);
                await _customerRepo.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _customerRepo.DeleteAsync(id);
            await _customerRepo.SaveChangesAsync();
        }
    }
}