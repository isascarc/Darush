namespace MyJob.Interfaces;

public interface ITokenService
{
    //string CreateToken(AppUser user);
    string CreateToken(string user);
    //string CreateTokenForRec(Recruiter user);
}
