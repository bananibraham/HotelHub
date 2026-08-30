using BLLayer1.ViewModel;

namespace BLLayer1.Interfaces
{
    public interface IReviewBL
    {
        Task<IEnumerable<ReviewVM>> GetAllAsync();
        Task<ReviewVM?> GetByIdAsync(int id);
        Task CreateAsync(ReviewVM reviewVm);
        Task UpdateAsync(ReviewVM reviewVm);
        Task DeleteAsync(int id);
    }
}