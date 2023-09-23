using MyJob.Models;

namespace MyJob.Controllers;

[Authorize(Roles = "user")]
public class ApplicantsController(DataContext Context, ITokenService TokenService) : BaseApiController
{
    [HttpPost("Create/{JobId}")]
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
