namespace Students_Interest_System.Models
{
    public class DashboardItems
    {
        public List<String> TopInterests { get; set; }
        public List<String> BottomInterests { get; set; }
        public int TotalInterests { get; set; }
        public Dictionary<String, int> CityDistribution { get; set; }
        public Dictionary<int, int> AgeDistribution { get; set; }
        public Dictionary<String, int> DepartmentDistribution { get; set; }
        public Dictionary<String, int> DegreeDistribution { get; set; }
        public Dictionary<String, int> GenderDistribution { get; set; }
        public Dictionary<DateTime, int> SubmissionChart { get; set; }
        public Dictionary<DateTime, int> Last30DaysActivity { get; set; }
        public Dictionary<DateTime, int> Last24HoursActivity { get; set; }
        public Dictionary<String, int> StudentsStatusGrid { get; set; }
        public List<string> MostActiveHours { get; set; }
        public List<string> LeastActiveHours { get; set; }
        public List<string> DeadHours { get; set; }
    }
}
