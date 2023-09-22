using System.ComponentModel.DataAnnotations.Schema;

namespace MyJob.Entities;

public class Job
{
    public int Id { get; set; }
    public string text { get; set; }
    public DateTime DateOfAdded { get; set; }
    public int salary { get; set; }
    public bool haveToar { get; set; }
    public bool EnglishNeed { get; set; }
    public bool Found { get; set; }
    public int Area { get; set; }
    public DateTime FoundDate { get; set; } = DateTime.UnixEpoch;
    public bool Deleted { get; set; }

    // Just for return to user
    [NotMapped]
    public bool IsSaved { get; set; }

    public List<Applicant> Applicants { get; set; } = new();

    public Recruiter recruiter { get; set; }
    public int RecruiterId { get; set; }
}