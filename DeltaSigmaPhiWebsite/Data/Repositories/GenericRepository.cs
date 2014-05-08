namespace DeltaSigmaPhiWebsite.Data.Repositories
{
    using Interfaces;
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Models;

    // Virtual methods can be rewritten by derived repositories to provide specialized functionality.
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal DspContext _context;

        public GenericRepository(DspContext context)
        {
            _context = context;
        }

        public virtual TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _context.Set<TEntity>().SingleOrDefault(predicate);
            return query;
        }

        public virtual TEntity GetById(object id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            var query = _context.Set<TEntity>();
            return query;
        }

        public virtual IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            var query = _context.Set<TEntity>().Where(predicate);
            return query;
        }

        public virtual bool Exists(TEntity entity)
        {
            return _context.Set<TEntity>().Select(e => e == entity).Any();
        }

        public virtual void Insert(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void DeleteById(object id)
        {
            var entityToDelete = _context.Set<TEntity>().Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }
        
        public virtual void Save()
        {
            _context.SaveChanges();
        }
    }
}