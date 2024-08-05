using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.Comments;
using TMP.Application.DTOs.CommentDtos;
using TMP.Application.Interfaces;
using TMP.Application.Hubs;
using TaskEntity = TMPDomain.Entities.Task;
using Amazon.Runtime.Internal.Util;
using TMPApplication.Interfaces;
using FluentValidation;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<CommentHub> _commentHubContext;
        private readonly ILogger<CommentService> _logger;
        private readonly IValidator<Comment> _commentValidator;
        private readonly ICacheService _cache;

        public CommentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<CommentHub> commentHubContext,
            ILogger<CommentService> logger,
            IValidator<Comment> commentValidator,
            ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _commentHubContext = commentHubContext;
            _logger = logger;
            _cache = cache;
            _commentValidator = commentValidator;
        }

        #region Read
        public async Task<IEnumerable<CommentDto>> GetCommentsAsync()
        {
            _logger.LogInformation("Fetching all comments");

            var comments = await _unitOfWork.Repository<Comment>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }

        public async Task<CommentDto> GetCommentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching comment with ID: {CommentId}", id);

            var comment = await _unitOfWork.Repository<Comment>()
                .GetById(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found", id);
                return null;
            }

            return _mapper.Map<CommentDto>(comment);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(string userId)
        {
            _logger.LogInformation("Fetching comments for user ID: {UserId}", userId);

            var comments = await _unitOfWork.Repository<Comment>()
                .GetByCondition(c => c.UserId == userId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CommentDto>>(comments);
        }
        #endregion

        #region Create
        public async Task<CommentDto> AddCommentAsync(AddCommentDto newComment, string userId)
        {
            _logger.LogInformation("Adding new comment for user ID: {UserId}", userId);

            var task = await _unitOfWork.Repository<TaskEntity>().GetById(t => t.Id == newComment.TaskId).FirstOrDefaultAsync();
            if (task == null)
            {
                _logger.LogWarning("Task with ID: {TaskId} not found", newComment.TaskId);
                return null;
            }

            var comment = _mapper.Map<Comment>(newComment);
            comment.UserId = userId;
            comment.CreatedAt = DateTime.UtcNow;

            // Validate the comment
            var validationResult = await _commentValidator.ValidateAsync(comment);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for comment: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<Comment>().Create(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();

            var commentDto = _mapper.Map<CommentDto>(comment);

            var projectId = task.ProjectId;

            await _commentHubContext.Clients.Group(projectId.ToString()).SendAsync("ReceiveComment", commentDto);

            _logger.LogInformation("Comment with ID: {CommentId} added successfully", comment.Id);

            await _cache.DeleteKeyAsync($"task_{newComment.TaskId}_comments");

            return commentDto;
        }
        #endregion

        #region Update
        public async Task<bool> UpdateCommentAsync(int id, AddCommentDto updatedComment)
        {
            _logger.LogInformation("Updating comment with ID: {CommentId}", id);

            var comment = await _unitOfWork.Repository<Comment>().GetById(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found for update", id);
                return false;
            }

            _mapper.Map(updatedComment, comment);

            // Validate the updated comment
            var validationResult = await _commentValidator.ValidateAsync(comment);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for updated comment: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validationResult.Errors);
            }

            _unitOfWork.Repository<Comment>().Update(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();

            _logger.LogInformation("Comment with ID: {CommentId} updated successfully", id);

            await _cache.DeleteKeyAsync($"task_{updatedComment.TaskId}_comments");

            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteCommentAsync(int id)
        {
            _logger.LogInformation("Deleting comment with ID: {CommentId}", id);

            var comment = await _unitOfWork.Repository<Comment>().GetById(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null)
            {
                _logger.LogWarning("Comment with ID: {CommentId} not found for deletion", id);
                return false;
            }

            _unitOfWork.Repository<Comment>().Delete(comment);
            await _unitOfWork.Repository<Comment>().SaveChangesAsync();

            _logger.LogInformation("Comment with ID: {CommentId} deleted successfully", id);

            return true;
        }
        #endregion
    }
}
