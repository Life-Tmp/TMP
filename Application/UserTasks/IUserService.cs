using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.DTOs.UserDtos;
using TMPDomain;
using TMPDomain.Entities;

namespace TMPApplication.UserTasks
{
    public interface IUserService
    {
        
        Task<UserProfileDto> GetUserProfileInfo();
        Task<UserProfileDto> UpdateUserProfile(UserProfileDto userRegister);

    }
}
