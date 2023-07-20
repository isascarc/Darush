using MyJob.Helpers;
using MyJob.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyJob.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyJob.DTOs;

namespace MyJob.Controllers;


public class UsersController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }
    private readonly IMapper _mapper;


    Dictionary<string, string> Types = new()
        {
            {"doc","application/msword" },
            {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            {"pdf","application/pdf" }
        };

    public UsersController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpGet("Get-all-cvs")]
    public async Task<ActionResult<List<CV>>> GetAllCVs()
    {
        // Check user
        var user = await GetUser();
        if (user == null)
            return NotFound();
        var a =   user.CVs.Where(x => !x.Deleted).ToList();
        return Ok(a);
    }

    [HttpGet("Get-cv/{CvId}")]
    public async Task<ActionResult> GetCV(int CvId)
    {
        // Check user
        var user = await GetUser();

        if (user == null)
            return NotFound();

        if (user.CVs.Count(x => !x.Deleted) > CvId)
            return new FileContentResult(user.CVs[CvId].FileContent, Types["docx"])
            // Types[user.CVs[CvId].FileContent.FileName.Split(".").Last()])
            {
                FileDownloadName = user.CVs[CvId].Name
            };
        return BadRequest("CV not exist");
    }

    [HttpPut("set-cv-as-default/{CvId}")]
    public async Task<ActionResult> SetCVAsDefault(int CvId)
    {
        var user = await GetUser();
        if (user == null)
            return NotFound();

        if (user.CVs.Count > CvId)
        {
            user.CVs.ForEach(x => x.IsDefault = false);
            user.CVs[CvId].IsDefault = true;
        }
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    [HttpPost("add-cv")]
    public async Task<ActionResult> AddCV([FromForm] CvDto cv)
    {
        const int maxSizeInBytes = 100000;
        const int maxCVs = 8;
        string[] SupportedFormats = new string[] { "doc", "docx" };

        // Check user
        var user = await GetUser();
        if (user == null)
            return NotFound();

        // Check file
        if (cv.File == null || cv.File.Length == 0)
            return BadRequest("No file was uploaded.");
        if (cv.File.Length > maxSizeInBytes)
            return BadRequest("File too large. The file must be up to 100 KB.");
        if (!SupportedFormats.Contains(cv.File.FileName.Split(".").Last()))
            return BadRequest("The system only accepts files in Word format.");

        // Check capacity
        if (user.CVs.Count >= maxCVs)
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
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem adding CV.");
    }

    [HttpDelete("delete-cv/{CvId}")]
    public async Task<ActionResult> DeleteCv(int CvId)
    {
        // Check user
        var user = await GetUser();
        if (user == null)
            return NotFound();


        if (user.CVs.Count(x => !x.Deleted) > CvId)
            user.CVs[CvId].Deleted = true;
        
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");       
    }

    public async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName);
    }
}