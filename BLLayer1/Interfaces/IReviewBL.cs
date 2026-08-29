using BLLayer1.ViewModel;

namespace BLLayer1.Interfaces
{
    public interface IReviewBL
    {
        IEnumerable<ReviewVM> GetAll();
        ReviewVM? GetById(int id);
        void Create(ReviewVM reviewVm);
        void Update(ReviewVM reviewVm);
        void Delete(int id);
    }
}