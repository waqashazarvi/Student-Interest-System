//using System.ComponentModel.DataAnnotations;
namespace Students_Interest_System.Models
{
    public class Student
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string RollNo { get; set; }
        public string Email { get; set;}
        public string Gender { get; set;}
        public DateTime DOB { get; set;}
        public string City { get; set;}
        public string Interest { get; set;}
        public string Department{ get; set;}
        public string Degree { get; set;}
        public string Subject { get; set;}
        public DateTime Start { get; set;}
        public DateTime End { get; set;}
    }
}
