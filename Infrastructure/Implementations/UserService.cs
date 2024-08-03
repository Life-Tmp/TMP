using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using TMP.Application.Interfaces;
using TMPApplication.DTOs.UserDtos;
using TMPApplication.UserTasks;
using TMPDomain.Entities;
using TMPDomain.HelperModels;

namespace TMPInfrastructure.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccess;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IDatabase _cache;
        public UserService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccess,
            IMapper mapper, IConfiguration configuration,
            ILogger<UserService> logger, IHttpClientFactory httpClientFactory,
            IConnectionMultiplexer redis)
        {

            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccess = httpContextAccess;
            _mapper = mapper;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _cache = redis.GetDatabase();
        }




        public async Task<LoginResponse> LoginWithCredentials(LoginRequest loginRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(); // TODO: Why u used client factory instead of new HttpClient();
                var requestBody = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", loginRequest.Email },
                    { "password", loginRequest.Password },
                    { "audience", _configuration["AuthoritySettings:Scope"] },
                    { "client_id", _configuration["AuthoritySettings:ClientId"] },
                    { "client_secret", _configuration["AuthoritySettings:ClientSecret"] },
                    { "scope", "openid profile email" }
                };

                var requestContent = new FormUrlEncodedContent(requestBody);
                var response = await client.PostAsync($"{_configuration["AuthoritySettings:TokenEndpoint"]}", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseContent);
                    var accessToken = responseJson["access_token"]?.ToString();

                    _logger.LogInformation($"User {loginRequest.Email} logged in successfully");

                    return new LoginResponse{ AccessToken = accessToken };
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    var errorJsonObject = JObject.Parse(errorResponse);
                    _logger.LogError(errorResponse, "Invalid login attempt");
                    return new LoginResponse{ Message = (string)errorJsonObject["error_description"], Error = errorResponse };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login");
                return new LoginResponse{ Message = "An error occurred during login", Error = ex.Message };
            }
        }

        public async Task<Dictionary<string, object>> RegisterWithCredentials(RegisterRequest registerRequest, string firstName, string lastName)
        {

            var client = _httpClientFactory.CreateClient(); //TODO:  check this

            var requestBody = new
            {
                email = registerRequest.Email,
                password = registerRequest.Password,
                connection = "Username-Password-Authentication",
                given_name = firstName, 
                family_name = lastName
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://dev-pt8z60gtcfp46ip0.us.auth0.com/api/v2/users")
            {
                Content = requestContent
            };

            request.Headers.Add("Authorization", $"Bearer {await GetManagementApiTokenAsync()}");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                _logger.LogInformation($"User {registerRequest.Email} registered successfully"); //TODO: dont use email as 
                return responseJson;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to register user {errorContent}");
                throw new Exception($"Failed to register user {errorContent}");  //TODO: Make this better 
            }
        }


        private async Task<string> GetManagementApiTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();  //TODO: You are creating a Client for each method, check this to enhance it
            var requestBody = new Dictionary<string, string> // Why u used dictionary - because we have key and value types
            {
                { "grant_type", "client_credentials" },
                { "client_id", _configuration["AuthoritySettings:ClientId"] },
                { "client_secret", _configuration["AuthoritySettings:ClientSecret"] },
                { "audience", _configuration["AuthoritySettings:ManagementEndpoint"] }
            };

            var requestContent = new FormUrlEncodedContent(requestBody);
            var response = await client.PostAsync("https://dev-pt8z60gtcfp46ip0.us.auth0.com/oauth/token", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                return responseJson["access_token"];
            }
            else
            {
                _logger.LogWarning("Unable to obtain Auth0 Management API token");
                throw new ApplicationException("Unable to obtain Auth0 Management API token."); //CHECK: What is Application Exception
            }
        }

        public async Task<UserProfileResponseDto> GetUserProfileAsync(string accessToken)
        {
            try
            {
                var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x=>x.Type == ClaimTypes.NameIdentifier).Value;
                var cacheKey = $"user_profile_{userId}";
               
                var cachedUserProfile = await _cache.StringGetAsync(cacheKey);

                if (cachedUserProfile.HasValue)
                {
                    return JsonConvert.DeserializeObject<UserProfileResponseDto>(cachedUserProfile);
                }

                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_configuration["AuthoritySettings:UserInfoEndpoint"]}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var profileResponse = await client.SendAsync(request);

                if (profileResponse.IsSuccessStatusCode)
                {
                    
                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    var profileDataTest = JsonConvert.DeserializeObject<UserProfileDto>(profileJson);

                    var userToUpdate = await _unitOfWork.Repository<User>().GetById(x => x.Id == profileDataTest.Id).FirstOrDefaultAsync();

                    _mapper.Map(profileDataTest, userToUpdate);
                   
                    _unitOfWork.Repository<User>().Update(userToUpdate);
                    await _unitOfWork.Repository<User>().SaveChangesAsync();

                    var userProfile = _mapper.Map<UserProfileResponseDto>(userToUpdate);

                    await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(userProfile),TimeSpan.FromMinutes(60*12)); //TODO: Remove time span

                    return userProfile;

                }
                else
                {
                    _logger.LogError("Failed to retrieve user profile. Status Code: {StatusCode}", profileResponse.StatusCode);
                    return null;
                }
                       
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user profile");
                return null;
            }
        }

        public async Task<IActionResult> UpdateUserProfileAsync(string userId, UserProfileUpdateDto updateRequest)
        {
            if (string.IsNullOrWhiteSpace(userId) || updateRequest == null)
            {
                return new BadRequestObjectResult("Invalid input.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                object requestBody = null;
                var provider = userId.Split("|")[0];
                if(provider  == "google-oauth2")
                {
                    requestBody = new
                    {
                        user_metadata = new
                        {
                            phone_number = updateRequest.PhoneNumber,
                            birthday = updateRequest.Birthday.ToString("yyyy-MM-dd") // Formated date
                        }
                    };
                }
                else
                {
                     requestBody = new
                    {
                        given_name = updateRequest.FirstName,
                        family_name = updateRequest.LastName,                 //TODO: Check this

                        picture = updateRequest.Picture,
                        user_metadata = new
                        {
                            phone_number = updateRequest.PhoneNumber,
                            birthday = updateRequest.Birthday.ToString("yyyy-MM-dd") // Formated date
                        }
                    };

                }
                

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var auth0ApiToken = await GetManagementApiTokenAsync(); // Token of ManagementAPI

                var auth0ApiUrl = new Uri($"{_configuration["AuthoritySettings:ManagementEndpoint"]}users/{userId}");
                    

                var request = new HttpRequestMessage(HttpMethod.Patch, auth0ApiUrl)
                {
                    Content = requestContent
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth0ApiToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // MEdia Type to represent a media type 
                                                                                                     // can be also text/html - also has quality factor

                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var userToUpdate = await _unitOfWork.Repository<User>().GetById(x => x.Id == userId).FirstOrDefaultAsync();
                    _mapper.Map(updateRequest, userToUpdate);
                    
                    _unitOfWork.Repository<User>().Update(userToUpdate); //To get the object User, not Task<User>
                    _unitOfWork.Complete();


                    var responseContent = await response.Content.ReadAsStringAsync();
                    await _cache.KeyDeleteAsync($"user_profile_{userId}");
                    _logger.LogInformation("User profile updated successfully");
                    return new OkObjectResult(userToUpdate);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update user profile : {errorContent}");
                    return new StatusCodeResult((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user profile");
                return new StatusCodeResult(500);
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(string userId)
        {
            var response = new ApiResponse { };

            if (string.IsNullOrWhiteSpace(userId))
            {
                response.Success = false;
                response.Message = "Invalid user ID.";
                response.StatusCode = 400;
                return response;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var auth0ApiToken = await GetManagementApiTokenAsync();
                var auth0ApiUrl = new Uri($"{_configuration["AuthoritySettings:ManagementEndpoint"]}users/{userId}");

                var request = new HttpRequestMessage(HttpMethod.Delete, auth0ApiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth0ApiToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var httpResponse = await client.SendAsync(request);

                if (httpResponse.IsSuccessStatusCode)
                {
                    response.Success = true;
                    response.Message = "User deleted successfully";
                    response.StatusCode = 200;

                    var userToDelete = await _unitOfWork.Repository<User>().GetById(x => x.Id == userId).FirstOrDefaultAsync();
                    _unitOfWork.Repository<User>().Delete(userToDelete);
                    _unitOfWork.Complete();

                    _logger.LogInformation("User with ID {UserId} deleted successfully", userId);
                    return response;
                }
                else
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    response.Success = false;
                    response.Message = "Failed to delete user";
                    response.StatusCode = (int)httpResponse.StatusCode;

                    _logger.LogError("Failed to delete user with ID {UserId}: {ErrorContent}", userId, errorContent);
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while deleting user: {ex.Message}";
                response.StatusCode = 500;

                _logger.LogError(ex, "An error occurred while deleting user with ID {UserId}", userId);
                return response;
            }

            
        }


        public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "Invalid input.",
                    StatusCode = 400
                };
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = "New password and confirmation do not match.",
                    StatusCode = 400
                };
            }
            var user = _httpContextAccess.HttpContext.User;
            var emailUser = user.FindFirst(ClaimTypes.Email)?.Value;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                
                // Verify the old password
                var authResult = await AuthenticateUserAsync(emailUser, request.OldPassword);
                if (!authResult.Success)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Old password is incorrect.",
                        StatusCode = 400
                    };
                }

                // Update the password
                var client = _httpClientFactory.CreateClient();
                var auth0ApiToken = await GetManagementApiTokenAsync();
                var auth0ApiUrl = new Uri($"{_configuration["AuthoritySettings:ManagementEndpoint"]}users/{userId}");

                var requestBody = new
                {
                    password = request.NewPassword,
                    connection = "Username-Password-Authentication" 
                };

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Patch, auth0ApiUrl)
                {
                    Content = requestContent
                };

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth0ApiToken);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Password changed successfully for user ID {UserId}",userId);
                    return new ApiResponse
                    {
                        Success = true,
                        Message = "Password changed successfully",
                        StatusCode = 200
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to change password for user ID {userId}: {errorContent}");
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Failed to change password",
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user ID {UserId}", userId);
                return new ApiResponse
                {
                    Success = false,
                    Message = $"An error occurred while changing password: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private async Task<ApiResponse> AuthenticateUserAsync(string email, string password)
        {
            var client = _httpClientFactory.CreateClient();
            var requestBody = new Dictionary<string, string>
    {
        { "grant_type", "password" },
        { "username", email },
        { "password", password },
        { "audience", _configuration["AuthoritySettings:Scope"] },
        { "client_id", _configuration["AuthoritySettings:ClientId"] },
        { "client_secret", _configuration["AuthoritySettings:ClientSecret"] },
        { "scope", "openid profile email" }
    };

            var requestContent = new FormUrlEncodedContent(requestBody);
            var response = await client.PostAsync($"{_configuration["AuthoritySettings:TokenEndpoint"]}", requestContent);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse { Success = true };
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse
                {
                    Success = false,
                    Message = "Authentication failed",
                    StatusCode = (int)response.StatusCode
                };
            }
        }

        public async Task<UserStatistics> GetUserStatistics()
        {
            try
            {
                var allUsersCount = await _unitOfWork.Repository<User>().GetAll().CountAsync();
                var verifiedUsersCount = await _unitOfWork.Repository<User>().GetByCondition(x => x.IsEmailVerified).CountAsync();
                var newSignUpsCount = await _unitOfWork.Repository<User>().GetByCondition(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)).CountAsync();


                var result = new UserStatistics
                {
                    AllUsers = allUsersCount,
                    VerifiedUsers = verifiedUsersCount,
                    NewSignsUps = newSignUpsCount
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving user statistics.", ex);
            }
        }
    }

}
