using System.Text.Json.Nodes;

namespace MyJob.Controllers;

// This class exist for users authorize
[ApiController]
[Route("jobs")]
[Authorize(Roles = Roles.User)]
public class JobsControllerForUser(DataContext Context) : ControllerBase
{
    [HttpGet("{JobId}/Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants(int JobId)
    {
        var user = await GetUser();
        var job = await Context.Jobs.Include(x => x.Applicants).FirstAsync(x => x.Id == JobId && !x.Deleted);

        return Ok(job.Applicants.Select(x => new
        {
            x.Create,
            x.CvId,
            x.Id,
            x.LinkedinLink,
            x.UserId,
        }));
    }

    private async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await Context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName && !x.Deleted);
    }
}


[Authorize(Roles = Roles.Recruiter)]
public class JobsController(DataContext Context) : BaseApiController
{
    readonly Dictionary<int, string>  areas = new ()
        {
            {1, "ירושלים" },
            {2, "מרכז" },
            {3, "יהודה ושומרון" },
            {4, "צפון" },
            {5, "שפלה" },
            {6, "דרום" },
        };

    [HttpPost]
    public async Task<ActionResult> Create(JobDto newJob)
    {
        var rec = (await GetRecInfo()).Value;
        if (rec is null)
            return BadRequest();

        Job newItem = new()
        {
            DateOfAdded = DateTime.Now,
            haveToar = newJob.haveToar,
            text = newJob.text,
            EnglishNeed = newJob.EnglishNeed,
            Salary = newJob.Salary,
            Area = newJob.Area,
        };

        rec.Jobs.Add(newItem);
        int affectedRows = Context.SaveChanges();
        return Ok(affectedRows > 0);
    }

    [HttpDelete("DeleteJob/{JobId}")]
    public async Task<bool> DeleteJob(int JobId)
    {
        var job = await Context.Jobs.FirstAsync(x => x.Id == JobId);
        job.Deleted = true;
        return await Context.SaveChangesAsync() > 0;
    }

    [HttpPut("FoundJob/{JobId}")]
    public async Task<bool> FoundJob(int JobId, [FromForm] bool found)
    {
        var job = await Context.Jobs.FirstAsync(x => x.Id == JobId);
        if (job.Found != found)
        {
            job.Found = found;
            if (found)
                job.FoundDate = DateTime.Now;
        }
        return await Context.SaveChangesAsync() > 0;
    }

    [HttpPut("UpdateJob/{JobId}")]
    public async Task<ActionResult<bool>> UpdateJob(int JobId, JobUpdateDto JobUpdate)
    {
        var job = await Context.Jobs.FirstAsync(x => x.Id == JobId);
        job.haveToar = JobUpdate.haveToar;
        job.EnglishNeed = JobUpdate.haveEnglish;
        job.text = JobUpdate.jobDetails;
        job.Salary = JobUpdate.salary;

        return (await Context.SaveChangesAsync() > 0) ? NoContent() : BadRequest("failed to update job");
    }



    [AllowAnonymous]
    [HttpGet("GetJobById/{JobId}")]
    public async Task<ActionResult<Job>> GetJobById(int JobId)
    {
        var job = await Context.Jobs.FirstOrDefaultAsync(x => x.Id == JobId && !x.Deleted && !x.Found);
        return (job is null) ? NotFound() : job;
    }

    [AllowAnonymous]
    [HttpGet("areas")]
    public async Task<ActionResult<JsonArray>> GetAreas()
    {
        return Ok(areas);
    }

    [AllowAnonymous]
    [HttpGet("GetJobs")]
    public async Task<ActionResult<List<Job>>> GetJobs(bool haveToar, int salary, bool haveEnglish, int area)
    {
        var salaryCond = Context.Jobs.Where(x => x.Salary >= salary && !x.Deleted && !x.Found);
        var res1 = (haveToar ? salaryCond : salaryCond.Where(x => !x.haveToar));
        var res2 = haveEnglish ? res1 : res1.Where(x => !x.EnglishNeed);
        var res3 = await (res2.Where(x => x.Area == area)).ToListAsync();


        // Add saved param, where is correct
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        res2.Where(x => user.SavedJobs.Contains(x.Id)).ToList().ForEach(x => x.IsSaved = true);


        return (res3.Count == 0) ? NotFound() : res3;
    }



    private async Task<ActionResult<Recruiter>> GetRecInfo()
    {
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await Context.Recruiters.Include(x => x.Jobs)
            .FirstOrDefaultAsync(x => x.RecName == userName && !x.Deleted);
    }
}