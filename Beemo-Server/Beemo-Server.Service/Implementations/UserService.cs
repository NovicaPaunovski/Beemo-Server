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
    public class UserService : BaseEntityService<User, IUserRepository>, IUserService
    {
        #region Fields
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<BeemoContext> _dbContextFactory;
        private IUserRepository _userRepository;
        #endregion

        #region Public Constructor
        public UserService(IUserRepository entityRepository, IDbContextFactory<BeemoContext> dbContextFactory, IConfiguration configuration) : base(entityRepository, dbContextFactory)
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

                if (user == null || !VerifyPassword(loginRequest.Password, user.Password)) { throw new UnauthorizedAccessException("Invalid user or password."); }

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

                User newUser = new User
                {
                    Username = registerRequest.Username,
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Email = registerRequest.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
                };

                var createdUser = _userRepository.Insert(newUser);

                return createdUser;
            }
        }
        
        public User ChangePassword(ChangePassword changePasswordRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingUser = _userRepository.GetByUsername(changePasswordRequest.Username);

                if (existingUser == null) { throw new ArgumentOutOfRangeException($"A user with the username {changePasswordRequest.Username} doesn't exist!"); }

                if (!VerifyPassword(changePasswordRequest.OldPassword, existingUser.Password)) { throw new ArgumentException("Invalid password"); }

                if (changePasswordRequest.NewPassword == changePasswordRequest.OldPassword) { throw new ArgumentException("New password can not be the same as old password"); }

                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);

                var updatedUser = _userRepository.Update(existingUser);

                return updatedUser;
            }
        }

        public override User Update(User user)
        {
            var existingUser = _userRepository.GetById(user.Id);

            if (existingUser == null) throw new ArgumentOutOfRangeException($"Cannot retrieve user information. User not found!");

            var existingUsername = _userRepository.GetByUsername(user.Username);

            if (existingUsername != null) throw new ArgumentException($"Cannot update user. A user with username {user.Username} already exists!");

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            return base.Update(existingUser);
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    // Token claims
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        #endregion

        #region Private Methods
        // Verify if a given password matches the hashed password
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        #endregion
    }
}