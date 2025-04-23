using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace TaskForge.Application.Interfaces.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<int> SaveChangesAsync();
    }
}
