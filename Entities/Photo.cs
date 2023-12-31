using System.ComponentModel.DataAnnotations.Schema;

namespace MyJob.Entities;

[Table("Photos")]
public class Photo
{
    public int Id { get; set; }
    public string url { get; set; }
    public bool isMain { get; set; }
    public string PublicId { get; set; }
    public AppUser AppUser  { get; set; }
    public int AppUserId { get; set; }
}