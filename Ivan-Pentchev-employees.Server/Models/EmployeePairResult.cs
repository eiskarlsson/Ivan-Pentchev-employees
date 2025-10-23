namespace Ivan_Pentchev_employees.Server.Models
{
    public class EmployeePairResult
    {
        public int Employee1 { get; set; }
        public int Employee2 { get; set; }

        public List<int> ProjectIDs { get; set; }
        public Dictionary<int, int> ProjectOverlaps { get; set; } = new Dictionary<int, int>();
        public int TotalDays { get; set; }
    }
}
