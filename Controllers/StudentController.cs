using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Students_Interest_System.Models;
using System.Data;
namespace Students_Interest_System.Controllers
{
    public class StudentController : Controller
    {
        private readonly LoggerService logger;
        public StudentController(LoggerService logger)
        {
            this.logger = logger;
        }
        private readonly string @string = "Server=localhost;Database=studentinterestsystem;User=root;Password=Mian@1234;Port=3306;";
        public MySqlConnection OpenConnection()
        {
            MySqlConnection connection = new(@string);
            return connection;
        }
        public void CloseConnection(MySqlConnection connection)
        {
            connection?.Close();
        }

        [HttpGet]
        public ActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Signup(User user, string role)
        {
            var conn = OpenConnection();
            conn.Open();
            role = "user";
            var alreadyRegister = "SELECT * FROM USERS WHERE username = @username;";
            using (MySqlCommand alreadyCheckCmd = new MySqlCommand(alreadyRegister, conn))
            {
                alreadyCheckCmd.Parameters.AddWithValue("@username", user.Username);
                using (MySqlDataReader reader = alreadyCheckCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        logger.LogActivity(user.Username, "User already registered");
                        TempData["alert"] = "User already registered!";
                        CloseConnection(conn);
                        return View("Signup");
                    }
                }
            }
            var insertQuery = "INSERT INTO USERS(username, password, ROLE_ID) VALUES(@username, @password, @roleId);";
            using (MySqlCommand insertUserCmd = new MySqlCommand(insertQuery, conn))
            {
                insertUserCmd.Parameters.AddWithValue("@username", user.Username);
                insertUserCmd.Parameters.AddWithValue("@password", user.Password);
                insertUserCmd.Parameters.AddWithValue("@roleId", role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 1 : 2);
                insertUserCmd.ExecuteNonQuery();
                logger.LogActivity(user.Username, "User registered successfully");
            }
            TempData["message"] = "Registered Successfully";
            CloseConnection(conn);
            return View("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            var conn = OpenConnection();
            conn.Open();
            var query1 = "select * from USERS where username=@uname and password=@pass;";
            using (MySqlCommand cmd = new(query1, conn))
            {
                cmd.Parameters.AddWithValue("@uname", user.Username);
                cmd.Parameters.AddWithValue("@pass", user.Password);
                Console.WriteLine(user.Username + " " + user.Password);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        logger.LogActivity(user.Username, "User login successfully");
                        var Username = reader.GetString("username");
                        int roleId = reader.GetInt32("Role_ID");
                        reader.Close();
                        HttpContext.Session.SetString("Username", user.Username);
                        HttpContext.Session.SetInt32("RoleId", roleId);
                        var incrementLoggedInQuery = "UPDATE USERS SET loggedIn = loggedIn + 1 WHERE username = @uname;";
                        using (MySqlCommand incrementCmd = new MySqlCommand(incrementLoggedInQuery, conn))
                        {
                            incrementCmd.Parameters.AddWithValue("@uname", user.Username);
                            incrementCmd.ExecuteNonQuery();
                        }
                        Items items = new Items 
                        { 
                            Departments = GetDepartments(), 
                            Interests = GetInterests(), 
                            Cities = GetCities(), 
                            Degrees = GetDegrees(), 
                            student = new Student() };
                        CloseConnection(conn);
                        return View("StudentForm", items);
                    }
                    else
                    {
                        CloseConnection(conn);
                        logger.LogActivity(user.Username, "Invalid login attempt");
                        TempData["alert"] = "Invalid Username or Password";
                        return View("Login");
                    }
                }
            }
        }
        public ActionResult Logout()
        {
            var conn = OpenConnection();
            conn.Open();
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                logger.LogActivity(username, "Logout from system");
                var decrementLoggedOutQuery = "UPDATE USERS SET loggedOut = loggedOut + 1 WHERE username = @uname;";
                using (MySqlCommand decrementCmd = new MySqlCommand(decrementLoggedOutQuery, conn))
                {
                    decrementCmd.Parameters.AddWithValue("@uname", username);
                    decrementCmd.ExecuteNonQuery();
                }
            }
            HttpContext.Session.Clear();
            return View("Login");
        }
        [HttpGet]
        public ActionResult StudentForm()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", "Unknown Person");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                logger.LogActivity(username, "Accessed StudentForm");
                Items items = new Items { Departments = GetDepartments(), Interests = GetInterests(), Cities = GetCities(), Degrees = GetDegrees(), student = new Student() };
                return View("StudentForm", items);
            }

        }
        [HttpPost]
        public ActionResult StudentForm(Items obj)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", "Attempted to access StudentForm without logging in");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                if (!CheckInterest(obj.student.Interest))
                {
                    AddInterest(obj.student.Interest);
                }
                 var conn = OpenConnection();
            conn.Open();
            var query = "INSERT INTO STUDENTS(Name, RollNo, Email, Gender, DOB, City, Interest, Department, Degree, Subject, Start, End, CREATEDATE)" +
                        " VALUES(@a, @b, @c, @d, @e, @f, @g, @h, @i, @j, @k, @l, @m);";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@a", obj.student.Name);
                cmd.Parameters.AddWithValue("@b", obj.student.RollNo);
                cmd.Parameters.AddWithValue("@c", obj.student.Email);
                cmd.Parameters.AddWithValue("@d", obj.student.Gender);
                cmd.Parameters.AddWithValue("@e", obj.student.DOB);
                cmd.Parameters.AddWithValue("@f", obj.student.City);
                cmd.Parameters.AddWithValue("@g", obj.student.Interest);
                cmd.Parameters.AddWithValue("@h", obj.student.Department);
                cmd.Parameters.AddWithValue("@i", obj.student.Degree);
                cmd.Parameters.AddWithValue("@j", obj.student.Subject);
                cmd.Parameters.AddWithValue("@k", obj.student.Start);
                cmd.Parameters.AddWithValue("@l", obj.student.End);
                cmd.Parameters.AddWithValue("@m", DateTime.Now);
                cmd.ExecuteNonQuery();
                CloseConnection(conn);
                IncrementInterest(obj.student.Interest);
            }
                List<Student> students = GetAllStudents();
                var roleId = HttpContext.Session.GetInt32("RoleId");
                if (roleId == 1)
                {
                    logger.LogActivity(username, "Submitted StudentForm and student added successfully");
                    TempData["message"] = "Student added successfully!";
                    return View("AllStudents_admin", students);
                }
                else
                {
                    logger.LogActivity(username, "Submitted StudentForm and student added successfully");
                    TempData["message"] = "Student added successfully!";
                    return View("AllStudents_user", students);
                }
            }

        }
        public ActionResult AllStudents()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", "Attempted to access all Students without logging in");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                List<Student> students = GetAllStudents();
                var roleId = HttpContext.Session.GetInt32("RoleId");
                logger.LogActivity(username, "Accessed all students successfully");
                return View("AllStudents_user", students);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {

            var username = HttpContext.Session.GetString("Username");
            logger.LogActivity(username, $"Accessed Edit page for student with ID {id}");
                var conn = OpenConnection();
                conn.Open();
                var query = "Select * from Students where ID=@id";
                using (MySqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    Student temp = new Student();
                    while (reader.Read())
                    {
                        temp.ID = reader.GetInt32("ID");
                        temp.Name = reader.GetString("Name");
                        temp.RollNo = reader.GetString("RollNo");
                        temp.Email = reader.GetString("Email");
                        temp.Gender = reader.GetString("Gender");
                        temp.DOB = reader.GetDateTime("DOB");
                        temp.City = reader.GetString("City");
                        temp.Interest = reader.GetString("Interest");
                        temp.Department = reader.GetString("Department");
                        temp.Degree = reader.GetString("Degree");
                        temp.Subject = reader.GetString("Subject");
                        temp.Start = reader.GetDateTime("Start");
                        temp.End = reader.GetDateTime("End");
                    }
                    Items items = new Items { Departments = GetDepartments(), Interests = GetInterests(), Cities = GetCities(), Degrees = GetDegrees(), student = temp };
                    return View("Edit", items);
            }

        }

        [HttpPost]
        public ActionResult Edit(Items obj)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", $"Wrong User");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                List<Student> students = GetAllStudents();
                foreach (Student s in students)
                {
                    if (s.ID == obj.student.ID)
                    {
                        logger.LogActivity(username, "Form Submitted");
                        var conn = OpenConnection();
                        conn.Open();
                        var query = "UPDATE Students SET Name=@name, RollNo=@rollNo, Email=@email, Gender=@gender, DOB=@dob, City=@city, " +
                            "Interest=@interest, Department=@department, Degree=@degree, Subject=@subject, Start=@start, End=@end, UPDATEDATE=@date " +
                            "WHERE ID=@id";
                        using (MySqlCommand cmd = new(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", obj.student.ID);
                            cmd.Parameters.AddWithValue("@name", obj.student.Name);
                            cmd.Parameters.AddWithValue("@rollNo", obj.student.RollNo);
                            cmd.Parameters.AddWithValue("@email", obj.student.Email);
                            cmd.Parameters.AddWithValue("@gender", obj.student.Gender);
                            cmd.Parameters.AddWithValue("@dob", obj.student.DOB);
                            cmd.Parameters.AddWithValue("@city", obj.student.City);
                            cmd.Parameters.AddWithValue("@interest", obj.student.Interest);
                            cmd.Parameters.AddWithValue("@department", obj.student.Department);
                            cmd.Parameters.AddWithValue("@degree", obj.student.Degree);
                            cmd.Parameters.AddWithValue("@subject", obj.student.Subject);
                            cmd.Parameters.AddWithValue("@start", obj.student.Start);
                            cmd.Parameters.AddWithValue("@end", obj.student.End);
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                            CloseConnection(conn);
                            if (obj.InterestStatus != obj.student.Interest)
                            {
                                DecrementInterest(obj.InterestStatus);
                                logger.LogActivity(obj.InterestStatus, "Decremented by 1");
                                IncrementInterest(obj.student.Interest);
                                logger.LogActivity(obj.student.Interest, "Incremented by 1");
                            }
                            logger.LogActivity(username, $"Edited student with ID {obj.student.ID}");
                            TempData["updated"] = "Student updated successfully!";
                        }
                    }
                }
                List<Student> updatedStudents = GetAllStudents();
                return View("AllStudents", updatedStudents);
            }

        }
        public ActionResult Details(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", $"Attempted to view details of student with ID {id} without logging in");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                logger.LogActivity(username, $"Viewed details of student with ID {id}");
                var conn = OpenConnection();
                conn.Open();
                var query = "Select * from Students where ID=@id";
                using (MySqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    Student student = new Student();
                    while (reader.Read())
                    {
                        student.ID = reader.GetInt32("ID");
                        student.Name = reader.GetString("Name");
                        student.RollNo = reader.GetString("RollNo");
                        student.Email = reader.GetString("Email");
                        student.Gender = reader.GetString("Gender");
                        student.DOB = reader.GetDateTime("DOB");
                        student.City = reader.GetString("City");
                        student.Interest = reader.GetString("Interest");
                        student.Department = reader.GetString("Department");
                        student.Degree = reader.GetString("Degree");
                        student.Subject = reader.GetString("Subject");
                        student.Start = reader.GetDateTime("Start");
                        student.End = reader.GetDateTime("End");
                    }
                    CloseConnection(conn);
                    return View("Details", student);
                }
            }

        }
        public ActionResult Delete(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", $"Attempted to delete student with ID {id} without logging in");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                string interest = "";
                var conn = OpenConnection();
                conn.Open();
                var query1 = "SELECT INTEREST FROM STUDENTS WHERE ID=@id";
                using (MySqlCommand cmd = new(query1, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            interest = reader.GetString("Interest");
                    }
                }
                DecrementInterest(interest);
                logger.LogActivity(interest, "Decremented");
                var query2 = "Delete from Students where ID=@id";
                using (MySqlCommand cmd = new(query2, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    CloseConnection(conn);
                }
                logger.LogActivity(username, $"Deleted student with ID {id}");
                TempData["message"] = "Student deleted successfully!";
                List<Student> students = GetAllStudents();
                return View("AllStudents_user", students);
            }
        }
        public IActionResult Dashboard()
        {

            var username = HttpContext.Session.GetString("Username");
            if (username == null)
            {
                logger.LogActivity("Anonymous", "Attempted to access dashboard without logging in");
                TempData["warning"] = "First login here";
                return View("Login");
            }
            else
            {
                logger.LogActivity(username, "Accessed Dashboard");
                DashboardItems items = new DashboardItems();
                List<String> TInterests = new List<string>();
                List<String> BInterests = new List<string>();
                var conn = OpenConnection();
                conn.Open();
                var query1 = "SELECT INTEREST FROM INTERESTS ORDER BY COUNT DESC LIMIT 5;";
                using (MySqlCommand cmd = new(query1, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString("interest");
                            TInterests.Add(name);
                        }
                    }
                }
                var query2 = "SELECT INTEREST FROM INTERESTS ORDER BY COUNT ASC LIMIT 5;";
                using (MySqlCommand cmd = new(query2, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString("interest");
                            BInterests.Add(name);
                        }
                    }
                }
                var query3 = "SELECT COUNT(DISTINCT INTEREST) AS DISTINCT_INTEREST FROM INTERESTS;";
                int interests;
                using (MySqlCommand cmd = new(query3, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            interests = reader.GetInt32(0);
                            items.TotalInterests = interests;
                        }
                    }
                }
                items.TopInterests = TInterests;
                items.BottomInterests = BInterests;
                items.CityDistribution = FetchCityDistribution();
                items.AgeDistribution = FetchAgeDistribution();
                items.DepartmentDistribution = FetchDepartmentDistribution();
                items.DegreeDistribution = FetchDegreeDistribution();
                items.GenderDistribution = FetchGenderDistribution();
                items.SubmissionChart = FetchSubmissionChart();
                items.Last30DaysActivity = FetchLast30DaysActivity();
                items.Last24HoursActivity = FetchLast24HoursActivity();
                items.StudentsStatusGrid = FetchStudentsStatusGrid();
                items.MostActiveHours = FetchMostActiveHoursLast30Days();
                items.LeastActiveHours = FetchLeastActiveHoursLast30Days();
                items.DeadHours = FetchDeadHoursLast30Days();

                CloseConnection(conn);

                var roleId = HttpContext.Session.GetInt32("RoleId");
                return View("Dashboard_user", items);
            }

        }
        public List<String> GetDepartments()
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "select dept_name from departments";
            List<String> departments = new List<String>();

            using (MySqlCommand cmd = new(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string departmentName = reader.GetString("DEPT_NAME");
                        departments.Add(departmentName);
                    }
                }
            }
            CloseConnection(conn);
            return departments;
        }
        public List<String> GetInterests()
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "select interest from interests";
            List<String> interests = new List<String>();

            using (MySqlCommand cmd = new(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string interestName = reader.GetString("interest");
                        interests.Add(interestName);
                    }
                }
            }
            CloseConnection(conn);
            return interests;
        }
        public void AddInterest(String interest)
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "insert into interests(interest) values(@input)";

            using (MySqlCommand cmd = new(query, conn))
            {
                cmd.Parameters.AddWithValue("@input", interest);
                cmd.ExecuteNonQuery();
            }
            CloseConnection(conn);
        }
        public bool CheckInterest(string interest)
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "select * from interests where interest=@input";
            using (MySqlCommand cmd = new(query, conn))
            {
                cmd.Parameters.AddWithValue("@input", interest);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        CloseConnection(conn);
                        return true;
                    }
                    else
                    {
                        CloseConnection(conn);
                        return false;
                    }
                }
            }
        }

        public void IncrementInterest(string interest)
        {
            var conn = OpenConnection();
            conn.Open();
            int id = -999999;
            var query1 = "SELECT ID FROM INTERESTS WHERE INTEREST = @input";
            using (MySqlCommand cmd = new(query1, conn))
            {
                cmd.Parameters.AddWithValue("@input", interest);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        id = reader.GetInt32("ID");
                }
            }
            var query2 = "update interests set count = count + 1 where ID = @input";
            using (MySqlCommand cmd = new(query2, conn))
            {
                cmd.Parameters.AddWithValue("@input", id);
                cmd.ExecuteNonQuery();
            }
            CloseConnection(conn);
        }

        public void DecrementInterest(string interest)
        {
            var conn = OpenConnection();
            conn.Open();
            int id = -999999;
            var query1 = "SELECT ID FROM INTERESTS WHERE INTEREST = @input";
            using (MySqlCommand cmd = new(query1, conn))
            {
                cmd.Parameters.AddWithValue("@input", interest);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        id = reader.GetInt32("ID");
                }
            }
            var query2 = "update interests set count = count - 1 where ID = @input";
            using (MySqlCommand cmd = new(query2, conn))
            {
                cmd.Parameters.AddWithValue("@input", id);
                cmd.ExecuteNonQuery();
            }
            CloseConnection(conn);
        }

        public List<String> GetCities()
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "select city from cities";
            List<String> array = new List<String>();

            using (MySqlCommand cmd = new(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cityName = reader.GetString("city");
                        array.Add(cityName);
                    }
                }
            }
            CloseConnection(conn);
            return array;
        }

        public List<String> GetDegrees()
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "select degree from degrees";
            List<String> array = new List<String>();

            using (MySqlCommand cmd = new(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string degreeName = reader.GetString("degree");
                        array.Add(degreeName);
                    }
                }
            }
            CloseConnection(conn);
            return array;
        }
        public List<Student> GetAllStudents()
        {
            var conn = OpenConnection();
            conn.Open();
            var query = "Select * from STUDENTS";
            List<Student> array = new List<Student>();

            using (MySqlCommand cmd = new(query, conn))
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            ID = reader.GetInt32("ID"),
                            Name = reader.GetString("Name"),
                            RollNo = reader.GetString("RollNo"),
                            Email = reader.GetString("Email"),
                            Gender = reader.GetString("Gender"),
                            DOB = reader.GetDateTime("DOB"),
                            City = reader.GetString("City"),
                            Interest = reader.GetString("Interest"),
                            Department = reader.GetString("Department"),
                            Degree = reader.GetString("Degree"),
                            Subject = reader.GetString("Subject"),
                            Start = reader.GetDateTime("Start"),
                            End = reader.GetDateTime("End")
                        };
                        array.Add(student);
                    }
                }
            }
            CloseConnection(conn);
            return array;
        }
        public Dictionary<string, int> FetchCityDistribution()
        {
            Dictionary<string, int> cityDistribution = new Dictionary<string, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT City, COUNT(*) AS StudentCount FROM Students GROUP BY City ORDER BY StudentCount DESC LIMIT 5";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string city = reader.GetString("City");
                        int studentCount = Convert.ToInt32(reader["StudentCount"]);
                        cityDistribution.Add(city, studentCount);
                    }
                }
            }
            CloseConnection(conn);
            return cityDistribution;
        }
        public Dictionary<int, int> FetchAgeDistribution()
        {
            Dictionary<int, int> ageDistribution = new Dictionary<int, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT YEAR(CURDATE()) - YEAR(DOB) AS Age, COUNT(*) AS StudentCount FROM Students GROUP BY Age;";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int age = reader.GetInt32("Age");
                        int studentCount = reader.GetInt32("StudentCount");
                        ageDistribution.Add(age, studentCount);
                    }
                }
            }
            CloseConnection(conn);
            return ageDistribution;
        }
        public Dictionary<string, int> FetchDepartmentDistribution()
        {
            Dictionary<string, int> departmentDistribution = new Dictionary<string, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT Department, COUNT(*) AS Count FROM Students GROUP BY Department";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string department = reader.GetString("Department");
                        int count = reader.GetInt32("Count");
                        departmentDistribution.Add(department, count);
                    }
                }
            }
            CloseConnection(conn);
            return departmentDistribution;
        }
        public Dictionary<string, int> FetchDegreeDistribution()
        {
            Dictionary<string, int> degreeDistribution = new Dictionary<string, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT Degree, COUNT(*) AS Count FROM Students GROUP BY Degree";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string degree = reader.GetString("Degree");
                        int count = reader.GetInt32("Count");
                        degreeDistribution.Add(degree, count);
                    }
                }
            }
            return degreeDistribution;
        }
        public Dictionary<string, int> FetchGenderDistribution()
        {
            Dictionary<string, int> genderDistribution = new Dictionary<string, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT Gender, COUNT(*) AS Count FROM Students GROUP BY Gender";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string gender = reader.GetString("Gender");
                        int count = reader.GetInt32("Count");
                        genderDistribution.Add(gender, count);
                    }
                }
            }
            return genderDistribution;
        }
        public Dictionary<DateTime, int> FetchSubmissionChart()
        {
            Dictionary<DateTime, int> submissionChart = new Dictionary<DateTime, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DATE(CREATEDATE) AS DateColumn, COUNT(*) AS Count FROM Students WHERE CREATEDATE >= (CURDATE() - INTERVAL 30 DAY) GROUP BY DateColumn;";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = reader.GetDateTime("DateColumn");
                        int count = reader.GetInt32("Count");
                        submissionChart.Add(date, count);
                    }
                }
            }
            return submissionChart;
        }
        public Dictionary<DateTime, int> FetchLast30DaysActivity()
        {
            Dictionary<DateTime, int> activityChart = new Dictionary<DateTime, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DATE(GREATEST(COALESCE(CREATEDATE, '0000-01-01'), COALESCE(UPDATEDATE, '0000-01-01'))) AS DateColumn, COUNT(*) AS Count " +
                    "FROM Students WHERE GREATEST(COALESCE(CREATEDATE, '0000-01-01'), COALESCE(UPDATEDATE, '0000-01-01')) >= (CURDATE() - INTERVAL 30 DAY) GROUP BY DateColumn; ";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = reader.GetDateTime("DateColumn");
                        int count = reader.GetInt32("Count");
                        activityChart.Add(date, count);
                    }
                }
            }
            return activityChart;
        }
        public Dictionary<DateTime, int> FetchLast24HoursActivity()
        {
            Dictionary<DateTime, int> activityChart = new Dictionary<DateTime, int>();
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DATE(IFNULL(LEAST(CREATEDATE, UPDATEDATE), CREATEDATE)) AS DateColumn, COUNT(*) AS Count FROM Students " +
                "WHERE IFNULL(LEAST(CREATEDATE, UPDATEDATE), CREATEDATE) >= (NOW() - INTERVAL 1 DAY) GROUP BY DateColumn;";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime dateTime = reader.GetDateTime("DateColumn");
                        int count = reader.GetInt32("Count");
                        activityChart.Add(dateTime, count);
                    }
                }
            }
            return activityChart;
        }
        public Dictionary<string, int> FetchStudentsStatusGrid()
        {
            Dictionary<string, int> statusGrid = new Dictionary<string, int>();
            var conn = OpenConnection();
            conn.Open();

            string query = "SELECT CASE " +
                "WHEN CURDATE() < DATE(Start) THEN 'Recently Enrolled' " +
                "WHEN CURDATE() BETWEEN DATE(Start) AND DATE(End) THEN 'Currently Studying' " +
                "WHEN CURDATE() > DATE(End) THEN 'Graduated' " +
                "ELSE 'About to Graduate' END AS Status, COUNT(*) AS Count FROM Students GROUP BY Status;";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string status = reader.GetString("Status");
                        int count = reader.GetInt32("Count");
                        statusGrid.Add(status, count);
                    }
                }
            }
            return statusGrid;
        }
        public List<string> FetchMostActiveHoursLast30Days()
        {
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DATE_FORMAT(LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)), '%h:%i %p') AS Hour, COUNT(*) AS Count " +
                "FROM Students WHERE LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)) >= (CURDATE() - INTERVAL 30 DAY) " +
                "GROUP BY Hour ORDER BY Count DESC; ";
            var mostActiveHours = new List<string>();
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hour = reader.GetString("Hour");
                        mostActiveHours.Add(hour);
                    }
                }
            }
            return mostActiveHours;
        }
        public List<string> FetchLeastActiveHoursLast30Days()
        {
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DATE_FORMAT(LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)), '%h:%i %p') AS Hour, COUNT(*) AS Count " +
                "FROM Students WHERE LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)) >= (CURDATE() - INTERVAL 30 DAY) " +
                "GROUP BY Hour ORDER BY Count ASC LIMIT 5; ";
            var leastActiveHours = new List<string>();

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hour = reader.GetString("Hour");
                        leastActiveHours.Add(hour);
                    }
                }
            }
            return leastActiveHours;
        }
        public List<string> FetchDeadHoursLast30Days()
        {
            var conn = OpenConnection();
            conn.Open();
            string query = "SELECT DISTINCT DATE_FORMAT(LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)), '%h:%i %p') AS FormattedHour FROM Students " +
                "WHERE LEAST(CREATEDATE, COALESCE(UPDATEDATE, CREATEDATE)) >= (CURDATE() - INTERVAL 30 DAY) ORDER BY FormattedHour;";
            var allHours = Enumerable.Range(1, 12).Select(hour => hour.ToString("00") + ":00 AM").ToList(); // All hours in a day as strings
            var activeHours = new List<string>();
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var formattedHour = reader.GetString("FormattedHour");
                        activeHours.Add(formattedHour);
                    }
                }
            }
            var deadHours = allHours.Except(activeHours).ToList();
            CloseConnection(conn);
            return deadHours;
        }
    }
}
