namespace MyJob.Controllers;

[Authorize(Roles = "user")]
public class ApplicantsController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }

    public ApplicantsController(DataContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("Create/{JobId}")]
    public async Task<ActionResult<UserDto>> Create(int JobId)
    {
        // Check user
        var user = await GetUser();
        if (user == null)
            return Unauthorized();
        if (user.CVs.Count < 1)
            return NotFound("Please upload a resume.");

        var job = await _context.Jobs.Include(x => x.Applicants).FirstOrDefaultAsync(x => x.Id == JobId);
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

        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to Applicant user.");
    }

    [HttpGet("Get-all-Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants()
    {
        return Ok(await _context.Applicants.Where(x => !x.Deleted).Select(x => new
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
        var user = await GetUser();
        if (user == null)
            return Unauthorized();

        var Applicant = await _context.Applicants.Where(x => x.Id == ApplicantId && x.UserId == user.Id && !x.Deleted).FirstOrDefaultAsync();
        if (Applicant == null)
            return NotFound("Applicant not exist.");

        Applicant.Deleted = true;

        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    public async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName && !x.Deleted);
    }
}
