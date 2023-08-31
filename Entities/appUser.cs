using System.ComponentModel.DataAnnotations;

namespace MyJob.Entities;

public class AppUser
{
    [Required] public int Id { get; set; }
    [StringLength(8, MinimumLength = 4)][Required] public string UserName { get; set; }
    [Required] public byte[] PasswordHash { get; set; }
    [Required] public byte[] PasswordSalt { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string KnownAs { get; set; }
    [Required] public DateTime Create { get; set; } = DateTime.UtcNow;
    [Required] public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string Gender { get; set; }        
    public string City { get; set; }
    public string Mail { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public string WebsiteLink { get; set; }
    public bool Deleted { get; set; } = false;

    public List<CV> CVs { get; set; } = new();
}