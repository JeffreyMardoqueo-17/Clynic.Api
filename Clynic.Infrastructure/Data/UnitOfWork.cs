using System.Data;
using Clynic.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clynic.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClynicDbContext _dbContext;

        public UnitOfWork(ClynicDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel);
                try
                {
                    await operation();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel);
                try
                {
                    var result = await operation();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
