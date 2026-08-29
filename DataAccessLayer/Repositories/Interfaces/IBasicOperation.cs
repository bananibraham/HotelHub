namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IBasicOperation<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        void SaveChanges();
    }
}
