using System.Security.Cryptography;
using System.Text;
using MyJob.DTOs;
using AutoMapper;
//using CloudinaryDotNet;
//using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using MyJob.Interfaces;
using MyJob.Entities;

namespace MyJob.Controllers;

public class AccountController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }
    private readonly IMapper _mapper;

    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
        _tokenService = tokenService;
    }


    #region Register & login   
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {

        if (await UserExist(registerDto.username))
            return BadRequest("username is taken");

        var user = _mapper.Map<AppUser>(registerDto);
        using var hmac = new HMACSHA512();

        user.UserName = registerDto.username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password));
        user.PasswordSalt = hmac.Key;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            UserName = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Users.Include(x=>x.CVs ).
            FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);
        if (user == null)
            return Unauthorized();
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        if (!computedHash.SequenceEqual(user.PasswordHash))
            return Unauthorized("invalid password!!");
       
        return new UserDto
        {
            UserName = user.UserName ,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    #endregion

    // [aut]
    [HttpGet("Get-all-users")]
    public async Task<ActionResult<List<object>>> GetAllUsers()
    {
        return Ok(await  _context.Users.Select(x => new { x.UserName , x.City, x.Create,x.Phone }).ToListAsync ());
    }

    //[HttpGet("Get-cv/{CvId}")]
    //public async Task<ActionResult> GetCV(int CvId)
    //{
    //    // Check user   
    //    var user = await GetUser();

    //    if (user == null)
    //        return NotFound();

    //    var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);
    //    return (cv is null) ?
    //        BadRequest("CV not exist")
    //        :
    //        new FileContentResult(cv.FileContent, Types["docx"])
    //        // ToDo: Types[user.CVs[CvId].FileContent.FileName.Split(".").Last()])
    //        {
    //            FileDownloadName = cv.Name
    //        };
    //}


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


    //[HttpDelete("delete-cv/{CvId}")]
    //public async Task<ActionResult> DeleteCv(int CvId)
    //{
    //    // Check user
    //    var user = await GetUser();
    //    if (user == null)
    //        return NotFound();


    //    var cv = GetAllActualCv(user).ElementAtOrDefault(CvId);
    //    if (cv is null)
    //        return BadRequest("CV not exist");

    //    // delete
    //    cv.Deleted = true;
    //    return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    //}








    public async Task<bool> UserExist(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
