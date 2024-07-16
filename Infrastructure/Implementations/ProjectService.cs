using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.ProjectDtos;
using TMP.Application.Interfaces;
using TMP.Application.Projects;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _unitOfWork.Repository<Project>().GetAll()
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        public async Task<ProjectDto> GetProjectByIdAsync(int id)
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetById(p => p.Id == id)
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .FirstOrDefaultAsync();

            if (project == null) return null;

            return _mapper.Map<ProjectDto>(project);
        }

        public async Task<ProjectDto> AddProjectAsync(AddProjectDto newProject, string userId)
        {
            var project = _mapper.Map<Project>(newProject);
            project.CreatedByUserId = userId;

            _unitOfWork.Repository<Project>().Create(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();

            return _mapper.Map<ProjectDto>(project);
        }

        public async Task<bool> UpdateProjectAsync(int id, AddProjectDto updatedProject)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            _mapper.Map(updatedProject, project);
            project.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Project>().Update(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _unitOfWork.Repository<Project>().GetById(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return false;

            _unitOfWork.Repository<Project>().Delete(project);
            await _unitOfWork.Repository<Project>().SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsByUserAsync(string userId)
        {
            var projects = await _unitOfWork.Repository<Project>()
                .GetByCondition(p => p.CreatedByUserId == userId)
                .Include(p => p.Tasks)
                .Include(p => p.Users)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }
    }
}
