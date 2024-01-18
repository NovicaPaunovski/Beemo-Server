using Beemo_Server.Data.Models.Entities;

namespace Beemo_Server.Data.Repositories.Interfaces
{
    public interface IUserRepository : IBaseEntityRepository<User>
    {
        User GetByEmail(string email);

        User GetByUsername(string username);
    }
}
