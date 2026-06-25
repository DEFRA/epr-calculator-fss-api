namespace EPR.Calculator.FSS.API.Data;

using EPR.Calculator.FSS.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
public class SynapseDbContext : DbContext
{
    public SynapseDbContext()
    {
    }

    public SynapseDbContext(DbContextOptions<SynapseDbContext> options)
    : base(options)
    {
    }

    public DbSet<AcceptedGrantedOrgDataResponseModel> AcceptedGrantedOrgDataResponseModel { get; set; } = null!;

    public virtual async Task<IList<TEntity>> RunSqlAsync<TEntity>(
        string sql,
        CancellationToken cancellationToken,
        params object[] parameters)
        where TEntity : class
    {
        return await Set<TEntity>()
            .FromSqlRaw(sql, parameters)
            .ToListAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcceptedGrantedOrgDataResponseModel>(entity =>
        {
            entity.HasNoKey();
        });
    }
}