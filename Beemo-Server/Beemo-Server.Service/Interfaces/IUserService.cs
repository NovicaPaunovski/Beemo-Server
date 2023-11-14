using Beemo_Server.Data.Models.Entities;
using Beemo_Server.Data.Models.Requests.User;
using Beemo_Server.Data.Repositories.Interfaces;

namespace Beemo_Server.Service.Interfaces
{
    public interface IUserService : IBaseEntityService<User, IUserRepository>
    {
        User GetByUsername(string username);

        User Register(Register registerRequest);

        User Authenticate(Login loginRequest);

        User ChangePassword(ChangePassword changePasswordRequest);

        string GenerateToken(User user);
    }
}
