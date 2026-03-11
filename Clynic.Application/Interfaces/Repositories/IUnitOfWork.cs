using System.Data;

namespace Clynic.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
