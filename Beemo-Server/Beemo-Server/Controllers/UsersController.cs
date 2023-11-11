using Beemo_Server.Data.Models.Requests.User;
using Beemo_Server.Data.Models.TransferObjects;
using Beemo_Server.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beemo_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] Register registerRequest)
        {
            try
            {
                var createdUser = _userService.Register(registerRequest);
                return Ok(new { Message = "Registration successful", User = createdUser });
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] Login loginRequest)
        {
            try
            {
                var authenticatedUser = _userService.Authenticate(loginRequest);
                var token = _userService.GenerateToken(authenticatedUser);
                return Ok(new { Token = token });
            }
            catch (Exception exception)
            {
                return Unauthorized(new { Message = exception.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePassword changePasswordRequest)
        {
            try
            {
                // Retrieve the username of the authenticated user (from the token)
                var username = User.Identity.Name;

                // Set the username in the request for additional verification
                changePasswordRequest.Username = username;

                var updatedUser = _userService.ChangePassword(changePasswordRequest);
                return Ok(new { Message = "Password changed successfully", User = updatedUser });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the request.", Error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{username}")]
        public IActionResult GetUserProfile(string username)
        {
            try
            {
                var user = _userService.GetByUsername(username);
                var userProfile = new UserProfile
                {
                    Username = user.Username,
                    Email = user.Email,
                    Name = user.Name,
                };
                return Ok(new { UserProfile = userProfile });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the request.", Error = exception.Message });
            }
        }
    }
}
