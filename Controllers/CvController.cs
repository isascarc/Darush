using Microsoft.AspNetCore.Authorization;

namespace MyJob.Controllers;


[Route("user/cv")]
public class CvController : BaseApiController
{
    public DataContext Context { get; }
    public ITokenService TokenService { get; }
    private readonly IMapper _mapper;

    readonly Dictionary<string, string> Types = new()
    {
        {"doc","application/msword" },
        {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {"pdf","application/pdf" }
    };

    const int maxSizeInBytes = 100000;
    const int maxCVs = 5;
    readonly string[] SupportedFormats = new string[] { /*"doc",*/ "docx" };

    public CvController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
        Context = context;
        TokenService = tokenService;
    }


    [HttpGet("Get-all")]
    public async Task<ActionResult<List<object>>> GetAllCVs()
    {
        var user = (await GetUserInfo()).Value;

        return Ok(GetAllActualCv(user).Select(x => new { x.IsDefault, x.Name, x.DateOfAdded }).ToList());
    }

    [HttpGet("{CvId}")]
    public async Task<ActionResult> GetCV(int CvId)
    {
        var user = (await GetUserInfo()).Value;

        var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);
        return (cv is null) ?
            BadRequest("CV not exist")
            :
            new FileContentResult(cv.FileContent, Types["docx"])
            // ToDo: Types[user.CVs[CvId].FileContent.FileName.Split(".").Last()])
            {
                FileDownloadName = cv.Name
            };
    }

    [HttpPut("set-cv-as-default/{CvId}")]
        public async Task<ActionResult> SetCVAsDefault(int CvId)
        {
        var user = await GetUser();
        if (user == null)
            return NotFound();

        if (GetAllActualCv(user).ElementAtOrDefault(CvId) is not null)
        {
            GetAllActualCv(user).ForEach(x => x.IsDefault = false);
            GetAllActualCv(user)[CvId].IsDefault = true;
        }
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    [HttpPut("cv-Change-Name/{CvId}")]
    public async Task<ActionResult> CVChangeName(int CvId, string newName)
    {
        var user = (await GetUserInfo()).Value;

        if (GetAllActualCv(user).Count > CvId)
            GetAllActualCv(user)[CvId].Name = newName;
        StringCollection f = new StringCollection();
        f.Add()
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    [HttpPost("add")]
    public async Task<ActionResult> AddCV([FromForm] CvDto cv)
    {
        var user = (await GetUserInfo()).Value;

        // Check file
        if (cv.File == null || cv.File.Length == 0)
            return BadRequest("No file was uploaded.");
        if (cv.File.Length > maxSizeInBytes)
            return BadRequest("File too large. The file must be up to 100 KB.");
        if (!SupportedFormats.Contains(cv.File.FileName.Split(".").Last()))
            return BadRequest("The system only accepts files in Word format.");

        // Check capacity
        if (user.CVs.Count(x => !x.Deleted) >= maxCVs)
            return BadRequest("It is not possible to add another file to your CV list.");


        using (var stream = new MemoryStream())
        {
            await cv.File.CopyToAsync(stream);
            user.CVs.Add(new CV()
            {
                Name = cv.Name,
                FileContent = stream.ToArray(),
                IsDefault = !user.CVs.Any(x => !x.Deleted)
            });
        }
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem adding CV.");
    }

    [HttpDelete("{CvId}")]
    public async Task<ActionResult> DeleteCv(int CvId)
    {
        // Check user
        var user = await GetUser();
        if (user == null)
            return NotFound();


        var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);
        if (cv is null)
            return BadRequest("CV not exist");

        // delete
        cv.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }



    [Authorize]
    [HttpGet("Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants()
    {
        // Check user
        var user = await GetUser();
        if (user == null)
            return Unauthorized();

        var Applicants = await Context.Applicants.Where(x => x.UserId == user.Id).Take(100).Select(x => new
        {
            x.Create,
            x.CvId,
            x.Id,
            x.LinkedinLink,
            x.UserId,
        }).ToListAsync();

        return Ok(Applicants);
    }


    public async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await Context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName);
    }

    public List<CV> GetAllActualCv(AppUser user)
    {
        return user.CVs.Where(x => !x.Deleted).ToList();
    }

    async Task<ActionResult<AppUser>> GetUserInfo()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await Context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName && !x.Deleted);

        if (user == null)
            return Unauthorized();
        return user;
    }
}