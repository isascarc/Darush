using MyJob.Models;

namespace MyJob.Controllers;


// This class exist for Admin authorize
[ApiController]
[Route("account")]
[Authorize(Roles = Roles.Admin)]
public class AccountControllerForAdmin : ControllerBase
{
    public DataContext Context { get; }
    public AccountControllerForAdmin(DataContext context)
    {
        Context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<object>>> GetAllUsers()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        return Ok(await Context.Users.Where(x => !x.Deleted).ToListAsync());
    }
}


[Authorize(Roles = Roles.User)]
public class AccountController : BaseApiController
{
    public DataContext Context { get; }
    public ITokenService TokenService { get; }

    public AccountController(DataContext context, ITokenService tokenService)
    {
        Context = context;
        TokenService = tokenService;
    }


    #region Register & login
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserFuncs.UserExist(Context, registerDto.username))
            return BadRequest("username is taken");

        var user = registerDto.Adapt<AppUser>();

        using var hmac = new HMACSHA512();

        user.UserName = registerDto.username.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password));
        user.PasswordSalt = hmac.Key;

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        return new UserDto
        {
            UserName = user.UserName,
            Token = TokenService.CreateToken(user.UserName, "user"),
            KnownAs = user.KnownAs
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await Context.Users.Include(x => x.CVs).
           FirstOrDefaultAsync(x => x.UserName == loginDto.UserName && !x.Deleted);
        if (user is null)
            return Unauthorized("User not exist!!");


        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        if (!computedHash.SequenceEqual(user.PasswordHash))
            return Unauthorized("invalid password!!");

        return new UserDto
        {
            UserName = user.UserName,
            Token = TokenService.CreateToken(user.UserName, "user"),
            KnownAs = user.KnownAs
        };
    }
    #endregion

    [HttpGet("user-data")]
    public async Task<ActionResult> GetUserData()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        return Ok(user);
    }

    [HttpPut("Update-User")]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);
        var isChangeName = memberUpdateDto.UserName == user.UserName;

        memberUpdateDto.Adapt(user);

        // Send new token if userName changed
        return (await Context.SaveChangesAsync()) > 0 ?
            (isChangeName ?
                Ok(new UserDto { UserName = user.UserName, Token = TokenService.CreateToken(user.UserName, "user"), KnownAs = user.KnownAs })
                :
                NoContent())
            : BadRequest("failed to update user.");
    }

    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, false);

        user.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }
}