using Beemo_Server.Data.Context;
using Beemo_Server.Data.Models.Entities;
using Beemo_Server.Data.Repositories.Interfaces;

namespace Beemo_Server.Data.Repositories.Implementations
{
    public class UserRepository : BaseEntityRepository<User>, IUserRepository
    {
        public UserRepository(BeemoContext context) : base(context)
        {

        }

        public User GetByEmail(string email)
        {
            var entity = _context.Set<User>().FirstOrDefault(entity => entity.Email == email);
            if (entity == null) { return null; }

            return entity;
        }

        public User GetByUsername(string username)
        {
            var entity = _context.Set<User>().FirstOrDefault(entity => entity.Username == username);
            if (entity == null) { return null; }

            return entity;
        }
    }
}
