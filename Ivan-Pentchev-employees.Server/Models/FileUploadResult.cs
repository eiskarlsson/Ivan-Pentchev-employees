namespace Ivan_Pentchev_employees.Server.Models
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
    }
}
