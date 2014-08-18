namespace DeltaSigmaPhiWebsite.Data.Interfaces
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public interface IGenericRepository<TEntity> where TEntity : class
    {
        TEntity Single(Expression<Func<TEntity, bool>> predicate);
        TEntity SingleById(object id);
        IQueryable<TEntity> SelectAll();
        IQueryable<TEntity> SelectBy(Expression<Func<TEntity, bool>> predicate);
        bool Exists(TEntity entity);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void DeleteById(object id);
        void Delete(TEntity entity);
        void SaveChanges();
    }
}