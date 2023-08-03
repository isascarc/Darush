namespace MyJob.Controllers;

// [aut]
public class ApplicantsController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }
    private readonly IMapper _mapper;

    public ApplicantsController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
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
        return Ok(await _context.Applicants.Select(x => new
        {
            x.Id,
            x.Create,
            x.LinkedinLink,
            x.CvId,
        }).ToListAsync());
    }

    [HttpGet("Get-User-Data/{UserId}")]
    public async Task<ActionResult> GetUserData(int UserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == UserId);

        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpPut("Update-User/{UserId}")]
    public async Task<ActionResult> UpdateUser(int UserId, MemberUpdateDto memberUpdateDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
        if (user == null)
            return NotFound();

        _mapper.Map(memberUpdateDto, user);
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to update user.");
    }

    //[HttpPut("cv-Change-Name/{CvId}")]
    //public async Task<ActionResult> CVChangeName(int CvId, string newName)
    //{
    //    var user = await GetUser();
    //    if (user == null)
    //        return NotFound();

    //    if (GetAllActualCv(user).Count > CvId)
    //        GetAllActualCv(user)[CvId].Name = newName;

    //    return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    //}


    [HttpDelete("delete/{UserId}")]
    public async Task<ActionResult> Delete(int UserId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => (x.Id == UserId) && (x.Deleted == false));

        if (user == null)
            return NotFound();
        user.Deleted = true;

        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    public async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName);
    }
}
