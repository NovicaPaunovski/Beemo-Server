using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Beemo_Server.Data.Models.Entities
{
    [DataContract]
    public class BaseEntity
    {
        [DataMember, Key] 
        public int Id { get; set; }

        [DataMember] 
        public DateTime CreationDate { get; set; }

        [DataMember] 
        public DateTime? ModifiedDate { get; set; }

        [DataMember] 
        public bool Deprecated { get; set; }
    }
}
