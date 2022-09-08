using System.Linq.Expressions;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IRepository<TEntity, TPKey> where TEntity : class
    {
        TEntity Get(TPKey id);
        Task<TEntity> GetAsync(TPKey id);
        IEnumerable<TEntity> GetAll();
        Task<IEnumerable<TEntity>> GetAllAsync();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> GetAll(int pageIndex, int pageSize);

        void Add(TEntity entity);
        Task AddAsync(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        int CountElements(); 
        Task<int> CountElementsAsync();
    }
}
