using System.ComponentModel.DataAnnotations.Schema;

namespace MyJob.Entities;

[Table("CVs")]
public class CV
{
    public int Id { get; set; }
    public string Text { get; set; }
    public byte[] FileContent { get; set; } // עמודה לאחסון תוכן הקובץ בצורה של מערך ביטים


    public DateTime DateOfAdded { get; set; } = DateTime.Now;
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public bool Deleted { get; set; }

    public AppUser AppUser { get; set; }
    public int AppUserId { get; set; }
}