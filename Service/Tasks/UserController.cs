﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data; //TODO: get more info about this
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text;
using TMPApplication.DTOs.UserDtos;
using TMPApplication.UserTasks;
using TMPDomain.HelperModels;

namespace TMPService.Tasks
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                var response = await _userService.RegisterWithCredentials(registerRequest);
                return Ok(response);
            }
            catch(Exception e)
            {
                return BadRequest(new {Message =" Registration failed", Error = e.Message}); //TODO: use the catch in the service method
            }
        }


        [HttpGet("user-profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfileInfo()
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token"); //TODO: Check more how this works
            var userProfileInfo = await _userService.GetUserProfileAsync(accessToken);

            if( userProfileInfo != null )
            {
                return Ok(userProfileInfo);
            }
            return BadRequest();
        }
        [HttpPatch("update/user")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(string userId, [FromBody] UserProfileUpdateDto updateRequest)
        {
            return await _userService.UpdateUserProfileAsync(userId, updateRequest); //TODO: Update this
        }

        [HttpDelete("delete")]
        [Authorize(Roles = "admin manager")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var deleteResponse = await _userService.DeleteUserAsync(userId);

            if(deleteResponse.StatusCode == 200)
            return Ok(deleteResponse);
            return BadRequest(deleteResponse);
        }


    }
}
