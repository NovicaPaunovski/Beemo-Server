using Beemo_Server.Data.Models.Entities;
using Beemo_Server.Data.Models.Requests.User;

namespace Beemo_Server.Service.Interfaces
{
    public interface IUserService
    {
        User GetByUsername(string username);

        User Register(Register registerRequest);

        User Authenticate(Login loginRequest);

        User ChangePassword(ChangePassword changePasswordRequest);

        string GenerateToken(User user);
    }
}
