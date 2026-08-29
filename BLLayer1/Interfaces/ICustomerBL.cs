using BLLayer1.ViewModel;

namespace BLLayer1.Interfaces
{
    public interface ICustomerBL
    {
        IEnumerable<CustomerVM> GetAll();
        CustomerVM? GetById(int id);
        void Create(CustomerVM customerVm);
        void Update(CustomerVM customerVm);
        void Delete(int id); // Soft Delete execution
    }
}