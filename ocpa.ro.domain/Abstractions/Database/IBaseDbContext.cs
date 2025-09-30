using System.Collections.Generic;

namespace ocpa.ro.domain.Abstractions.Database;

public interface IBaseDbContext
{

    int Insert<T>(T entity) where T : class, IDbEntity;
    int InsertRange<T>(IEnumerable<T> entities) where T : class, IDbEntity;

    int Update<T>(T entity) where T : class, IDbEntity;
    int Delete<T>(T entity) where T : class, IDbEntity;
    int ExecuteSqlRaw(string query, params object[] args);

    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}