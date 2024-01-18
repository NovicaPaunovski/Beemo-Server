using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beemo_Server.Service.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(string subject, string body, string recipient);

        public void SendEmail(string subject, string body, List<string> recipient);
    }
}
