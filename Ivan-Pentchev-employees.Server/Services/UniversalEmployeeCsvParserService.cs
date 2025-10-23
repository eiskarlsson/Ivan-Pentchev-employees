using Ivan_Pentchev_employees.Server.Models;
using Ivan_Pentchev_employees.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions; // For ExtractDateFromString method
using static Ivan_Pentchev_employees.Server.Services.UniversalEmployeeCsvParserService;

namespace Ivan_Pentchev_employees.Server.Services
{
    public interface IUniversalEmployeeCsvParserService
    {
        public ParseResult ParseCsvContent(string csvContent);
    }
    public class UniversalEmployeeCsvParserService : IUniversalEmployeeCsvParserService
    {
        private readonly IUniversalDateParserService _dateParser;

        public UniversalEmployeeCsvParserService(IUniversalDateParserService _dateParser)
        {
            this._dateParser = _dateParser;
        }

        public class ParseResult
        {
            public bool Success { get; set; }
            public List<EmployeeProjectsInput> Employees { get; set; }
            public List<string> Errors { get; set; }
            public List<string> Warnings { get; set; }
            public int TotalLines { get; set; }
            public int SuccessfullyParsed { get; set; }

            public ParseResult()
            {
                Employees = new List<EmployeeProjectsInput>();
                Errors = new List<string>();
                Warnings = new List<string>();
            }
        }

        public ParseResult ParseCsvContent(string csvContent)
        {
            var result = new ParseResult();

            if (string.IsNullOrWhiteSpace(csvContent))
            {
                result.Errors.Add("CSV content is empty");
                result.Success = false;
                return result;
            }

            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();

            result.TotalLines = lines.Length;

            if (result.TotalLines == 0)
            {
                result.Errors.Add("No data found in CSV");
                result.Success = false;
                return result;
            }

            // Smart header detection
            int startLine = DetectHeaderLine(lines);

            for (int i = startLine; i < lines.Length; i++)
            {
                var line = lines[i];

                try
                {
                    var employee = ParseLine(line, i + 1, result); // Pass result as parameter
                    if (employee != null)
                    {
                        result.Employees.Add(employee);
                        result.SuccessfullyParsed++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Line {i + 1}: {ex.Message}");
                }
            }

            result.Success = result.Employees.Count > 0;

            // Add warnings if there were any parsing issues but we still got some data
            if (result.Errors.Count > 0 && result.Success)
            {
                result.Warnings.Add(
                    $"{result.Errors.Count} lines had errors but {result.SuccessfullyParsed} lines were successfully parsed");
            }

            return result;
        }

        private int DetectHeaderLine(string[] lines)
        {
            if (lines.Length == 0) return 0;

            var firstLine = lines[0].ToLower();

            // Check for common header patterns
            var headerIndicators = new[]
                { "empid", "projectid", "datefrom", "dateto", "employee", "project", "start", "end" };

            bool hasHeader = headerIndicators.Any(header => firstLine.Contains(header));

            return hasHeader ? 1 : 0;
        }

        private EmployeeProjectsInput ParseLine(string line, int lineNumber, ParseResult result)
        {
            var fields = ParseCsvLine(line);

            if (fields.Length < 4)
            {
                throw new FormatException($"Expected 4 fields but found {fields.Length}. Line format: EmpID,ProjectID,DateFrom,DateTo");
            }

            // Parse EmpID with flexible parsing
            if (!TryParseInt(fields[0], out int empId))
            {
                throw new FormatException($"Invalid EmpID: '{fields[0]}'. Must be a valid integer.");
            }

            // Parse ProjectID with flexible parsing
            if (!TryParseInt(fields[1], out int projectId))
            {
                throw new FormatException($"Invalid ProjectID: '{fields[1]}'. Must be a valid integer.");
            }

            // Parse dates using universal parser
            DateTime dateFrom, dateTo;

            try
            {
                dateFrom = _dateParser.ParseAnyDate(fields[2]);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Invalid DateFrom: '{fields[2]}'. {ex.Message}");
            }

            try
            {
                dateTo = _dateParser.ParseAnyDate(fields[3]);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Invalid DateTo: '{fields[3]}'. {ex.Message}");
            }

            // Validate business rules
            if (empId <= 0)
            {
                throw new FormatException($"EmpID must be positive: {empId}");
            }

            if (projectId <= 0)
            {
                throw new FormatException($"ProjectID must be positive: {projectId}");
            }

            // Add warning if date range is invalid
            if (dateTo < dateFrom)
            {
                result.Warnings.Add($"Line {lineNumber}: DateTo ({dateTo:yyyy-MM-dd}) is earlier than DateFrom ({dateFrom:yyyy-MM-dd})");
                // Still create the record but log warning
            }

            return new EmployeeProjectsInput
            {
                EmpId = empId,
                ProjectID = projectId,
                DateFrom = dateFrom,
                DateTo = dateTo
            };
        }

        private bool TryParseInt(string value, out int result)
        {
            // Remove any non-digit characters except minus sign
            var cleanValue = new string(value.Where(c => char.IsDigit(c) || c == '-').ToArray());

            return int.TryParse(cleanValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = new System.Text.StringBuilder();
            bool inQuotes = false;
            char quoteChar = '"';

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if ((c == '"' || c == '\'') && !inQuotes)
                {
                    inQuotes = true;
                    quoteChar = c;
                }
                else if (c == quoteChar && inQuotes)
                {
                    inQuotes = false;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            result.Add(currentField.ToString());
            return result.ToArray();
        }
    }

    
}

