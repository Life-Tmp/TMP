using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMPApplication.DTOs.UserDtos;
using TMPDomain.HelperModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.Data;
using TMPApplication.Interfaces.UserTasks;

namespace TMPService.Controllers.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
        }

        #region Authentication

        /// <summary>
        /// Authenticates a user and returns an access token.
        /// </summary>
        /// <param name="loginRequest">The login request containing credentials.</param>
        /// <returns>200 OK with the access token; 401 Unauthorized if authentication fails.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var loginResponse = await _userService.LoginWithCredentials(loginRequest);

            if (loginResponse.AccessToken != null)
            {
                return Ok(loginResponse.AccessToken);
            }
            else
            {
                return Unauthorized(loginResponse.Message);
            }
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerRequest">The registration request containing user details.</param>
        /// <param name="firstName">The first name of the user.</param>
        /// <param name="lastName">The last name of the user.</param>
        /// <returns>200 OK if registration is successful; 400 Bad Request if registration fails.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest, string firstName, string lastName)
        {
            try
            {
                var response = await _userService.RegisterWithCredentials(registerRequest, firstName, lastName);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = "Registration failed", Error = e.Message });
            }
        }
        #endregion

        #region Profile

        /// <summary>
        /// Retrieves the profile information of the currently authenticated user.
        /// </summary>
        /// <returns>200 OK with the user's profile information; 400 Bad Request if retrieval fails.</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfileInfo()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var userProfileInfo = await _userService.GetUserProfileAsync(accessToken);

            if (userProfileInfo != null)
            {
                return Ok(userProfileInfo);
            }
            return BadRequest();
        }

        /// <summary>
        /// Updates the profile information of a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="updateRequest">The updated profile information.</param>
        /// <returns>200 OK if the update is successful; 400 Bad Request if the update fails.</returns>
        [HttpPut("profile/update")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserProfileUpdateDto updateRequest)
        {
            return await _userService.UpdateUserProfileAsync(userId, updateRequest);
        }
        #endregion

        #region User Management

        /// <summary>
        /// Deletes a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>200 OK if the deletion is successful; 400 Bad Request if the deletion fails.</returns>
        [HttpDelete("delete")]
        [Authorize(Policy = "AdminRoleRequired")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var deleteResponse = await _userService.DeleteUserAsync(userId);

            if (deleteResponse.StatusCode == 200)
                return Ok(deleteResponse);
            return BadRequest(deleteResponse);
        }

        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="request">The change password request containing old and new passwords.</param>
        /// <returns>200 OK if the password change is successful; 400 Bad Request if the change fails.</returns>
        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }

            var response = await _userService.ChangePasswordAsync(request);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(response.StatusCode, response);
            }
        }
        #endregion

        #region Statistics

        /// <summary>
        /// Retrieves statistics about the authenticated user's activity.
        /// </summary>
        /// <returns>200 OK with the user's statistics; 204 No Content if no statistics are available.</returns>
        [HttpGet("activity")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsersStatistics()
        {
            var userStatistics = await _userService.GetUserStatistics();
            if (userStatistics == null)
                return NoContent();
            return Ok(userStatistics);
        }

        /// <summary>
        /// Retrieves a paginated list of users.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of users per page.</param>
        /// <returns>200 OK with a paginated list of users; 404 Not Found if no users are found.</returns>
        [HttpGet("paged")]
        [Authorize]
        public async Task<IActionResult> GetPagedUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagedUsers = await _userService.GetPagedAsync(pageNumber, pageSize);

            if (pagedUsers == null || pagedUsers.Items.Count == 0)
            {
                return NotFound("No users found");
            }

            return Ok(pagedUsers);
        }
        #endregion
    }
}
