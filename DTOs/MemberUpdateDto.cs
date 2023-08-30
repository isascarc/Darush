namespace MyJob.DTOs;

public class MemberUpdateDto
{
    public string UserName { get; set; }
    // update pass
    
    public string Mail { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public string WebsiteLink { get; set; }
    //todo:  public DateOnly DateOfBirth { get; set; }
}