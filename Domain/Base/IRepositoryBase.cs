using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Domain.Base
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> DataSet { get; }

        Task<T> GetByIdAsync(int id);

        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);

        Task<List<T>> FindAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> FindAsync(Expression<Func<T, T>> selector);
        Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);
        Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);
        Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);

        Task<T> FindFirstAsync(Expression<Func<T, bool>> expression);
        Task<T> FindFirstAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression);
        Task<T> FindFirstAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);
        Task<T> FindFirstAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);

        Task<T> FindLastAsync(Expression<Func<T, bool>> expression);
        Task<T> FindLastAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression);
        Task<T> FindLastAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);
        Task<T> FindLastAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction);

        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);

        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> expression);

        T Create(T entity);
        void CreateMultiple(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateMultiple(IEnumerable<T> entities);
        void Delete(T entity);
        void DeleteMultiple(IEnumerable<T> entities);
        Task SaveAsync();

        IDbContextTransaction BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void ExecuteInTransaction(Action action);
        Task ExecuteInTransaction(Func<Task> action);
        void Attach(T entity);
    }
}
