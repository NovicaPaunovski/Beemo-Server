using Beemo_Server.Data.Context;
using Beemo_Server.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using Beemo_Server.Service.Interfaces;
using Beemo_Server.Data.Repositories.Interfaces;
using Beemo_Server.Data.Models.Requests.User;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Beemo_Server.Service.Implementations
{
    public class UserService : IUserService
    {
        #region Fields
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<BeemoContext> _dbContextFactory;
        private IUserRepository _userRepository;
        #endregion

        #region Public Constructor
        public UserService(IUserRepository entityRepository, IDbContextFactory<BeemoContext> dbContextFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbContextFactory = dbContextFactory;
            _userRepository = entityRepository;
        }
        #endregion

        #region Public Methods
        public User GetByUsername(string username)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = _userRepository.GetByUsername(username);

                if (user == null) { throw new ArgumentOutOfRangeException("User not found."); }

                return user;
            }
        }

        public User Authenticate(Login loginRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = _userRepository.GetByUsername(loginRequest.Username);

                if (user == null || !user.VerifyPassword(loginRequest.Password)) { throw new UnauthorizedAccessException("Invalid user or password."); }

                return user;
            }
        }
        public User Register(Register registerRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingUser = _userRepository.GetByUsername(registerRequest.Username);
                if (existingUser != null) { throw new ArgumentException($"A user with the username {registerRequest.Username} already exists"); }

                var existingUserEmail = _userRepository.GetByEmail(registerRequest.Email);
                if (existingUserEmail != null) { throw new ArgumentException($"A user with the email {registerRequest.Email} already exists"); }

                User newUser = new User();

                newUser.Username = registerRequest.Username;
                newUser.Name = registerRequest.Name;
                newUser.Email = registerRequest.Email;
                newUser.Password = registerRequest.Password;

                var createdUser = _userRepository.Insert(newUser);

                return createdUser;
            }
        }

        public User ChangePassword(ChangePassword changePasswordRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingUser = _userRepository.GetByUsername(changePasswordRequest.Username);

                if (existingUser == null) { throw new ArgumentOutOfRangeException($"A user with the username {changePasswordRequest.Username} doesn't exists"); }

                if (!existingUser.VerifyPassword(changePasswordRequest.OldPassword)) { throw new ArgumentException("Invalid password"); }

                if (changePasswordRequest.NewPassword == changePasswordRequest.OldPassword) { throw new ArgumentException("New password can not be the same as old password"); }

                existingUser.Password = changePasswordRequest.NewPassword;

                var updatedUser = _userRepository.Update(existingUser);

                return updatedUser;
            }
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Token claims
                    new Claim(ClaimTypes.Name, user.Username),
                }),
                Expires = DateTime.UtcNow.AddDays(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion
    }
}