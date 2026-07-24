using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialGoal.Web.Data;
using SocialGoal.Web.Models;

namespace SocialGoal.Web.Controllers;

/// <summary>
/// Sprint 5 vertical slice: read-only goal detail rendered from the real
/// schema. Legacy reference: GoalController.Index(int id) (MVC 5). No auth is
/// wired into the host until Sprint 8, so the page is reachable anonymously
/// for now; the Phase 2 standing constraints (policy-based authorization,
/// resource handlers) attach when the slices land for real in Sprints 9-11.
/// </summary>
public class GoalController(SocialGoalDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var goal = await context.Goals
            .AsNoTracking()
            .Where(g => g.GoalId == id)
            .Select(g => new GoalDetailViewModel
            {
                GoalId = g.GoalId,
                GoalName = g.GoalName,
                Desc = g.Desc,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                Target = g.Target,
                GoalType = g.GoalType,
                MetricType = g.Metric != null ? g.Metric.Type : null,
                GoalStatusType = g.GoalStatus != null ? g.GoalStatus.GoalStatusType : null,
                // Coalesce before concatenating: SQL's + yields NULL if either
                // side is NULL, which would blank the whole display name when
                // only one part is missing (Copilot run 1). Legacy CLR concat
                // kept partial names; this preserves that behavior server-side.
                OwnerDisplayName = g.User != null
                    ? ((g.User.FirstName ?? "") + " " + (g.User.LastName ?? "")).Trim()
                    : null,
                CreatedDate = g.CreatedDate,
            })
            .SingleOrDefaultAsync(cancellationToken);

        return goal is null ? NotFound() : View(goal);
    }
}
