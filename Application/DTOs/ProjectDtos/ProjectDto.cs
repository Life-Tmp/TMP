using System;
using System.Collections.Generic;
using TMP.Application.DTOs.TaskDtos;
using TMPApplication.DTOs.UserDtos;

namespace TMP.Application.DTOs.ProjectDtos
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<TaskDto> Tasks { get; set; } = new List<TaskDto>();
        public ICollection<UserProfileDto> Users { get; set; } = new List<UserProfileDto>();
    }
}
