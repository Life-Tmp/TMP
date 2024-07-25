using AutoMapper;
using Microsoft.EntityFrameworkCore;
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

        public SubtaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubtaskDto>> GetAllSubtasksAsync()
        {
            var subtasks = await _unitOfWork.Repository<Subtask>().GetAll()
                .Include(st => st.Task)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SubtaskDto>>(subtasks);
        }

        public async Task<IEnumerable<SubtaskDto>> GetSubtasksByTaskIdAsync(int taskId)
        {
            var subtasks = await _unitOfWork.Repository<Subtask>().GetByCondition(st => st.TaskId == taskId)
                .Include(st => st.Task)
                .ToListAsync();

            return _mapper.Map<IEnumerable<SubtaskDto>>(subtasks);
        }

        public async Task<SubtaskDto> GetSubtaskByIdAsync(int id)
        {
            var subtask = await _unitOfWork.Repository<Subtask>().GetById(st => st.Id == id)
                .Include(st => st.Task)
                .FirstOrDefaultAsync();

            if (subtask == null) return null;

            return _mapper.Map<SubtaskDto>(subtask);
        }

        public async Task<SubtaskDto> AddSubtaskAsync(AddSubtaskDto newSubtask)
        {
            var taskExists = await _unitOfWork.Repository<Task>()
                .GetById(t => t.Id == newSubtask.TaskId)
                .AnyAsync();

            if (!taskExists)
            {
                throw new Exception("Task does not exist");
            }

            var subtask = _mapper.Map<Subtask>(newSubtask);
            _unitOfWork.Repository<Subtask>().Create(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            return _mapper.Map<SubtaskDto>(subtask);
        }


        public async Task<bool> UpdateSubtaskAsync(int id, UpdateSubtaskDto updatedSubtask)
        {
            var subtask = await _unitOfWork.Repository<Subtask>().GetById(s => s.Id == id).FirstOrDefaultAsync();
            if (subtask == null)
            {
                return false; 
            }

            _mapper.Map(updatedSubtask, subtask);

            _unitOfWork.Repository<Subtask>().Update(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();

            return true;
        }


        public async Task<bool> DeleteSubtaskAsync(int id)
        {
            var subtask = await _unitOfWork.Repository<Subtask>().GetById(st => st.Id == id).FirstOrDefaultAsync();
            if (subtask == null) return false;

            _unitOfWork.Repository<Subtask>().Delete(subtask);
            await _unitOfWork.Repository<Subtask>().SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSubtaskCompletionAsync(UpdateSubtaskCompletionDto dto)
        {
            var subtask = await _unitOfWork.Repository<Subtask>().GetById(s => s.Id == dto.SubtaskId).FirstOrDefaultAsync();
            if (subtask == null)
            {
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

            return true;
        }
    }
}
