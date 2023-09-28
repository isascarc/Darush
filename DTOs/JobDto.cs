namespace MyJob.DTOs;
public class JobDto
{
    public int Id { get; set; }
    public int Salary { get; set; }
    public bool  EnglishNeed { get; set; }
    public string Profession { get; set; } = "";
    public string Toppings { get; set; } = "";
    public bool haveToar { get; set; }
    public string text { get; set; }
    public int Area { get; set; }
    public DateTime DateOfAdded { get; set; }
    // public string other { get; set; }
}
