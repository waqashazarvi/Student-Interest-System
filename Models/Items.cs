namespace Students_Interest_System.Models
{
    public class Items
    {
        public List<String> Departments { get; set; }
        public List<String> Interests { get; set; }
        public List<String> Cities { get; set; }
        public List<String> Degrees { get; set; }
        public Student student { get; set; }
      
        public string InterestStatus { get; set; }
        
    }
}
