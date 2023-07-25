namespace MyJob.Entities;

public class Recruiter
{
    public int Id { get; set; }
    public string RecName { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime Create { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string Gender { get; set; }
    public string InShort { get; set; }
    public string LogoProfile { get; set; }
    public string City { get; set; }
    public string Mail { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public bool Deleted { get; set; } = false;

    public List<Job> Jobs { get; set; }
}