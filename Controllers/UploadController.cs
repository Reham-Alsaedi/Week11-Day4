using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Threading.Channels;

[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly Channel<FileUploadTask> _uploadChannel;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ConcurrentDictionary<string, string> _statusMap;

    private static readonly ConcurrentDictionary<string, List<DateTime>> UploadLog = new();

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
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("File too large.");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (IsRateLimitExceeded(ip))
            return BadRequest("Rate limit exceeded. Please try again later.");

        if (IsExecutableFile(file))
            return BadRequest("Executable files are not allowed.");

        var id = Guid.NewGuid().ToString();
        var sanitizedFileName = SanitizeFileName(file.FileName);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var content = ms.ToArray();

        _statusMap[id] = "Pending";

        await _uploadChannel.Writer.WriteAsync(new FileUploadTask
        {
            ProcessingId = id,
            FileContent = content,
            OriginalFileName = sanitizedFileName,
            SimulateScan = _config.GetValue<bool>("FileUploadSettings:SimulateAntivirusScan"),
            ScanDelayMs = _config.GetValue<int>("FileUploadSettings:ScanDelayMilliseconds"),
            StoragePath = Path.Combine(_env.WebRootPath, "uploads")
        });

        return Ok(new { processingId = id });
    }

    [HttpGet("status/{id}")]
    public IActionResult Status(string id)
    {
        if (!_statusMap.TryGetValue(id, out var status))
            return NotFound("Invalid ID");
        return Ok(new { status });
    }

    private bool IsRateLimitExceeded(string ip, int maxUploads = 5, int intervalSeconds = 60)
    {
        if (string.IsNullOrEmpty(ip))
            return false;

        var now = DateTime.UtcNow;
        var list = UploadLog.GetOrAdd(ip, _ => new List<DateTime>());

        lock (list)
        {
            list.RemoveAll(t => (now - t).TotalSeconds > intervalSeconds);

            if (list.Count >= maxUploads)
                return true;

            list.Add(now);
            return false;
        }
    }

    private bool IsExecutableFile(IFormFile file)
    {
        using var reader = new BinaryReader(file.OpenReadStream());
        var headerBytes = reader.ReadBytes(2);
        return headerBytes.SequenceEqual(new byte[] { 0x4D, 0x5A }); // "MZ"
    }

    private string SanitizeFileName(string fileName)
    {
        return Path.GetFileName(fileName)
                   .Replace("..", "")
                   .Replace("//", "")
                   .Replace("\\", "")
                   .Replace(":", "");
    }
}
