namespace MyJob.DTOs;

public class ApplicantDto
{
    public int Id { get; set; }
    public DateTime Create { get; set; } = DateTime.Now;
    public string LinkedinLink { get; set; }
    [Required] public int CvId { get; set; }
    [Required] public int UserId { get; set; }
}