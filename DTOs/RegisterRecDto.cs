using System.ComponentModel.DataAnnotations;

namespace MyJob.DTOs
{
    public class RegisterRecDto
    {
        [Required] public string RecName { get; set; }        
        [Required] public string Gender { get; set; }
        [Required] public DateOnly? DateOfBirth { get; set; }
        [Required] public string City { get; set; }
        [Required] public string Country { get; set; }
        [Required] public string Mail { get; set; }
        [Required] public string Phone { get; set; }
        [Required] public string LinkedinLink { get; set; }

        [Required][StringLength(8, MinimumLength = 4)]
        public string Password { get; set; }



        
            
       
         
    }
}