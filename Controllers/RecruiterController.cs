using Microsoft.AspNetCore.Authorization;

namespace MyJob.Controllers;


public class RecsController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }

    public RecsController(DataContext context, ITokenService tokenService /*IMapper mapper*/)
    {
        _context = context;
        _tokenService = tokenService;
    }

    #region Register & login   
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRecDto registerDto)
    {
        if (await RecExist(registerDto.RecName))
            return BadRequest("username is taken");

        Recruiter user = new();// TODO: _mapper.Map<Recruiter>(registerDto);

        using (HMACSHA512 hmac = new())
        {
            user.RecName = registerDto.RecName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
        }
        _context.Recruiters.Add(user);
        await _context.SaveChangesAsync();
        var ret = new UserDto
        {
            UserName = user.RecName,
            Token = _tokenService.CreateTokenForRec(user)
        };
        return Ok(ret);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _context.Recruiters.
            FirstOrDefaultAsync(x => x.RecName == loginDto.UserName);
        if (user == null)
            return Unauthorized();

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        if (!computedHash.SequenceEqual(user.PasswordHash))
            return Unauthorized("invalid password!!");

        return new UserDto
        {
            UserName = user.RecName,
            Token = _tokenService.CreateTokenForRec(user)
        };
    }

    #endregion


    [Authorize]
    [HttpGet("Get-all-recs")]
    public async Task<ActionResult<List<object>>> GetAllRecs()
    {
        return Ok(await _context.Recruiters.Where(x => !x.Deleted).Select(x => new
        {
            x.Id,
            x.RecName,
            x.City,
            x.Create,
            x.Phone,
            x.Gender,
            x.LastActive,
            x.LinkedinLink,
            x.Mail,
            x.Deleted
        }).ToListAsync());
    }

    [Authorize]
    [HttpGet("Get-rec-Data")]
    public async Task<ActionResult> GetRecData()
    {
        var rec = (await GetRecInfo()).Value;
        return Ok(rec);
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult> UpdateRec(RecUpdateDto recUpdateDto)
    {
        var rec = (await GetRecInfo()).Value;
        recUpdateDto.Adapt(rec);
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to update rec.");
    }

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        var rec = (await GetRecInfo()).Value;

        rec.Deleted = true;
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    private async Task<bool> RecExist(string username)
        => await _context.Recruiters.AnyAsync(x => string.Equals(x.RecName, username, StringComparison.OrdinalIgnoreCase));


    
    private  async Task<ActionResult<Recruiter>> GetRecInfo()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _context.Recruiters.FirstOrDefaultAsync(x => x.RecName == usName && !x.Deleted);

        if (user == null)
            return Unauthorized();
        return user;
    }
}
