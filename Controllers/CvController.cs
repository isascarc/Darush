using System.IO.Compression;
using ClosedXML.Excel;

namespace MyJob.Controllers;

[Authorize(Roles = Roles.User)]
[Route("user/cv")]
public class CvController(DataContext Context) : BaseApiController
{
    const int maxSizeInBytes = 100000;
    const int maxCVs = 5;
    readonly string[] SupportedFormats = new string[] { "doc", "docx", "pdf" };
    readonly Dictionary<string, string> Types = new()
    {
        {"doc","application/msword" },
        {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        {"pdf","application/pdf" }
    };
    const string zipFormat = "application/zip";
    const string excelFormat = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    
    [HttpGet]
    public async Task<ActionResult<List<object>>> GetAllCVsData()
    {
        var user = await UserFuncs.GetUserInfo(Context, User);
        return Ok(user.GetActualCvs().Select(x => new { x.IsDefault, x.Name, x.Text, x.DateOfAdded }));
    }

    [HttpGet("as-zip")]
    public async Task<ActionResult> GetAllCVs()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        var allCvs = user.GetActualCvs();

        if (allCvs.Count > 0)
        {
            var zipName = $"All cvs {DateTime.Now:yyyy-MM-dd HH.mm.ss}.zip";
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                allCvs.ForEach(file =>
                {
                    var entry = zip.CreateEntry(file.Name);
                    using var fileStream = new MemoryStream(file.FileContent);
                    using var entryStream = entry.Open();
                    fileStream.CopyTo(entryStream);
                });
            }
            return File(ms.ToArray(), zipFormat, zipName);
        }
        return BadRequest("You don't have a resume yet.");
    }

    [HttpGet("{CvId}")]
    public async Task<ActionResult> GetCV(int CvId)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        var cv = user.GetActualCvs().ElementAtOrDefault(CvId);

        if (cv is not null)
            return File(cv.FileContent, Types[cv.Text], $"{cv.Name}");

        return BadRequest("CV not exist");
    }

    [HttpGet("GetAllAsExcel")]
    public async Task<ActionResult> GetAllAsExcel()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        var allCvs = user.GetActualCvs();

        if (allCvs.Count > 0)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Data from My CV");
                var listOfArr = new List<object[]>();
                listOfArr.Add(new string[] { "שם הקובץ", "תאריך העלאה", });

                foreach (var cv in allCvs)
                {
                    listOfArr.Add(new object[]
                    {
                        cv.Name,
                        cv.DateOfAdded
                    });
                }
                ws.Cell(1, 1).InsertData(listOfArr);

                // Table style
                ws.Row(1).Style.Font.FontColor = XLColor.CoolBlack;
                ws.Columns().AdjustToContents();

                // Return file
                var spreadsheetStream = new MemoryStream();
                workbook.SaveAs(spreadsheetStream);
                spreadsheetStream.Position = 0;
                return File(spreadsheetStream, excelFormat, "All my cvs.xlsx");
            };
        }
        return BadRequest("CV not exist");
    }

    [HttpPut("set-as-default/{CvId}")]
    public async Task<ActionResult> SetCVAsDefault(int CvId)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);

        if (user.GetActualCvs().ElementAtOrDefault(CvId) is not null)
        {
            user.GetActualCvs().ForEach(x => x.IsDefault = false);
            user.GetActualCvs()[CvId].IsDefault = true;
        }
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }

    [HttpPut("cv-Change-Name/{CvId}")]
    public async Task<ActionResult> CVChangeName(int CvId, string newName)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);

        var allCvs = user.GetActualCvs();
        if (allCvs.Count > CvId)
        {
            if (allCvs.Any(x => x.Name == newName))
                return BadRequest("A file with the same name already exists in your resume list.");
            allCvs[CvId].Name = newName;
        }

        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    [HttpPost]
    public async Task<ActionResult> AddCV([FromForm] CvDto cv)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);

        // Check file
        if (cv.File is null || cv.File.Length < 1)
            return BadRequest("No file was uploaded.");
        if (cv.File.Length > maxSizeInBytes)
            return BadRequest($"File too large. The file must be up to {maxSizeInBytes / 1000} KB.");

        var extension = cv.File.FileName.Split(".").Last();
        if (!SupportedFormats.Contains(extension))
            return BadRequest("The system only accepts files in Word / PDF format.");


        // Validity check in DB
        if (user.CVs.Count(x => !x.Deleted) >= maxCVs)
            return BadRequest("It is not possible to add another file to your CV list.");
        if (user.CVs.Any(x => x.Name == cv.Name && !x.Deleted))
            return BadRequest("A file with the same name already exists in your resume list.");


        using (var stream = new MemoryStream())
        {
            await cv.File.CopyToAsync(stream);
            user.CVs.Add(new CV()
            {
                Name = cv.Name,
                FileContent = stream.ToArray(),
                Text = extension,
                IsDefault = !user.CVs.Any(x => !x.Deleted)
            });
        }
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem adding CV.");
    }

    [HttpDelete("{CvId}")]
    public async Task<ActionResult> DeleteCv(int CvId)
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);

        var cv = user.GetActualCvs().ElementAtOrDefault(CvId);
        if (cv is null)
            return BadRequest("CV not exist");


        cv.Deleted = true;
        return (await Context.SaveChangesAsync()) > 0 ? NoContent() : BadRequest("Problem occurred.");
    }


    [HttpGet("Applicants")]
    public async Task<ActionResult<List<object>>> GetAllApplicants()
    {
        var user = await UserFuncs.GetUserInfo(Context, User, true);
        var Applicants = await Context.Applicants.Where(x => x.UserId == user.Id).Take(100).Select(x => new
        {
            x.Create,
            x.CvId,
            x.Id,
            x.LinkedinLink,
            x.UserId,
        }).ToListAsync();

        return Ok(Applicants);
    }
}