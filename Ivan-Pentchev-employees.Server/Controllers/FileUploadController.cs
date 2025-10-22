using Ivan_Pentchev_employees.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Text;
using System.Text.Json;

namespace Ivan_Pentchev_employees.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IWebHostEnvironment environment, ILogger<FileUploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<string>> UploadSingle(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new FileUploadResult
                    {
                        Success = false,
                        Message = "No file uploaded"
                    });
                }

                // Validate file size (e.g., 10MB limit)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new FileUploadResult
                    {
                        Success = false,
                        Message = "File size exceeds 10MB limit"
                    });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".csv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new FileUploadResult
                    {
                        Success = false,
                        Message = "Invalid file type"
                    });
                }

                // Create uploads directory if it doesn't exist
                //var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads");
                //if (!Directory.Exists(uploadsPath))
                //{
                //    Directory.CreateDirectory(uploadsPath);
                //}

                //Generate the same name for the file
                var fileName = "employees.csv";
                //var filePath = Path.Combine(uploadsPath, fileName);

                //To work in Azure
                var tempPath = Path.GetTempPath();
                var filePath = Path.Combine(tempPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {fileName}");

                var algorithm = new EmployeeAlgorithm();

                var inputData = algorithm.ParseCsvFile();

                var result = algorithm.FindLongestWorkingPair(inputData);

                return ToJson(
                    result);

                //return Ok(new FileUploadResult
                //{
                //    Success = true,
                //    Message = "File uploaded successfully",
                //    FileName = file.FileName,
                //    FileSize = file.Length,
                //    FilePath = fileName
                //});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new FileUploadResult
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads");
                var filePath = Path.Combine(uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;

                // Get content type
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(filePath, out string contentType))
                {
                    contentType = "application/octet-stream";
                }

                return File(memory, contentType, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }



        /// <summary>
        /// Helper method to json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string ToJson(object obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(obj, options);
        }
    }
}