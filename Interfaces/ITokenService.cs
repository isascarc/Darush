namespace MyJob.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
    string CreateTokenForRec(Recruiter user);
}
