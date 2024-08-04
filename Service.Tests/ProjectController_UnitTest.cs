using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.ProjectUserDtos;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.DTOs.TeamDtos;
using TMPApplication.DTOs.ProjectDtos;
using TMPApplication.Interfaces;
using TMPApplication.Interfaces.Projects;
using TMPService.Controllers.Projects;

namespace TMP.Tests
{
    public class ProjectController_UnitTest
    {
        private readonly ProjectController _controller;
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly Mock<ISearchService<ProjectDto>> _searchServiceMock;
        private readonly string _userId;
        public ProjectController_UnitTest()
        {            
            _projectServiceMock = new Mock<IProjectService>();
            _searchServiceMock = new Mock<ISearchService<ProjectDto>>();
            _controller = new ProjectController(_projectServiceMock.Object, _searchServiceMock.Object);
            _userId = "user1";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext 
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId)
                }, "mock"))
                }
            };
            
        }
        

        [Fact]
        public async Task GetProject_ReturnsOkResult_WithProjects()
        {
            // Arrange
            var mockProjects = new List<ProjectDto> {
                new ProjectDto { Id = 1, Name = "Project Skydance", Description = "This is a project" },
                new ProjectDto { Id = 2, Name = "Project Nevermind", Description = "This is a project" } 
            };

            _projectServiceMock.Setup(service => service.GetAllProjectsAsync()).ReturnsAsync(mockProjects);

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<ProjectDto>>>(result); //Result gets the action 
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProjects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(okResult.Value);
            Assert.Equal(mockProjects.Count,returnedProjects.Count());

        }

        [Fact]
        public async Task Get_Project_ReturnsOkResult_WithProject()
        {
            // Arrange
            var projectId = 1;
            var mockProject =  new ProjectDto { Id = projectId, Name = "Project Skydance", Description = "This is a project" };
            _projectServiceMock.Setup(service => service.GetProjectByIdAsync(projectId)).ReturnsAsync(mockProject);

            // Act
            var result = await _controller.GetProject(projectId);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProject = Assert.IsType<ProjectDto>(okResult.Value);
            Assert.Equal(mockProject, returnedProject);
        }

        [Fact]
        public async Task GetMyProjects_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // No claims, so it means that its not authenticated, go authenticate.
                }
            };

            // Act
            var result = await _controller.GetMyProjects();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetMyProjects_ReturnsOkResult_WithProjects() 
        {
            // Arrange
            var userId = "user1";
            var mockProjects = new List<ProjectDto> { new ProjectDto { Id = 1, Name = "Project 1" } };
            _projectServiceMock.Setup(service => service.GetProjectsByUserAsync(userId)).ReturnsAsync(mockProjects);

            // Act
            var result = await _controller.GetMyProjects();

            // Assert
           
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProjects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(okResult.Value);
            Assert.Single(returnedProjects);
        }

        [Fact]
        public async Task GetProjectUsers_ReturnsOkResult_WithProjectUsers()
        {
            // Arrange
            var projectId = 1;
            var mockProjectUsers = new ProjectUsersDto { ProjectId = projectId, Users = new List<ProjectUserDto> { new ProjectUserDto { UserId = "user1" } } };
            _projectServiceMock.Setup(service => service.GetProjectUsersAsync(projectId)).ReturnsAsync(mockProjectUsers);

            // Act
            var result = await _controller.GetProjectUsers(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProjectUsers = Assert.IsType<ProjectUsersDto>(okResult.Value);
            Assert.Equal(projectId, returnedProjectUsers.ProjectId);
        }

        [Fact]
        public async Task GetProjectTeams_ReturnsOkResult_WithProjectTeams()
        {
            // Arrange
            var projectId = 1;
            var mockProjectTeams = new ProjectTeamsDto { ProjectId = projectId, Teams = new List<TeamDto> { new TeamDto { Id = 1, Name="team1",} } };
            _projectServiceMock.Setup(service => service.GetProjectTeamsAsync(projectId)).ReturnsAsync(mockProjectTeams);

            // Act
            var result = await _controller.GetProjectTeams(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProjectTeams = Assert.IsType<ProjectTeamsDto>(okResult.Value);
            Assert.Equal(projectId, returnedProjectTeams.ProjectId);
        }


        [Fact]
        public async Task GetProjectTasks_ReturnsOkResult_WithProjectTasks()
        {
            // Arrange
            var projectId = 1;
            var mockProjectTasks = new ProjectTasksDto
            {
                ProjectId = projectId,
                Tasks = new List<TaskDto>
            {
                new TaskDto
                {
                    Id = 1, Description = "Task description lets go", Tags = new List<string> { "tag1, tag2" },
                }
            }
            };

            _projectServiceMock.Setup(service => service.GetProjectTasksAsync(projectId)).ReturnsAsync(mockProjectTasks);

            // Act
            var result = await _controller.GetProjectTasks(projectId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProjectTasks = Assert.IsType<ProjectTasksDto>(okResult.Value);
            Assert.Equal(projectId, returnedProjectTasks.ProjectId);
        }

        [Fact]
        public async Task AssignTeamToProject_ReturnsOkResult_WhenTeamAssigned()
        {
            // Arrange
            var manageTeamDto = new ManageProjectTeamDto { ProjectId = 1, TeamId = 1 };
            _projectServiceMock.Setup(service => service.AssignTeamToProjectAsync(manageTeamDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.AssignTeamToProject(manageTeamDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Team assigned to project successfully.", okResult.Value);
        }

        [Fact]
        public async Task SearchProjects_ReturnsOkResult_WithProjects()
        {
            // Arrange
            var searchQuery = "Project";
            var mockProjects = new List<ProjectDto>
            {
                new ProjectDto { Id = 1, Name = "Project Skydance", Description = "This is a project" },
                new ProjectDto { Id = 2, Name = "Project Tsushima", Description = "This is another project" }
            };
            _searchServiceMock.Setup(service => service.SearchDocumentAsync(searchQuery, "projects"))
                              .ReturnsAsync(mockProjects);

            // Act
            var result = await _controller.SearchProjects(searchQuery);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(okResult.Value);
            Assert.Equal(mockProjects.Count, returnedProjects.Count());
        }
    }

}
