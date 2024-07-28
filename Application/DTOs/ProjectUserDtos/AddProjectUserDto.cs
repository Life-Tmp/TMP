﻿using TMPDomain.Enumerations;

namespace TMP.Application.DTOs.ProjectUserDtos
{
    public class AddProjectUserDto
    {
        public int ProjectId { get; set; }
        public string UserId { get; set; }
        public MemberRole Role { get; set; }
    }
}

