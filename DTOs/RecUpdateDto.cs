using System.ComponentModel.DataAnnotations;

namespace MyJob.DTOs;

public class RecUpdateDto
{
    public string RecName { get; set; }
    public string Mail { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public string Gender { get; set; }

    public string InShort { get; set; }
    public string LogoProfile { get; set; }
    // update pass
}