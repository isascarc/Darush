namespace MyJob.DTOs;
public class SendMailDto
{
    [Required] public int jobNumber { get; set; }
    [StringLength(50)] public string emailAddress { get; set; }
}