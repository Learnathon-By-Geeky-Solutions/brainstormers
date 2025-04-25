using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace TaskForge.Application.Interfaces.Repositories.Common
{
    public interface IUnitOfWork : IDisposable
    {
		Task<IDbContextTransaction> BeginTransactionAsync();
		Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
		Task<int> SaveChangesAsync();
	}
}
