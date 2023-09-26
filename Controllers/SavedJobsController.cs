namespace MyJob.Controllers;

[Authorize(Roles = Roles.User)]
public class SavedJobsController(DataContext Context) : BaseApiController
{
    [HttpGet("as-ids")]
    public async Task<ActionResult<SortedSet<int>>> GetAllSavedJobIds()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        return user.SavedJobs;
    }

    [RequireHttps]
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






       
}
