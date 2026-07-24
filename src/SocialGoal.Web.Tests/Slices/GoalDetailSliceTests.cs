using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Tests.TestSupport;

namespace SocialGoal.Web.Tests.Slices;

/// <summary>
/// Sprint 5 gating spike 3: the read-only vertical slice, end to end. The
/// .NET 10 host serves /Goal/Details/{id} over real HTTP (TestServer) from a
/// database carrying the legacy schema baseline. This is also the first
/// in-process HTTP test in the epic -- impossible against System.Web MVC 5
/// (see D11); from Sprint 9 on, this is how slice matrix tests run in
/// enforcement mode.
/// </summary>
[TestFixture]
[NonParallelizable]
public class GoalDetailSliceTests
{
    private const string SliceDb = "SocialGoal_ModernSlice";

    private WebApplicationFactory<Program> factory = null!;
    private HttpClient client = null!;
    private int seededGoalId;
    private int partialNameGoalId;

    [OneTimeSetUp]
    public async Task CreateDatabaseAndHost()
    {
        LocalDb.CreateDatabase(SliceDb);
        LocalDb.ExecuteScript(SliceDb, File.ReadAllText(LocalDb.SchemaBaselinePath()));

        using (var context = new SocialGoalDbContext(
            new DbContextOptionsBuilder<SocialGoalDbContext>()
                .UseSqlServer(LocalDb.ConnectionStringFor(SliceDb))
                .Options))
        {
            var owner = new ApplicationUser
            {
                Id = "slice-user-0001",
                UserName = "sliceuser",
                FirstName = "Slice",
                LastName = "Owner",
            };
            var goal = new Goal
            {
                GoalName = "Run a marathon",
                Desc = "Train for and finish a full marathon.",
                StartDate = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                EndDate = new DateTime(2014, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
                Target = 42.2,
                GoalType = true,
                CreatedDate = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                User = owner,
                Metric = new Metric { Type = "Kms" },
                GoalStatus = new GoalStatus { GoalStatusType = "In Progress" },
            };
            context.Goals.Add(goal);

            // Owner with only a first name: pins the NULL-coalescing in the
            // projection (SQL + would null the whole concat; Copilot run 1).
            var partialGoal = new Goal
            {
                GoalName = "Read twelve books",
                StartDate = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                EndDate = new DateTime(2014, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
                GoalType = false,
                CreatedDate = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                User = new ApplicationUser
                {
                    Id = "slice-user-0002",
                    UserName = "partialuser",
                    FirstName = "Mononym",
                    LastName = null,
                },
                GoalStatus = new GoalStatus { GoalStatusType = "Not Started" },
            };
            context.Goals.Add(partialGoal);
            await context.SaveChangesAsync();
            seededGoalId = goal.GoalId;
            partialNameGoalId = partialGoal.GoalId;
        }

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting(
                "ConnectionStrings:SocialGoal",
                LocalDb.ConnectionStringFor(SliceDb)));
        client = factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task DropDatabaseAndHost()
    {
        // OneTimeTearDown runs even when OneTimeSetUp failed: guard the
        // disposables and drop the database unconditionally so a setup
        // failure is not masked and the catalog is not leaked.
        try
        {
            client?.Dispose();
            if (factory is not null)
            {
                await factory.DisposeAsync();
            }
        }
        finally
        {
            LocalDb.DropDatabase(SliceDb);
        }
    }

    [Test]
    public async Task GoalDetail_RendersTheSeededGoal_FromTheBaselineSchema()
    {
        var response = await client.GetAsync($"/Goal/Details/{seededGoalId}");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("Run a marathon"), "goal name");
        Assert.That(html, Does.Contain("Train for and finish a full marathon."), "description");
        Assert.That(html, Does.Contain("In Progress"), "status from the GoalStatus lookup");
        Assert.That(html, Does.Contain("Kms"), "metric from the Metrics lookup");
        Assert.That(html, Does.Contain("Slice Owner"), "owner display name from AspNetUsers");
    }

    [Test]
    public async Task GoalDetail_OwnerWithOnlyFirstName_RendersThePresentPart()
    {
        var response = await client.GetAsync($"/Goal/Details/{partialNameGoalId}");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("Mononym"),
            "A NULL LastName must not blank the whole owner display name.");
    }

    [Test]
    public async Task GoalDetail_UnknownId_Returns404()
    {
        var response = await client.GetAsync("/Goal/Details/999999");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }

    [Test]
    public async Task HealthEndpoint_ReportsHealthy()
    {
        var response = await client.GetAsync("/health");

        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Healthy"));
    }
}
