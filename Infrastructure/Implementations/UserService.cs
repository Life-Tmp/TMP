using AutoMapper;
using Mailjet.Client.Resources;
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
using User = TMPDomain.Entities.User;
using TMPDomain.HelperModels;
using TMPApplication.Interfaces.UserTasks;

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

        #region Authentication
        public async Task<LoginResponse> LoginWithCredentials(LoginRequest loginRequest)
        {
            try
            {
                _logger.LogInformation("User attempting login with email: {Email}", loginRequest.Email);

                var client = _httpClientFactory.CreateClient();
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

                    _logger.LogInformation("User {Email} logged in successfully", loginRequest.Email);

                    return new LoginResponse { AccessToken = accessToken };
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    var errorJsonObject = JObject.Parse(errorResponse);
                    _logger.LogError("Login failed for {Email}: {Error}", loginRequest.Email, errorJsonObject["error_description"]);
                    return new LoginResponse { Message = (string)errorJsonObject["error_description"], Error = errorResponse };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login for user {Email}", loginRequest.Email);
                return new LoginResponse { Message = "An error occurred during login", Error = ex.Message };
            }
        }

        public async Task<Dictionary<string, object>> RegisterWithCredentials(RegisterRequest registerRequest, string firstName, string lastName)
        {
            try
            {
                _logger.LogInformation("Registering new user: {Email}", registerRequest.Email);

                var client = _httpClientFactory.CreateClient();
                var requestBody = new
                {
                    email = registerRequest.Email,
                    password = registerRequest.Password,
                    connection = "Username-Password-Authentication",
                    given_name = firstName,
                    family_name = lastName
                };

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_configuration["AuthoritySettings:ManagementEndpoint"]}/users")
                {
                    Content = requestContent
                };

                request.Headers.Add("Authorization", $"Bearer {await GetManagementApiTokenAsync()}");

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    _logger.LogInformation("User {Email} registered successfully", registerRequest.Email);
                    return responseJson;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to register user {Email}: {Error}", registerRequest.Email, errorContent);
                    throw new Exception($"Failed to register user: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user {Email}", registerRequest.Email);
                throw;
            }
        }

        private async Task<string> GetManagementApiTokenAsync()
        {
            try
            {
                _logger.LogInformation("Fetching management API token");

                var client = _httpClientFactory.CreateClient();
                var requestBody = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _configuration["AuthoritySettings:ClientId"] },
                    { "client_secret", _configuration["AuthoritySettings:ClientSecret"] },
                    { "audience", _configuration["AuthoritySettings:ManagementEndpoint"] }
                };

                var requestContent = new FormUrlEncodedContent(requestBody);
                var response = await client.PostAsync(_configuration["AuthoritySettings:TokenEndpoint"], requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                    _logger.LogInformation("Management API token fetched successfully");
                    return responseJson["access_token"];
                }
                else
                {
                    _logger.LogWarning("Failed to obtain management API token");
                    throw new ApplicationException("Unable to obtain Auth0 Management API token.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while obtaining management API token");
                throw;
            }
        }
        #endregion

        #region Profile
        public async Task<UserProfileResponseDto> GetUserProfileAsync(string accessToken)
        {
            try
            {
                var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                var cacheKey = $"user_profile_{userId}";

                _logger.LogInformation("Fetching user profile for user ID: {UserId}", userId);

                var cachedUserProfile = await _cache.StringGetAsync(cacheKey);

                if (cachedUserProfile.HasValue)
                {
                    _logger.LogInformation("Returning cached user profile for user ID: {UserId}", userId);
                    return JsonConvert.DeserializeObject<UserProfileResponseDto>(cachedUserProfile);
                }

                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["AuthoritySettings:UserInfoEndpoint"]);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var profileResponse = await client.SendAsync(request);

                if (profileResponse.IsSuccessStatusCode)
                {
                    var profileJson = await profileResponse.Content.ReadAsStringAsync();
                    var profileData = JsonConvert.DeserializeObject<UserProfileDto>(profileJson);

                    var userToUpdate = await _unitOfWork.Repository<User>().GetById(x => x.Id == profileData.Id).FirstOrDefaultAsync();

                    _mapper.Map(profileData, userToUpdate);

                    _unitOfWork.Repository<User>().Update(userToUpdate);
                    await _unitOfWork.Repository<User>().SaveChangesAsync();

                    var userProfile = _mapper.Map<UserProfileResponseDto>(userToUpdate);

                    await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(userProfile), TimeSpan.FromMinutes(60 * 12));

                    _logger.LogInformation("User profile fetched and cached successfully for user ID: {UserId}", userId);
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
                _logger.LogError(ex, "An error occurred while retrieving user profile for user ID: {UserId}", _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
                return null;
            }
        }

        public async Task<IActionResult> UpdateUserProfileAsync(string userId, UserProfileUpdateDto updateRequest)
        {
            if (string.IsNullOrWhiteSpace(userId) || updateRequest == null)
            {
                _logger.LogWarning("Invalid input received for updating user profile");
                return new BadRequestObjectResult("Invalid input");
            }

            try
            {
                _logger.LogInformation("Updating profile for user ID: {UserId}", userId);

                var client = _httpClientFactory.CreateClient();
                object requestBody;

                var provider = userId.Split("|")[0];
                if (provider == "google-oauth2")
                {
                    requestBody = new
                    {
                        user_metadata = new
                        {
                            phone_number = updateRequest.PhoneNumber,
                            //birthday = updateRequest.Birthday.ToString("yyyy-MM-dd") // Formated date
                        }
                    };
                }
                else
                {
                    requestBody = new
                    {
                        given_name = updateRequest.FirstName,
                        family_name = updateRequest.LastName,

                        //picture = updateRequest.Picture,
                        user_metadata = new
                        {
                            phone_number = updateRequest.PhoneNumber,
                            //birthday = updateRequest.Birthday.ToString("yyyy-MM-dd") // Formated date
                        }
                    };
                }

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var auth0ApiToken = await GetManagementApiTokenAsync();
                var auth0ApiUrl = new Uri($"{_configuration["AuthoritySettings:ManagementEndpoint"]}users/{userId}");

                var request = new HttpRequestMessage(HttpMethod.Patch, auth0ApiUrl)
                {
                    Content = requestContent
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth0ApiToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var userToUpdate = await _unitOfWork.Repository<User>().GetById(x => x.Id == userId).FirstOrDefaultAsync();
                    _mapper.Map(updateRequest, userToUpdate);

                    _unitOfWork.Repository<User>().Update(userToUpdate);
                    _unitOfWork.Complete();

                    await _cache.KeyDeleteAsync($"user_profile_{userId}");

                    _logger.LogInformation("User profile updated successfully for user ID: {UserId}", userId);
                    return new OkObjectResult(userToUpdate);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to update user profile for user ID: {UserId}. Error: {Error}", userId, errorContent);
                    return new StatusCodeResult((int)response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user profile for user ID: {UserId}", userId);
                return new StatusCodeResult(500);
            }
        }
        #endregion

        #region User Management
        public async Task<ApiResponse> DeleteUserAsync(string userId)
        {
            var response = new ApiResponse { };

            if (string.IsNullOrWhiteSpace(userId))
            {
                response.Success = false;
                response.Message = "Invalid user ID.";
                response.StatusCode = 400;
                _logger.LogWarning("Attempted to delete user with invalid user ID");
                return response;
            }

            try
            {
                _logger.LogInformation("Deleting user with ID: {UserId}", userId);

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
                _logger.LogWarning("Invalid input received for changing password");
                return new ApiResponse
                {
                    Success = false,
                    Message = "Invalid input.",
                    StatusCode = 400
                };
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("New password and confirmation do not match");
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

            _logger.LogInformation("Changing password for user ID: {UserId}", userId);

            try
            {
                var authResult = await AuthenticateUserAsync(emailUser, request.OldPassword);
                if (!authResult.Success)
                {
                    _logger.LogWarning("Old password is incorrect for user ID: {UserId}", userId);
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Old password is incorrect.",
                        StatusCode = 400
                    };
                }

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
                    _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
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
                    _logger.LogError("Failed to change password for user ID: {UserId}. Error: {Error}", userId, errorContent);
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
                _logger.LogError(ex, "An error occurred while changing password for user ID: {UserId}", userId);
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
                _logger.LogWarning("Authentication failed for email: {Email}. Error: {Error}", email, errorContent);
                return new ApiResponse
                {
                    Success = false,
                    Message = "Authentication failed",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        #endregion

        #region Statistics
        public async Task<UserStatistics> GetUserStatistics()
        {
            try
            {
                _logger.LogInformation("Fetching user statistics");

                var allUsersCount = await _unitOfWork.Repository<User>().GetAll().CountAsync();
                var verifiedUsersCount = await _unitOfWork.Repository<User>().GetByCondition(x => x.IsEmailVerified).CountAsync();
                var newSignUpsCount = await _unitOfWork.Repository<User>().GetByCondition(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)).CountAsync();

                var result = new UserStatistics
                {
                    AllUsers = allUsersCount,
                    VerifiedUsers = verifiedUsersCount,
                    NewSignsUps = newSignUpsCount
                };

                _logger.LogInformation("User statistics fetched successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user statistics");
                throw new ApplicationException("An error occurred while retrieving user statistics", ex);
            }
        }

        public async Task<PagedResult<UserInfoDto>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching paged users. Page number: {PageNumber}, Page size: {PageSize}", pageNumber, pageSize);
                var allUsers = _unitOfWork.Repository<User>().GetAll();

                if (allUsers == null)
                {
                    _logger.LogWarning("No users found");
                    return new PagedResult<UserInfoDto>();
                }

                var userQuery = await allUsers.GetPagedAsync(pageNumber, pageSize);
                var usersDtos = _mapper.Map<PagedResult<UserInfoDto>>(userQuery);

                _logger.LogInformation("Paged users fetched successfully");
                return usersDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching paged users");
                throw new ApplicationException("An error occurred while fetching paged users", ex);
            }
        }
        #endregion
    }
}
