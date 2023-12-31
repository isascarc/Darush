using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MyJob.Entities;

public class AppUser
{
    [Required] public int Id { get; set; }
    [StringLength(8, MinimumLength = 4)][Required] public string UserName { get; set; }
    [Required] public byte[] PasswordHash { get; set; }
    [Required] public byte[] PasswordSalt { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string KnownAs { get; set; }
    [Required] public DateTime Create { get; set; } = DateTime.UtcNow;
    [Required] public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string Gender { get; set; }
    public string City { get; set; }
    public string Mail { get; set; }
    public string Phone { get; set; }
    public string LinkedinLink { get; set; }
    public string WebsiteLink { get; set; }
    public bool ShowMe { get; set; } = true;
    public bool Deleted { get; set; } = false;

    [Column("SavedJobs")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public string _savedJobs { get; set; }

    [NotMapped]
    public SortedSet<int> SavedJobs
    {
        get { return JsonSerializer.Deserialize< SortedSet<int>>(_savedJobs); }
        set { _savedJobs = JsonSerializer.Serialize(value); }
    }

    public List<CV> CVs { get; set; } = new();
}