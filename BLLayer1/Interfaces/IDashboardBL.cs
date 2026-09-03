using BLLayer1.ViewModel;
using System.Threading.Tasks;

namespace BLLayer1.Interfaces
{
    public interface IDashboardBL
    {
        Task<AdminDashboardVM> GetDashboardDataAsync();
    }
}
