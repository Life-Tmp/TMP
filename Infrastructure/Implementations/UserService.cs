using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TMP.Application.Interfaces;
using TMPApplication.UserTasks;
using TMPDomain.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TMPApplication.DTOs.UserDtos;
using AutoMapper;

namespace TMPInfrastructure.Implementations
{
    public class UserService : IUserService
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccess; 

        public UserService(IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccess, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccess = httpContextAccess;
            _mapper = mapper;
        }

     

        public async Task<UserProfileDto> GetUserProfileInfo()
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _unitOfWork.Repository<User>().GetById(x => x.Id == userId).FirstOrDefaultAsync(); //DONE: Use Dtos
            var userMapped  = _mapper.Map<UserProfileDto>(user);
            return userMapped;
        }

        public async Task<UserProfileDto> UpdateUserProfile(UserProfileDto userDto)
        {
            var userId = _httpContextAccess.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            if(userId == null)
            {
                return null;
            }
            var userToUpdate = await _unitOfWork.Repository<User>().GetById(x => x.Id == userId).FirstOrDefaultAsync();
            userToUpdate.FirstName = userDto.FirstName;
            userToUpdate.LastName = userDto.LastName;

            _unitOfWork.Repository<User>().Update(userToUpdate);
            _unitOfWork.Complete();
            return userDto;
        }

       
    }
}
