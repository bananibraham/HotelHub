using DataAccessLayer;
using DataAccessLayer.Repositories.Interfaces;
using HotelHub.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories.Classes
{
    public class BasicOperation<T> : IBasicOperation<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BasicOperation(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.ToList();
        public T? GetById(int id) => _dbSet.Find(id);
        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null) _dbSet.Remove(entity);
        }
        public void SaveChanges() => _context.SaveChanges();
    }
}
