namespace MyJob.Controllers;

[Authorize]
public class RecsController : BaseApiController
{
    public DataContext Context { get; }
    public ITokenService TokenService { get; }

    public RecsController(DataContext context, ITokenService tokenService)
    {
        Context = context;
        TokenService = tokenService;
    }

    #region Register & login  
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRecDto registerDto)
    {
        if (await RecExist(registerDto.RecName))
            return BadRequest("username is taken");

        Recruiter user = new();
        registerDto.Adapt(user);

        using (HMACSHA512 hmac = new())
        {
            user.RecName = registerDto.RecName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
        }
        Context.Recruiters.Add(user);
        await Context.SaveChangesAsync();
        var ret = new UserDto
        {
            UserName = user.RecName,
            Token = TokenService.CreateToken(user.RecName, "user")
        };
        return Ok(ret);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await Context.Recruiters.
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
            Token = TokenService.CreateToken(user.RecName, "user")
        };
    }
    #endregion



    [HttpGet("Get-all-recs")]
    public async Task<ActionResult<List<object>>> GetAllRecs()
    {
        return Ok(await Context.Recruiters.Where(x => !x.Deleted).Select(x => new
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

    [HttpGet("Get-rec-Data")]
    public async Task<ActionResult> GetRecData()
    {
        var rec = (await GetRecInfo()).Value;
        return Ok(rec);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateRec(RecUpdateDto recUpdateDto)
    {
        var rec = (await GetRecInfo()).Value;
        recUpdateDto.Adapt(rec);
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to update rec.");
    }

    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        var rec = (await GetRecInfo()).Value;

        rec.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    private async Task<bool> RecExist(string username)
        => await Context.Recruiters.AnyAsync(x => string.Equals(x.RecName, username.ToLower()));
    
    private  async Task<ActionResult<Recruiter>> GetRecInfo()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await Context.Recruiters.FirstOrDefaultAsync(x => x.RecName == usName && !x.Deleted);

        if (user == null)
            return Unauthorized();
        return user;
    }
}
