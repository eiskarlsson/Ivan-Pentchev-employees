namespace Ivan_Pentchev_employees.Server.Models
{
    public class EmployeeProjectsInput
    {
        public int EmpId { get; set; }
        public int ProjectID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

    }
}
