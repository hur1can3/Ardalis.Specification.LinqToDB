﻿using LinqToDB.Data;

namespace Ardalis.Specification.LinqToDb
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TRepository">The Interface of the repository created by this Factory</typeparam>
    /// <typeparam name="TConcreteRepository">
    /// The Concrete implementation of the repository interface to create
    /// </typeparam>
    /// <typeparam name="TContext">The DbContext derived class to support the concrete repository</typeparam>
    public class Lin2DbRepositoryFactory<TRepository, TConcreteRepository, TContext> : IRepositoryFactory<TRepository>
      where TConcreteRepository : TRepository
      where TContext : DataConnection
    {
        private readonly IDbContextFactory<TContext> _dbContextFactory;

        /// <summary>
        /// Initialises a new instance of the EFRepositoryFactory
        /// </summary>
        /// <param name="dbContextFactory">The IDbContextFactory to use to generate the TContext</param>
        public Lin2DbRepositoryFactory(IDbContextFactory<TContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <inheritdoc />
        public TRepository CreateRepository()
        {
            var args = new object[] { _dbContextFactory.CreateDbContext() };
            return (TRepository)Activator.CreateInstance(typeof(TConcreteRepository), args)!;
        }
    }
}
