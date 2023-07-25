namespace MyJob.DTOs;

public class ApplicantDto
{
    public int Id { get; set; }
    public DateTime Create { get; set; } = DateTime.Now;
    public string LinkedinLink { get; set; }
    public int CvId { get; set; }
    public int UserId { get; set; }
}