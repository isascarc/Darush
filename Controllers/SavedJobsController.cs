namespace MyJob.Controllers;

[Authorize(Roles = Roles.User)]
public class SavedJobsController : BaseApiController
{
    public DataContext Context { get; }

    public SavedJobsController(DataContext context)
    {
        Context = context;
    }

    [HttpGet("as-ids")]
    public async Task<ActionResult<List<int>>> GetAllSavedJobIds()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        return user.SavedJobs;
    }

    [HttpGet]
    public async Task<ActionResult<List<Job>>> GetAllSavedJobs()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        var jobs = Context.Jobs.Where(x => user.SavedJobs.Contains(x.Id)).ToList();
        return jobs;
    }

    [HttpPost]
    public async Task<ActionResult> AddSavedJob([FromBody] int jobId)
    {
        var user = (await UserFuncs.GetUserInfo(Context, User, false));

        if (!user.SavedJobs.Contains(jobId))
        {
            var SavedJobs = user.SavedJobs;
            SavedJobs.Add(jobId);
            user.SavedJobs = new(SavedJobs);
        }

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }
    
    
    [HttpDelete("{jobId}")]
    public async Task<ActionResult> RemoveSavedJob(int jobId)
    {
        var user = (await UserFuncs.GetUserInfo(Context, User, false));

        if (user.SavedJobs.Contains(jobId))
        {
            var SavedJobs = user.SavedJobs;
            SavedJobs.Remove(jobId);
            user.SavedJobs = new(SavedJobs);
        }

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }






    [HttpPost("{JobId}")]
    public async Task<ActionResult<UserDto>> Create(int JobId)
    {
        // Check user
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        if (user == null)
            return Unauthorized();
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
