using LinqToDB;

namespace Ardalis.Specification.LinqToDb
{
    public interface IDbContextFactory<TIDataContext>
    {
        TIDataContext CreateDbContext();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TRepository">The Interface of the repository created by this Factory</typeparam>
    /// <typeparam name="TConcreteRepository">
    /// The Concrete implementation of the repository interface to create
    /// </typeparam>
    /// <typeparam name="TContext">The DbContext derived class to support the concrete repository</typeparam>
    public class LinqToDBDataContextFactory<TContext> : IDbContextFactory<IDataContext>
        where TContext : IDataContext
    {
        private readonly IDataContext _dbContextFactory;

        /// <summary>
        /// Initialises a new instance of the EFRepositoryFactory
        /// </summary>
        /// <param name="dbContextFactory">The IDbContextFactory to use to generate the TContext</param>
        public LinqToDBDataContextFactory(DataContext dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <inheritdoc />
        public IDataContext CreateDbContext()
        {
            return _dbContextFactory;
        }
    }
}
