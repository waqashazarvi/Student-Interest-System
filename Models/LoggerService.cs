namespace Students_Interest_System.Models
{
    public class LoggerService
    {
        private readonly string logFilePath;
        public LoggerService(string logFilePath)
        {
            this.logFilePath = logFilePath;
        }
        public void LogActivity(string username, string activity)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    string logMessage = $"{DateTime.Now} - User: {username}, Activity: {activity}";
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
        }
    }
}
