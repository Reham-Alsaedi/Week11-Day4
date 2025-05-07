# Secure File Upload API 🛡️

A secure and robust file upload backend built with ASP.NET Core Web API.  
This project demonstrates modern best practices for handling file uploads securely, including:

-  Multipart file upload
-  Antivirus scanning (simulated)
-  Asynchronous background processing
-  Upload status tracking
-  Rate limiting (in-memory)
-  Real-time UI updates via polling
- 🖼 Basic HTML frontend with progress feedback

---

##  Features

- **Secure Upload Endpoint:** Accepts image and document files using `multipart/form-data`.
- **Virus Detection Simulation:** Randomly detects "viruses" for demonstration purposes.
- **Async File Processing:** Files are queued and processed in the background using `Task.Run`.
- **Status Check Endpoint:** Track file processing status with a unique `uploadId`.
- **Rate Limiting:** Prevents rapid repeated uploads per client (in-memory).
- **Frontend:** Lightweight HTML+JS page with live polling and upload progress.

---

##  Technologies

- ASP.NET Core 8 Web API
- C#
- HTML / JavaScript (Vanilla)
- System.Reactive for throttling (optional)
- IHostedService for background processing

---


---

##  How to Run

1. Clone the repo:

   git clone https://github.com/your-username/SecureFileUploadAPI.git
   cd SecureFileUploadAPI
   dotnet run
   https://localhost:5001/index.html




