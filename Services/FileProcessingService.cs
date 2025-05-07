using System.Collections.Concurrent;
using System.Threading.Channels;

public class FileProcessingService : BackgroundService
{
    private readonly Channel<FileUploadTask> _uploadChannel;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        Channel<FileUploadTask> uploadChannel,
        ILogger<FileProcessingService> logger
    )
    {
        _uploadChannel = uploadChannel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var task in _uploadChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing file: {FileName}", task.OriginalFileName);
                UploadStatusTracker.StatusMap[task.ProcessingId] = "Scanning";

                // Simulate antivirus scan
                if (task.SimulateScan)
                {
                    //Put a delay by reading this key from appsettings "ScanDelayMilliseconds"
                    await Task.Delay(task.ScanDelayMs);
                }

                // Basic header/content check (simulate)
                if (!IsFileHeaderValid(task.FileContent))
                {
                    UploadStatusTracker.StatusMap[task.ProcessingId] = "VirusDetected";
                    continue;
                }

                UploadStatusTracker.StatusMap[task.ProcessingId] = "Processing";

                // Ensure the target directory exists
                Directory.CreateDirectory(task.StoragePath);
                // Upload the file to the path set in the FileUpload task object

                // Save file
                var filePath = Path.Combine(task.StoragePath, task.OriginalFileName);
                await File.WriteAllBytesAsync(filePath, task.FileContent);

                UploadStatusTracker.StatusMap[task.ProcessingId] = "Completed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process file {FileName}", task.OriginalFileName);
                UploadStatusTracker.StatusMap[task.ProcessingId] = "Failed";
            }
        }
    }

    private bool IsFileHeaderValid(byte[] content)
    {
        // Simulate checking file "magic bytes"
        // e.g., check for PDF: 0x25 0x50 0x44 0x46
        if (content.Length < 4)
            return false;

        // PDF
        if (content.Take(4).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }))
            return true;
        // JPEG
        if (content.Take(2).SequenceEqual(new byte[] { 0xFF, 0xD8 }))
            return true;
        // DOCX/ZIP
        if (content.Take(4).SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }))
            return true;
        // TXT (basic check for ASCII/UTF-8)
        return content.Take(4).All(b => b >= 0x09 && b <= 0x7F);

        // If header does not match known types
        return false;
    }
}
