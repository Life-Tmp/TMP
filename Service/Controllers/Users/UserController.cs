using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data; //TODO: get more info about this
using Microsoft.AspNetCore.Mvc;
using TMPApplication.DTOs.UserDtos;
using TMPApplication.UserTasks;
using TMPDomain.HelperModels;

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

        [HttpPost("login")]
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


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest, string firstName, string lastName)
        {
            try
            {
                var response = await _userService.RegisterWithCredentials(registerRequest, firstName, lastName);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = " Registration failed", Error = e.Message });
            }
        }


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


        [HttpPut("profile/update")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserProfileUpdateDto updateRequest)
        {
            return await _userService.UpdateUserProfileAsync(userId, updateRequest); //TODO: Update this
        }


        [HttpDelete("delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var deleteResponse = await _userService.DeleteUserAsync(userId);

            if (deleteResponse.StatusCode == 200)
                return Ok(deleteResponse);
            return BadRequest(deleteResponse);
        }


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

        [HttpGet("activity")]
        [Authorize]
        public async Task<IActionResult> GetUsersStatistics()
        {
            var userStatistics = await _userService.GetUserStatistics();
            if (userStatistics == null)
                return NoContent();
            return Ok(userStatistics);
        }
    }
}
