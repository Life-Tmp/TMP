using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.DTOs.UserDtos;
using TMPDomain;
using TMPDomain.Entities;
using TMPDomain.HelperModels;
namespace TMPApplication.MapperProfiles
{
    public class UserMappingProfile: Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<UserRegisterDto, User>().ReverseMap();
            CreateMap<UserProfileDto, User>().ReverseMap();
            CreateMap<UserProfileUpdateDto, User>().ReverseMap();
            CreateMap<User, UserProfileResponseDto>().ReverseMap();
            CreateMap<User, UserInfoDto>().ReverseMap();
            CreateMap<PagedResult<User>, PagedResult<UserInfoDto>>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        }
    }
}
