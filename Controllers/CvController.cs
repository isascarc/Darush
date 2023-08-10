using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJob.Entities;

namespace MyJob.Controllers;

[Authorize]
[Route("user/cv")]
public class CvController : BaseApiController
{
    public DataContext Context { get; }
    public ITokenService TokenService { get; }
    private readonly IMapper _mapper;

    const int maxSizeInBytes = 100000;
    const int maxCVs = 5;
    readonly string[] SupportedFormats = new string[] { "doc", "docx", "pdf" };
    readonly Dictionary<string, string> Types = new()
    {
        {"doc","application/msword" },
        {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {"pdf","application/pdf" }
    };



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

        return Ok(GetAllActualCv(user).Select(x => new { x.IsDefault, x.Name,x.Text, x.DateOfAdded }).ToList());
    }

    [HttpGet("{CvId}")]
    public async Task<ActionResult> GetCV(int CvId)
    {
        var user = (await GetUserInfo()).Value;
        var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);

        if (cv is not null)
            return new FileContentResult(cv.FileContent, Types[cv.Text]) { FileDownloadName = $"{cv.Name}.{cv.Text}" };

        return BadRequest("CV not exist");
    }

    [HttpPut("set-cv-as-default/{CvId}")]
    public async Task<ActionResult> SetCVAsDefault(int CvId)
    {
        var user = (await GetUserInfo()).Value;

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
        {
            if (GetAllActualCv(user).Any(x => x.Name == newName))
                return BadRequest("A file with the same name already exists in your resume list.");
            GetAllActualCv(user)[CvId].Name = newName;
        }

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    [HttpPost("add")]
    public async Task<ActionResult> AddCV([FromForm] CvDto cv)
    {
        var user = (await GetUserInfo()).Value;

        // Check file
        if (cv.File is null || cv.File.Length < 1)
            return BadRequest("No file was uploaded.");
        if (cv.File.Length > maxSizeInBytes)
            return BadRequest($"File too large. The file must be up to {maxSizeInBytes / 1000} KB.");

        var extension = cv.File.FileName.Split(".").Last();
        if (!SupportedFormats.Contains(extension))
            return BadRequest("The system only accepts files in Word / PDF format.");


        // Validity check in DB
        if (user.CVs.Count(x => !x.Deleted) >= maxCVs)
            return BadRequest("It is not possible to add another file to your CV list.");
        if (user.CVs.Any(x => x.Name == cv.Name))
            return BadRequest("A file with the same name already exists in your resume list.");


        using (var stream = new MemoryStream())
        {
            await cv.File.CopyToAsync(stream);
            user.CVs.Add(new CV()
            {
                Name = cv.Name,
                FileContent = stream.ToArray(),
                Text = extension,
                IsDefault = !user.CVs.Any(x => !x.Deleted)
            });
        }
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem adding CV.");
    }

    [HttpDelete("{CvId}")]
    public async Task<ActionResult> DeleteCv(int CvId)
    {
        var user = (await GetUserInfo()).Value;

        var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);
        if (cv is null)
            return BadRequest("CV not exist");


        cv.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }



    [Authorize]
    [HttpGet("Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants()
    {
        var user = (await GetUserInfo()).Value;

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