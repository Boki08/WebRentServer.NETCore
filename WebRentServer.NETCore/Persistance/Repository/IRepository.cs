using System.Linq.Expressions;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public interface IRepository<TEntity, TPKey> where TEntity : class
    {
        TEntity Get(TPKey id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        IEnumerable<TEntity> GetAll(int pageIndex, int pageSize);

        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        int CountElements();
    }
}
