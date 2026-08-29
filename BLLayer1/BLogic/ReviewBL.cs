using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;

namespace BLLayer1.BLogic
{
    public class ReviewBL : IReviewBL
    {
        private readonly IBasicOperation<Review> _reviewRepo;

        public ReviewBL(IBasicOperation<Review> reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        public IEnumerable<ReviewVM> GetAll()
        {
            return _reviewRepo.GetAll().Select(r => new ReviewVM
            {
                ReviewId = r.ReviewId,
                CustomerId = r.CustomerId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            });
        }

        public ReviewVM? GetById(int id)
        {
            var r = _reviewRepo.GetById(id);
            if (r == null) return null;

            return new ReviewVM
            {
                ReviewId = r.ReviewId,
                CustomerId = r.CustomerId,
                BookingId = r.BookingId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            };
        }

        public void Create(ReviewVM vm)
        {
            var review = new Review
            {
                CustomerId = vm.CustomerId,
                BookingId = vm.BookingId,
                Rating = vm.Rating,
                Comment = vm.Comment,
                CreatedAt = DateTime.Now
            };

            _reviewRepo.Add(review);
            _reviewRepo.SaveChanges();
        }

        public void Update(ReviewVM vm)
        {
            var review = _reviewRepo.GetById(vm.ReviewId);
            if (review != null)
            {
                review.CustomerId = vm.CustomerId;
                review.BookingId = vm.BookingId;
                review.Rating = vm.Rating;
                review.Comment = vm.Comment;

                _reviewRepo.Update(review);
                _reviewRepo.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            _reviewRepo.Delete(id);
            _reviewRepo.SaveChanges();
        }
    }
}