using DocumentFormat.OpenXml.InkML;
using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Models.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Base
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected DbContext RepositoryContext { get; set; }
        private readonly Lazy<DbSet<T>> _dataSet = null;
        protected DbSet<T> DbSet => _dataSet.Value;
        public IQueryable<T> DataSet => _dataSet.Value;

        protected RepositoryBase(DbContext repositoryContext)
        {
            RepositoryContext = repositoryContext;
            _dataSet = new Lazy<DbSet<T>>(() =>
            {
                return RepositoryContext.Set<T>();
            });
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task<List<T>> GetAllAsync(Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await DbSet.Where(expression).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Where(expression)).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, T>> selector)
        {
            return await DbSet.Select(selector).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Select(selector)).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression)
        {
            return await DbSet.Select(selector).Where(expression).ToListAsync();
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Select(selector).Where(expression)).ToListAsync();
        }

        public async Task<T> FindFirstAsync(Expression<Func<T, bool>> expression)
        {
            return await DbSet.FirstOrDefaultAsync(expression);
        }

        public async Task<T> FindFirstAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Where(expression)).FirstOrDefaultAsync();
        }

        public async Task<T> FindFirstAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression)
        {
            return await DbSet.Select(selector).FirstOrDefaultAsync(expression);
        }

        public async Task<T> FindFirstAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Select(selector).Where(expression)).FirstOrDefaultAsync();
        }

        public async Task<T> FindLastAsync(Expression<Func<T, bool>> expression)
        {
            return await DbSet.LastOrDefaultAsync(expression);
        }

        public async Task<T> FindLastAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Where(expression)).LastOrDefaultAsync();
        }

        public async Task<T> FindLastAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression)
        {
            return await DbSet.Select(selector).LastOrDefaultAsync(expression);
        }

        public async Task<T> FindLastAsync(Expression<Func<T, T>> selector, Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>> orderAction)
        {
            return await orderAction.Invoke(DbSet.Select(selector).Where(expression)).LastOrDefaultAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await DbSet.AnyAsync(expression);
        }

        public async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await DbSet.CountAsync(expression);
        }

        public T Create(T entity)
        {
            return DbSet.Add(entity)?.Entity ?? entity;
        }

        public void CreateMultiple(IEnumerable<T> entities)
        {
            DbSet.AddRange(entities);
        }

        public void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public void UpdateMultiple(IEnumerable<T> entities)
        {
            DbSet.UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public void DeleteMultiple(IEnumerable<T> entities)
        {
            DbSet.RemoveRange(entities);
        }

        public async Task SaveAsync()
        {
            await RepositoryContext.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return RepositoryContext.Database.BeginTransaction();
        }

        public void CommitTransaction()
        {
            RepositoryContext.Database.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            RepositoryContext.Database.RollbackTransaction();
        }

        public void ExecuteInTransaction(Action action)
        {
            if (RepositoryContext.Database.CurrentTransaction != null)
            {
                action?.Invoke();
            }
            else
            {
                using (IDbContextTransaction transaction = RepositoryContext.Database.BeginTransaction())
                {
                    action?.Invoke();
                    transaction.Commit();
                }
            }
        }

        public async Task ExecuteInTransaction(Func<Task> action)
        {
            if (RepositoryContext.Database.CurrentTransaction != null)
            {
                await action?.Invoke();
            }
            else
            {
                using (IDbContextTransaction transaction = RepositoryContext.Database.BeginTransaction())
                {
                    await action?.Invoke();
                    transaction.Commit();
                }
            }
        }
        public void Attach(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            DbSet.Attach(entity);
        }
    }
}
