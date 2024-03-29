﻿using Beemo_Server.Data.Models.Entities;
using Beemo_Server.Data.Models.Requests.User;
using Beemo_Server.Data.Models.TransferObjects;
using Beemo_Server.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beemo_Server.Controllers
{
    [Route("api/users")]
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
                return Ok(new { Message = "Registration successful", Username = createdUser.Username });
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
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] Verify verificationRequest)
        {
            try
            {
                var verifiedUser = _userService.Verify(verificationRequest);
                return Ok(new { Message = "Verification successful", Username = verifiedUser.Username });
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        [Authorize]
        [HttpPost("resend-verification-code")]
        public IActionResult ResendCode()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = _userService.GetByUsername(username);

                _userService.ResendVerificationToken(user);

                return Ok();
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePassword changePasswordRequest)
        {
            try
            {
                // Retrieve the username of the authenticated user (from the token)
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;

                if (username != changePasswordRequest.Username || email != changePasswordRequest.Email)
                {
                    return Unauthorized(new { Message = "User does not match the authorized user." });
                }

                var updatedUser = _userService.ChangePassword(changePasswordRequest);
                return Ok(new { Message = "Password changed successfully", Username = updatedUser.Username });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the request.", Error = exception.Message });
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
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                };
                return Ok(new { UserProfile = userProfile });
            }
            catch (Exception exception)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the request.", Error = exception.Message });
            }
        }

        [Authorize]
        [HttpPut("update")]
        public IActionResult Update([FromBody] User user)
        {
            try
            {
                var updatedUser = _userService.Update(user);
                return Ok(new { Message = "Update successful", Username = updatedUser.Username });
            }
            catch (Exception exception)
            {
                return BadRequest(new { Message = exception.Message });
            }
        }
    }
}
