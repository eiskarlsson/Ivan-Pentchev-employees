using Ivan_Pentchev_employees.Server.Models;
using Ivan_Pentchev_employees.Server.Services;

namespace Ivan_Pentchev_employees.Server
{
    public class EmployeeAlgorithm
    {
        private readonly UniversalEmployeeCsvParserService _csvParser;
        public EmployeeAlgorithm()
        {
            _csvParser = new UniversalEmployeeCsvParserService();
        }


        public string ReadFirstFile()
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

            if (!Directory.Exists(uploadsPath))
            {
                return "Uploads directory does not exist";
            }

            var files = Directory.GetFiles(uploadsPath);
            if (files.Length == 0)
            {
                return "No files found in Uploads directory";
            }

            var firstFile = files[0]; // Get first file
            return File.ReadAllText(firstFile);
        }

        public List<EmployeeProjectsInput> ParseCsvFile()
        {
            var filelString = ReadFirstFile();

            var result = _csvParser.ParseCsvContent(filelString);

            return result.Employees ;
        }


    }
}
