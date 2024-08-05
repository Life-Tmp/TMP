using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.TeamDtos;
using TMP.Application.Interfaces;
using TMPDomain.Entities;
using TMPDomain.Enumerations;
using TMPDomain.Validations;
using FluentValidation;

namespace TMP.Infrastructure.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TeamService> _logger;
        private readonly IValidator<Team> _teamValidator;
        private readonly IValidator<TeamMember> _teamMemberValidator;
        private readonly IValidator<User> _userValidator;

        public TeamService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TeamService> logger,
            IValidator<Team> teamValidator,
            IValidator<TeamMember> teamMemberValidator,
            IValidator<User> userValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _teamValidator = teamValidator;
            _teamMemberValidator = teamMemberValidator;
            _userValidator = userValidator;
        }

        #region Read
        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            _logger.LogInformation("Fetching all teams");
            var teams = await _unitOfWork.Repository<Team>().GetAll().ToListAsync();
            var teamDtos = _mapper.Map<IEnumerable<TeamDto>>(teams);
            _logger.LogInformation("Fetched {Count} teams", teamDtos.Count());
            return teamDtos;
        }

        public async Task<TeamDto> GetTeamByIdAsync(int id)
        {
            _logger.LogInformation("Fetching team with ID: {TeamId}", id);
            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return null;
            }

            var teamDto = _mapper.Map<TeamDto>(team);
            _logger.LogInformation("Fetched team with ID: {TeamId}", id);
            return teamDto;
        }

        public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId)
        {
            _logger.LogInformation("Fetching teams for user with ID: {UserId}", userId);
            var teams = await _unitOfWork.Repository<TeamMember>()
                .GetByCondition(tm => tm.UserId == userId)
                .Include(tm => tm.Team)
                .Select(tm => tm.Team)
                .ToListAsync();

            var teamDtos = _mapper.Map<IEnumerable<TeamDto>>(teams);
            _logger.LogInformation("Fetched {Count} teams for user with ID: {UserId}", teamDtos.Count(), userId);
            return teamDtos;
        }

        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int teamId)
        {
            _logger.LogInformation("Fetching team members for team with ID: {TeamId}", teamId);
            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == teamId)
                .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", teamId);
                return null;
            }

            var teamMemberDtos = _mapper.Map<IEnumerable<TeamMemberDto>>(team.TeamMembers);
            _logger.LogInformation("Fetched {Count} team members for team with ID: {TeamId}", teamMemberDtos.Count(), teamId);
            return teamMemberDtos;
        }

        public async Task<TeamProjectsDto> GetTeamProjectsAsync(int teamId)
        {
            _logger.LogInformation("Fetching projects for team with ID: {TeamId}", teamId);
            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == teamId)
                .Include(t => t.ProjectTeams)
                    .ThenInclude(pt => pt.Project)
                        .ThenInclude(p => p.Columns)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", teamId);
                return null;
            }

            var teamProjectsDto = new TeamProjectsDto
            {
                TeamId = teamId,
                Projects = team.ProjectTeams.Select(pt => new ProjectDto
                {
                    Id = pt.Project.Id,
                    Name = pt.Project.Name,
                    Description = pt.Project.Description,
                    CreatedByUserId = pt.Project.CreatedByUserId,
                    Columns = pt.Project.Columns.Select(c => c.Name).ToList()
                }).ToList()
            };

            _logger.LogInformation("Fetched projects for team with ID: {TeamId}", teamId);
            return teamProjectsDto;
        }
        #endregion

        #region Create
        public async Task<TeamDto> AddTeamAsync(AddTeamDto newTeam, string userId)
        {
            _logger.LogInformation("Adding new team");

            // Mapping and Validation
            var team = _mapper.Map<Team>(newTeam);
            team.CreatedAt = DateTime.UtcNow;
            team.UpdatedAt = DateTime.UtcNow;

            var validationResult = await _teamValidator.ValidateAsync(team);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for new team: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            // Create Team and assign the creator as TeamLead
            _unitOfWork.Repository<Team>().Create(team);
            _unitOfWork.Complete();

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role = MemberRole.TeamLead
            };

            validationResult = await _teamMemberValidator.ValidateAsync(teamMember);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for team member: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<TeamMember>().Create(teamMember);
            _unitOfWork.Complete();

            var teamDto = _mapper.Map<TeamDto>(team);
            _logger.LogInformation("Added new team with ID: {TeamId}", team.Id);
            return teamDto;
        }

        public async Task<bool> AddUserToTeamAsync(AddTeamMemberDto addTeamMemberDto)
        {
            _logger.LogInformation("Adding user with ID: {UserId} to team with ID: {TeamId}", addTeamMemberDto.UserId, addTeamMemberDto.TeamId);

            // Validate TeamMember
            var teamMember = new TeamMember
            {
                TeamId = addTeamMemberDto.TeamId,
                UserId = addTeamMemberDto.UserId,
                Role = addTeamMemberDto.Role
            };

            var validationResult = await _teamMemberValidator.ValidateAsync(teamMember);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for team member: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            // Check if user exists
            var user = await _unitOfWork.Repository<User>().GetById(u => u.Id == addTeamMemberDto.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found", addTeamMemberDto.UserId);
                throw new Exception("User not found.");
            }

            validationResult = await _userValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for user: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<TeamMember>().Create(teamMember);
            await _unitOfWork.Repository<TeamMember>().SaveChangesAsync();
            _logger.LogInformation("Added user with ID: {UserId} to team with ID: {TeamId} successfully", addTeamMemberDto.UserId, addTeamMemberDto.TeamId);
            return true;
        }
        #endregion

        #region Update
        public async Task<bool> UpdateTeamAsync(int id, AddTeamDto updatedTeam)
        {
            _logger.LogInformation("Updating team with ID: {TeamId}", id);

            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return false;
            }

            _mapper.Map(updatedTeam, team);
            team.UpdatedAt = DateTime.UtcNow;

            var validationResult = await _teamValidator.ValidateAsync(team);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for updated team: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<Team>().Update(team);
            var result = _unitOfWork.Complete();
            _logger.LogInformation("Team with ID: {TeamId} updated successfully", id);
            return result;
        }

        public async Task<bool> UpdateUserRoleInTeamAsync(int teamId, string userId, MemberRole newRole)
        {
            _logger.LogInformation("Updating role of user with ID: {UserId} in team with ID: {TeamId}", userId, teamId);

            var teamMember = await _unitOfWork.Repository<TeamMember>()
                .GetByCondition(tm => tm.TeamId == teamId && tm.UserId == userId)
                .FirstOrDefaultAsync();

            if (teamMember == null)
            {
                _logger.LogWarning("Team member with user ID: {UserId} in team with ID: {TeamId} not found", userId, teamId);
                return false;
            }

            teamMember.Role = newRole;

            var validationResult = await _teamMemberValidator.ValidateAsync(teamMember);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for updated team member: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<TeamMember>().Update(teamMember);
            await _unitOfWork.Repository<TeamMember>().SaveChangesAsync();
            _logger.LogInformation("Updated role of user with ID: {UserId} in team with ID: {TeamId} successfully", userId, teamId);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteTeamAsync(int id)
        {
            _logger.LogInformation("Deleting team with ID: {TeamId}", id);

            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (team == null)
            {
                _logger.LogWarning("Team with ID: {TeamId} not found", id);
                return false;
            }

            _unitOfWork.Repository<Team>().Delete(team);
            var result = _unitOfWork.Complete();
            _logger.LogInformation("Deleted team with ID: {TeamId} successfully", id);
            return result;
        }

        public async Task<bool> RemoveUserFromTeamAsync(RemoveTeamMemberDto removeUserFromTeamDto)
        {
            _logger.LogInformation("Removing user with ID: {UserId} from team with ID: {TeamId}", removeUserFromTeamDto.UserId, removeUserFromTeamDto.TeamId);
            var teamMember = await _unitOfWork.Repository<TeamMember>()
                .GetById(tm => tm.TeamId == removeUserFromTeamDto.TeamId && tm.UserId == removeUserFromTeamDto.UserId)
                .FirstOrDefaultAsync();

            if (teamMember == null)
            {
                _logger.LogWarning("Team member with user ID: {UserId} in team with ID: {TeamId} not found", removeUserFromTeamDto.UserId, removeUserFromTeamDto.TeamId);
                return false;
            }

            _unitOfWork.Repository<TeamMember>().Delete(teamMember);
            var result = _unitOfWork.Complete();
            _logger.LogInformation("Removed user with ID: {UserId} from team with ID: {TeamId} successfully", removeUserFromTeamDto.UserId, removeUserFromTeamDto.TeamId);
            return result;
        }
        #endregion
    }
}