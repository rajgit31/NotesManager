﻿using System;
using System.Collections;
using System.Data.Entity;
using NotesManager.Domain.Interfaces.Data;

namespace NotesManager.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContext _context;
        private bool _disposed;
        private Hashtable _repositories;
        private DbContextTransaction _dbContextTransaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public UnitOfWork(IDbContext dbContext)
        {
            _context = dbContext;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns>1 if succeeded</returns>
        public int Save()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Repositories this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>Repository instance</returns>
        public IWriteRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            var repositoryType = typeof(EfWriteRepository<>);

            if (!_repositories.Contains(repositoryType))
            {
                Type genericRepoType = repositoryType.MakeGenericType(typeof(TEntity));
                
                object instance = Activator.CreateInstance(genericRepoType, _context);
                _repositories.Add(repositoryType, instance);
            }

            return (IWriteRepository<TEntity>)_repositories[repositoryType];
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public void BeginTransaction()
        {
            //prior to EF6 old...
            /*
             * _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;
                if (_objectContext.Connection.State != ConnectionState.Open)
                {
                    _objectContext.Connection.Open();
                }
             */
            //New in EF6
            _dbContextTransaction = _context.Database.BeginTransaction();
        }

        /// <summary>
        /// Commits this transaction.
        /// </summary>
        public void Commit()
        {
            if (_dbContextTransaction != null)
            {
                _dbContextTransaction.Commit();
            }
        }

        /// <summary>
        /// Rollbacks this transaction.
        /// </summary>
        public void Rollback()
        {
            if (_dbContextTransaction != null)
            {
                _dbContextTransaction.Rollback();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }
    }

  
   
}