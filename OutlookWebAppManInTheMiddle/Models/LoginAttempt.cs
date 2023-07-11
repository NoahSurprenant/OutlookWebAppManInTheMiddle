namespace OutlookWebAppManInTheMiddle.Models
{
    public class LoginAttempt
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Valid { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }
}
