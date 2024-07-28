using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.DTOs.TeamDtos;
using TMP.Application.Interfaces;
using TMPDomain.Entities;
using TMPDomain.Enumerations;

namespace TMP.Infrastructure.Implementations
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TeamService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync()
        {
            var teams = await _unitOfWork.Repository<Team>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<TeamDto> GetTeamByIdAsync(int id)
        {
            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<TeamDto>(team);
        }

        public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId)
        {
            var teams = await _unitOfWork.Repository<TeamMember>()
                .GetByCondition(tm => tm.UserId == userId)
                .Include(tm => tm.Team)
                .Select(tm => tm.Team)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TeamDto>>(teams);
        }

        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(int teamId)
        {
            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == teamId)
                .Include(t => t.TeamMembers).ThenInclude(tm => tm.User)
                .FirstOrDefaultAsync();

            if (team == null) return null;

            return _mapper.Map<IEnumerable<TeamMemberDto>>(team.TeamMembers);
        }

        public async Task<TeamProjectsDto> GetTeamProjectsAsync(int teamId)
        {
            var team = await _unitOfWork.Repository<Team>()
                .GetById(t => t.Id == teamId)
                .Include(t => t.ProjectTeams).ThenInclude(pt => pt.Project)
                .FirstOrDefaultAsync();

            if (team == null) return null;

            var teamProjectsDto = new TeamProjectsDto
            {
                TeamId = teamId,
                Projects = team.ProjectTeams.Select(pt => new ProjectDto
                {
                    Id = pt.Project.Id,
                    Name = pt.Project.Name,
                    Description = pt.Project.Description,
                    CreatedByUserId = pt.Project.CreatedByUserId
                }).ToList()
            };

            return teamProjectsDto;
        }

        public async Task<TeamDto> AddTeamAsync(AddTeamDto newTeam, string userId)
        {
            var team = _mapper.Map<Team>(newTeam);
            team.CreatedAt = DateTime.UtcNow;
            team.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Team>().Create(team);
            _unitOfWork.Complete();

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role = MemberRole.TeamLead 
            };

            _unitOfWork.Repository<TeamMember>().Create(teamMember);
            _unitOfWork.Complete();

            return _mapper.Map<TeamDto>(team);
        }

        public async Task<bool> AddUserToTeamAsync(AddTeamMemberDto addTeamMemberDto)
        {
            var teamMember = new TeamMember
            {
                TeamId = addTeamMemberDto.TeamId,
                UserId = addTeamMemberDto.UserId,
                Role = addTeamMemberDto.Role
            };

            _unitOfWork.Repository<TeamMember>().Create(teamMember);
            await _unitOfWork.Repository<TeamMember>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTeamAsync(int id, AddTeamDto updatedTeam)
        {
            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (team == null)
                return false;

            _mapper.Map(updatedTeam, team);
            team.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Team>().Update(team);
            return _unitOfWork.Complete();
        }

        public async Task<bool> UpdateUserRoleInTeamAsync(int teamId, string userId, MemberRole newRole)
        {
            var teamMember = await _unitOfWork.Repository<TeamMember>()
                .GetByCondition(tm => tm.TeamId == teamId && tm.UserId == userId)
                .FirstOrDefaultAsync();

            if (teamMember == null)
                return false;

            teamMember.Role = newRole;
            _unitOfWork.Repository<TeamMember>().Update(teamMember);
            await _unitOfWork.Repository<TeamMember>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTeamAsync(int id)
        {
            var team = await _unitOfWork.Repository<Team>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (team == null)
                return false;

            _unitOfWork.Repository<Team>().Delete(team);
            return _unitOfWork.Complete();
        }

        public async Task<bool> RemoveUserFromTeamAsync(RemoveTeamMemberDto removeUserFromTeamDto)
        {
            var teamMember = await _unitOfWork.Repository<TeamMember>()
                .GetById(tm => tm.TeamId == removeUserFromTeamDto.TeamId && tm.UserId == removeUserFromTeamDto.UserId)
                .FirstOrDefaultAsync();

            if (teamMember == null)
                return false;

            _unitOfWork.Repository<TeamMember>().Delete(teamMember);
            return _unitOfWork.Complete();
        }
    }
}
