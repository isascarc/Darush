namespace MyJob.DTOs;

public class RegisterDto
{
    [Required] public string username { get; set; }
    public string KnownAs { get; set; }
    public string Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string City { get; set; }
    public string Country { get; set; }


    [StringLength(8, MinimumLength = 4)]
    [Required] public string password { get; set; }
}