using Microsoft.EntityFrameworkCore;
using ocpa.ro.domain.Abstractions.Database;
using System.Collections.Generic;
using System.Linq;

namespace ocpa.ro.persistence;

public abstract class BaseDbContext : DbContext, IBaseDbContext
{
    public BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    void IBaseDbContext.BeginTransaction() => Database.BeginTransaction();

    void IBaseDbContext.CommitTransaction() => Database.CommitTransaction();

    void IBaseDbContext.RollbackTransaction() => Database.RollbackTransaction();


    int IBaseDbContext.ExecuteSqlRaw(string query, params object[] args) => Database.ExecuteSqlRaw(query, args);


    int IBaseDbContext.Delete<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Remove(entity);
            return SaveChanges();
        }

        return 0;
    }

    int IBaseDbContext.Insert<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Add(entity);
            return SaveChanges();
        }

        return 0;
    }

    int IBaseDbContext.InsertRange<T>(IEnumerable<T> entities)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.AddRange(entities);
            return SaveChanges();
        }

        return 0;
    }


    int IBaseDbContext.Update<T>(T entity)
    {
        var dbSet = GetDbContext<T>();
        if (dbSet != null)
        {
            dbSet.Update(entity);
            return SaveChanges();
        }

        return 0;
    }

    protected DbSet<T> GetDbContext<T>() where T : class, IDbEntity
    {
        var dbSetType = typeof(DbSet<T>);
        var pi = GetType().GetProperties().Where(p => p.PropertyType == dbSetType).FirstOrDefault();
        return pi?.GetValue(this) as DbSet<T>;
    }


}
