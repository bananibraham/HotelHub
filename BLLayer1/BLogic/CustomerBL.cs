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

        public IEnumerable<CustomerVM> GetAll()
        {
            return _customerRepo.GetAll().Select(c => new CustomerVM
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

        public CustomerVM? GetById(int id)
        {
            var c = _customerRepo.GetById(id);
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

        public void Create(CustomerVM vm)
        {
            var customer = new Customer
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                NationalId = vm.NationalId,
                Address = vm.Address,
                City = vm.City,
                Country = vm.Country,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _customerRepo.Add(customer);
            _customerRepo.SaveChanges();
        }

        public void Update(CustomerVM vm)
        {
            var customer = _customerRepo.GetById(vm.CustomerId);
            if (customer != null)
            {
                customer.FullName = vm.FullName;
                customer.Email = vm.Email;
                customer.Phone = vm.Phone;
                customer.NationalId = vm.NationalId;
                customer.Address = vm.Address;
                customer.City = vm.City;
                customer.Country = vm.Country;

                _customerRepo.Update(customer);
                _customerRepo.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            // Soft Delete Policy according to ERD rules
            var customer = _customerRepo.GetById(id);
            if (customer != null)
            {
                customer.IsActive = false;
                _customerRepo.Update(customer);
                _customerRepo.SaveChanges();
            }
        }
    }
}