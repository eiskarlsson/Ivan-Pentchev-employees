using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ivan_Pentchev_employees.Server.Services
{
    public class UniversalDateParserService
    {
        private readonly List<CultureInfo> _cultures;
        private readonly DateTimeStyles _dateTimeStyles;

        public UniversalDateParserService()
        {
            // Include common cultures from around the world
            _cultures = new List<CultureInfo>
            {
                CultureInfo.InvariantCulture, // yyyy-MM-dd (ISO standard)
                new CultureInfo("en-US"), // MM/dd/yyyy (United States)
                new CultureInfo("en-GB"), // dd/MM/yyyy (United Kingdom)
                new CultureInfo("en-CA"), // Mixed (Canada)
                new CultureInfo("fr-FR"), // dd/MM/yyyy (France)
                new CultureInfo("de-DE"), // dd.MM.yyyy (Germany)
                new CultureInfo("es-ES"), // dd/MM/yyyy (Spain)
                new CultureInfo("it-IT"), // dd/MM/yyyy (Italy)
                new CultureInfo("pt-BR"), // dd/MM/yyyy (Brazil)
                new CultureInfo("ru-RU"), // dd.MM.yyyy (Russia)
                new CultureInfo("ja-JP"), // yyyy/MM/dd (Japan)
                new CultureInfo("zh-CN"), // yyyy/M/d (China)
                new CultureInfo("ko-KR"), // yyyy. MM. dd (Korea)
                new CultureInfo("ar-SA"), // dd/MM/yyyy (Saudi Arabia)
                new CultureInfo("hi-IN"), // dd-MM-yyyy (India)
                new CultureInfo("tr-TR"), // dd.MM.yyyy (Turkey)
                new CultureInfo("nl-NL"), // dd-MM-yyyy (Netherlands)
                new CultureInfo("pl-PL"), // dd.MM.yyyy (Poland)
                new CultureInfo("sv-SE"), // yyyy-MM-dd (Sweden)
                new CultureInfo("da-DK") // dd-MM-yyyy (Denmark)
            };

            _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;
        }

        public DateTime ParseAnyDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return DateTime.Today;

            // Clean the input string
            dateString = CleanDateString(dateString);

            // Handle special cases
            if (IsNullValue(dateString))
                return DateTime.Today;

            // First, try culture-specific parsing
            foreach (var culture in _cultures)
            {
                if (DateTime.TryParse(dateString, culture, _dateTimeStyles, out DateTime result))
                {
                    return result.Date; // Return only date part
                }
            }

            // Try specific formats as fallback
            var specificFormats = GetSpecificDateFormats();
            foreach (var format in specificFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, _dateTimeStyles,
                        out DateTime result))
                {
                    return result.Date;
                }
            }

            // Last resort: try to extract date from complex string
            var extractedDate = ExtractDateFromString(dateString);
            if (extractedDate.HasValue)
            {
                return extractedDate.Value;
            }

            throw new FormatException($"Unable to parse date: {dateString}");
        }

        private string CleanDateString(string dateString)
        {
            return dateString.Trim()
                .Trim('"', '\'', '[', ']', '(', ')')
                .Replace("\\", "/")
                .Replace(" at ", " ")
                .Replace(" on ", " ")
                .Replace("  ", " ");
        }

        private bool IsNullValue(string dateString)
        {
            var nullValues = new[] { "NULL", "N/A", "NULL", "EMPTY", "", "TBD", "TBA" };
            return nullValues.Contains(dateString, StringComparer.OrdinalIgnoreCase);
        }

        private string[] GetSpecificDateFormats()
        {
            return new[]
            {
                // Basic formats
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "yyyy.MM.dd",
                "yyyy MM dd",
                "MM/dd/yyyy",
                "MM-dd-yyyy",
                "MM.dd.yyyy",
                "MM dd yyyy",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "dd.MM.yyyy",
                "dd MM yyyy",

                // Without leading zeros
                "M/d/yyyy",
                "M-d-yyyy",
                "d/M/yyyy",
                "d-M-yyyy",
                "yyyy-M-d",

                // With two-digit years
                "yy-MM-dd",
                "MM/dd/yy",
                "dd/MM/yy",
                "M/d/yy",
                "d/M/yy",

                // Month names
                "dd MMMM yyyy",
                "dd MMM yyyy",
                "MMMM dd, yyyy",
                "MMM dd, yyyy",
                "yyyy MMMM dd",
                "yyyy MMM dd",
                "dd-MMM-yyyy",
                "dd/MMM/yyyy",
                "MMM-dd-yyyy",

                // With time
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd HH:mm",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm",
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy HH:mm",

                // Compact formats
                "yyyyMMdd",
                "ddMMyyyy",
                "MMddyyyy",
                "yyMMdd",

                // Unix timestamp (try as fallback)
                "Unix"
            };
        }

        private DateTime? ExtractDateFromString(string dateString)
        {
            try
            {
                // Try to find date patterns using regex
                var patterns = new[]
                {
                    @"\b\d{1,4}[-./]\d{1,2}[-./]\d{1,4}\b", // 2023-12-25, 25/12/2023, etc.
                    @"\b\d{1,2}\s+[A-Za-z]{3,}\s+\d{4}\b", // 25 December 2023
                    @"\b[A-Za-z]{3,}\s+\d{1,2}\s*,\s*\d{4}\b", // December 25, 2023
                    @"\b\d{8}\b", // 20231225
                    @"\b\d{1,2}[-./]\d{1,2}[-./]\d{2,4}\b" // 25-12-23, 25-12-2023
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(dateString, pattern);
                    if (match.Success)
                    {
                        foreach (var culture in _cultures)
                        {
                            if (DateTime.TryParse(match.Value, culture, _dateTimeStyles, out DateTime result))
                            {
                                return result.Date;
                            }
                        }
                    }
                }

                // Try Unix timestamp
                if (long.TryParse(dateString, out long unixTimestamp) && unixTimestamp > 0)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                }
            }
            catch
            {
                // If extraction fails, return null
            }

            return null;
        }

        // Additional utility methods
        public bool TryParseAnyDate(string dateString, out DateTime result)
        {
            try
            {
                result = ParseAnyDate(dateString);
                return true;
            }
            catch
            {
                result = DateTime.MinValue;
                return false;
            }
        }

        public List<string> GetSupportedCultureNames()
        {
            return _cultures.Select(c => $"{c.Name} - {c.DateTimeFormat.ShortDatePattern}").ToList();
        }
    }
}