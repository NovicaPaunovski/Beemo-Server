using System.Runtime.Serialization;

namespace Beemo_Server.Data.Models.Entities
{
    [DataContract]
    public class User : BaseEntity
    {
        #region Fields
        private string hashedPassword;
        #endregion

        #region Properties
        [DataMember] 
        public string Name { get; set; }

        [DataMember] 
        public string Email { get; set; }

        [DataMember] 
        public string Username { get; set; }

        [DataMember]
        public string Password
        {
            get { return hashedPassword; }
            set { hashedPassword = HashPassword(value); }
        }
        #endregion

        #region Public Methods
        // Verify if a given password matches the hashed password
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        #endregion

        #region Private Methods
        private string HashPassword(string password)
        {
            // Generate a new salt for each password
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            // Hash the password with the salt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }
        #endregion
    }
}