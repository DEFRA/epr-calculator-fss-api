namespace EPR.Calculator.FSS.API.Common.Data;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using EPR.Calculator.FSS.API.Common.Data.Entities;

[ExcludeFromCodeCoverage]
public class SynapseDbContext : DbContext
{
    public DbSet<AcceptedGrantedOrgDataResponseModel> AcceptedGrantedOrgDataResponseModel { get; set; } = null!;

    public SynapseDbContext()
    {
    }

    public SynapseDbContext(DbContextOptions<SynapseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcceptedGrantedOrgDataResponseModel>(entity => {
            entity.HasNoKey();
        });
    }

    public virtual async Task<IList<TEntity>> RunSqlAsync<TEntity>(string sql, params object[] parameters) where TEntity : class
    {
        return await Set<TEntity>().FromSqlRaw(sql, parameters).AsAsyncEnumerable().ToListAsync();
    }
}
