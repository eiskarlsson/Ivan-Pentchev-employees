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
            var fileString = ReadFirstFile();

            UniversalEmployeeCsvParserService.ParseResult result = null;

            try
            {
                result = _csvParser.ParseCsvContent(fileString);
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result.Employees;
        }


        public LongestPairResult FindLongestWorkingPair(List<EmployeeProjectsInput> inputData)
        {
            // Step 1: Organize by project
            var projects = OrganizeByProjectLinq(inputData);

            // Step 2: Calculate overlaps for each project
            var pairOverlaps = CalculateProjectOverlaps(projects);

            // Step 3: Find the pair with maximum total overlap
            return FindLongestPair(pairOverlaps);
        }

       
        private static Dictionary<int, List<EmployeeProjectsInput>> OrganizeByProjectLinq(List<EmployeeProjectsInput> inputData)
        {
            return inputData
                .GroupBy(record => record.ProjectID)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private static List<EmployeePairResult> CalculateProjectOverlaps(Dictionary<int, List<EmployeeProjectsInput>> projects)
        {
            var pairOverlaps = new Dictionary<(int, int), EmployeePairResult>();

            foreach (var project in projects)
            {
                var projectId = project.Key;
                var employees = project.Value;

                for (int i = 0; i < employees.Count; i++)
                {
                    for (int j = i + 1; j < employees.Count; j++)
                    {
                        var emp1 = employees[i];
                        var emp2 = employees[j];

                        var overlapDays = CalculateOverlap(
                            emp1.DateFrom, emp1.DateTo ?? DateTime.Now,
                            emp2.DateFrom, emp2.DateTo ?? DateTime.Now
                        );

                        if (overlapDays > 0)
                        {
                            var pairKey = GetSortedPairKey(emp1.EmpId, emp2.EmpId);

                            if (!pairOverlaps.ContainsKey(pairKey))
                            {
                                pairOverlaps[pairKey] = new EmployeePairResult
                                {
                                    Employee1 = pairKey.Item1,
                                    Employee2 = pairKey.Item2
                                };
                            }

                            pairOverlaps[pairKey].ProjectOverlaps[projectId] = overlapDays;
                            pairOverlaps[pairKey].TotalDays += overlapDays;
                        }
                    }
                }
            }

            return pairOverlaps.Values.ToList();
        }

        private static int CalculateOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            var latestStart = start1 > start2 ? start1 : start2;
            var earliestEnd = end1 < end2 ? end1 : end2;

            if (latestStart <= earliestEnd)
            {
                return (int)(earliestEnd - latestStart).TotalDays + 1;
            }

            return 0;
        }

        private static (int, int) GetSortedPairKey(int EmpId1, int EmpId2)
        {
            return EmpId1 < EmpId2 ? (EmpId1, EmpId2) : (EmpId2, EmpId1);
        }

        
        private static LongestPairResult FindLongestPair(List<EmployeePairResult> overlapResults)
        {
            if (overlapResults == null || overlapResults.Count == 0)
                return new LongestPairResult();

            // Find the longest pair in one query
            return overlapResults
                .GroupBy(r => (r.Employee1, r.Employee2))
                .Select(g => new LongestPairResult
                {
                    Employee1 = g.Key.Employee1,
                    Employee2 = g.Key.Employee2,
                    TotalDays = g.Sum(x => x.TotalDays),
                    ProjectDetails = g.ToList()
                })
                .OrderByDescending(x => x.TotalDays)
                .First();
        }


    }
}


