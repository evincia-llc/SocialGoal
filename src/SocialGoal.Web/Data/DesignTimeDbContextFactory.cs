using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SocialGoal.Web.Data;

/// <summary>
/// Design-time only: dotnet-ef needs a context instance to scaffold and compare
/// migrations, and the host's registration is conditional on a configured
/// connection string (see Program.cs), so there is nothing for the tooling to
/// discover at design time.
///
/// The connection string below is never opened -- scaffolding needs a provider,
/// not a database -- and points at a throwaway LocalDB catalog so that even if
/// a tooling command did connect, it could not touch anything that matters.
/// No runtime code path constructs this factory.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SocialGoalDbContext>
{
    private const string DesignTimeConnectionString =
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SocialGoal_DesignTime;Integrated Security=True;Encrypt=False";

    public SocialGoalDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SocialGoalDbContext>()
            .UseSqlServer(DesignTimeConnectionString)
            .Options);
}
