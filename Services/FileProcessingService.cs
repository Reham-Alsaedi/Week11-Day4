using System.Threading.Channels;
using System.Collections.Concurrent;

public class FileProcessingService : BackgroundService
{
    private readonly Channel<FileUploadTask> _uploadChannel;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(Channel<FileUploadTask> uploadChannel, ILogger<FileProcessingService> logger)
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
                }

                // Basic header/content check (simulate)
                if (!IsFileHeaderValid(task.FileContent))
                {
                    UploadStatusTracker.StatusMap[task.ProcessingId] = "VirusDetected";
                    continue;
                }

                UploadStatusTracker.StatusMap[task.ProcessingId] = "Processing";

                // Ensure the target directory exists
                // Upload the file to the path set in the FileUpload task object
                

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
        if (content.Length >= 4)
        {
            //IF  PDF            
            //   return true;

            // JPEG
            //   return true;

            // DOCX (ZIP-based format)
            //   return true;

            // TXT (Basic ASCII/UTF-8 heuristic)
            //   return true;
        }

        // If header does not match known types
        return false;
    }
}
