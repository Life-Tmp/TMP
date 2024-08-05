using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TMP.Application.DTOs.TeamDtos;
using TMPApplication.Interfaces;
using TMP.Service.Controllers;
using TMPDomain.Enumerations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.Interfaces;

namespace TMP.Service.Tests
{
    public class TeamController_UnitTest
    {
        private readonly Mock<ITeamService> _teamServiceMock;
        private readonly Mock<ILogger<TeamController>> _loggerServiceMock;
        private readonly TeamController _controller;

        public TeamController_UnitTest()
        {
            _teamServiceMock = new Mock<ITeamService>();
            _loggerServiceMock = new Mock<ILogger<TeamController>>();

            _controller = new TeamController(
                _teamServiceMock.Object,
                _loggerServiceMock.Object
            );

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "user1")
                    }, "mock"))
                }
            };
        }

        [Fact]
        public async Task GetTeams_ReturnsOkResult_WithTeams()
        {
            var mockTeams = new List<TeamDto> { new TeamDto { Id = 1, Name = "Team 1" } };
            _teamServiceMock.Setup(service => service.GetAllTeamsAsync()).ReturnsAsync(mockTeams);

            var result = await _controller.GetTeams();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TeamDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTeams = Assert.IsType<List<TeamDto>>(okResult.Value);
            Assert.Single(returnedTeams);
        }

        [Fact]
        public async Task GetTeams_ReturnsOkResult_WithEmptyList()
        {
            _teamServiceMock.Setup(service => service.GetAllTeamsAsync()).ReturnsAsync(new List<TeamDto>());

            var result = await _controller.GetTeams();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TeamDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTeams = Assert.IsType<List<TeamDto>>(okResult.Value);
            Assert.Empty(returnedTeams);
        }

        [Fact]
        public async Task GetTeam_ReturnsOkResult_WithTeam()
        {
            var teamDto = new TeamDto { Id = 1, Name = "Team 1" };
            _teamServiceMock.Setup(service => service.GetTeamByIdAsync(1)).ReturnsAsync(teamDto);

            var result = await _controller.GetTeam(1);

            var actionResult = Assert.IsType<ActionResult<TeamDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTeam = Assert.IsType<TeamDto>(okResult.Value);
            Assert.Equal("Team 1", returnedTeam.Name);
        }

        [Fact]
        public async Task GetTeam_ReturnsNotFound_WhenTeamDoesNotExist()
        {
            _teamServiceMock.Setup(service => service.GetTeamByIdAsync(1)).ReturnsAsync((TeamDto)null);

            var result = await _controller.GetTeam(1);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUserTeams_ReturnsOkResult_WithTeams()
        {
            var mockTeams = new List<TeamDto> { new TeamDto { Id = 1, Name = "Team 1" } };
            _teamServiceMock.Setup(service => service.GetUserTeamsAsync("user1")).ReturnsAsync(mockTeams);

            var result = await _controller.GetUserTeams();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TeamDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedTeams = Assert.IsType<List<TeamDto>>(okResult.Value);
            Assert.Single(returnedTeams);
        }

        [Fact]
        public async Task GetTeamMembers_ReturnsOkResult_WithMembers()
        {
            var mockMembers = new List<TeamMemberDto> { new TeamMemberDto { FirstName = "John", LastName = "Doe", Role = MemberRole.Developer } };
            _teamServiceMock.Setup(service => service.GetTeamMembersAsync(1)).ReturnsAsync(mockMembers);

            var result = await _controller.GetTeamMembers(1);

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TeamMemberDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedMembers = Assert.IsType<List<TeamMemberDto>>(okResult.Value);
            Assert.Single(returnedMembers);
        }

        [Fact]
        public async Task GetTeamProjects_ReturnsOkResult_WithProjects()
        {
            var mockProjects = new TeamProjectsDto { Projects = new List<ProjectDto> { new ProjectDto { Id = 1, Name = "Project 1" } } };
            _teamServiceMock.Setup(service => service.GetTeamProjectsAsync(1)).ReturnsAsync(mockProjects);

            var result = await _controller.GetTeamProjects(1);

            var actionResult = Assert.IsType<ActionResult<TeamProjectsDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedProjects = Assert.IsType<TeamProjectsDto>(okResult.Value);
            Assert.Single(returnedProjects.Projects);
        }

        [Fact]
        public async Task AddTeam_ReturnsCreatedAtActionResult_WhenSuccessful()
        {
            var newTeam = new AddTeamDto { Name = "New Team" };
            var teamDto = new TeamDto { Id = 1, Name = "New Team" };
            _teamServiceMock.Setup(service => service.AddTeamAsync(newTeam, "user1")).ReturnsAsync(teamDto);

            var result = await _controller.AddTeam(newTeam);

            var actionResult = Assert.IsType<ActionResult<TeamDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedTeam = Assert.IsType<TeamDto>(createdAtActionResult.Value);
            Assert.Equal("New Team", returnedTeam.Name);
        }

        [Fact]
        public async Task AddUserToTeam_ReturnsOkResult_WhenSuccessful()
        {
            var addTeamMemberDto = new AddTeamMemberDto { TeamId = 1, UserId = "user1" };
            _teamServiceMock.Setup(service => service.AddUserToTeamAsync(addTeamMemberDto)).ReturnsAsync(true);

            var result = await _controller.AddUserToTeam(addTeamMemberDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User added to the team successfully.", okResult.Value);
        }

        [Fact]
        public async Task UpdateTeam_ReturnsNoContentResult_WhenSuccessful()
        {
            var updateDto = new AddTeamDto { Name = "Updated Team" };
            _teamServiceMock.Setup(service => service.UpdateTeamAsync(1, updateDto)).ReturnsAsync(true);

            var result = await _controller.UpdateTeam(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUserRoleInTeam_ReturnsOkResult_WhenSuccessful()
        {
            var updateRoleDto = new UpdateTeamMemberRoleDto { UserId = "user1", NewRole = MemberRole.TeamLead };
            _teamServiceMock.Setup(service => service.UpdateUserRoleInTeamAsync(1, "user1", MemberRole.TeamLead)).ReturnsAsync(true);

            var result = await _controller.UpdateUserRoleInTeam(1, updateRoleDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Role updated successfully.", okResult.Value);
        }

        [Fact]
        public async Task DeleteTeam_ReturnsNoContentResult_WhenSuccessful()
        {
            _teamServiceMock.Setup(service => service.DeleteTeamAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteTeam(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveUserFromTeam_ReturnsOkResult_WhenSuccessful()
        {
            var removeUserFromTeamDto = new RemoveTeamMemberDto { TeamId = 1, UserId = "user1" };
            _teamServiceMock.Setup(service => service.RemoveUserFromTeamAsync(removeUserFromTeamDto)).ReturnsAsync(true);

            var result = await _controller.RemoveUserFromTeam(removeUserFromTeamDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User removed from team successfully.", okResult.Value);
        }
    }
}
