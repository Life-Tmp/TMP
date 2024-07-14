using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RestSharp;
using TMPApplication.DTOs.UserDtos;
using TMPApplication.UserTasks;
using static TMPInfrastructure.Implementations.UserService;

namespace TMPService.Tasks
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
           
        }

       

        [HttpGet("profile")]
        public async Task<IActionResult> UserProfile()
        {
            var userProfile = await _userService.GetUserProfileInfo();
            return Ok(userProfile);
        }
        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateUserProfile(UserProfileDto userProfile)
        {
            var userUpdated = await _userService.UpdateUserProfile(userProfile);

            return Ok(userUpdated);
        }
       
        
    }
}
