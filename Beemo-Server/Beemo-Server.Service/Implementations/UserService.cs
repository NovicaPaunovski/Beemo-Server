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
        private IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        #endregion

        #region Public Constructor
        public UserService(IUserRepository entityRepository, IDbContextFactory<BeemoContext> dbContextFactory, IEmailService emailService) : base(entityRepository, dbContextFactory)
        {
            _userRepository = entityRepository;
            _emailService = emailService;
        }
        #endregion

        #region Public Methods
        public User GetByUsername(string username)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = _userRepository.GetByUsername(username.ToLower());

                CheckExistingUser(user);

                return user;
            }
        }

        public User Authenticate(Login loginRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var user = _userRepository.GetByUsername(loginRequest.Username.ToLower());

                if (user == null || !VerifyPassword(loginRequest.Password, user.Password)) { throw new UnauthorizedAccessException("Invalid user or password."); }

                return user;
            }
        }

        public User Register(Register registerRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingUser = _userRepository.GetByUsername(registerRequest.Username.ToLower());
                if (existingUser != null) { throw new ArgumentException($"A user with the username {registerRequest.Username} already exists"); }

                var existingUserEmail = _userRepository.GetByEmail(registerRequest.Email.ToLower());
                if (existingUserEmail != null) { throw new ArgumentException($"A user with the email {registerRequest.Email} already exists"); }

                User newUser = new User
                {
                    Username = registerRequest.Username.ToLower(),
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    Email = registerRequest.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                    VerificationToken = GenerateVerificationToken(),
                    VerificationTokenExpiration = DateTime.Now.AddHours(1),
                    IsVerified = false
                };

                var createdUser = _userRepository.Insert(newUser);

                _emailService.SendEmail("Your Beemo account has been created!", GetVerificationEmail(createdUser.VerificationToken), createdUser.Email);

                return createdUser;
            }
        }
        
        public User ChangePassword(ChangePassword changePasswordRequest)
        {
            using (var context = _dbContextFactory.CreateDbContext())
            {
                var existingUser = _userRepository.GetByUsername(changePasswordRequest.Username.ToLower());

                CheckExistingUser(existingUser);

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

            CheckExistingUser(existingUser);

            var existingUsername = _userRepository.GetByUsername(user.Username.ToLower());

            if (existingUsername != null) throw new ArgumentException($"Cannot update user. A user with the username {user.Username} already exists!");

            if (existingUser.Email != user.Email) existingUser.IsVerified = false;

            existingUser.Username = user.Username.ToLower();
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            return base.Update(existingUser);
        }

        public User Verify(Verify verificationRequest)
        {
            var existingUser = _userRepository.GetByUsername(verificationRequest.Username.ToLower());

            CheckExistingUser(existingUser);

            if (existingUser.IsVerified) throw new ArgumentException($"User already verified.");

            if (existingUser.VerificationTokenExpiration < verificationRequest.VerificationTime)
            {
                throw new InvalidOperationException($"Verification token has expired.");
            }

            if (existingUser.VerificationToken == verificationRequest.VerificationToken)
            {
                existingUser.IsVerified = true;
                _userRepository.Update(existingUser);
                return existingUser;
            }

            throw new ArgumentException($"Wrong verification token.");
        }

        public void ResendVerificationToken(User user)
        {
            var existingUser = _userRepository.GetById(user.Id);

            CheckExistingUser(existingUser);

            if (existingUser.IsVerified) throw new InvalidOperationException($"User email already verified.");

            existingUser.VerificationToken = GenerateVerificationToken();
            existingUser.VerificationTokenExpiration = DateTime.Now.AddHours(1);

            _userRepository.Update(existingUser);

            _emailService.SendEmail("Your new Beemo verificaton code!", GetVerificationEmail(existingUser.VerificationToken), existingUser.Email);
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("BeemoJwtKey"));

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
                Issuer = Environment.GetEnvironmentVariable("BeemoJwtIssuer"),
                Audience = Environment.GetEnvironmentVariable("BeemoJwtAudience"),
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

        private string GetVerificationEmail(string verificationCode)
        {
            return $"Only thing left is to verify the email.\n\nYour verification code is: \t{verificationCode}\t.\n\nThis code will expire in 1 hour.";
        }

        private string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        private void CheckExistingUser(User user)
        {
            if (user == null) throw new ArgumentOutOfRangeException($"Cannot retrieve user information. User not found!");
        }
        #endregion
    }
}