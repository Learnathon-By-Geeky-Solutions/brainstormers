using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace TaskForge.Application.Interfaces.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<int> SaveChangesAsync();
    }
}
