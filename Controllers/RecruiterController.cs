using System.Text.Json.Nodes;

namespace MyJob.Controllers;

// [aut]
public class RecsController : BaseApiController
{
    public DataContext _context { get; }
    public ITokenService _tokenService { get; }
    private readonly IMapper _mapper;

    public RecsController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
        _tokenService = tokenService;
    }

    #region Register & login   
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRecDto registerDto)
    {
        if (await UserExist(registerDto.RecName))
            return BadRequest("username is taken");

        var user = _mapper.Map<Recruiter>(registerDto);

        using var hmac = new HMACSHA512();
        user.RecName = registerDto.RecName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        user.PasswordSalt = hmac.Key;

        _context.Recruiters.Add(user);
        await _context.SaveChangesAsync();

        var ret = new JsonObject
        {
            { "UserName", user.RecName},
            { "Token", _tokenService.CreateTokenForRec(user) }
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

    [HttpGet("Get-rec-Data/{recId}")]
    public async Task<ActionResult> GetRecData(int recId)
    {
        var rec = await _context.Recruiters.FirstOrDefaultAsync(x => x.Id == recId);

        if (rec == null)
            return NotFound();
        return Ok(rec);
    }

    [HttpPut("Update-rec/{recId}")]
    public async Task<ActionResult> UpdateRec(int recId, RecUpdateDto recUpdateDto)
    {
        var rec = await _context.Recruiters.FirstOrDefaultAsync(x => x.Id == recId);
        if (rec == null)
            return NotFound();

        _mapper.Map(recUpdateDto, rec);
        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("failed to update user.");
    }


    [HttpDelete("delete/{recId}")]
    public async Task<ActionResult> Delete(int recId)
    {
        var rec = await _context.Recruiters.FirstOrDefaultAsync(x => (x.Id == recId) && (x.Deleted == false));

        if (rec == null)
            return NotFound();
        rec.Deleted = true;

        return (await _context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    public async Task<bool> UserExist(string username)
        => await _context.Recruiters.AnyAsync(x => x.RecName == username.ToLower()); 
}
