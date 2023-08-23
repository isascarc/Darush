using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using RestSharp.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyJob.Controllers;


public class JobsController : BaseApiController
{
    public DataContext _context { get; }
    public JobsController(DataContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Create(JobDto newJob)
    {
        var rec = (await GetRecInfo()).Value;
        if (rec is null)
            return BadRequest();
         
        Job newItem = new()
        {
            DateOfAdded = DateTime.Now,
            haveToar = newJob.HaveToar,
            text = newJob.JobDetails,
            EnglishNeed = newJob.HaveEnglish,
            salary = newJob.Salary,
            Area = newJob.Area,
        };
        
        rec.Jobs.Add(newItem);
        int affectedRows = _context.SaveChanges();
        return Ok(affectedRows > 0);
    }

    [HttpGet("GetJobById/{JobId}")]
    public async Task<ActionResult<Job>> GetJobById(int JobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(x => x.Id == JobId && !x.Deleted && !x.Found);
        return (job is null) ? NotFound() : job;
    }

    [HttpGet("GetJobs")]
    public async Task<ActionResult<List<Job>>> GetJobs(bool haveToar, int salary, bool haveEnglish)
    {
        var salaryCond = _context.Jobs.Where(x => x.salary >= salary && !x.Deleted && !x.Found);
        var res1 = (haveToar ? salaryCond : salaryCond.Where(x => !x.haveToar));
        var res2 = await (haveEnglish ? res1 : res1.Where(x => !x.EnglishNeed)).ToListAsync();
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

        return (await _context.SaveChangesAsync() > 0) ? NoContent() : BadRequest("failed to update job");
    }

    [Authorize]
    [HttpGet("{JobId}/Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants(int JobId)
    {
        var user = await GetUser();

        // Todo: Add auth for rec
        var job = await _context.Jobs.Include(x => x.Applicants).FirstAsync(x => x.Id == JobId && !x.Deleted);

        return Ok(job.Applicants.Select(x => new
        {
            x.Create,
            x.CvId,
            x.Id,
            x.LinkedinLink,
            x.UserId,
        }));
    }
    private  async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName && !x.Deleted);
    }

    private  async Task<ActionResult<Recruiter>> GetRecInfo()
    {
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Recruiters.Include(x => x.Jobs)
            .FirstOrDefaultAsync(x => x.RecName == userName && !x.Deleted);
    }
}