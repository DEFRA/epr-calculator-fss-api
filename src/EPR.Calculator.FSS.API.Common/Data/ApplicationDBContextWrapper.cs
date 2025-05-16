namespace EPR.Calculator.API.Data;

using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

// TODO: This can be removed when EPR.Calculator.Api.Data.ApplicationDBContext
// is updated to use DbContextOptions<ApplicationDBContextWrapper> in the constructor.
[ExcludeFromCodeCoverage]
public class ApplicationDBContextWrapper : ApplicationDBContext
{
    public ApplicationDBContextWrapper(DbContextOptions<ApplicationDBContextWrapper> options)
        : base(options)
    {
    }
}