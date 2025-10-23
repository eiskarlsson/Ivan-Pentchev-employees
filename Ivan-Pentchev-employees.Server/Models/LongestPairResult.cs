namespace Ivan_Pentchev_employees.Server.Models
{
    public class LongestPairResult
    {
        public int Employee1 { get; set; }
        public int Employee2 { get; set; }
        public int TotalDays { get; set; }
        public List<EmployeePairResult> ProjectDetails { get; set; } = new List<EmployeePairResult>();

        // Helper property to get all project IDs
        public List<int> ProjectIds { get; set; }

        // Helper property to get overlap days by project
        public Dictionary<int, int> ProjectOverlapDays { get; set; }
    }
}
