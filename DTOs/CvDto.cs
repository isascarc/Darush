namespace MyJob.DTOs;
public class CvDto
{
    [Required][StringLength(50, MinimumLength = 1)] public string Name { get; set; }
    [Required] public IFormFile File { get; set; }
}