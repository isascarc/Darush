using MimeKit;

namespace MyJob.Controllers;

[Authorize]
public class ShareController(DataContext Context) : BaseApiController
{
    const string ourEmailAddress = "isscr01@gmail.com";

    [HttpPost("Send-to-email")]
    public async Task<ActionResult<UserDto>> Create(SendMailDto emailDetails)
    {
        var user = (await GetUser());
        var job = Context.Jobs.First(x => x.Id == emailDetails.jobNumber);


        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Darush", ourEmailAddress));
        message.To.Add(new MailboxAddress(user.UserName, emailDetails.emailAddress.Length > 0 ? emailDetails.emailAddress : user.Mail));
        message.Subject = "יש לנו משרה חדשה עבורך!!";


        message.Body = new TextPart("plain")
        {
            Text = $"בקשת לשלוח את המשרה הזו אלייך למייל. מספר המשרה הוא: {emailDetails.jobNumber}. תוכן המשרה הוא: {job.text} בהצלחה!!"
        };

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate(ourEmailAddress,Helpers.Globals.GmailCode);
            var a = client.Send(message);
            client.Disconnect(true);
        }
        return Ok();
    }

    public async Task<AppUser> GetUser()
    {
        var usName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await Context.Users.Include(p => p.CVs).FirstOrDefaultAsync(x => x.UserName == usName && !x.Deleted);
    }
}
