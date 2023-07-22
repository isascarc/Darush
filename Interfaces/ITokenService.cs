using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyJob.Entities;

namespace MyJob.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
    string CreateTokenForRec(Recruiter user);
}
