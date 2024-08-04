using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.SubtaskDtos;
using TMP.Application.Interfaces;
using TMPApplication.Interfaces.Subtasks;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;

namespace TMPInfrastructure.Implementations.Subtasks
{
    public class SubtaskService : ISubtaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SubtaskService> _logger;

        public SubtaskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SubtaskService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Read
        public async Task<IEnumerable<SubtaskDto>> GetAllSubtasksAsync()
        {
            _logger.LogInformation("Fetching all subtasks");

            var subtasks = await _unitOfWork.Repository<Subtask>().GetAll()
                .Include(st => st.Task)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SubtaskDto>>(subtasks);
        }

        public async Task<SubtaskDto> GetSubtaskByIdAsync(int id)
        {
            _logger.LogInformation("Fetching subtask with ID: {SubtaskId}", id);

            var subtask = await _unitOfWork.Repository<Subtask>().GetById(st => st.Id == id)
                .Include(st => st.Task)
                .FirstOrDefaultAsync();

            if (subtask == null)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return null;
            }

            return _mapper.Map<SubtaskDto>(subtask);
        }
        #endregion

        #region Create
        public async Task<SubtaskDto> AddSubtaskAsync(AddSubtaskDto newSubtask)
        {
            _logger.LogInformation("Adding new subtask for task with ID: {TaskId}", newSubtask.TaskId);

            var taskExists = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == newSubtask.TaskId)
                .AnyAsync();

            if (!taskExists)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", newSubtask.TaskId);
                throw new Exception("Task does not exist");
            }

            var subtask = _mapper.Map<Subtask>(newSubtask);
            _unitOfWork.Repository<Subtask>().Create(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            _logger.LogInformation("Subtask for task with ID: {TaskId} added successfully", newSubtask.TaskId);
            return _mapper.Map<SubtaskDto>(subtask);
        }
        #endregion

        #region Update
        public async Task<bool> UpdateSubtaskAsync(int id, UpdateSubtaskDto updatedSubtask)
        {
            _logger.LogInformation("Updating subtask with ID: {SubtaskId}", id);

            var subtask = await _unitOfWork.Repository<Subtask>().GetById(s => s.Id == id).FirstOrDefaultAsync();
            if (subtask == null)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return false;
            }

            _mapper.Map(updatedSubtask, subtask);

            _unitOfWork.Repository<Subtask>().Update(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            _logger.LogInformation("Subtask with ID: {SubtaskId} updated successfully", id);
            return true;
        }

        public async Task<bool> UpdateSubtaskCompletionAsync(UpdateSubtaskCompletionDto dto)
        {
            _logger.LogInformation("Updating completion status of subtask with ID: {SubtaskId}", dto.SubtaskId);

            var subtask = await _unitOfWork.Repository<Subtask>().GetById(s => s.Id == dto.SubtaskId).FirstOrDefaultAsync();
            if (subtask == null)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", dto.SubtaskId);
                return false;
            }

            subtask.IsCompleted = dto.IsCompleted;

            if (dto.IsCompleted)
            {
                subtask.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                subtask.CompletedAt = null;
            }

            _unitOfWork.Repository<Subtask>().Update(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            _logger.LogInformation("Subtask with ID: {SubtaskId} completion status updated successfully", dto.SubtaskId);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteSubtaskAsync(int id)
        {
            _logger.LogInformation("Deleting subtask with ID: {SubtaskId}", id);

            var subtask = await _unitOfWork.Repository<Subtask>().GetById(st => st.Id == id).FirstOrDefaultAsync();
            if (subtask == null)
            {
                _logger.LogWarning("Subtask with ID: {SubtaskId} not found", id);
                return false;
            }

            _unitOfWork.Repository<Subtask>().Delete(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            _logger.LogInformation("Subtask with ID: {SubtaskId} deleted successfully", id);
            return true;
        }
        #endregion
    }
}
