namespace Beemo_Server.Data.Models.Requests.User
{
    public class Verify
    {
        public string Username { get; set; }

        public string VerificationToken { get; set; }

        public DateTime VerificationTime { get; set; }
    }
}
