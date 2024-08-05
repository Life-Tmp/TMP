using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.DTOs.UserDtos;
using TMPDomain;
using TMPDomain.Entities;
using TMPDomain.HelperModels;

namespace TMPApplication.Interfaces.UserTasks
{
    public interface IUserService
    {
        Task<LoginResponse> LoginWithCredentials(LoginRequest loginRequest);
        Task<Dictionary<string, object>> RegisterWithCredentials(RegisterRequest registerRequest, string firstName, string lastName);
        Task<UserProfileResponseDto> GetUserProfileAsync(string acessToken);
        Task<IActionResult> UpdateUserProfileAsync(string userId, UserProfileUpdateDto updateRequest);
        Task<ApiResponse> DeleteUserAsync(string userId);
        Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request);
        Task<UserStatistics> GetUserStatistics();
        Task<PagedResult<UserInfoDto>> GetPagedAsync(int pageNumber, int pageSize);

    }
}
