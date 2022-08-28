using Microsoft.EntityFrameworkCore;

namespace WebRentServer.NETCore.Persistance.Repository
{
    public class Repository<TEntity, TPKey> : IRepository<TEntity, TPKey> where TEntity : class
    {
        protected readonly RVDBContext context;
        public Repository(RVDBContext context)
        {
            this.context = context;
        }

        public void Add(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().AddRange(entities);
        }

        public IEnumerable<TEntity> Find(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate);
        }

        public TEntity Get(TPKey id)
        {
            return context.Set<TEntity>().Find(id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return context.Set<TEntity>().ToList();
        }

        public IEnumerable<TEntity> GetAll(int pageIndex, int pageSize)
        {
            return context.Set<TEntity>().ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public void Remove(TEntity entity)
        {
            context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().RemoveRange(entities);
        }

        public int CountElements()
        {
            return context.Set<TEntity>().Count();
        }

        public void Update(TEntity entity)
        {
            context.Set<TEntity>().Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}