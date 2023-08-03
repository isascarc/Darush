using System.ComponentModel.DataAnnotations.Schema;

namespace MyJob.Entities;

[Table("Applicants")]
public class Applicant
{
    public int Id { get; set; }
    public DateTime Create { get; set; } = DateTime.Now;
    public string LinkedinLink { get; set; }
    public int CvId { get; set; }
    public bool Deleted { get; set; }



    public int UserId { get; set; }
    public Job Job { get; set; }
    public int JobId { get; set; }
}