using System.Runtime.Serialization;

namespace Beemo_Server.Data.Models.Entities
{
    [DataContract]
    public class User : BaseEntity
    {
        #region Properties
        [DataMember] 
        public string Name { get; set; }

        [DataMember] 
        public string Email { get; set; }

        [DataMember] 
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }
        #endregion
    }
}