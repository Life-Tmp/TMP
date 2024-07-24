using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TMP.Application.DTOs.TaskDtos;
using TMP.Application.Interfaces;
using TMP.Application.Tasks;
using TMPApplication.Notifications;
using TMPDomain.Entities;
using TMPDomain.ValueObjects;
using Task = TMPDomain.Entities.Task;

namespace TMP.Infrastructure.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _unitOfWork.Repository<Task>().GetAll()
                .Include(t => t.Project)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProjectIdAsync(int projectId)
        {
            var tasks = await _unitOfWork.Repository<Task>().GetByCondition(t => t.ProjectId == projectId)
                .Include(t => t.Project)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<TaskDto> GetTaskByIdAsync(int id)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id)
                .Include(t => t.Project)
                .FirstOrDefaultAsync();

            if (task == null) return null;

            return _mapper.Map<TaskDto>(task);
        }

        public async Task<TaskDto> AddTaskAsync(AddTaskDto newTask)
        {
            var task = _mapper.Map<Task>(newTask);

            _unitOfWork.Repository<Task>().Create(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();
            return _mapper.Map<TaskDto>(task);
        }

        public async Task<bool> UpdateTaskAsync(int id, AddTaskDto updatedTask)
        {

            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return false;

            _mapper.Map(updatedTask, task);
            task.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Task>().Update(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();


            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _unitOfWork.Repository<Task>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return false;

            _unitOfWork.Repository<Task>().Delete(task);
            await _unitOfWork.Repository<Task>().SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync(int? projectId)
        {
            IQueryable<Task> query = _unitOfWork.Repository<Task>().GetAll();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            var tasks = await query.Include(t => t.Project).ToListAsync();
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
    }
}
