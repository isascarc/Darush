using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using MyJob.Models;
using System.Linq;
using System.Text.Json.Nodes;

namespace MyJob.Controllers;

[Authorize(Roles = Roles.User)]
public class ApplicantsController(DataContext Context) : BaseApiController
{
    [HttpPost("Create/{JobId}")]
    public async Task<ActionResult> Create(int JobId)
    {
        var user = await UserFuncs.GetUserInfo(Context, User);

        if (user.CVs.Count < 1)
            return NotFound("Please upload a resume.");

        var job = await Context.Jobs.Include(x => x.Applicants).FirstOrDefaultAsync(x => x.Id == JobId);
        if (job == null)
            return NotFound("Job Not Found.");

        if (!job.Applicants.Any(x => x.UserId == user.Id))
            job.Applicants.Add(
                new()
                {
                    LinkedinLink = user.LinkedinLink,
                    CvId = user.CVs.Where(x => x.IsDefault).First().Id,
                    UserId = user.Id
                }
            );

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to Applicant user.");
    }

    [HttpGet("Get-all-Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants()
    {
        return Ok(await Context.Applicants.Where(x => !x.Deleted).Select(x => new
        {
            x.Id,
            x.JobId,
            x.Create,
            x.LinkedinLink,
            x.CvId,
            x.UserId,
        }).ToListAsync());
    }

    [HttpGet]
    public async Task<ActionResult<JsonArray>> GetMyApplicants()
    {
        var user = await UserFuncs.GetUserInfo(Context, User); 
        var res = await (from applicant in Context.Set<Applicant>()
                         where applicant.UserId == user.Id
                         join job in Context.Set<Job>()
                         on applicant.JobId equals job.Id into appJobs
                         from appJob in appJobs.DefaultIfEmpty()
                         select new { Job = appJob, applicant }).ToListAsync();
        
        var apps = res.Select(x => x.applicant);
        var appsRet = apps.Adapt<List<ApplicantDto>>();

        var jobs = res.Select(x => x.Job);
        var jobsRet = apps.Adapt<List<JobDto>>();

        return Ok(new JsonArray() { appsRet, jobsRet });
    }

    [HttpDelete("delete/{ApplicantId}")]
    public async Task<ActionResult> Delete(int ApplicantId)
    {
        // Check user
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        if (user == null)
            return Unauthorized();

        var Applicant = await Context.Applicants.Where(x => x.Id == ApplicantId && x.UserId == user.Id && !x.Deleted).FirstOrDefaultAsync();
        if (Applicant == null)
            return NotFound("Applicant not exist.");

        Applicant.Deleted = true;

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }
}
