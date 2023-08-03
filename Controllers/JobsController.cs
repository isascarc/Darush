using Microsoft.AspNetCore.Authorization;

namespace MyJob.Controllers;


public class JobsController : BaseApiController
{
    public DataContext _context { get; }
    public JobsController(DataContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("create/{recId}")]
    public async Task<ActionResult> Create(int recId, JobDto newJob)
    {
        var rec = await _context.Recruiters.Include(x => x.Jobs).FirstOrDefaultAsync(x => x.Id == recId);

        if (rec == null)
            return NotFound();

        Job newItem = new()
        {
            DateOfAdded = DateTime.Now,
            haveToar = newJob.haveToar,
            text = newJob.jobDetails,
            EnglishNeed = newJob.haveEnglish,
            salary = newJob.salary
        };
        rec.Jobs.Add(newItem);
        int affectedRows = _context.SaveChanges();
        return Ok(affectedRows > 0);
    }

    [HttpGet("GetJobById")]
    public async Task<ActionResult<Job>> GetJobById(int JobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => (x.Id == JobId && (x.Deleted == false) && x.Found ==false ));
        return (job is null) ? NotFound() : job;
    }

    [HttpGet("GetJobs")]
    public async Task<ActionResult<List<Job>>> GetJobs(bool haveToar, int salary, bool haveEnglish)
    {
        var salaryCond = _context.Jobs.Where(x => x.salary >= salary && (x.Deleted == false) && x.Found == false);
        var res1 = (haveToar ? salaryCond : salaryCond.Where(x => x.haveToar == false));
        var res2 = await (haveEnglish ? res1 : res1.Where(x => x.EnglishNeed == false)).ToListAsync();
        return (res2.Count == 0) ? NotFound() : res2;
    }

    [Authorize]
    [HttpDelete("DeleteJob/{JobId}")]
    public async Task<bool> DeleteJobById(int JobId)
    {
        var job = await _context.Jobs.FirstAsync(x => x.Id == JobId);
        job.Deleted = true;
        return await _context.SaveChangesAsync() > 0;
    }

    [Authorize]
    [HttpPut("FoundJob/{JobId}")]
    public async Task<bool> FoundJob(int JobId, [FromForm] bool found)
    {
        var job = await _context.Jobs.FirstAsync(x => x.Id == JobId);
        if (job.Found != found)
        {
            job.Found = found;
            if (found)
                job.FoundDate = DateTime.Now;
        }
        return await _context.SaveChangesAsync() > 0;
    }

    [Authorize]
    [HttpPut("UpdateJob/{JobId}")]
    public async Task<ActionResult<bool>> UpdateJob(int JobId, JobUpdateDto JobUpdate)
    {
        var job = await _context.Jobs.FirstAsync(x => x.Id == JobId);
        job.haveToar = JobUpdate.haveToar;
        job.EnglishNeed = JobUpdate.haveEnglish;
        job.text = JobUpdate.jobDetails;
        job.salary = JobUpdate.salary;
        // Update in DB
        return (await _context.SaveChangesAsync() > 0) ? NoContent() : BadRequest("failed to update job");
    }
}