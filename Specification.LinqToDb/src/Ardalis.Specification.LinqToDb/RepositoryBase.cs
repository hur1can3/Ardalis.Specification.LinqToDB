using LinqToDB;
using LinqToDB.Data;

namespace Ardalis.Specification.LinqToDb
{
    public interface IRepositoryFactory<TRepository>
    {
        /// <summary>
        /// Generates a new repository instance
        /// </summary>
        /// <returns>The generated repository instance</returns>
        public TRepository CreateRepository();
    }

    /// <inheritdoc/>
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly DataConnection _dbContext;
        private readonly ISpecificationEvaluator _specificationEvaluator;

        public RepositoryBase(DataConnection dbContext)
            : this(dbContext, SpecificationEvaluator.Default)
        {
        }

        /// <inheritdoc/>
        public RepositoryBase(DataConnection dbContext, ISpecificationEvaluator specificationEvaluator)
        {
            _dbContext = dbContext;
            _specificationEvaluator = specificationEvaluator;
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            var table = _dbContext.GetTable<T>();
            await _dbContext.InsertAsync(entity, token: cancellationToken);

            await SaveChangesAsync(cancellationToken);

            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            //_dbContext.GetTable<T>().AddRange(entities);

            //await SaveChangesAsync(cancellationToken);

            return entities;
        }

        /// <inheritdoc/>
        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            //_dbContext.GetTable<T>().Update(entity);
            await _dbContext.UpdateAsync(entity, token: cancellationToken);

            await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            //_dbContext.UpdateRange(entities);

            //await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.DeleteAsync<T>(entity, token: cancellationToken);
            //_dbContext.GetTable<T>().Delete(entity);

            await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {

            //_dbContext.GetTable<T>().RemoveRange(entities);

            //await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteRangeAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            //var query = ApplySpecification(specification);
            //_dbContext.GetTable<T>().RemoveRange(query);

            //await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //return await _dbContext.SaveChangesAsync(cancellationToken);
            
            _dbContext.CommitTransaction();
            return 1;
        }

        /// <inheritdoc/>
        public virtual async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
        {
            return await _dbContext.ObjectGetByIdAsync<T,TId>(id, cancellationToken);
        }

        /// <inheritdoc/>
        [Obsolete]
        public virtual async Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        [Obsolete]
        public virtual async Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).SingleOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.GetTable<T>().ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);

            return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
        }

        /// <inheritdoc/>
        public virtual async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
        {
            var queryResult = await ApplySpecification(specification).ToListAsync(cancellationToken);

            return specification.PostProcessingAction == null ? queryResult : specification.PostProcessingAction(queryResult).ToList();
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification, true).CountAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.GetTable<T>().CountAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification, true).AnyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.GetTable<T>().AnyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
        {
            return ApplySpecification(specification).AsAsyncEnumerable();
        }

        /// <summary>
        /// Filters the entities  of <typeparamref name="T"/>, to those that match the encapsulated query logic of the
        /// <paramref name="specification"/>.
        /// </summary>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>The filtered entities as an <see cref="IQueryable{T}"/>.</returns>
        protected virtual IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false)
        {
            return _specificationEvaluator.GetQuery(_dbContext.GetTable<T>().AsQueryable(), specification, evaluateCriteriaOnly);
        }

        /// <summary>
        /// Filters all entities of <typeparamref name="T" />, that matches the encapsulated query logic of the
        /// <paramref name="specification"/>, from the database.
        /// <para>
        /// Projects each entity into a new form, being <typeparamref name="TResult" />.
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the projection.</typeparam>
        /// <param name="specification">The encapsulated query logic.</param>
        /// <returns>The filtered projected entities as an <see cref="IQueryable{T}"/>.</returns>
        protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
        {
            return _specificationEvaluator.GetQuery(_dbContext.GetTable<T>().AsQueryable(), specification);
        }
    }
}
