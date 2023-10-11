namespace MyJob.DTOs;

public class MemberUpdateDto
{
    [Required] public string UserName { get; set; }
    // update pass

    [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format")]
    [StringLength(50)]
    public string Mail { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public string WebsiteLink { get; set; }
    public bool ShowMe { get; set; }
    //todo:  public DateOnly DateOfBirth { get; set; }
}