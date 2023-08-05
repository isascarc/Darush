namespace MyJob.Controllers;

// [aut]
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
    public async Task<ActionResult<UserDto>> Register2(RegisterDto2 registerDto)
    {
        return await Register(new RegisterDto() { username = registerDto.username , password = registerDto.password });
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
        var user = await _context.Users.Include(x => x.CVs).
            FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);
        if (user == null)
            return Unauthorized();
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        if (!computedHash.SequenceEqual(user.PasswordHash))
            return Unauthorized("invalid password!!");

        return new UserDto
        {
            UserName = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs
        };
    }

    #endregion


    [HttpGet("Get-all-users")]
    public async Task<ActionResult<List<object>>> GetAllUsers()
    {
        return Ok(await _context.Users.Where(x => !x.Deleted).Select(x => new
        {
            x.Id,
            x.UserName,
            x.City,
            x.Create,
            x.Phone,
            x.DateOfBirth,
            x.Gender,
            x.LastActive,
            x.LinkedinLink,
            x.Mail,
            x.WebsiteLink,
            x.Deleted
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



    public async Task<bool> UserExist(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
}
