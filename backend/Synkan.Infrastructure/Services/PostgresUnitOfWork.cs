using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Synkan.Application.Interfaces;
using Synkan.Infrastructure.Persistence;

namespace Synkan.Infrastructure.Services;

public class PostgresUnitOfWork(AppDbContext context, ILogger<PostgresUnitOfWork> logger) : IUnitOfWork
{
    public async Task SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict during SaveChanges");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            logger.LogWarning(
                ex, 
                "Database constraint violation: {SqlState}, constraint: {ConstraintName}, schema: {Schema}, Table: {Table}, Column: {Column}, at: {Where}",
                pg.SqlState, pg.ConstraintName, pg.SchemaName, pg.TableName, pg.ColumnName, pg.Where
            );
        }
    }
}