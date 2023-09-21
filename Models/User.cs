namespace MyJob.Models;

public static class UserFuncs
{
    public static List<CV> GetActualCvs(this AppUser user)
       => user.CVs.Where(x => !x.Deleted).ToList();

    public static async Task<bool> UserExist(DataContext context, string username)
    {
        return await context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }

    public static async Task<AppUser> GetUserInfo(DataContext context, ClaimsPrincipal user, bool withCvs = true)
    {
        var userName = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return withCvs ?
            await context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == userName && !x.Deleted)
            :
            await context.Users.FirstOrDefaultAsync(x => x.UserName == userName && !x.Deleted);
    }
}
