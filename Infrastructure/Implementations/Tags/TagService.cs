using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMP.Application.DTOs.TagDtos;
using TMP.Application.Interfaces;
using TMP.Application.Interfaces.Tags;
using TMPApplication.Interfaces;
using TMPDomain.Entities;

namespace TMP.Infrastructure.Implementations.Tags
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISearchService<TagDto> _searchService;
        private readonly ILogger<TagService> _logger;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper, ISearchService<TagDto> searchService, ILogger<TagService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _searchService = searchService;
            _logger = logger;
        }

        #region Read
        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            _logger.LogInformation("Fetching all tags");

            var tags = await _unitOfWork.Repository<Tag>().GetAll().ToListAsync();
            return _mapper.Map<IEnumerable<TagDto>>(tags);
        }

        public async Task<TagDto> GetTagByIdAsync(int id)
        {
            _logger.LogInformation("Fetching tag with ID: {TagId}", id);

            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return null;
            }

            return _mapper.Map<TagDto>(tag);
        }

        public async Task<IEnumerable<TagDto>> SearchTagsAsync(string searchTerm)
        {
            _logger.LogInformation("Searching tags with term: {SearchTerm}", searchTerm);

            var tags = await _searchService.SearchDocumentAsync(searchTerm, "tags");
            return tags;
        }
        #endregion

        #region Create
        public async Task<TagDto> AddTagAsync(AddTagDto newTag)
        {
            _logger.LogInformation("Adding new tag");

            var tag = _mapper.Map<Tag>(newTag);

            _unitOfWork.Repository<Tag>().Create(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();

            var tagDto = _mapper.Map<TagDto>(tag);
            await _searchService.IndexDocumentAsync(tagDto, "tags");

            _logger.LogInformation("Tag added successfully with ID: {TagId}", tag.Id);
            return tagDto;
        }
        #endregion

        #region Update
        public async Task<bool> UpdateTagAsync(int id, AddTagDto updatedTag)
        {
            _logger.LogInformation("Updating tag with ID: {TagId}", id);

            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return false;
            }

            _mapper.Map(updatedTag, tag);
            _unitOfWork.Repository<Tag>().Update(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();
            await _searchService.IndexDocumentAsync(_mapper.Map<TagDto>(tag), "tags");

            _logger.LogInformation("Tag with ID: {TagId} updated successfully", id);
            return true;
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteTagAsync(int id)
        {
            _logger.LogInformation("Deleting tag with ID: {TagId}", id);

            var tag = await _unitOfWork.Repository<Tag>().GetById(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null)
            {
                _logger.LogWarning("Tag with ID: {TagId} not found", id);
                return false;
            }

            _unitOfWork.Repository<Tag>().Delete(tag);
            await _unitOfWork.Repository<Tag>().SaveChangesAsync();

            await _searchService.DeleteDocumentAsync(id.ToString(), "tags");

            _logger.LogInformation("Tag with ID: {TagId} deleted successfully", id);
            return true;
        }
        #endregion
    }
}
