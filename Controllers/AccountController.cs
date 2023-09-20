using MimeKit;

namespace MyJob.Controllers;

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
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register2(RegisterDto2 registerDto)
    {
        return await Register(new RegisterDto() { username = registerDto.username, password = registerDto.password });


        //if (await UserExist(registerDto.username))
        //    return BadRequest("username is taken");

        //var user = _mapper.Map<AppUser>(registerDto);
        //using var hmac = new HMACSHA512();

        //user.UserName = registerDto.username.ToLower();
        //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password));
        //user.PasswordSalt = hmac.Key;

        //_context.Users.Add(user);
        //await _context.SaveChangesAsync();

        //return new UserDto
        //{
        //    UserName = user.UserName,
        //    Token = _tokenService.CreateToken(user),
        //    KnownAs = user.KnownAs
        //};
    }
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExist(registerDto.username))
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

    [HttpGet("Get-all-users")]
    public async Task<ActionResult<List<object>>> GetAllUsers()
    {
        var user = await GetUserInfo(false);
        return Ok(await Context.Users.Where(x => !x.Deleted).ToListAsync());
    }

    [HttpGet("Get-User-Data")]
    public async Task<ActionResult> GetUserData()
    {
        var x = await GetUserInfo(false);
        return Ok(x);
    }

    [HttpPut("Update-User")]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = (await GetUserInfo());
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
        var user = (await GetUserInfo());

        user.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    [HttpGet("send-email")]
    public async Task<ActionResult> Sendemail()
    {
        var user = (await GetUserInfo());

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Darush", "isascarch@gmail.com"));
        message.To.Add(new MailboxAddress(user.UserName, user.Mail));
        message.Subject = "How you doin'?";


        message.Body = new TextPart("plain")
        {
            Text = "Hey Chandler,I just wanted to let you know that Monica and I were going to go play some paintball, you in?"
        };

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("isscr01@gmail.com", Helpers.Globals.GmailCode);
            var a = client.Send(message);
            client.Disconnect(true);
        }
        return Ok();
    }

    public async Task<bool> UserExist(string username)
    {
        return await Context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }

    public async Task<AppUser> GetUserInfo(bool withCvs = true)
    {
        var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = withCvs ?
            await Context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == userName && !x.Deleted)
            :
            await Context.Users.FirstOrDefaultAsync(x => x.UserName == userName && !x.Deleted);

        return user;
    }
}