using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading.Channels;
using static System.Runtime.InteropServices.JavaScript.JSType;

[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly Channel<FileUploadTask> _uploadChannel;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ConcurrentDictionary<string, string> _statusMap;

    public UploadController(Channel<FileUploadTask> uploadChannel, IConfiguration config,
                            IWebHostEnvironment env, ConcurrentDictionary<string, string> statusMap)
    {
        _uploadChannel = uploadChannel;
        _config = config;
        _env = env;
        _statusMap = statusMap;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // check if the file length is greater than 10 * 1024 * 1024
        //return BadRequest("File too large.");

        // Rate Limiting Check
        //var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        //  if (IsRateLimitExceeded(ip))
        //return "Rate limit exceeded. Please try again later."

        //  Executable File Check
        // if (IsExecutableFile(file))
        //return "Executable files are not allowed."

        // Generate UniqueID for tracking
        var id = Guid.NewGuid().ToString();
        // SanitizeFileName

        //Read all the file content here

        UploadStatusTracker.StatusMap[id] = "Pending";

        //Write the file to the stream so that it can be trackable
        await _uploadChannel.Writer.WriteAsync(new FileUploadTask
        {
            //ProcessingId = id,
            //FileContent = //Content of the file,
            //OriginalFileName = sanitized,
            //SimulateScan = //Read key "SimulateAntivirusScan" from appsettings to enable and disable the scan,
            //ScanDelayMs = //Read key "ScanDelayMilliseconds" from appsettings to put the delay to simulate the virus scan,
            //StoragePath = Path.Combine(_env.WebRootPath, "uploads") // Set the storage path
        });

        return Ok(new { processingId = id });
    }


    [HttpGet("status/{id}")]
    public IActionResult Status(string id)
    {
        //if (!_statusMap.TryGetValue(id, out var status))
        //    return NotFound("Invalid ID");
        //return Ok(new { status });
    }

    private static readonly Dictionary<string, List<DateTime>> UploadLog = new();

    private bool IsRateLimitExceeded(string ip, int maxUploads = 5, int intervalSeconds = 60)
    {
        //Implement the logic that the request should not exceel the maxUploads under the intervalSeconds
    }

    private bool IsExecutableFile(IFormFile file)
    {
        //using var reader = new BinaryReader(file.OpenReadStream());
        //var headerBytes = reader.ReadBytes(4);
        //return headerBytes.Take(2).SequenceEqual(new byte[] { 0x4D, 0x5A });
    }

    private string SanitizeFileName(string fileName)
    {
       // .Replace("..", "").Replace("//", "").Replace("\\", "").Replace(":", "");
    }
}