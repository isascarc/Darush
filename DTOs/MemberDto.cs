namespace MyJob.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PhotoUrl { get; set; }
    public int Age { get; set; }
    public string KnownAs { get; set; }
    public DateTime Create { get; set; }
    public DateTime LastActive { get; set; }
    public string Gender { get; set; }
    public string Introdduction { get; set; }
    public string LookingFor { get; set; }
    public string Interest { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    //public List<PhotoDto> Photos { get; set; }
}